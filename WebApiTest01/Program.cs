using WebApiTest01;
using WebApiTest01.HostedServices;
using WebApiTest01.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<MqttManager>();

builder.Services.Configure<MqttConfig>(builder.Configuration.GetSection("MqttConfig"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolucyBuilder =>
{
    corsPolucyBuilder.AllowAnyOrigin();
});

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ControlHub>("/controlHub");
app.MapFallbackToFile("index.html");

app.Run();
