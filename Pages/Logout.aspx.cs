using System;
using System.Web.UI;
using WAPP.Utils;

namespace WAPP.Pages
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Session["UserId"] != null)
                {
                    int currentUserId = Convert.ToInt32(Session["UserId"]);

                    // 1. Log the logout (Status 'AUDIT' passes your database Check Constraint)
                    SystemLogService.Write("AUTH_LOGOUT", "User explicitly signed out.", LogLevel.INFO, currentUserId, "AUDIT");

                    // 2. Ping SignalR so Admin Dashboard updates Active Sessions instantly
                    try
                    {
                        var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<WAPP.Hubs.NotificationHub>();
                        hubContext.Clients.All.receiveNotification(0);
                    }
                    catch { /* Ignore if SignalR is disconnected */ }
                }
            }
            catch (Exception)
            {
                // Ignore DB errors to ensure the user can still log out smoothly
            }
            finally
            {
                // 3. Destroy session and redirect to the public home page
                Session.Clear();
                Session.Abandon();
                Response.Redirect("~/Pages/Guest/Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}