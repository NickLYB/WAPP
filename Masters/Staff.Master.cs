using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace WAPP.Masters
{
    public partial class Staff : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["LastNotifCheck"] == null) Session["LastNotifCheck"] = DateTime.Now;

                // Store the User ID in the hidden field so JS can read it for SignalR
                if (Session["UserId"] != null)
                {
                    hfMyUserId.Value = Session["UserId"].ToString();
                }
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            UpdateUnreadCount();
            UpdateChatCount();
        }

        public void UpdateUnreadCount()
        {
            if (Session["UserId"] == null) return;
            int userId = Convert.ToInt32(Session["UserId"]);
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT COUNT(1) FROM [notification] WHERE user_id = @userId AND status = 'UNREAD'";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        conn.Open();
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        lblUnreadCount.Text = count > 99 ? "99+" : count.ToString();
                        lblUnreadCount.Visible = count > 0;
                    }
                }
            }
            catch { }
        }

        private void UpdateChatCount()
        {
            if (Session["UserId"] == null) return;

            try
            {
                int currentUserId = Convert.ToInt32(Session["UserId"]);

                string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connString))
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

        // Fired quietly in the background by SignalR
        protected void btnStaffSignalRUpdate_Click(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) return;

            Session["LastNotifCheck"] = DateTime.Now;
            UpdateUnreadCount();
            UpdateChatCount();
            upBell.Update();
            upChatIcon.Update();
        }
    }
}