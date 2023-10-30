using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using Microsoft.Extensions.Options;

namespace LedStripeWithSensors.MqttManager;

internal sealed class MqttClient
{
    private readonly Mutex _allowReconnect;

    private const string MessageLeft = "LEFT";
    private const string MessageRight = "RIGHT";

    private readonly ClientConfig _config;
    private HiveMQClient _client;
    private ChannelManagerWithRecovery _channelManager;
    private Task _consumerTask;

    public MqttClient(IOptions<ClientConfig> config)
    {
        _config = config.Value;
        _allowReconnect = new Mutex(false);
    }

    public MqttClient Connect(Action onOverrideLeft, Action onOverrideRight, CancellationToken ct)
    {
        var options = new HiveMQClientOptions
        {
            Host = _config.Host,
            Port = _config.Port,
            UserName = _config.User,
            Password = _config.Password,
        };

        _consumerTask = ChannelManagerWithRecovery.StartConsumer(
            async (CancellationToken ct) => await ReconnectAsync().ConfigureAwait(false),
            0, ct);

        _client = new HiveMQClient(options);

        _client.OnMessageReceived += (sender, args) =>
        {
            string payload = args.PublishMessage.PayloadAsString.ToUpper();
            switch (payload)
            {
                case MessageLeft: onOverrideLeft(); break;
                case MessageRight: onOverrideRight(); break;
                default: break;
            }
        };

        _client.AfterDisconnect += async (sender, args) => await ReconnectWithRetryAsync(0, ct).ConfigureAwait(false);

        _ = ReconnectWithRetryAsync(0, ct);

        _ = _client.SubscribeAsync(_config.OverrideTopic).ConfigureAwait(false);

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
                var result = await _client.PublishAsync(_config.MotionDetectedTopic, msg);
                return true;
            },
            ExpirationDate = DateTime.UtcNow.AddSeconds(10)
        });
    }

    private async ValueTask<bool> ReconnectAsync()
    {
        if (_client.IsConnected())
            return true;

        if (!_allowReconnect.WaitOne(1000))
            return false;

        try
        {
            var connectResult = await _client.ConnectAsync().ConfigureAwait(false);
            if (connectResult.ReasonCode == HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                return true;
        }
        catch (HiveMQttClientException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _allowReconnect.ReleaseMutex();
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
}
