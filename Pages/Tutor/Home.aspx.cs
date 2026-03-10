using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Pages.Student;

namespace WAPP.Pages.Tutor
{
    public partial class Home : System.Web.UI.Page
    {
         string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            else
            {
                lblTutorName.Text = Session["UserName"].ToString();
                int tutorId = Convert.ToInt32(Session["UserId"]);

                hfMyId.Value = tutorId.ToString();

                CheckTutorStatus(tutorId);
                LoadNotifications(tutorId);
                LoadTeachingOverview();
                LoadRecentAnnouncements();
                LoadRecentUnreadMessages(tutorId);

                // NEW: Check if they were redirected here from the Teaching dashboard
                if (!IsPostBack && Request.QueryString["err"] == "unverified")
                {
                    string script = "alert('Access Denied: You must be a fully verified tutor to access the teaching dashboard. Please wait for your application to be approved.');";
                    ClientScript.RegisterStartupScript(this.GetType(), "AccessDeniedAlert", script, true);
                }
            }
        }
        private void CheckTutorStatus(int tutorId)
        {
            string status = "UNKNOWN";

            using (SqlConnection conn = new SqlConnection(cs))
            {
                string sql = "SELECT TOP 1 status FROM tutorApplication WHERE tutor_id = @tutorId ORDER BY submitted_at DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tutorId", tutorId);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        status = result.ToString();
                    }
                }
            }

            pnlApplicationStatus.Visible = true;
            Session["TutorAppStatus"] = status;

            // Base styling for the sleek pill look
            string baseStyle = "display: inline-flex; align-items: center; gap: 6px; padding: 4px 12px; border-radius: 50px; font-size: 0.85rem; width: fit-content; ";

            switch (status.ToUpper())
            {
                case "APPROVED":
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #d1e7dd; color: #0f5132; border: 1px solid #badbcc;";
                    iconStatus.Attributes["class"] = "bi bi-check-circle-fill";
                    lblStatusText.Text = "Verified";
                    // Leave the button alone, it works normally
                    break;

                case "PENDING":
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #fff3cd; color: #664d03; border: 1px solid #ffecb5;";
                    iconStatus.Attributes["class"] = "bi bi-hourglass-split";
                    lblStatusText.Text = "Pending";
                    DisableCourseButton();
                    break;

                case "REJECTED":
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #f8d7da; color: #842029; border: 1px solid #f5c2c7;";
                    iconStatus.Attributes["class"] = "bi bi-x-octagon-fill";
                    lblStatusText.Text = "Rejected";
                    DisableCourseButton();
                    break;

                default:
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #e2e3e5; color: #41464b; border: 1px solid #d3d6d8;";
                    iconStatus.Attributes["class"] = "bi bi-question-circle-fill";
                    lblStatusText.Text = "Action Required";
                    DisableCourseButton();
                    break;
            }
        }
        private void LoadNotifications(int userId)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Updated query to use a CASE statement for the title
                string query = @"
                SELECT 
                    CASE 
                        WHEN n.appointment_id IS NOT NULL THEN 'Appointment'
                        WHEN n.announcement_id IS NOT NULL THEN 'Announcement'
                        ELSE 'System Alert'
                    END AS title, 
                    COALESCE(a.message, n.content) AS message, 
                    n.created_at, 
                    n.Id AS notification_id
                FROM notification n
                LEFT JOIN announcement a ON n.announcement_id = a.Id
                WHERE n.user_id = @UserId AND n.status = 'UNREAD'
                ORDER BY n.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    conn.Open();
                    da.Fill(dt);
                    conn.Close();

                    // 1. Update Notification Bubble Count
                    int unreadCount = dt.Rows.Count;
                    if (unreadCount > 0)
                    {
                        lblNotificationCount.Text = unreadCount > 99 ? "99+" : unreadCount.ToString();
                        lblNotificationCount.Visible = true;
                    }
                    else
                    {
                        lblNotificationCount.Visible = false; // Hide bubble if 0
                    }

                    // 2. Bind the Repeater
                    if (dt.Rows.Count > 0)
                    {
                        // Bind only top 5 if you don't want the card to get too long
                        DataTable top5 = dt.Clone();
                        for (int i = 0; i < Math.Min(5, dt.Rows.Count); i++)
                        {
                            top5.ImportRow(dt.Rows[i]);
                        }

                        rptNotifications.DataSource = top5;
                        rptNotifications.DataBind();
                        lblNoNotifications.Visible = false;
                    }
                    else
                    {
                        rptNotifications.DataSource = null;
                        rptNotifications.DataBind();
                        lblNoNotifications.Visible = true;
                    }
                }
            }
        }
        private void DisableCourseButton()
        {
            lnkCreateCourse.Enabled = false;
            lnkCreateCourse.CssClass = "btn-main w-100 rounded-pill disabled";

            // Force it to look gray and stop pointer clicks via CSS
            lnkCreateCourse.Attributes.Add("style", "background-color: #6c757d !important; border-color: #6c757d !important; color: white !important; opacity: 0.6; pointer-events: none;");

            lnkViewAllAnnouncements.Enabled = false;
            lnkViewAllAnnouncements.CssClass = "btn-main w-100 rounded-pill disabled";

            // Force it to look gray and stop pointer clicks via CSS
            lnkViewAllAnnouncements.Attributes.Add("style", "background-color: #6c757d !important; border-color: #6c757d !important; color: white !important; opacity: 0.6; pointer-events: none;");
        }
        private void LoadTeachingOverview()
        {
            if (Session["UserId"] == null) return;
            int tutorId = Convert.ToInt32(Session["UserId"]);

            // 1. Get Course Counts (Published & Pending)
            using (SqlConnection con = new SqlConnection(cs))
            {
                string courseQuery = @"
            SELECT 
                SUM(CASE WHEN status = 'PUBLISHED' THEN 1 ELSE 0 END) AS PublishedCount,
                SUM(CASE WHEN status = 'PENDING' THEN 1 ELSE 0 END) AS PendingCount
            FROM course 
            WHERE tutor_id = @TutorId";

                using (SqlCommand cmd = new SqlCommand(courseQuery, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblPublishedCourses.Text = reader["PublishedCount"] != DBNull.Value ? reader["PublishedCount"].ToString() : "0";
                            lblPendingCourses.Text = reader["PendingCount"] != DBNull.Value ? reader["PendingCount"].ToString() : "0";
                        }
                    }
                }
            } // Connection for query 1 is closed and returned to the pool here

            // 2. Get Total Learning Resources Count
            using (SqlConnection con = new SqlConnection(cs))
            {
                string resourceQuery = "SELECT COUNT(Id) FROM learningResource WHERE tutor_id = @TutorId";
                using (SqlCommand cmd = new SqlCommand(resourceQuery, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    lblTotalResources.Text = result != DBNull.Value ? result.ToString() : "0";
                }
            } // Connection for query 2 is closed and returned to the pool here

            // 3. Get Pending Appointments Count
            using (SqlConnection con = new SqlConnection(cs))
            {
                string apptQuery = "SELECT COUNT(Id) FROM appointment WHERE tutor_id = @TutorId AND status = 'PENDING'";
                using (SqlCommand cmd = new SqlCommand(apptQuery, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    lblPendingAppts.Text = result != DBNull.Value ? result.ToString() : "0";
                }
            } // Connection for query 3 is closed and returned to the pool here
        }

        private void LoadRecentAnnouncements()
        {
            if (Session["UserId"] == null) return;
            int tutorId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT TOP 2 title, message, created_at 
                    FROM announcement 
                    WHERE created_by = @TutorId AND status = 'ACTIVE' 
                    ORDER BY created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptRecentAnnouncements.DataSource = dt;
                            rptRecentAnnouncements.DataBind();
                            lblNoAnnouncements.Visible = false;
                        }
                        else
                        {
                            rptRecentAnnouncements.Visible = false;
                            lblNoAnnouncements.Visible = true;
                        }
                    }
                }
            }
        }

        // Helper Method: Formats the date to look like "Feb 14, 2026"
        protected string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value) return "";
            if (DateTime.TryParse(dateObj.ToString(), out DateTime dt))
            {
                return dt.ToString("MMM dd, yyyy");
            }
            return dateObj.ToString();
        }

        // Helper Method: Truncates long messages so they don't break the UI card layout
        protected string TruncateMessage(object messageObj, int maxLength)
        {
            if (messageObj == null || messageObj == DBNull.Value) return "";
            string msg = messageObj.ToString();

            if (msg.Length > maxLength)
            {
                return msg.Substring(0, maxLength) + "...";
            }
            return msg;
        }

        private void LoadRecentUnreadMessages(int userId)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // This query gets the top 4 students who have sent unread messages.
                // It counts how many unread messages they sent and grabs the text of their latest one.
                string query = @"
            SELECT TOP 4
                u.Id AS SenderId,
                (u.fname + ' ' + u.lname) AS SenderName,
                COUNT(m.Id) AS UnreadCount,
                MAX(m.created_at) AS LastMessageTime,
                (
                    SELECT TOP 1 message_text 
                    FROM chatMessage 
                    WHERE sender_id = u.Id AND receiver_id = @UserId AND is_read = 0 
                    ORDER BY created_at DESC
                ) AS LastMessage
            FROM chatMessage m
            INNER JOIN [user] u ON m.sender_id = u.Id
            WHERE m.receiver_id = @UserId AND m.is_read = 0
            GROUP BY u.Id, u.fname, u.lname
            ORDER BY LastMessageTime DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    conn.Open();
                    da.Fill(dt);
                    conn.Close();

                    if (dt.Rows.Count > 0)
                    {
                        rptUnreadMessages.DataSource = dt;
                        rptUnreadMessages.DataBind();
                        rptUnreadMessages.Visible = true;
                        lblNoUnreadMessages.Visible = false;
                    }
                    else
                    {
                        rptUnreadMessages.Visible = false;
                        lblNoUnreadMessages.Visible = true;
                    }
                }
            }
        }
        protected void btnSignalRUpdate_Click(object sender, EventArgs e)
        {
            int tutorId = Convert.ToInt32(Session["UserId"]);

            // Refresh the data
            LoadNotifications(tutorId);
            LoadRecentUnreadMessages(tutorId);

            // Tell the UpdatePanels to push the new HTML to the browser
            upNotifications.Update();
            upRecentMessages.Update();
        }
    }
}