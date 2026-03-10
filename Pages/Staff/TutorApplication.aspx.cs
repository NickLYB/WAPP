using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace WAPP.Pages.Staff
{
    public partial class TutorApplications : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Login.aspx");
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

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // LEFT JOIN to get reviewer details formatted as "ID - fname lname"
                string sql = @"SELECT t.Id, t.tutor_id, (u.fname + ' ' + u.lname) AS FullName, 
                                      t.submitted_at, t.status, t.verification_document,
                                      t.verified_at, t.reviewed_by,
                                      (CAST(r.Id AS VARCHAR) + ' - ' + r.fname + ' ' + r.lname) AS ReviewerName
                               FROM [tutorApplication] t
                               INNER JOIN [user] u ON t.tutor_id = u.Id
                               LEFT JOIN [user] r ON t.reviewed_by = r.Id
                               WHERE 1=1";

                string filter = ddlFilterStatus.SelectedValue;
                if (filter != "All") sql += " AND t.status = @FilterStatus";

                // Retrieve the sort direction directly from the dropdown list
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

        // --- PERMANENT PAGER LOGIC ---
        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalAppRecords"] != null ? Convert.ToInt32(ViewState["TotalAppRecords"]) : 0;
            int startRecord = (gvApplications.PageIndex * gvApplications.PageSize) + 1;
            int endRecord = startRecord + gvApplications.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} applications";

            btnPrev.Enabled = gvApplications.PageIndex > 0;
            btnPrev.CssClass = btnPrev.Enabled ? "pager-link" : "pager-link disabled";

            int totalPages = (int)Math.Ceiling((double)totalRecords / gvApplications.PageSize);
            btnNext.Enabled = gvApplications.PageIndex < (totalPages - 1);
            btnNext.CssClass = btnNext.Enabled ? "pager-link" : "pager-link disabled";
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
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvApplications.PageSize);

            if (gvApplications.PageIndex < totalPages - 1)
            {
                gvApplications.PageIndex++;
                BindGrid();
            }
        }
        // ---------------------------------

        // Handles both Filter and Sort dropdown changes
        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvApplications.PageIndex = 0;
            BindGrid();
        }

        protected void gvApplications_DataBound(object sender, EventArgs e)
        {
            // Empty placeholder to fulfill the asp:GridView OnDataBound property requirement
        }

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
            }
        }

        protected void Action_Changed(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            GridViewRow row = (GridViewRow)rb.NamingContainer;
            int appId = Convert.ToInt32(gvApplications.DataKeys[row.RowIndex].Value);

            string newStatus = rb.ID == "rbVerify" ? "APPROVED" : "REJECTED";
            int reviewerId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 1;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string sqlApp = @"UPDATE [tutorApplication] 
                                              SET status = @status, 
                                                  verified_at = GETDATE(), 
                                                  reviewed_by = @reviewerId 
                                              WHERE Id = @Id";

                            using (SqlCommand cmdApp = new SqlCommand(sqlApp, conn, trans))
                            {
                                cmdApp.Parameters.AddWithValue("@status", newStatus);
                                cmdApp.Parameters.AddWithValue("@reviewerId", reviewerId);
                                cmdApp.Parameters.AddWithValue("@Id", appId);
                                cmdApp.ExecuteNonQuery();
                            }

                            if (newStatus == "APPROVED")
                            {
                                string sqlRole = @"UPDATE [user] 
                                                   SET role_id = 3 
                                                   WHERE Id = (SELECT tutor_id FROM [tutorApplication] WHERE Id = @AppId)";

                                using (SqlCommand cmdRole = new SqlCommand(sqlRole, conn, trans))
                                {
                                    cmdRole.Parameters.AddWithValue("@AppId", appId);
                                    cmdRole.ExecuteNonQuery();
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

                BindGrid();

                lblMessage.Visible = true;
                if (newStatus == "APPROVED")
                {
                    lblMessage.Text = $"Application [{appId}] verified successfully! User is now a Tutor.";
                    lblMessage.CssClass = "alert alert-success d-block";
                }
                else
                {
                    lblMessage.Text = $"Application [{appId}] rejected.";
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error updating application: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<string> selectedIds = new List<string>();

            foreach (GridViewRow row in gvApplications.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(gvApplications.DataKeys[row.RowIndex].Value.ToString());
                }
            }

            if (selectedIds.Count > 0)
            {
                litSelectedIds.Text = "[" + string.Join("], [", selectedIds) + "]";
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
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvApplications.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(Convert.ToInt32(gvApplications.DataKeys[row.RowIndex].Value));
                }
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
                            for (int i = 0; i < selectedIds.Count; i++)
                            {
                                cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                            }
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    BindGrid();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected application(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
        }
    }
}