﻿using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using Microsoft.Extensions.Options;

namespace LedStripeWithSensors.MqttManager;

internal sealed class MqttClient : IAsyncDisposable
{
    private readonly SemaphoreSlim _allowReconnect;

    private const string MessageLeft = "LEFT";
    private const string MessageRight = "RIGHT";

    private readonly MqttClientConfig _config;
    private HiveMQClient _client;
    private ChannelManagerWithRecovery _channelManager;

    public MqttClient(IOptions<MqttClientConfig> config)
    {
        _config = config.Value;
        _allowReconnect = new SemaphoreSlim(1);
    }

    public MqttClient Connect(Action onOverrideLeft, Action onOverrideRight, CancellationToken ct)
    {
        var options = new HiveMQClientOptions
        {
            ClientId = _config.ClientId,
            Host = _config.Host,
            Port = _config.Port,
            UseTLS = _config.UseTLS,
            UserName = _config.User,
            Password = _config.Password,
        };

        _channelManager = ChannelManagerWithRecovery.StartConsumer(
            recoveryAsyncFunc: async (CancellationToken ct) => await ReconnectAsync().ConfigureAwait(false),
            maxAttempts: 0, 
            ct);

        _client = new HiveMQClient(options);

        _client.OnMessageReceived += (sender, args) =>
        {
            string payload = args.PublishMessage.PayloadAsString.ToUpper();
            switch (payload)
            {
                //TODO: clear
                case MessageLeft: onOverrideLeft(); Send("left override detected"); break;
                case MessageRight: onOverrideRight(); Send("right override detected"); break;
                default: break;
            }
        };

        _client.AfterDisconnect += async (sender, args) =>
        {
            await ReconnectWithRetryAsync(0, ct).ConfigureAwait(false);
        };

        _ = ReconnectWithRetryAsync(0, ct);

        return this;
    }

    public void Send(string msg)
    {
        if (_channelManager is null)
            throw new Exception("Channel manager not created");

        _channelManager.Send(new ExpiratingAsyncDelegate()
        {
            Delegate = async (CancellationToken ct) =>
            {
                var result = await _client.PublishAsync(_config.MotionDetectedTopic, msg).ConfigureAwait(false);
                return true;
            },
            ExpirationDate = DateTime.UtcNow.AddSeconds(10)
        });
    }

    private async ValueTask<bool> ReconnectAsync()
    {
        if (_client.IsConnected())
            return true;

        if (!await _allowReconnect.WaitAsync(1000).ConfigureAwait(false))
            return false;

        try
        {
            var connectResult = await _client.ConnectAsync().ConfigureAwait(false);
            if (connectResult.ReasonCode == HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
            {
                foreach (var sub in _client.Subscriptions)
                {
                    await _client.UnsubscribeAsync(sub).ConfigureAwait(false);
                }
                var subscribeResult = await _client.SubscribeAsync(_config.OverrideTopic).ConfigureAwait(false);
                return subscribeResult != null;
            }
        }
        catch (HiveMQttClientException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _allowReconnect.Release();
        }
        return false;
    }


    private async ValueTask<bool> ReconnectWithRetryAsync(int maxAttempts, CancellationToken ct = default)
    {
        int delayMs = 100;
        int attemptNo = 0;

        while (!ct.IsCancellationRequested)
        {
            var success = await ReconnectAsync().ConfigureAwait(false);
            if (success)
                return true;

            attemptNo++;
            if (maxAttempts != 0 && attemptNo >= maxAttempts)
                return false;

            await Task.Delay(delayMs).ConfigureAwait(false);

            delayMs = Math.Min(delayMs * 2, 60_000);
        }
        return false;
    }

    public async ValueTask DisposeAsync()
    {
        if (_client?.IsConnected() == true)
            await _client.DisconnectAsync().ConfigureAwait(false);
        _client?.Dispose();
    }
}
