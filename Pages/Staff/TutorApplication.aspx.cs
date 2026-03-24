using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Utils; // Accesses SystemLogService and LogLevel
using Microsoft.AspNet.SignalR;
using WAPP.Hubs;

namespace WAPP.Pages.Staff
{
    public partial class TutorApplication : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                string passedStatus = Request.QueryString["status"];
                if (!string.IsNullOrEmpty(passedStatus) && passedStatus.ToUpper() == "PENDING")
                {
                    ddlFilterStatus.SelectedValue = "PENDING";
                }
                else
                {
                    ddlFilterStatus.SelectedValue = "All";
                }
                BindGrid();
            }
        }

        // NEW EVENT HANDLER: Clear Filters logic
        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSortBy.SelectedValue = "DESC";
            ddlFilterStatus.SelectedValue = "All";
            lblMessage.Visible = false;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ClearSearch", "document.getElementById('" + txtSearch.ClientID + "').value = ''; SearchTable();", true);

            gvApplications.PageIndex = 0;
            BindGrid();
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // LEFT JOIN handles pending applications with NULL reviewers safely
                string sql = @"SELECT t.Id, t.tutor_id, (u.fname + ' ' + ISNULL(u.lname, '')) AS FullName, 
                                      t.submitted_at, t.status, t.verification_document,
                                      t.verified_at, t.reviewed_by,
                                      (CAST(r.Id AS VARCHAR) + ' - ' + r.fname + ' ' + ISNULL(r.lname, '')) AS ReviewerName
                               FROM [tutorApplication] t
                               INNER JOIN [user] u ON t.tutor_id = u.Id
                               LEFT JOIN [user] r ON t.reviewed_by = r.Id
                               WHERE 1=1";

                string filter = ddlFilterStatus.SelectedValue;
                if (filter != "All") sql += " AND t.status = @FilterStatus";

                string sortDirection = ddlSortBy.SelectedValue;
                sql += $" ORDER BY t.submitted_at {sortDirection}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (filter != "All") cmd.Parameters.AddWithValue("@FilterStatus", filter);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ViewState["TotalAppRecords"] = dt.Rows.Count;
                        gvApplications.DataSource = dt;
                        gvApplications.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalAppRecords"] != null ? Convert.ToInt32(ViewState["TotalAppRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvApplications.PageSize);
            if (totalPages == 0) totalPages = 1;

            int startRecord = (gvApplications.PageIndex * gvApplications.PageSize) + 1;
            int endRecord = startRecord + gvApplications.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} applications";
            txtPageJump.Text = (gvApplications.PageIndex + 1).ToString();

            btnPrev.Enabled = gvApplications.PageIndex > 0;
            btnNext.Enabled = gvApplications.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAppRecords"] != null ? Convert.ToInt32(ViewState["TotalAppRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvApplications.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvApplications.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvApplications.PageIndex = 0;
                else gvApplications.PageIndex = totalPages - 1;
            }
            BindGrid();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvApplications.PageIndex > 0)
            {
                gvApplications.PageIndex--;
                BindGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAppRecords"] != null ? Convert.ToInt32(ViewState["TotalAppRecords"]) : 0;
            if (gvApplications.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvApplications.PageSize) - 1))
            {
                gvApplications.PageIndex++;
                BindGrid();
            }
        }

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvApplications.PageIndex = 0;
            BindGrid();
        }

        protected void gvApplications_DataBound(object sender, EventArgs e) { }

        protected string GetStatusText(object statusObj)
        {
            if (statusObj == null) return "Pending";
            string status = statusObj.ToString().ToUpper();
            if (status == "APPROVED") return "Verified";
            if (status == "REJECTED") return "Rejected";
            return "Pending";
        }

        protected string GetStatusDotClass(object statusObj)
        {
            if (statusObj == null) return "status-dot dot-pending";
            string status = statusObj.ToString().ToUpper();
            if (status == "APPROVED") return "status-dot dot-verified";
            if (status == "REJECTED") return "status-dot dot-rejected";
            return "status-dot dot-pending";
        }

        protected void gvApplications_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string status = DataBinder.Eval(e.Row.DataItem, "status").ToString().ToUpper();
                RadioButton rbVerify = (RadioButton)e.Row.FindControl("rbVerify");
                RadioButton rbReject = (RadioButton)e.Row.FindControl("rbReject");

                if (status == "APPROVED") rbVerify.Checked = true;
                else if (status == "REJECTED") rbReject.Checked = true;

                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvApplications, "View$" + e.Row.RowIndex);
            }
        }

        protected void gvApplications_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int appId = Convert.ToInt32(gvApplications.DataKeys[rowIndex].Value);
                LoadApplicationDetails(appId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void LoadApplicationDetails(int appId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT t.Id, ('T' + RIGHT('000' + CAST(t.tutor_id AS VARCHAR(10)), 3) + ' ' + u.fname + ' ' + ISNULL(u.lname, '')) AS FullName, 
                                      t.submitted_at, t.status, t.verification_document, t.verified_at,
                                      (CAST(r.Id AS VARCHAR) + ' - ' + ISNULL(r.fname, '') + ' ' + ISNULL(r.lname, '')) AS ReviewerName
                               FROM [tutorApplication] t
                               INNER JOIN [user] u ON t.tutor_id = u.Id
                               LEFT JOIN [user] r ON t.reviewed_by = r.Id
                               WHERE t.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", appId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            litViewId.Text = "AT" + rdr["Id"].ToString().PadLeft(3, '0');
                            litViewDate.Text = Convert.ToDateTime(rdr["submitted_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewName.Text = rdr["FullName"].ToString();

                            string status = rdr["status"].ToString();
                            litViewStatus.Text = $"<span class='{GetStatusDotClass(status)}'></span> {GetStatusText(status)}";

                            string docLink = rdr["verification_document"].ToString();
                            litViewDoc.Text = $"<a href='{ResolveUrl("~/Uploads/Verification/" + docLink)}' target='_blank' class='text-primary fw-bold'><i class='bi bi-file-earmark-pdf'></i> View Doc</a>";

                            litViewReviewer.Text = string.IsNullOrWhiteSpace(rdr["ReviewerName"].ToString()) ? "Not Reviewed Yet" : rdr["ReviewerName"].ToString();
                        }
                    }
                }
            }
        }

        protected void Action_Changed(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            GridViewRow row = (GridViewRow)rb.NamingContainer;
            int appId = Convert.ToInt32(gvApplications.DataKeys[row.RowIndex].Value);

            string newStatus = rb.ID == "rbVerify" ? "APPROVED" : "REJECTED";
            int reviewerId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;

            string applicantEmail = "";
            string applicantFirstName = "";
            int applicantId = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string sqlApp = @"UPDATE [tutorApplication] SET status = @status, verified_at = GETDATE(), reviewed_by = @reviewerId WHERE Id = @Id";
                            using (SqlCommand cmdApp = new SqlCommand(sqlApp, conn, trans))
                            {
                                cmdApp.Parameters.AddWithValue("@status", newStatus);
                                cmdApp.Parameters.AddWithValue("@reviewerId", reviewerId);
                                cmdApp.Parameters.AddWithValue("@Id", appId);
                                cmdApp.ExecuteNonQuery();
                            }

                            string sqlTutor = @"SELECT u.email, u.fname, t.tutor_id 
                                        FROM [tutorApplication] t 
                                        INNER JOIN [user] u ON t.tutor_id = u.Id 
                                        WHERE t.Id = @Id";
                            using (SqlCommand cmdTutor = new SqlCommand(sqlTutor, conn, trans))
                            {
                                cmdTutor.Parameters.AddWithValue("@Id", appId);
                                using (SqlDataReader rdr = cmdTutor.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        applicantEmail = rdr["email"].ToString();
                                        applicantFirstName = rdr["fname"].ToString();
                                        applicantId = Convert.ToInt32(rdr["tutor_id"]);
                                    }
                                }
                            }

                            if (newStatus == "APPROVED")
                            {
                                string sqlRole = @"UPDATE [user] SET role_id = 3 WHERE Id = @ApplicantId";
                                using (SqlCommand cmdRole = new SqlCommand(sqlRole, conn, trans))
                                {
                                    cmdRole.Parameters.AddWithValue("@ApplicantId", applicantId);
                                    cmdRole.ExecuteNonQuery();
                                }
                            }

                            if (applicantId > 0)
                            {
                                string notificationContent = newStatus == "APPROVED"
                                    ? "Congratulations! Your tutor application has been approved. You are now a Tutor on EduConnect."
                                    : "Your tutor application has been reviewed and rejected. Please check your email for more details.";

                                string sqlNotif = @"INSERT INTO [notification] (user_id, content, status) VALUES (@userId, @content, 'UNREAD')";
                                using (SqlCommand cmdNotif = new SqlCommand(sqlNotif, conn, trans))
                                {
                                    cmdNotif.Parameters.AddWithValue("@userId", applicantId);
                                    cmdNotif.Parameters.AddWithValue("@content", notificationContent);
                                    cmdNotif.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }

                SystemLogService.Write("STAFF_TUTOR_APP_UPDATED", $"Staff updated tutor application ID {appId} status to {newStatus}.", LogLevel.INFO, reviewerId);

                if (applicantId > 0)
                {
                    try
                    {
                        var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                        hubContext.Clients.All.receiveNotification(applicantId);
                    }
                    catch (Exception ex)
                    {
                        SystemLogService.Write("SIGNALR_ERROR", $"Failed to send real-time notification to user {applicantId}: {ex.Message}", LogLevel.WARNING, reviewerId);
                    }
                }

                if (!string.IsNullOrEmpty(applicantEmail))
                {
                    string subject = newStatus == "APPROVED" ? "Congratulations! You are now an EduConnect Tutor!" : "Update on your Tutor Application";
                    string emailMessage = newStatus == "APPROVED"
                        ? $"Great news! Your tutor application has been reviewed and <strong>approved</strong> by our staff. <br><br>Your account has been upgraded. Next time you log in, you will have access to the Tutor Dashboard where you can start creating courses, uploading materials, and managing students. Welcome to the teaching community!"
                        : $"Thank you for your interest in teaching on EduConnect. We have reviewed your tutor application, but unfortunately, we are unable to approve it at this time. Please ensure your verification documents meet our guidelines and try submitting a new application from your dashboard.";

                    EmailHelper.SendNotificationEmail(applicantEmail, applicantFirstName, subject, emailMessage);
                }

                BindGrid();
                lblMessage.Visible = true;

                if (newStatus == "APPROVED")
                {
                    lblMessage.Text = $"Application [{appId}] verified! User is now a Tutor. Notifications sent.";
                    lblMessage.CssClass = "alert alert-success d-block";
                }
                else
                {
                    lblMessage.Text = $"Application [{appId}] rejected. Notifications sent.";
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
            catch (Exception ex)
            {
                SystemLogService.Write("STAFF_TUTOR_APP_UPDATE_ERROR", $"DB Error updating application ID {appId}: {ex.Message}", LogLevel.ERROR, reviewerId);

                lblMessage.Visible = true;
                lblMessage.Text = "Error updating application or sending email: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<int> selectedIds = new List<int>();
            foreach (GridViewRow row in gvApplications.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked) selectedIds.Add(Convert.ToInt32(gvApplications.DataKeys[row.RowIndex].Value));
            }

            if (selectedIds.Count > 0)
            {
                List<string> displayTitles = new List<string>();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string[] paramNames = new string[selectedIds.Count];
                    for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;
                    string sql = $@"SELECT t.Id, ('T' + RIGHT('000' + CAST(t.tutor_id AS VARCHAR(10)), 3) + ' ' + u.fname + ' ' + ISNULL(u.lname, '')) AS FullName FROM [tutorApplication] t INNER JOIN [user] u ON t.tutor_id = u.Id WHERE t.Id IN ({string.Join(",", paramNames)})";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string formattedAppId = "AT" + rdr["Id"].ToString().PadLeft(3, '0');
                                displayTitles.Add($"{formattedAppId} - {rdr["FullName"].ToString()}");
                            }
                        }
                    }
                }
                litSelectedIds.Text = string.Join("<br/>", displayTitles);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one application to remove.";
                lblMessage.CssClass = "alert alert-warning d-block";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            int staffId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvApplications.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked) selectedIds.Add(Convert.ToInt32(gvApplications.DataKeys[row.RowIndex].Value));
            }

            if (selectedIds.Count > 0)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string[] paramNames = new string[selectedIds.Count];
                        for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;
                        string sql = $"DELETE FROM [tutorApplication] WHERE Id IN ({string.Join(",", paramNames)})";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string idList = string.Join(", ", selectedIds);
                    SystemLogService.Write("STAFF_TUTOR_APP_DELETED", $"Staff deleted tutor application IDs: {idList}.", LogLevel.WARNING, staffId);

                    BindGrid();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected application(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                }
                catch (Exception ex)
                {
                    SystemLogService.Write("STAFF_TUTOR_APP_DELETE_ERROR", $"DB Error deleting applications: {ex.Message}", LogLevel.ERROR, staffId);

                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
        }
    }
}