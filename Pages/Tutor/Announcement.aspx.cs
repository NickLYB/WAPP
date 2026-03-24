using Hangfire;            
using Microsoft.AspNet.SignalR; 
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Hubs;           
using WAPP.Utils;           

namespace WAPP.Pages.Tutor
{
    public partial class Announcement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            if (!IsPostBack)
            {
                lblMsg.Visible = false;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SiteMap.SiteMapResolve += SiteMap_Resolve;
        }
        protected override void OnUnload(EventArgs e)
        {
            SiteMap.SiteMapResolve -= SiteMap_Resolve;
            base.OnUnload(e);
        }
        private SiteMapNode SiteMap_Resolve(object sender, SiteMapResolveEventArgs e)
        {
            var ctx = e.Context;
            if (ctx?.Request == null) return SiteMap.CurrentNode;

            // only apply to Announcement page
            string path = ctx.Request.Path;
            if (!path.EndsWith("/Announcement.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/Announcement", StringComparison.OrdinalIgnoreCase))
                return SiteMap.CurrentNode;

            SiteMapNode current = SiteMap.CurrentNode;
            if (current == null) return null;

            // Clone(true) clones the current node AND its ancestors (parents)
            SiteMapNode clone = current.Clone(true);

            // get course id
            if (!int.TryParse(ctx.Request.QueryString["id"], out int courseId))
                return clone;

            string courseTitle = GetCourseName(courseId);
            if (string.IsNullOrWhiteSpace(courseTitle))
                return clone;

            clone.Url += $"?id={courseId}";

            // Walk up: Announcement -> Edit -> Courses
            if (clone.ParentNode != null)
            {
                // Update the title
                clone.ParentNode.Title = $"Edit - {courseTitle}";

                // ensures the breadcrumb link actually sends you back to the right course.
                clone.ParentNode.Url += $"?id={courseId}";
            }

            return clone;
        }
        
        private string GetCourseName(int courseId)
                {
                    string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

                    using (SqlConnection con = new SqlConnection(cs))
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 title FROM course WHERE Id = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", courseId);
                        con.Open();
                        return cmd.ExecuteScalar()?.ToString();
                    }
                }

        protected void Button1_Click(object sender, EventArgs e)
        {
            lblMsg.ForeColor = System.Drawing.Color.Red;

            // 1. Validate Form Inputs
            if (string.IsNullOrWhiteSpace(title.Text))
            {
                lblMsg.Text = "Please enter an announcement title.";
                lblMsg.Visible = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(description.Text))
            {
                lblMsg.Text = "Please enter the announcement message.";
                lblMsg.Visible = true;
                return;
            }

            // 2. Ensure Tutor is logged in
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            // GETDATE() will immediately make it active/published time
            SaveAnnouncement("ACTIVE", DateTime.Now);
        }

        protected void btnConfirmSchedule_Click(object sender, EventArgs e)
        {
            lblMsg.ForeColor = System.Drawing.Color.Red;

            // 1. Validate Form Inputs
            if (string.IsNullOrWhiteSpace(title.Text))
            {
                lblMsg.Text = "Please enter an announcement title before scheduling.";
                lblMsg.Visible = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(description.Text))
            {
                lblMsg.Text = "Please enter the announcement message before scheduling.";
                lblMsg.Visible = true;
                return;
            }

            // 2. Validate Scheduled Date
            if (string.IsNullOrWhiteSpace(txtScheduleDate.Text) || !DateTime.TryParse(txtScheduleDate.Text, out DateTime scheduledDate))
            {
                lblMsg.Text = "Please select a valid date and time.";
                lblMsg.Visible = true;
                return;
            }

            if (scheduledDate <= DateTime.Now)
            {
                lblMsg.Text = "Scheduled time must be in the future.";
                lblMsg.Visible = true;
                return;
            }

            // 3. Ensure Tutor is logged in
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            // Pass the user selected future date
            SaveAnnouncement("ACTIVE", scheduledDate);
        }

        private void SaveAnnouncement(string status, DateTime scheduledDate)
        {
            int createdBy = Convert.ToInt32(Session["UserId"]);
            int targetRoleId = 4; // Target is Students
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"
                        INSERT INTO announcement (target_role_id, course_id, title, message, created_by, status, scheduled_at) 
                        VALUES (@targetRoleId, @courseId, @title, @message, @createdBy, @status, @scheduledAt)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@targetRoleId", targetRoleId);
                        cmd.Parameters.AddWithValue("@title", title.Text.Trim());
                        cmd.Parameters.AddWithValue("@message", description.Text.Trim());
                        cmd.Parameters.AddWithValue("@createdBy", createdBy);
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@scheduledAt", scheduledDate);

                        // Check if we are inside a specific course or making a general announcement
                        string courseIdStr = Request.QueryString["id"];
                        if (!string.IsNullOrEmpty(courseIdStr) && int.TryParse(courseIdStr, out int courseId))
                        {
                            cmd.Parameters.AddWithValue("@courseId", courseId);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@courseId", DBNull.Value); // Saves as NULL in database
                        }

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    // --- LOGGING ---
                    string logMsg = scheduledDate <= DateTime.Now
                        ? $"Tutor created and published a new announcement: '{title.Text.Trim()}'"
                        : $"Tutor scheduled a new announcement: '{title.Text.Trim()}' for {scheduledDate}";

                    SystemLogService.Write("TUTOR_ANNOUNCEMENT_CREATED", logMsg, LogLevel.INFO, createdBy);
                }

                if (scheduledDate <= DateTime.Now)
                {
                    BackgroundJob.Enqueue(() => new WAPP.Startup().ProcessAnnouncementsJob());

                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    hubContext.Clients.All.receiveNotification(0); // 0 = Global/Role Announcement Broadcast
                }

                lblMsg.ForeColor = System.Drawing.Color.Green;
                lblMsg.Text = scheduledDate <= DateTime.Now
                    ? "Announcement sent successfully!"
                    : $"Announcement successfully scheduled for {scheduledDate.ToString("MMM dd, yyyy h:mm tt")}!";
                lblMsg.Visible = true;

                // Clear the fields
                title.Text = "";
                description.Text = "";
                txtScheduleDate.Text = "";
            }
            catch (Exception ex)
            {
                SystemLogService.Write("TUTOR_ANNOUNCEMENT_ERROR", $"DB Error saving announcement: {ex.Message}", LogLevel.ERROR, createdBy);

                lblMsg.ForeColor = System.Drawing.Color.Red;
                lblMsg.Text = "An error occurred: " + ex.Message;
                lblMsg.Visible = true;
            }
        }
    }
}