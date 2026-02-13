using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Master
{
    public partial class Guest : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPassword.Text;

            // Replace with your actual database logic
            if (email == "user@test.com" && password == "password123")
            {
                Session["User"] = email;
                Response.Redirect("~/Pages/Member/Dashboard.aspx");
            }
            else
            {
                lblError.Text = "Invalid email or password. Please try again.";
                // No need for extra JS to keep modal open because UpdatePanel 
                // handles the partial postback automatically!
            }
        }
    }


}