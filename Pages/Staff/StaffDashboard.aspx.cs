using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace WAPP.Pages.Staff
{
    public partial class StaffDashboard : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Hook into the Master Page's SignalR button!
            Button btnMasterUpdate = Master.FindControl("btnStaffSignalRUpdate") as Button;
            if (btnMasterUpdate != null)
            {
                btnMasterUpdate.Click += new EventHandler(btnSignalRUpdate_Click);
            }

            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string fullName = Session["UserName"].ToString();
                lblStaffName.Text = fullName.Split(' ')[0];

                if (Session["ProfilePic"] != null && !string.IsNullOrWhiteSpace(Session["ProfilePic"].ToString()))
                {
                    imgStaff.ImageUrl = Session["ProfilePic"].ToString();
                }
                else
                {
                    imgStaff.ImageUrl = "~/Images/profile_f.png";
                }

                int staffId = Convert.ToInt32(Session["UserId"]);
                hfMyId.Value = staffId.ToString();

                BindSystemStats();
                LoadPendingTutorCount();
                BindRecentAnnouncements(staffId);
                LoadRecentNotifications(staffId);
                LoadRecentUnreadMessages(staffId);
            }
        }

        protected void btnAddCourse_Click(object sender, EventArgs e) { Response.Redirect("~/Pages/Staff/AddCourse.aspx"); }
        protected void btnAddResource_Click(object sender, EventArgs e) { Response.Redirect("~/Pages/Staff/AddResource.aspx"); }
        protected void btnViewActiveCourses_Click(object sender, EventArgs e) { Response.Redirect("~/Pages/Staff/CourseManagement.aspx?status=PUBLISHED"); }

        protected void btnReviewTutors_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Staff/TutorApplication.aspx?status=PENDING");
        }

        private void LoadPendingTutorCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // UPDATED SQL: Added AND tutor_id IS NOT NULL
                    string sql = "SELECT COUNT(*) FROM [tutorApplication] WHERE status = 'PENDING' AND tutor_id IS NOT NULL";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        lblPendingCount.Text = count.ToString();
                    }
                }
            }
            catch (Exception)
            {
                lblPendingCount.Text = "0";
            }
        }

        private void BindRecentAnnouncements(int staffId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT TOP 3 
                                    a.title, 
                                    a.message, 
                                    a.created_at,
                                    ISNULL(r.name, 'All Users') AS TargetRole
                               FROM [announcement] a
                               LEFT JOIN [role] r ON a.target_role_id = r.Id
                               WHERE a.created_by = @StaffId
                               ORDER BY a.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptRecentAnnouncements.DataSource = dt;
                            rptRecentAnnouncements.DataBind();
                            rptRecentAnnouncements.Visible = true;
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

        private void LoadRecentNotifications(int staffId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT TOP 3 
                                    ISNULL(n.content, 'New System Notification') as content, 
                                    a.message AS announcement_message,
                                    n.created_at 
                               FROM [notification] n
                               LEFT JOIN [announcement] a ON n.announcement_id = a.Id
                               WHERE n.user_id = @UserId 
                               ORDER BY n.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", staffId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptRecentNotifications.DataSource = dt;
                            rptRecentNotifications.DataBind();
                            rptRecentNotifications.Visible = true;
                            lblNoRecentNotifications.Visible = false;
                        }
                        else
                        {
                            rptRecentNotifications.Visible = false;
                            lblNoRecentNotifications.Visible = true;
                        }
                    }
                }
            }
        }

        private void BindSystemStats()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                try { using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [user] WHERE role_id = 4", conn)) { lblTotalStudents.Text = cmd.ExecuteScalar().ToString(); } } catch { lblTotalStudents.Text = "0"; }
                try { using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [user] WHERE role_id = 3", conn)) { lblTotalTutors.Text = cmd.ExecuteScalar().ToString(); } } catch { lblTotalTutors.Text = "0"; }
                try { using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [course] WHERE status = 'PUBLISHED'", conn)) { lblActiveCourses.Text = cmd.ExecuteScalar().ToString(); } } catch { lblActiveCourses.Text = "0"; }
            }
        }

        private void LoadRecentUnreadMessages(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
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
            int staffId = Convert.ToInt32(Session["UserId"]);

            // 1. Update Chat
            LoadRecentUnreadMessages(staffId);
            upRecentMessages.Update();

            // 2. Update Announcements
            BindRecentAnnouncements(staffId);
            upDashboardAnnouncements.Update();

            // 3. Update Notifications
            LoadRecentNotifications(staffId);
            upRecentNotifications.Update();

            // 4. Update the Stats & Pending Tutors (to keep everything live!)
            BindSystemStats();
            LoadPendingTutorCount();
        }

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
    }
}