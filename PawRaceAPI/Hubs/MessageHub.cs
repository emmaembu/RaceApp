using Microsoft.AspNetCore.SignalR;

namespace PawRaceAPI.Hubs
{
    public class MessageHub : Hub
    {

        public async Task SendMessager(string user, string message)
        {
            await Clients.All.SendAsync(user, message);
        }
    }
}
