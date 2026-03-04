using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace WAPP.Pages.Staff
{
    public partial class AnnouncementManagement : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 2) // Verify Staff Role
            {
                Response.Redirect("~/Pages/Guest/Login.aspx");
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["msg"] == "success")
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Announcement processed successfully!";
                    lblMessage.CssClass = "alert alert-success d-block";
                }

                ddlFilterStatus.SelectedValue = "All";
                BindGrid();
            }
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. AUTO-PUBLISH SCHEDULED ANNOUNCEMENTS
                string publishSql = "UPDATE [announcement] SET status = 'ACTIVE' WHERE status = 'SCHEDULED' AND scheduled_at <= GETDATE()";
                using (SqlCommand cmdPublish = new SqlCommand(publishSql, conn))
                {
                    cmdPublish.ExecuteNonQuery();
                }

                // 2. FETCH ANNOUNCEMENTS
                string sql = @"SELECT a.Id, a.title, a.message, a.created_at, a.scheduled_at, a.status, r.name AS role_name 
                               FROM [announcement] a
                               LEFT JOIN [role] r ON a.target_role_id = r.Id
                               INNER JOIN [user] u ON a.created_by = u.Id
                               WHERE u.role_id = 2";

                if (ddlFilterRole.SelectedValue == "All")
                {
                    sql += " AND (a.target_role_id IN (3, 4) OR a.target_role_id IS NULL)";
                }
                else
                {
                    sql += " AND a.target_role_id = @role";
                }

                if (ddlFilterStatus.SelectedValue != "All")
                    sql += " AND a.status = @status";

                if (ddlFilterMonth.SelectedValue != "All")
                    sql += " AND MONTH(a.created_at) = @month";

                if (ddlSortBy.SelectedValue == "Oldest")
                    sql += " ORDER BY a.created_at ASC";
                else
                    sql += " ORDER BY a.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (ddlFilterRole.SelectedValue != "All")
                        cmd.Parameters.AddWithValue("@role", Convert.ToInt32(ddlFilterRole.SelectedValue));

                    if (ddlFilterStatus.SelectedValue != "All")
                        cmd.Parameters.AddWithValue("@status", ddlFilterStatus.SelectedValue);

                    if (ddlFilterMonth.SelectedValue != "All")
                        cmd.Parameters.AddWithValue("@month", Convert.ToInt32(ddlFilterMonth.SelectedValue));

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        ViewState["TotalAnnounceRecords"] = dt.Rows.Count;

                        gvAnnouncements.DataSource = dt;
                        gvAnnouncements.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        // --- NEW PERMANENT PAGER LOGIC ---
        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalAnnounceRecords"] != null ? Convert.ToInt32(ViewState["TotalAnnounceRecords"]) : 0;
            int startRecord = (gvAnnouncements.PageIndex * gvAnnouncements.PageSize) + 1;
            int endRecord = startRecord + gvAnnouncements.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} announcements";

            btnPrev.Enabled = gvAnnouncements.PageIndex > 0;
            btnPrev.CssClass = btnPrev.Enabled ? "pager-link" : "pager-link disabled";

            int totalPages = (int)Math.Ceiling((double)totalRecords / gvAnnouncements.PageSize);
            btnNext.Enabled = gvAnnouncements.PageIndex < (totalPages - 1);
            btnNext.CssClass = btnNext.Enabled ? "pager-link" : "pager-link disabled";
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvAnnouncements.PageIndex > 0)
            {
                gvAnnouncements.PageIndex--;
                BindGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAnnounceRecords"] != null ? Convert.ToInt32(ViewState["TotalAnnounceRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvAnnouncements.PageSize);

            if (gvAnnouncements.PageIndex < totalPages - 1)
            {
                gvAnnouncements.PageIndex++;
                BindGrid();
            }
        }
        // ---------------------------------

        protected void gvAnnouncements_DataBound(object sender, EventArgs e)
        {
            // Empty placeholder to fulfill the asp:GridView OnDataBound property requirement
        }

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvAnnouncements.PageIndex = 0;
            BindGrid();
        }

        protected string GetStatusDotClass(object statusObj)
        {
            if (statusObj == null) return "status-dot dot-archived";
            string status = statusObj.ToString().ToUpper();
            if (status == "ACTIVE") return "status-dot dot-active";
            if (status == "SCHEDULED") return "status-dot dot-scheduled";
            return "status-dot dot-archived";
        }

        protected void btnComposeRedirect_Click(object sender, EventArgs e)
        {
            Response.Redirect("ComposeAnnouncement.aspx");
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<string> selectedTitles = new List<string>();

            foreach (GridViewRow row in gvAnnouncements.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedTitles.Add($"'{row.Cells[2].Text}'");
                }
            }

            if (selectedTitles.Count > 0)
            {
                litSelectedTitles.Text = string.Join(", ", selectedTitles);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one announcement to remove.";
                lblMessage.CssClass = "alert alert-warning d-block";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvAnnouncements.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(Convert.ToInt32(gvAnnouncements.DataKeys[row.RowIndex].Value));
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

                        string sql = $"DELETE FROM [announcement] WHERE Id IN ({string.Join(",", paramNames)})";

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
                    lblMessage.Text = "Selected announcement(s) removed successfully.";
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