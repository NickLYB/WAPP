using Microsoft.AspNet.SignalR;

namespace WAPP.Hubs
{
    public class NotificationHub : Hub
    {
        // This method can be called by the server to push a notification to a specific user
        public void SendNotification(int targetUserId)
        {
            Clients.All.receiveNotification(targetUserId);
        }

        // Overload: Send a global notification (like an Announcement) to everyone
        public void SendGlobalNotification()
        {
            Clients.All.receiveNotification(0); // 0 means it's for everyone
        }

        public void SendSystemAlert(string logMessage)
        {
            // We use a different client method name so students/tutors ignore it
            Clients.All.receiveSystemAlert(logMessage);
        }
    }
}