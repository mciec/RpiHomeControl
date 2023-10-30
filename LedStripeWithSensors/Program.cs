using LedStripeWithSensors;
using LedStripeWithSensors.MqttManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions<ClientConfig>("MqttConfig");
builder.Services.AddScoped<IAnimation, FlyingBallsAnimation>();
builder.Services.AddSingleton<MqttClient>();

using IHost host = builder.Build();


var mqttClient = host.Services.GetService<MqttClient>();

CancellationToken ct = new CancellationToken();

mqttClient.Connect(
    () => Console.WriteLine("Override LEFT"),
    () => Console.WriteLine("Override RIGHT"),
    ct);

await host.RunAsync();