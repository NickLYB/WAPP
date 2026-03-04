using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            else
            {
                lblStudentName.Text = Session["UserName"].ToString();
            }
        }
    }
}