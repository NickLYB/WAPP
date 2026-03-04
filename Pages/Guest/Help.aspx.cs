using System;
using System.Web.UI;

namespace WAPP.Pages.Guest
{
    public partial class Help : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            // Check if a user is logged in (Assuming you store their Role in a Session variable)
            if (Session["role_id"] != null)
            {
                string userRole = Session["role_id"].ToString().ToUpper();

                // Swap the master page based on the role
                switch (userRole)
                {
                    case "1":
                        this.MasterPageFile = "~/Masters/Admin.master";
                        break;
                    case "2":
                        this.MasterPageFile = "~/Masters/Staff.Master";
                        break;
                    case "3":
                        this.MasterPageFile = "~/Masters/Tutor.Master";
                        break;
                    case "4":
                        this.MasterPageFile = "~/Masters/Student.Master";
                        break;
                    default:
                        this.MasterPageFile = "~/Masters/Guest.Master";
                        break;
                }
            }
            else
            {
                // If no session exists, they are a guest. 
                // It defaults to Guest.Master as defined in the .aspx file.
                this.MasterPageFile = "~/Masters/Guest.Master";
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Page load logic goes here
            }
        }

        protected void btnSearchHelp_Click(object sender, EventArgs e)
        {
            string query = txtSearchHelp.Text.Trim();

            if (!string.IsNullOrEmpty(query))
            {
                // If you have a dedicated search results page, redirect there.
                // Otherwise, you can just reload the page with a query string for now.
                Response.Redirect($"~/Help.aspx?q={Server.UrlEncode(query)}");
            }
        }
    }
}