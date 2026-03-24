using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using Hangfire;
using Microsoft.AspNet.SignalR;
using WAPP.Hubs;
using WAPP.Utils;

namespace WAPP.Pages.Admin
{
    public partial class ComposeAnnouncement : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 1)
            {
                Response.Redirect("~/Pages/Guest/Login.aspx");
                return;
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
                    lblError.Visible = true;
                    lblError.Text = "Please select a valid date and time to schedule.";
                    return;
                }

                DateTime scheduledDate;
                if (DateTime.TryParse(txtScheduleDate.Text, out scheduledDate))
                {
                    if (scheduledDate <= DateTime.Now)
                    {
                        lblError.Visible = true;
                        lblError.Text = "Scheduled time must be in the future.";
                        return;
                    }
                    SaveAnnouncement("ACTIVE", scheduledDate);
                }
                else
                {
                    lblError.Visible = true;
                    lblError.Text = "Invalid date format.";
                }
            }
        }

        private void SaveAnnouncement(string status, DateTime scheduledDate)
        {
            bool isSuccess = false;

            // Extract Admin ID
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : (Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 1);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Logic: If Admin selects "0" (All Roles), insert 3 separate records for Staff, Tutor, and Student
                    int[] targetRoles;
                    if (rblTarget.SelectedValue == "0")
                    {
                        targetRoles = new int[] { 2, 3, 4 };
                    }
                    else
                    {
                        targetRoles = new int[] { Convert.ToInt32(rblTarget.SelectedValue) };
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
                            cmd.Parameters.AddWithValue("@created_by", adminId);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    // --- LOGGING ---
                    string logMsg = scheduledDate <= DateTime.Now
                        ? $"Admin created and published a new announcement: '{txtTitle.Text.Trim()}'"
                        : $"Admin scheduled a new announcement: '{txtTitle.Text.Trim()}' for {scheduledDate}";

                    SystemLogService.Write("ADMIN_ANNOUNCEMENT_CREATED", logMsg, LogLevel.INFO, adminId);

                    isSuccess = true;
                }

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
                SystemLogService.Write("ADMIN_ANNOUNCEMENT_ERROR", $"DB Error saving announcement: {ex.Message}", LogLevel.ERROR, adminId);

                lblError.Visible = true;
                lblError.Text = "Error saving announcement: " + ex.Message;
            }

            if (isSuccess)
            {
                Response.Redirect("ManageAnnouncements.aspx?msg=success", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}