using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Admin
{
    public partial class Home : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Button btnMasterUpdate = Master.FindControl("btnAdminSignalRUpdate") as Button;
            if (btnMasterUpdate != null)
            {
                btnMasterUpdate.Click += new EventHandler(btnSignalRUpdate_Click);
            }

            // Ensure user is logged in AND has role_id 1 (Admin), otherwise bounce to Home
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 1)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            else
            {
                if (!IsPostBack)
                {
                    BindStats();
                    lblAdminName.Text = Session["UserName"].ToString();
                    hfMyId.Value = Session["UserId"].ToString();

                    if (Session["ProfilePic"] != null && !string.IsNullOrWhiteSpace(Session["ProfilePic"].ToString()))
                    {
                        imgAdmin.ImageUrl = Session["ProfilePic"].ToString();
                    }
                    else
                    {
                        imgAdmin.ImageUrl = "~/Images/profile_m.png";
                    }

                    BindRecentUsers();
                    BindSystemLogs();
                    BindAnnouncementQueue();
                    int adminId = Convert.ToInt32(Session["UserId"]);
                    LoadRecentUnreadMessages(adminId);
                }
            }
        }

        private void BindRecentUsers()
        {
            string sql = @"SELECT TOP 5 
                            u.Id AS [ID], 
                            (u.fname + ' ' + u.lname) AS [Name], 
                            r.name AS [Role], 
                            u.email AS [Email] 
                          FROM [user] u
                          INNER JOIN [role] r ON u.role_id = r.id
                          ORDER BY u.Id DESC";
            try
            {
                DataTable dt = FetchData(sql);
                gvRecentUsers.DataSource = dt;
                gvRecentUsers.DataBind();
                lblUserEmpty.Visible = (dt.Rows.Count == 0);
            }
            catch (Exception ex)
            {
                lblUserEmpty.Visible = true;
                lblUserEmpty.Text = "User Data Error: " + ex.Message;
            }
        }

        private void BindAnnouncementQueue()
        {
            string sql = "SELECT TOP 2 [title] AS [QueueText] FROM [announcement] ORDER BY [Id] DESC";

            try
            {
                DataTable dt = FetchData(sql);
                rptAnnouncementQueue.DataSource = dt;
                rptAnnouncementQueue.DataBind();

                if (dt.Rows.Count == 0)
                {
                    lblQueueEmpty.Visible = true;
                    lblQueueEmpty.Text = "No announcements in queue.";
                }
                else
                {
                    lblQueueEmpty.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblQueueEmpty.Visible = true;
                lblQueueEmpty.Text = "Error: " + ex.Message;
            }
        }

        private DataTable FetchData(string sql)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                con.Open();
                da.Fill(dt);
                return dt;
            }
        }

        private void LoadRecentUnreadMessages(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
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
            int adminId = Convert.ToInt32(Session["UserId"]);

            BindStats();
            upStats.Update();

            LoadRecentUnreadMessages(adminId);
            upRecentMessages.Update();
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

        protected string FormatTimeAgo(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value) return "";
            DateTime dt = Convert.ToDateTime(dateObj);
            TimeSpan ts = DateTime.Now - dt;

            if (ts.TotalMinutes < 1) return "Just now";
            if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m ago";
            if (ts.TotalHours < 24) return $"{(int)ts.TotalHours}h ago";
            return dt.ToString("MMM dd");
        }

        private void BindStats()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string sqlActive = @"
                    SELECT COUNT(*) FROM (
                        SELECT user_id, 
                               MAX(CASE WHEN action_type = 'AUTH_LOGIN_SUCCESS' THEN created_at ELSE NULL END) as LastLogin,
                               MAX(CASE WHEN action_type = 'AUTH_LOGOUT' THEN created_at ELSE NULL END) as LastLogout
                        FROM systemLog
                        WHERE action_type IN ('AUTH_LOGIN_SUCCESS', 'AUTH_LOGOUT') 
                          AND created_at >= DATEADD(minute, -60, SYSDATETIME())
                        GROUP BY user_id
                    ) AS UserStates
                    WHERE LastLogin IS NOT NULL 
                      AND (LastLogout IS NULL OR LastLogin > LastLogout)";

                using (SqlCommand cmd = new SqlCommand(sqlActive, conn))
                {
                    lblActiveSessions.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                }

                string sqlRecent = @"
                    SELECT TOP 5 
                        sl.user_id, 
                        (u.fname + ' ' + ISNULL(u.lname, '')) AS UserName,
                        r.name AS RoleName,
                        sl.created_at AS LoginTime
                    FROM systemLog sl
                    INNER JOIN [user] u ON sl.user_id = u.Id
                    INNER JOIN [role] r ON u.role_id = r.Id
                    WHERE sl.action_type = 'AUTH_LOGIN_SUCCESS'
                    ORDER BY sl.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sqlRecent, conn))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptRecentLogins.DataSource = dt;
                        rptRecentLogins.DataBind();

                        lblNoRecentLogins.Visible = (dt.Rows.Count == 0);
                    }
                }
            }
        }

        private void BindSystemLogs()
        {
            string sql = @"
                SELECT TOP 3 [description] AS LogText 
                FROM [systemLog] 
                WHERE action_type NOT LIKE 'AUTH_%' 
                ORDER BY [created_at] DESC";

            try
            {
                DataTable dt = FetchData(sql);
                rptSystemLogs.DataSource = dt;
                rptSystemLogs.DataBind();
                lblLogsEmpty.Visible = (dt.Rows.Count == 0);
            }
            catch (Exception ex)
            {
                lblLogsEmpty.Visible = true;
                lblLogsEmpty.Text = "Log Error: " + ex.Message;
            }
        }
    }
}