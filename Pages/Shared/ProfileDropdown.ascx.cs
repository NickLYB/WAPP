using System;
using System.Web.UI;
using WAPP.Utils; // Needed to access SystemLogService and LogLevel

namespace WAPP.Pages.Shared
{
    public partial class ProfileDropdown : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. Set User's Name
                if (Session["UserName"] != null)
                {
                    lblUserFullName.Text = Session["UserName"].ToString();
                }
                else
                {
                    lblUserFullName.Text = "User";
                }

                // 2. Set User's Role dynamically
                if (Session["role_id"] != null)
                {
                    int roleId = Convert.ToInt32(Session["role_id"]);
                    switch (roleId)
                    {
                        case 1:
                            litUserRole.Text = "System Admin";
                            break;
                        case 2:
                            litUserRole.Text = "Staff";
                            break;
                        case 3:
                            litUserRole.Text = "Tutor";
                            break;
                        case 4:
                            litUserRole.Text = "Student";
                            break;
                        default:
                            litUserRole.Text = "Guest";
                            break;
                    }
                }

                // 3. Load Dynamic Profile Picture
                if (Session["ProfilePic"] != null && !string.IsNullOrWhiteSpace(Session["ProfilePic"].ToString()))
                {
                    // User has a custom picture, use it!
                    imgProfileNavbar.ImageUrl = Session["ProfilePic"].ToString();
                    imgDropdownThumb.ImageUrl = Session["ProfilePic"].ToString();
                }
                else
                {
                    // Null or empty: Fallback to the default image
                    imgProfileNavbar.ImageUrl = "~/Images/profile_m.png";
                    imgDropdownThumb.ImageUrl = "~/Images/profile_m.png";
                }
            }
        }

        // Universal Sign Out Method
        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session["UserId"] != null)
                {
                    int currentUserId = Convert.ToInt32(Session["UserId"]);

                    // 1. Log the logout action (Status 'AUDIT' avoids Check Constraint error)
                    SystemLogService.Write("AUTH_LOGOUT", "User explicitly signed out.", LogLevel.INFO, currentUserId, "AUDIT");

                    // 2. Ping SignalR to update the Admin Dashboard instantly
                    try
                    {
                        var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<WAPP.Hubs.NotificationHub>();
                        hubContext.Clients.All.receiveNotification(0);
                    }
                    catch {}
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                // 3. Clear sessions and redirect safely
                Session.Clear();
                Session.Abandon();

                // Using false prevents ThreadAbortException from killing the DB transaction
                Response.Redirect("~/Pages/Guest/Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}