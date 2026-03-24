using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class Notifications : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure user is logged in AND is a Student (role_id = 4)
            if (Session["UserId"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
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
                // Catch the async postback fired by Student.Master when an announcement notification is received
                string eventTarget = Request.Params["__EVENTTARGET"];
                if (!string.IsNullOrEmpty(eventTarget) && eventTarget.Contains("btnStudentSignalRUpdate"))
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
                // Smart Title Logic matching your new standard
                string query = @"
                    SELECT 
                        n.Id AS notification_id,
                        n.appointment_id,
                        CASE 
                            WHEN n.appointment_id IS NOT NULL THEN 'Appointment'
                            WHEN n.announcement_id IS NOT NULL THEN 'Announcement'
                            ELSE 'System Alert'
                        END AS title, 
                        COALESCE(a.message, n.content) AS message, 
                        n.created_at, 
                        n.status
                    FROM notification n
                    LEFT JOIN announcement a ON n.announcement_id = a.Id
                    WHERE n.user_id = @UserId AND n.status != 'ARCHIVED'
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
                UpdateMasterBell(); // Update Bell Count since deleting an unread item reduces the count
            }
            else if (e.CommandName == "View")
            {
                string[] data = e.CommandArgument.ToString().Split('|');
                if (data.Length == 5)
                {
                    int notificationId = Convert.ToInt32(data[0]);
                    string title = data[1];
                    string message = data[2];
                    string createdAt = data[3];
                    string appointmentIdStr = data[4];

                    // 1. Mark as read in the database
                    UpdateNotificationStatus(notificationId, userId, "READ");

                    // 2. Reload the list so the "New" badge disappears
                    LoadNotifications();
                    UpdateMasterBell(); // Immediately reduce the red bell count on the master page!

                    // 3. Populate Modal
                    lblModalTitle.Text = title;
                    litModalMessage.Text = message;
                    lblModalDate.Text = FormatDate(createdAt);

                    // 4. Trigger Modal via JavaScript
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
                    WHERE user_id = @UserId AND status = 'UNREAD'";

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
            var master = this.Master as WAPP.Masters.Student;
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
                if (dt.Date == DateTime.Today)
                    return "Today at " + dt.ToString("h:mm tt");
                else
                    return dt.ToString("MMM dd, yyyy h:mm tt");
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