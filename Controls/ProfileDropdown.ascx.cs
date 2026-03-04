using System;
using System.Web.UI;

namespace WAPP.Controls
{
    public partial class ProfileDropdown : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. Set User Name
                if (Session["UserName"] != null)
                {
                    lblUserFullName.Text = Session["UserName"].ToString();
                }
                else
                {
                    lblUserFullName.Text = "Guest User";
                }

                // 2. Fetch role_id safely (defaults to 0 if null or invalid)
                int roleId = 0;
                if (Session["role_id"] != null)
                {
                    int.TryParse(Session["role_id"].ToString(), out roleId);
                }

                string roleName = "GUEST";

                // 3. Map the role_id to the correct text and URLs
                switch (roleId)
                {
                    case 1: // Admin
                        roleName = "Admin";
                        hlProfile.NavigateUrl = "~/Pages/Admin/Profile.aspx";
                        hlSettings.NavigateUrl = "~/Pages/Admin/Settings.aspx";
                        break;

                    case 2: // Staff
                        roleName = "Staff";
                        hlProfile.NavigateUrl = "~/Pages/Staff/Profile.aspx"; // Adjust this folder path if Staff uses the Admin folder
                        hlSettings.NavigateUrl = "~/Pages/Staff/Settings.aspx";
                        break;

                    case 3: // Tutor
                        roleName = "Tutor";
                        hlProfile.NavigateUrl = "~/Pages/Tutor/Profile.aspx";
                        hlSettings.NavigateUrl = "~/Pages/Tutor/Settings.aspx";
                        break;

                    case 4: // Student
                        roleName = "Student";
                        hlProfile.NavigateUrl = "~/Pages/Student/Profile.aspx";
                        hlSettings.NavigateUrl = "~/Pages/Student/Settings.aspx";
                        break;

                    default: // Unauthenticated or Unknown ID
                        roleName = "GUEST";
                        hlProfile.Visible = false;
                        hlSettings.Visible = false;
                        break;
                }

                // 4. Update the UI Label
                lblUserRole.Text = roleName;
            }
        }

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            // Clear all sessions and force the user back to the public homepage
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Pages/Guest/Home.aspx");
        }
    }
}