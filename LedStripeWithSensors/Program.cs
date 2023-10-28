using LedStripeWithSensors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions<ClientConfig>("MqttConfig");
builder.Services.AddScoped<IAnimation, FlyingBallsAnimation>();
builder.Services.AddSingleton<MqttClient>(provider =>
{
    var config = provider.GetService<ClientConfig>();
    var client = new MqttClient()
})

using IHost host = builder.Build();


//    .ConfigureServices(services =>
//    {
//        services.AddSingleton<MyClass2>();
//        services.AddSingleton<MyClass>();
//    })
//    .Build();

//var myClass = host.Services.GetService<MyClass>();
//await myClass!.DoStuff();

await host.RunAsync();