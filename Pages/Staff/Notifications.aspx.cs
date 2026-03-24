using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Staff
{
    public partial class Notifications : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadNotifications();
            }
            else
            {
                // Catch the async postback fired by Staff.Master when an announcement notification is received
                string eventTarget = Request.Params["__EVENTTARGET"];
                if (!string.IsNullOrEmpty(eventTarget) && eventTarget.Contains("btnStaffSignalRUpdate"))
                {
                    // Master page got an announcement! Piggyback and refresh our notifications panel!
                    LoadNotifications();
                    upNotifications.Update();
                }
            }
        }

        private void LoadNotifications()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(cs))
            {
                string query = @"
                    SELECT 
                        n.Id AS notification_id,
                        CASE 
                            WHEN n.announcement_id IS NOT NULL THEN 'Announcement'
                            ELSE 'System Alert'
                        END AS title, 
                        COALESCE(a.message, n.content) AS message, 
                        n.created_at, 
                        n.status
                    FROM notification n
                    LEFT JOIN announcement a ON n.announcement_id = a.Id
                    WHERE n.user_id = @UserId AND n.status != 'ARCHIVED' AND n.created_at <= GETDATE()
                    ORDER BY 
                        CASE WHEN n.status = 'UNREAD' THEN 0 ELSE 1 END,
                        n.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    conn.Open();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        rptAllNotifications.DataSource = dt;
                        rptAllNotifications.DataBind();
                        rptAllNotifications.Visible = true;
                        divNoNotifications.Visible = false;
                        btnMarkAllRead.Visible = true;
                    }
                    else
                    {
                        rptAllNotifications.Visible = false;
                        divNoNotifications.Visible = true;
                        btnMarkAllRead.Visible = false;
                    }
                }
            }
        }

        protected void rptAllNotifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            if (e.CommandName == "Archive")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);
                UpdateNotificationStatus(notificationId, userId, "ARCHIVED");
                LoadNotifications();
                UpdateMasterBell(); // Force the red bell count to recalculate instantly
            }
            else if (e.CommandName == "View")
            {
                string[] data = e.CommandArgument.ToString().Split('|');
                if (data.Length >= 4)
                {
                    int notificationId = Convert.ToInt32(data[0]);
                    string title = data[1];
                    string message = data[2];
                    string createdAt = data[3];

                    // 1. Mark as read in the database
                    UpdateNotificationStatus(notificationId, userId, "READ");

                    // 2. Reload the UI list so the 'New' badge drops
                    LoadNotifications();

                    // 3. Drop the unread counter on the Master Page bell
                    UpdateMasterBell();

                    lblModalTitle.Text = title;
                    litModalMessage.Text = message;
                    lblModalDate.Text = FormatDate(createdAt);

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowNotificationModal",
                        "var modal = new bootstrap.Modal(document.getElementById('notificationModal')); modal.show();", true);
                }
            }
        }

        protected void btnMarkAllRead_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(cs))
            {
                string query = @"
                    UPDATE notification 
                    SET status = 'READ', read_at = GETDATE() 
                    WHERE user_id = @UserId AND status = 'UNREAD' AND created_at <= GETDATE()";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            LoadNotifications();
            UpdateMasterBell(); // Clear the bell!
        }

        private void UpdateNotificationStatus(int notificationId, int userId, string newStatus)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                string query = @"
                    UPDATE notification 
                    SET status = @Status, 
                        read_at = CASE WHEN @Status = 'READ' THEN GETDATE() ELSE read_at END
                    WHERE Id = @NotifId AND user_id = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@NotifId", notificationId);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // REACH UP to the Master Page and forcefully update the Notification Bell UI
        private void UpdateMasterBell()
        {
            var master = this.Master as WAPP.Masters.Staff;
            if (master != null)
            {
                master.UpdateUnreadCount();
                var upBell = master.FindControl("upBell") as UpdatePanel;
                if (upBell != null)
                {
                    upBell.Update();
                }
            }
        }

        protected string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value) return "";
            if (DateTime.TryParse(dateObj.ToString(), out DateTime dt))
            {
                if (dt.Date == DateTime.Today) return "Today at " + dt.ToString("h:mm tt");
                else return dt.ToString("MMM dd, yyyy h:mm tt");
            }
            return dateObj.ToString();
        }

        protected string StripHTML(object input)
        {
            if (input == null || input == DBNull.Value) return "";
            string html = input.ToString();
            return Regex.Replace(html, "<.*?>", String.Empty);
        }
    }
}