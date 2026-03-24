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
            if (Session["role_id"] == null || (int)Session["role_id"] != 2) Response.Redirect("~/Pages/Guest/Home.aspx");

            if (!IsPostBack)
            {
                if (Request.QueryString["msg"] == "success")
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Announcement processed successfully!";
                    lblMessage.CssClass = "alert alert-success d-block mb-3";
                }
                BindDropdowns();
                ddlFilterStatus.SelectedValue = "All";
                BindGrid();
            }
        }

        private void BindDropdowns()
        {
            ddlFilterMonth.Items.Clear();
            ddlFilterMonth.Items.Add(new ListItem("All Months", "All"));
            ddlFilterMonth.Items.Add(new ListItem("January", "1"));
            ddlFilterMonth.Items.Add(new ListItem("February", "2"));
            ddlFilterMonth.Items.Add(new ListItem("March", "3"));
            ddlFilterMonth.Items.Add(new ListItem("April", "4"));
            ddlFilterMonth.Items.Add(new ListItem("May", "5"));
            ddlFilterMonth.Items.Add(new ListItem("June", "6"));
            ddlFilterMonth.Items.Add(new ListItem("July", "7"));
            ddlFilterMonth.Items.Add(new ListItem("August", "8"));
            ddlFilterMonth.Items.Add(new ListItem("September", "9"));
            ddlFilterMonth.Items.Add(new ListItem("October", "10"));
            ddlFilterMonth.Items.Add(new ListItem("November", "11"));
            ddlFilterMonth.Items.Add(new ListItem("December", "12"));

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM [announcement] ORDER BY YearVal DESC", conn))
                {
                    using (SqlDataReader rdrYear = cmdYear.ExecuteReader())
                    {
                        ddlFilterYear.DataSource = rdrYear;
                        ddlFilterYear.DataTextField = "YearVal";
                        ddlFilterYear.DataValueField = "YearVal";
                        ddlFilterYear.DataBind();
                        ddlFilterYear.Items.Insert(0, new ListItem("All Years", "All"));
                    }
                }
            }
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // Retrieve is_published instead of status/scheduled
                string sql = @"SELECT a.Id, a.title, a.message, a.created_at, ISNULL(a.is_published, 0) as is_published, r.name AS role_name 
                               FROM [announcement] a
                               LEFT JOIN [role] r ON a.target_role_id = r.Id
                               INNER JOIN [user] u ON a.created_by = u.Id
                               WHERE u.role_id = 2";

                if (ddlFilterRole.SelectedValue == "All") sql += " AND (a.target_role_id IN (3, 4) OR a.target_role_id IS NULL)";
                else sql += " AND a.target_role_id = @role";

                // Filter based on is_published (1 = Active, 0 = Archived)
                if (ddlFilterStatus.SelectedValue != "All") sql += " AND a.is_published = @is_published";
                if (ddlFilterMonth.SelectedValue != "All") sql += " AND MONTH(a.created_at) = @month";
                if (ddlFilterYear.SelectedValue != "All") sql += " AND YEAR(a.created_at) = @year";

                if (ddlSortBy.SelectedValue == "Oldest")
                {
                    sql += " ORDER BY a.created_at ASC";
                }
                else
                {
                    sql += " ORDER BY a.created_at DESC";
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (ddlFilterRole.SelectedValue != "All") cmd.Parameters.AddWithValue("@role", Convert.ToInt32(ddlFilterRole.SelectedValue));
                    if (ddlFilterStatus.SelectedValue != "All") cmd.Parameters.AddWithValue("@is_published", ddlFilterStatus.SelectedValue);
                    if (ddlFilterMonth.SelectedValue != "All") cmd.Parameters.AddWithValue("@month", Convert.ToInt32(ddlFilterMonth.SelectedValue));
                    if (ddlFilterYear.SelectedValue != "All") cmd.Parameters.AddWithValue("@year", Convert.ToInt32(ddlFilterYear.SelectedValue));

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

        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalAnnounceRecords"] != null ? Convert.ToInt32(ViewState["TotalAnnounceRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvAnnouncements.PageSize);
            if (totalPages == 0) totalPages = 1;

            int startRecord = (gvAnnouncements.PageIndex * gvAnnouncements.PageSize) + 1;
            int endRecord = startRecord + gvAnnouncements.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} announcements";
            txtPageJump.Text = (gvAnnouncements.PageIndex + 1).ToString();

            btnPrev.Enabled = gvAnnouncements.PageIndex > 0;
            btnNext.Enabled = gvAnnouncements.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAnnounceRecords"] != null ? Convert.ToInt32(ViewState["TotalAnnounceRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvAnnouncements.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvAnnouncements.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvAnnouncements.PageIndex = 0;
                else gvAnnouncements.PageIndex = totalPages - 1;
            }
            BindGrid();
        }

        protected void btnPrev_Click(object sender, EventArgs e) { if (gvAnnouncements.PageIndex > 0) { gvAnnouncements.PageIndex--; BindGrid(); } }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAnnounceRecords"] != null ? Convert.ToInt32(ViewState["TotalAnnounceRecords"]) : 0;
            if (gvAnnouncements.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvAnnouncements.PageSize) - 1)) { gvAnnouncements.PageIndex++; BindGrid(); }
        }

        protected void FilterGrid_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvAnnouncements.PageIndex = 0; BindGrid(); }

        // Clear Filters logic
        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSortBy.SelectedValue = "Latest";
            ddlFilterRole.SelectedValue = "All";
            ddlFilterStatus.SelectedValue = "All";
            ddlFilterMonth.SelectedValue = "All";
            ddlFilterYear.SelectedValue = "All";
            lblMessage.Visible = false;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ClearSearch", "document.getElementById('" + txtSearch.ClientID + "').value = ''; SearchTable();", true);

            gvAnnouncements.PageIndex = 0;
            BindGrid();
        }

        protected void gvAnnouncements_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvAnnouncements, "View$" + e.Row.RowIndex);
            }
        }

        protected void gvAnnouncements_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int announceId = Convert.ToInt32(gvAnnouncements.DataKeys[rowIndex].Value);
                LoadAnnouncementDetails(announceId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void LoadAnnouncementDetails(int id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Updated to select is_published
                string sql = @"SELECT a.Id, a.title, a.message, a.created_at, ISNULL(a.is_published, 0) as is_published, r.name AS role_name 
                               FROM [announcement] a LEFT JOIN [role] r ON a.target_role_id = r.Id WHERE a.Id = @Id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            litViewId.Text = "A" + rdr["Id"].ToString().PadLeft(3, '0');
                            litViewTitle.Text = rdr["title"].ToString();
                            litViewDate.Text = Convert.ToDateTime(rdr["created_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewTarget.Text = rdr["role_name"] == DBNull.Value ? "All Roles" : rdr["role_name"].ToString();

                            // Check is_published boolean to display Status string
                            bool isPublished = Convert.ToBoolean(rdr["is_published"]);

                            if (isPublished)
                            {
                                litViewStatus.Text = $"<span class='badge bg-success rounded-pill fw-bold px-3 py-2'>ACTIVE</span>";
                            }
                            else
                            {
                                litViewStatus.Text = $"<span class='badge bg-secondary rounded-pill fw-bold px-3 py-2'>ARCHIVED</span>";
                            }

                            litViewMessage.Text = rdr["message"].ToString().Replace("\n", "<br/>");
                        }
                    }
                }
            }
        }

        protected void btnComposeRedirect_Click(object sender, EventArgs e) { Response.Redirect("ComposeAnnouncement.aspx"); }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<string> selectedTitles = new List<string>();
            foreach (GridViewRow row in gvAnnouncements.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    string formattedId = "A" + gvAnnouncements.DataKeys[row.RowIndex].Value.ToString().PadLeft(3, '0');
                    selectedTitles.Add($"{formattedId} - {row.Cells[2].Text}");
                }
            }

            if (selectedTitles.Count > 0)
            {
                litSelectedTitles.Text = string.Join("<br/>", selectedTitles);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one announcement to remove.";
                lblMessage.CssClass = "alert alert-warning d-block mb-3";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = new List<int>();
            foreach (GridViewRow row in gvAnnouncements.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked) selectedIds.Add(Convert.ToInt32(gvAnnouncements.DataKeys[row.RowIndex].Value));
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
                            for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    BindGrid();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected announcement(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block mb-3";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block mb-3";
                }
            }
        }
    }
}