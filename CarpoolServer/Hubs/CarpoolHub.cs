using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CarpoolServer.Hubs
{
    public class CarpoolHub : Hub
    {
        public async Task SendKidOnBoard(int carpoolId, int kidId)
        {
            IClientProxy proxy = Clients.Group(carpoolId.ToString());
            await proxy.SendAsync("UpdateKidOnBoard", kidId);
        }
        public async Task SendLocation(int carpoolId, double longitude, double latitude)
        {
            IClientProxy proxy = Clients.Group(carpoolId.ToString());
            await proxy.SendAsync("UpdateDriverLocation", longitude, latitude);
        }
        public async Task SendArriveToDestination(int carpoolId)
        {
            IClientProxy proxy = Clients.Group(carpoolId.ToString());
            await proxy.SendAsync("UpdateArriveToDestination");
        }
        public async Task OnConnect(int carpoolId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, carpoolId.ToString());
            await base.OnConnectedAsync();
        }

        //public async Task OnConnect(string groupName)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //    await base.OnConnectedAsync();
        //}

        public async Task OnDisconnect(int carpoolId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, carpoolId.ToString());
            await base.OnDisconnectedAsync(null);
        }

        //public async Task OnDisconnect(string groupName)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        //    await base.OnDisconnectedAsync(null);
        //}

        public async Task StartDrive(int carpoolId)
        {
            IClientProxy proxy = Clients.Group(carpoolId.ToString());
            await proxy.SendAsync("StartDrive");
        }
    }
}
