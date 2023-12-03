using Iot.Device.Ws28xx;
using LedStripeWithSensors.AnimationManager;
using LedStripeWithSensors.Animations;
using LedStripeWithSensors.MotionSensor;
using LedStripeWithSensors.MqttManager;
using LedStripeWithSensors.Neopixel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Device.Spi;
using System.Reflection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appSettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .AddEnvironmentVariables(src =>
    {
        src.Prefix = "DOTNET_LEDSTRIPEWITHSENSORS_";
    });

var mqttPaswword = builder.Configuration["MqttConfig:Password"];
builder.Services.AddOptions<MqttClientConfig>().BindConfiguration("MqttConfig")
    .PostConfigure(config => { config.Password = mqttPaswword!; });
builder.Services.AddOptions<MotionSensorsConfig>().BindConfiguration("MotionSensorsConfig");
builder.Services.AddOptions<AnimationsConfig>().BindConfiguration("Animations");
builder.Services.AddOptions<NeopixelConfig>().BindConfiguration("Neopixel");
builder.Services.AddOptions<AnimationManagerConfig>().BindConfiguration("Manager");

builder.Services.AddScoped<AnimationFactory>();
builder.Services.AddSingleton<MqttClient>();
builder.Services.AddSingleton<AnimationManager>();
builder.Services.AddSingleton(provider =>
    {
        return null;
        SpiConnectionSettings settings = new(0, 0)
        {
            ClockFrequency = 2_400_000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        };

        var spi = SpiDevice.Create(settings);
        var neopixelConfig = provider.GetService<IOptions<NeopixelConfig>>();
        if (neopixelConfig is null)
            throw new Exception("NeopixelConfig not available");
        var ws2812B = new Ws2812b(spi, neopixelConfig.Value.Width);
        return ws2812B;
    });

using IHost host = builder.Build();

CancellationToken ct = new();

using var animationManager = host.Services.GetRequiredService<AnimationManager>();

_ = Task.Run(() => animationManager.Run(ct));

//var mqttClient = host.Services.GetService<MqttClient>();
//mqttClient.Connect(
//    () => Console.WriteLine("Override LEFT"),
//    () => Console.WriteLine("Override RIGHT"),
//    ct);

await host.RunAsync();