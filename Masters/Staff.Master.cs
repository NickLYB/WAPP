using System;
using System.Web.UI;

namespace WAPP.Masters
{
    public partial class Staff : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Security Check: Ensure only Staff (Role 2) can access pages using this master
            if (Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Login.aspx");
            }
        }
    }
}