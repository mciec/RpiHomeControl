using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Microsoft.Extensions.Options;

namespace WebApiTest01.HostedServices;

public class MqttManager : BackgroundService
{
    private readonly MqttConfig _config;

    public MqttManager(IOptions<MqttConfig> options)
    {
        _config = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var options = new HiveMQClientOptions()
        {
            Host = _config.Host,
            Port = _config.Port,
            UseTLS = true,
            UserName = _config.User,
            Password = _config.Password,
        };

        var client = new HiveMQClient(options);
        try
        {
            var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex) 
        { 
            Console.WriteLine(ex);
        }

        client.OnMessageReceived += async (sender, args) =>
        {
            var msg = args.PublishMessage.PayloadAsString;
            Console.WriteLine($"Message: {msg}");
            await client.PublishAsync("outside/set-motion", $"Received: [{msg}]");
        };

        _ =await client.SubscribeAsync("outside/read-motion").ConfigureAwait(false);

        while (true)
        {
            await Task.Delay(1000).ConfigureAwait(false);
        }
    }

}
