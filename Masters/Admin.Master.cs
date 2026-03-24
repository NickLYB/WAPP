using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WAPP.Masters
{
    public partial class AdminMaster : System.Web.UI.MasterPage
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                UpdateUnreadCount();
                UpdateChatCount();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Recalculate counts right before the page sends HTML back to the browser
            UpdateUnreadCount();
            UpdateChatCount();

            // ensure the Bell and Chat icons are also told to visually refresh
            if (ScriptManager.GetCurrent(Page) != null && ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                upBell.Update();
                upChatIcon.Update();
            }
        }

        // Fired instantly when SignalR detects a new log
        protected void btnAdminSignalRUpdate_Click(object sender, EventArgs e)
        {
            UpdateUnreadCount();
            UpdateChatCount();

            // Push the updated bell HTML to the browser without refreshing the page
            upBell.Update();
            upChatIcon.Update();
        }

        private void UpdateUnreadCount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    string sql = "SELECT COUNT(Id) FROM [systemLog] WHERE status = 'OPEN'";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        con.Open();
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            lblUnreadCount.Text = count > 99 ? "99+" : count.ToString();
                            lblUnreadCount.Visible = true;
                        }
                        else
                        {
                            lblUnreadCount.Visible = false;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void UpdateChatCount()
        {
            if (Session["UserId"] == null) return;

            try
            {
                int currentUserId = Convert.ToInt32(Session["UserId"]);
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    // Count how many messages in the DB are addressed to ME and are NOT read yet.
                    string sql = "SELECT COUNT(Id) FROM [chatMessage] WHERE receiver_id = @MyId AND is_read = 0";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@MyId", currentUserId);
                        con.Open();
                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            lblUnreadChatCount.Text = count > 99 ? "99+" : count.ToString();
                            lblUnreadChatCount.Visible = true;
                        }
                        else
                        {
                            lblUnreadChatCount.Visible = false;
                        }
                    }
                }
            }
            catch { }
        }
    }
}