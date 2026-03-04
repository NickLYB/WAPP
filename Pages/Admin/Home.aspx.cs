using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WAPP.Pages.Admin
{
    public partial class Home : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Security Check: Ensure user is logged in AND has role_id 1 (Admin)
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 1)
            {
                // Unauthorized or session expired, kick them back to the public homepage
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            else
            {
                // 2. Load Dashboard Data (Only on first page load, not on button clicks)
                if (!IsPostBack)
                {
                    // Directly use the session username since we verified it exists above
                    lblAdminName.Text = Session["UserName"].ToString();

                    BindRecentUsers();
                    BindSystemLogs();
                    BindAnnouncementQueue();
                }
            }
        }

        private void BindRecentUsers()
        {
            // FIX: Changed r.role_name to r.name to match your role table schema
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

        private void BindSystemLogs()
        {
            // FIX: Changed 'message' to 'description' and 'id' to 'created_at' 
            // to match your systemLog table schema
            string sql = "SELECT TOP 3 [description] AS LogText FROM [systemLog] ORDER BY [created_at] DESC";

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

        private void BindAnnouncementQueue()
        {
            // Brackets around [announcement] are safe practice
            // Using TOP 2 to match your dashboard design
            string sql = "SELECT TOP 2 [title] AS [QueueText] FROM [announcement] ORDER BY [Id] DESC";

            try
            {
                DataTable dt = FetchData(sql);
                rptAnnouncementQueue.DataSource = dt;
                rptAnnouncementQueue.DataBind();

                // Show the error label if table is empty
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
    }
}