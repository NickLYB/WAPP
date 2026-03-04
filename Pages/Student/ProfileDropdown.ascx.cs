using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class ProfileDropdown : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Pull name from session set during login
                if (Session["UserName"] != null)
                {
                    lblUserFullName.Text = Session["UserName"].ToString();
                }
            }
        }

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Pages/Guest/Home.aspx");
        }
    }
}