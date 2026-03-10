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

        public void SendVideoSignal(string senderId, string receiverId, string signalData)
        {
            // Broadcast the signal to all connected clients.
            // The JavaScript on Chat.aspx will filter it using the receiverId 
            // so only the correct student actually receives the call.
            Clients.All.receiveVideoSignal(senderId, receiverId, signalData);
        }
    }
}