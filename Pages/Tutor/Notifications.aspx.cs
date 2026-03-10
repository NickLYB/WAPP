using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class Notifications : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadNotifications();
            }
        }

        private void LoadNotifications()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Updated query to use a CASE statement for the title
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
            }
            else if (e.CommandName == "View")
            {
                // The CommandArgument contains multiple pieces of data separated by the pipe symbol '|'
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

                    // 2. Reload the list so the "New" badge disappears immediately
                    LoadNotifications();

                    // 3. Populate Modal
                    lblModalTitle.Text = title;
                    litModalMessage.Text = message; // Using Literal to render TinyMCE HTML correctly
                    lblModalDate.Text = FormatDate(createdAt);

                    // 4. Toggle the "View Appointment" button visibility
                    if (!string.IsNullOrEmpty(appointmentIdStr))
                    {
                        hlViewAppointment.Visible = true;
                    }
                    else
                    {
                        hlViewAppointment.Visible = false;
                    }

                    // 5. Trigger Modal via JavaScript
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

        // Helper Method: Strips HTML tags for the preview text in the list
        protected string StripHTML(object input)
        {
            if (input == null || input == DBNull.Value) return "";
            string html = input.ToString();
            return Regex.Replace(html, "<.*?>", String.Empty);
        }
    }
}