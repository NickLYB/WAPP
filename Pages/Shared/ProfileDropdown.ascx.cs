using System;
using System.Web.UI;

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
            }
        }

        // Universal Sign Out Method
        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Pages/Guest/Home.aspx");
        }
    }
}