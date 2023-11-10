using LedStripeWithSensors;
using LedStripeWithSensors.MqttManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appSettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddUserSecrets(Assembly.GetExecutingAssembly());

var mqttPaswword = builder.Configuration["MqttConfig:Password"];
builder.Services.AddOptions<ClientConfig>().BindConfiguration("MqttConfig")
    .PostConfigure(config => { config.Password = mqttPaswword!; });

builder.Services.AddScoped<IAnimation, FlyingBallsAnimation>();
builder.Services.AddSingleton<MqttClient>();
builder.Services.AddSingleton<AnimationManager>();

using IHost host = builder.Build();

CancellationToken ct = new();

await using var animationManager = host.Services.GetRequiredService<AnimationManager>();

_ = Task.Run(() => animationManager.Run(ct));

//var mqttClient = host.Services.GetService<MqttClient>();
//mqttClient.Connect(
//    () => Console.WriteLine("Override LEFT"),
//    () => Console.WriteLine("Override RIGHT"),
//    ct);

await host.RunAsync();