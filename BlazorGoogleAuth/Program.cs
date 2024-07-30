using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGoogleAuth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
    .AddAuthentication()
    .AddIdentityServerJwt().AddGoogle(o =>
    {
        o.ClientId = builder.Configuration.GetValue<string>("Authentication:Google:ClientId");
        o.ClientSecret = builder.Configuration.GetValue<string>("Authentication:Google:ClientSecret");
    });

await builder.Build().RunAsync();
