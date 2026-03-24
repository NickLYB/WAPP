using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Masters;

namespace WAPP.Pages.Student
{
    public partial class Home : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        int currentStudentId;

        protected void Page_Load(object sender, EventArgs e)
        {
            Button btnMasterUpdate = Master.FindControl("btnStudentSignalRUpdate") as Button;
            if (btnMasterUpdate != null)
            {
                btnMasterUpdate.Click += new EventHandler(btnSignalRUpdate_Click);
            }

            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            currentStudentId = Convert.ToInt32(Session["UserId"]);

            if (!IsPostBack)
            {
                lblStudentName.Text = Session["UserName"].ToString();
                hfMyId.Value = currentStudentId.ToString();

                if (Session["ProfilePic"] != null && !string.IsNullOrWhiteSpace(Session["ProfilePic"].ToString()))
                {
                    imgStudent.ImageUrl = Session["ProfilePic"].ToString();
                }
                else
                {
                    imgStudent.ImageUrl = "~/Images/profile_m.png";
                }

                LoadRecentActivity();
                LoadRecentCourses();
                LoadNotifications();
                LoadRecentUnreadMessages(currentStudentId);
            }
        }

        private void LoadRecentActivity()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Combines Quizzes taken AND Lessons completed, sorted by the newest first
                string query = @"
                    SELECT TOP 3 ActivityType, Title, Score, ActivityDate FROM (
                        SELECT 'Quiz' as ActivityType, q.title as Title, qa.score as Score, qa.finished_at as ActivityDate
                        FROM quizAttempt qa 
                        JOIN quiz q ON qa.quiz_id = q.Id 
                        JOIN enrollment e ON qa.enrollment_id = e.Id
                        WHERE e.student_id = @sid AND qa.status = 'GRADED'

                        UNION ALL

                        SELECT 'Lesson' as ActivityType, c.title as Title, NULL as Score, rp.completed_at as ActivityDate
                        FROM resourceProgress rp
                        JOIN learningResource lr ON rp.resource_id = lr.Id
                        JOIN course c ON lr.course_id = c.Id
                        JOIN enrollment e ON rp.enrollment_id = e.Id
                        WHERE e.student_id = @sid AND rp.completed_at IS NOT NULL
                    ) as combined
                    ORDER BY ActivityDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", currentStudentId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptRecentActivity.DataSource = dt;
                    rptRecentActivity.DataBind();
                }
                else
                {
                    phNoActivity.Visible = true;
                }
            }
        }

        private void LoadRecentCourses()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT TOP 1 
                        c.Id as CourseId, 
                        c.title as CourseTitle, 
                        MAX(rp.last_accessed) as LastAccess,
                        
                        (SELECT COUNT(*) FROM learningResource lr WHERE lr.course_id = c.Id) as TotalLessons,
                        (SELECT COUNT(*) FROM resourceProgress rp_check 
                         JOIN learningResource lr_check ON rp_check.resource_id = lr_check.Id 
                         WHERE rp_check.enrollment_id = e.Id AND rp_check.completed_at IS NOT NULL AND lr_check.course_id = c.Id) as CompletedLessons,
                        
                        (SELECT TOP 1 rp2.resource_id 
                         FROM resourceProgress rp2 
                         JOIN learningResource lr2 ON rp2.resource_id = lr2.Id 
                         WHERE lr2.course_id = c.Id AND rp2.enrollment_id = e.Id 
                         ORDER BY rp2.last_accessed DESC) as LastResourceId,
                         
                        (SELECT TOP 1 Id FROM learningResource 
                         WHERE course_id = c.Id 
                         ORDER BY sequence_order ASC, created_at ASC) as FirstResourceId
                         
                    FROM resourceProgress rp
                    JOIN learningResource lr ON rp.resource_id = lr.Id
                    JOIN course c ON lr.course_id = c.Id
                    JOIN enrollment e ON rp.enrollment_id = e.Id
                    WHERE e.student_id = @sid
                    GROUP BY c.Id, c.title, e.Id
                    ORDER BY LastAccess DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", currentStudentId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    dt.Columns.Add("IsCompleted", typeof(bool));
                    dt.Columns.Add("TargetResource", typeof(int));

                    foreach (DataRow row in dt.Rows)
                    {
                        int total = Convert.ToInt32(row["TotalLessons"]);
                        int completed = Convert.ToInt32(row["CompletedLessons"]);

                        bool isFinished = (total > 0 && total == completed);
                        row["IsCompleted"] = isFinished;

                        row["TargetResource"] = isFinished ? row["FirstResourceId"] : row["LastResourceId"];
                    }

                    rptRecentCourses.DataSource = dt;
                    rptRecentCourses.DataBind();
                }
                else
                {
                    phNoCourses.Visible = true;
                }
            }
        }

        private void LoadNotifications()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Updated Query to use the CASE statement for smart titles
                string query = @"
                    SELECT TOP 5 
                        CASE 
                            WHEN n.appointment_id IS NOT NULL THEN 'Appointment'
                            WHEN n.announcement_id IS NOT NULL THEN 'Announcement'
                            ELSE 'System Alert'
                        END AS title, 
                        COALESCE(a.message, n.content) AS message, 
                        n.status, 
                        n.created_at 
                    FROM notification n 
                    LEFT JOIN announcement a ON n.announcement_id = a.Id 
                    WHERE n.user_id = @sid AND n.status = 'UNREAD'
                    ORDER BY n.created_at DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", currentStudentId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptNotifications.DataSource = dt;
                    rptNotifications.DataBind();

                    lblNoNotifications.Visible = false;
                }
                else
                {
                    rptNotifications.DataSource = null;
                    rptNotifications.DataBind();

                    lblNoNotifications.Visible = true;
                }

                // Get UNREAD count for the badge
                string countQuery = "SELECT COUNT(*) FROM notification WHERE user_id = @sid AND status = 'UNREAD'";
                SqlCommand countCmd = new SqlCommand(countQuery, conn);
                countCmd.Parameters.AddWithValue("@sid", currentStudentId);
                conn.Open();
                int unreadCount = (int)countCmd.ExecuteScalar();

                if (unreadCount > 0)
                {
                    lblNotificationCount.Text = unreadCount > 99 ? "99+" : unreadCount.ToString();
                    lblNotificationCount.Visible = true;
                }
                else
                {
                    lblNotificationCount.Visible = false;
                }
            }
        }

        protected string GetActivityIcon(object type)
        {
            if (type.ToString() == "Quiz") return "bi bi-trophy-fill";
            return "bi bi-check-circle-fill"; // Lesson
        }

        protected string GetActivityIconClass(object type, object scoreObj)
        {
            if (type.ToString() == "Quiz")
            {
                int score = scoreObj != DBNull.Value ? Convert.ToInt32(scoreObj) : 0;
                return score >= 50 ?
                    "bg-primary bg-opacity-10 text-primary rounded-circle d-flex justify-content-center align-items-center me-3" :
                    "bg-danger bg-opacity-10 text-danger rounded-circle d-flex justify-content-center align-items-center me-3";
            }
            // For completed lessons
            return "bg-success bg-opacity-10 text-success rounded-circle d-flex justify-content-center align-items-center me-3";
        }

        protected string GetActivityTextClass(object type, object scoreObj)
        {
            if (type.ToString() == "Quiz")
            {
                int score = scoreObj != DBNull.Value ? Convert.ToInt32(scoreObj) : 0;
                return score >= 50 ? "text-primary" : "text-danger";
            }
            return "text-success";
        }

        protected string GetActivityDescription(object type, object scoreObj)
        {
            if (type.ToString() == "Quiz")
            {
                int score = scoreObj != DBNull.Value ? Convert.ToInt32(scoreObj) : 0;
                return score >= 50 ? $"Score: {score}% • Passed Assessment" : $"Score: {score}% • Needs Review";
            }
            return "Completed a Lesson";
        }

        // Helper Method: Formats the date nicely
        protected string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value) return "";
            if (DateTime.TryParse(dateObj.ToString(), out DateTime dt))
            {
                if (dt.Date == DateTime.Today)
                    return "Today at " + dt.ToString("h:mm tt");
                else
                    return dt.ToString("MMM dd, yyyy");
            }
            return dateObj.ToString();
        }

        // Helper Method: Truncates long messages so they don't break the UI card layout
        protected string TruncateMessage(object messageObj, int maxLength)
        {
            if (messageObj == null || messageObj == DBNull.Value) return "";
            string msg = messageObj.ToString();

            // Strip HTML tags just in case
            msg = System.Text.RegularExpressions.Regex.Replace(msg, "<.*?>", string.Empty);

            if (msg.Length > maxLength)
            {
                return msg.Substring(0, maxLength) + "...";
            }
            return msg;
        }

        private void LoadRecentUnreadMessages(int userId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(cs))
            {
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
            int studentId = Convert.ToInt32(Session["UserId"]);

            // 1. Update Chat
            LoadRecentUnreadMessages(studentId);
            upRecentMessages.Update();

            // 2. Update Dashboard Notifications
            LoadNotifications();
            upNotifications.Update();
        }
    }
}