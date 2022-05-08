using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CarpoolServer.Hubs
{
    public class CarpoolHub : Hub
    {
        public async Task SendMessageToGroup(string user, string message, string groupName)
        {
            IClientProxy proxy = Clients.Group(groupName);
            await proxy.SendAsync("ReceiveMessageFromGroup", user, message, groupName);
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task OnConnect(string[] groupNames)
        {
            foreach (string groupName in groupNames)
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await base.OnConnectedAsync();
        }

        //public async Task OnConnect(string groupName)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //    await base.OnConnectedAsync();
        //}

        public async Task OnDisconnect(string[] groupNames)
        {
            foreach (string groupName in groupNames)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await base.OnDisconnectedAsync(null);
        }

        //public async Task OnDisconnect(string groupName)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        //    await base.OnDisconnectedAsync(null);
        //}
    }
}
