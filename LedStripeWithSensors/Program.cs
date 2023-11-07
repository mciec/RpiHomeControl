using LedStripeWithSensors;
using LedStripeWithSensors.MqttManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
//IHostEnvironment env = builder.Environment;
//builder.Configuration.Sources.Clear();
//builder.Configuration
//    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appSettings.{env.EnvironmentName}.json", true, true);

builder.Services.AddOptions<ClientConfig>().BindConfiguration("MqttConfig");
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