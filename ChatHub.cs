using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAPP
{
    public class ChatHub : Hub
    {
        // This method is called from the Javascript on the client
        public void SendMessage(string senderId, string receiverId)
        {
            // We ping EVERY connected client. 
            // The client-side Javascript will decide if the message belongs to them.
            Clients.All.receiveNewMessage(senderId, receiverId);
        }
    }
}