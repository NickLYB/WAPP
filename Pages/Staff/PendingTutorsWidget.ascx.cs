using System;
using System.Web.UI;

namespace WAPP.Pages.Staff
{
    public partial class PendingTutorsWidget : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // In the future, replace "3" with a database count query
                lblPendingCount.Text = "3";
            }
        }

        protected void btnReview_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Staff/VerifyTutors.aspx");
        }
    }
}