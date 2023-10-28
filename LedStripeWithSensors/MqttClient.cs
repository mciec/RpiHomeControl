using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using Microsoft.Extensions.Options;

namespace LedStripeWithSensors;




internal sealed class MqttClient
{
    private static bool IsConnectingInProgress = false;

    private const string MessageLeft = "LEFT";
    private const string MessageRight = "RIGHT";

    private readonly ClientConfig _config;
    private HiveMQClient _client;
    public MqttClient(IOptions<ClientConfig> config)
    {
        _config = config.Value;
    }

    public async MqttClient ConnectAsync(Action onOverrideLeft, Action onOverrideRight)
    {
        var options = new HiveMQClientOptions
        {
            Host = _config.Host,
            Port = _config.Port,
        };

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

        _client.AfterDisconnect += async (sender, args) => await ConnectWithRetry(0);

        await _client.SubscribeAsync(_config.OverrideTopic).ConfigureAwait(false);

        return this;
    }

    public async Task Send(string msg)
    {
        try
        {
            await _client.PublishAsync(_config.MotionDetectedTopic, msg;
        }
        catch (HiveMQttClientException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task ConnectWithRetry(int maxAttempts = 0)
    {
        IsConnectingInProgress = true;
        int delayMs = 100;
        int attempts = 1;

        while (true)
        {
            try
            {
                var connectResult = await _client.ConnectAsync().ConfigureAwait(false);
                if (connectResult.ReasonCode == HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                {
                    IsConnectingInProgress = false;
                    break;
                }
            }
            catch (HiveMQttClientException ex)
            {
                Console.WriteLine(ex.Message);
            }

            await Task.Delay(delayMs).ConfigureAwait(false);
            attempts++;
            delayMs = Math.Min(delayMs * 2, 60_000);

            if (attempts > maxAttempts && maxAttempts != 0)
            {
                IsConnectingInProgress = false;
                break;
            }
        }
    }
}

// Publish a message
Console.WriteLine("Publishing a test message...");
var resultPublish = await client.PublishAsync(
    topic,
    JsonSerializer.Serialize(new
    {
        Command = "Hello",
    })
).ConfigureAwait(false);

while (true)
{
    await Task.Delay(2000).ConfigureAwait(false);
Console.WriteLine("Press q exit...");
    if (Console.ReadKey().Key == ConsoleKey.Q)
    {
        Console.WriteLine("\n");
        break;
    }
}

Console.WriteLine("Disconnecting gracefully and waiting 5 seconds...");
await client.DisconnectAsync().ConfigureAwait(false);
await Task.Delay(5000).ConfigureAwait(false);



}

