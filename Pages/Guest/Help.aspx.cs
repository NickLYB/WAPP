using System;
using System.Web.UI;

namespace WAPP.Pages.Guest
{
    public partial class Help : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            // Dynamic Master Page switching based on role
            if (Session["role_id"] != null)
            {
                string userRole = Session["role_id"].ToString();

                switch (userRole)
                {
                    case "1": this.MasterPageFile = "~/Masters/Admin.master"; break;
                    case "2": this.MasterPageFile = "~/Masters/Staff.Master"; break;
                    case "3": this.MasterPageFile = "~/Masters/Tutor.Master"; break;
                    case "4": this.MasterPageFile = "~/Masters/Student.Master"; break;
                    default: this.MasterPageFile = "~/Masters/Guest.Master"; break;
                }
            }
            else
            {
                this.MasterPageFile = "~/Masters/Guest.Master";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}