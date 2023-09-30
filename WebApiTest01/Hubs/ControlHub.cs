using Microsoft.AspNetCore.SignalR;

namespace WebApiTest01.Hubs;

public class ControlHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}

