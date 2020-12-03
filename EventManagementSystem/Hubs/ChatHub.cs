using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace EventManagementSystem.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(string name, string message)
        {
            //This function is called to update the clients of new messages sent 
            Clients.All.addNewMessageToPage(name, message);
        }

        public void JoinGroup(string groupName)
        {
            Groups.Add(Context.ConnectionId, groupName);
        }

        public void LeaveGroup(string groupName)
        {
            Groups.Remove(Context.ConnectionId, groupName);
            Clients.Group(groupName).addChatMessage(Context.User.Identity.Name + " joined.");
        }

    }
}