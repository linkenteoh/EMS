using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace EventManagementSystem.Hubs
{
    [HubName("TaskHub")]
    public class TaskHub : Hub
    {
        [HubMethodName("show")]
        public static void Show()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<TaskHub>();
            context.Clients.All.newTask();
        }
    }
}