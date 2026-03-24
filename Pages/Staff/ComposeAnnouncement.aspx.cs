using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using WAPP.Utils;
using Hangfire;
using Microsoft.AspNet.SignalR;
using WAPP.Hubs;

namespace WAPP.Pages.Staff
{
    public partial class ComposeAnnouncement : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 2) // Verify Staff Role
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
        }

        protected void btnSendNow_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                SaveAnnouncement("ACTIVE", DateTime.Now);
            }
        }

        protected void btnConfirmSchedule_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if (string.IsNullOrWhiteSpace(txtScheduleDate.Text))
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Please select a valid date and time to schedule.";
                    lblMessage.CssClass = "alert alert-danger d-block fw-bold";
                    return;
                }

                DateTime scheduledDate;
                if (DateTime.TryParse(txtScheduleDate.Text, out scheduledDate))
                {
                    if (scheduledDate <= DateTime.Now)
                    {
                        lblMessage.Visible = true;
                        lblMessage.Text = "Scheduled time must be in the future.";
                        lblMessage.CssClass = "alert alert-danger d-block fw-bold";
                        return;
                    }
                    SaveAnnouncement("ACTIVE", scheduledDate);
                }
                else
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Invalid date format.";
                    lblMessage.CssClass = "alert alert-danger d-block fw-bold";
                }
            }
        }

        private void SaveAnnouncement(string status, DateTime scheduledDate)
        {
            bool isSuccess = false;

            // Extract staff ID once at the top so it's easily available for both DB and Logging
            int staffId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Decide exactly who gets it. 
                    int[] targetRoles;
                    if (rblTarget.SelectedValue == "0") // "All" selected
                    {
                        targetRoles = new int[] { 3, 4 }; // Send separate records to Tutors(3) and Students(4)
                    }
                    else
                    {
                        targetRoles = new int[] { Convert.ToInt32(rblTarget.SelectedValue) }; // Send to specific role
                    }

                    // Loop through the target roles and insert an announcement for each
                    foreach (int roleId in targetRoles)
                    {
                        string sql = @"INSERT INTO [announcement] 
                                       (target_role_id, title, message, created_by, status, scheduled_at) 
                                       VALUES (@role, @title, @message, @created_by, @status, @scheduled_at)";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@message", txtMessage.Text.Trim());
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@scheduled_at", scheduledDate);
                            cmd.Parameters.AddWithValue("@role", roleId);
                            cmd.Parameters.AddWithValue("@created_by", staffId);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    // --- LOGGING ADDED: INFO ---
                    string logMsg = scheduledDate <= DateTime.Now
                        ? $"Staff created and published a new announcement: '{txtTitle.Text.Trim()}'"
                        : $"Staff scheduled a new announcement: '{txtTitle.Text.Trim()}' for {scheduledDate}";

                    SystemLogService.Write("STAFF_ANNOUNCEMENT_CREATED", logMsg, LogLevel.INFO, staffId);

                    isSuccess = true;
                } // Connection closes here

                // =========================================================
                // INSTANT SEND LOGIC 
                // =========================================================
                // If "Send Now" was clicked (or scheduled time is now/past), 
                // queue the job immediately rather than waiting for the schedule tick.
                if (scheduledDate <= DateTime.Now)
                {
                    BackgroundJob.Enqueue(() => new WAPP.Startup().ProcessAnnouncementsJob());

                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    hubContext.Clients.All.receiveNotification(0); // 0 = Global Announcement
                }
            }
            catch (Exception ex)
            {
                // --- LOGGING ADDED: ERROR ---
                SystemLogService.Write("STAFF_ANNOUNCEMENT_ERROR", $"DB Error saving announcement: {ex.Message}", LogLevel.ERROR, staffId);

                lblMessage.Visible = true;
                lblMessage.Text = "Error saving announcement: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block fw-bold";
            }

            if (isSuccess)
            {
                Response.Redirect("AnnouncementManagement.aspx?msg=success", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}