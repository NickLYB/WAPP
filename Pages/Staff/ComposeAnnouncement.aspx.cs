using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace WAPP.Pages.Staff
{
    public partial class ComposeAnnouncement : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 2) // Verify Staff Role
            {
                Response.Redirect("~/Pages/Guest/Login.aspx");
            }
        }

        // Post Immediately
        protected void btnSendNow_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                SaveAnnouncement("ACTIVE", null);
            }
        }

        // Post on a Schedule
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
                        lblMessage.Text = "Schedule date must be in the future.";
                        lblMessage.CssClass = "alert alert-danger d-block fw-bold";
                        return;
                    }
                    SaveAnnouncement("SCHEDULED", scheduledDate);
                }
                else
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Invalid date format.";
                    lblMessage.CssClass = "alert alert-danger d-block fw-bold";
                }
            }
        }

        private void SaveAnnouncement(string status, DateTime? scheduledDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"INSERT INTO [announcement] 
                                   (title, message, target_role_id, status, scheduled_at, created_by) 
                                   VALUES (@title, @message, @role, @status, @scheduled_at, @created_by)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@message", txtMessage.Text.Trim());
                        cmd.Parameters.AddWithValue("@status", status);

                        // 🔴 THE REAL FIX: 
                        // Your LoginModal.ascx saves the ID as Session["UserId"].
                        // We must grab exactly that session name and convert it to an Integer for SQL.
                        if (Session["UserId"] != null)
                        {
                            cmd.Parameters.AddWithValue("@created_by", Convert.ToInt32(Session["UserId"]));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@created_by", DBNull.Value);
                        }

                        // Handle Role Filter
                        if (rblTarget.SelectedValue == "0")
                            cmd.Parameters.AddWithValue("@role", DBNull.Value); // Broadcast to All
                        else
                            cmd.Parameters.AddWithValue("@role", rblTarget.SelectedValue);

                        // Handle Schedule Date
                        if (scheduledDate.HasValue)
                            cmd.Parameters.AddWithValue("@scheduled_at", scheduledDate.Value);
                        else
                            cmd.Parameters.AddWithValue("@scheduled_at", DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Redirect back to management page on success
                Response.Redirect("AnnouncementManagement.aspx?msg=success");
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error saving announcement: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block fw-bold";
            }
        }
    }
}