using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using WAPP.Utils;

namespace WAPP.Pages.Admin
{
    public partial class ManageAnnouncements : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 1)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["msg"] == "success")
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Announcement created successfully!";
                    lblMessage.CssClass = "alert alert-success d-block fw-bold mb-4";
                }

                LoadFilters();
                BindData();
            }
        }

        private void LoadFilters()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT id, name FROM [role]", con);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                ddlFilterRole.DataSource = dt;
                ddlFilterRole.DataTextField = "name";
                ddlFilterRole.DataValueField = "id";
                ddlFilterRole.DataBind();
                ddlFilterRole.Items.Insert(0, new ListItem("All Roles", "All"));

                ddlFilterMonth.Items.Clear();
                ddlFilterMonth.Items.Add(new ListItem("All Months", "All"));
                for (int i = 1; i <= 12; i++)
                {
                    ddlFilterMonth.Items.Add(new ListItem(System.Globalization.DateTimeFormatInfo.InvariantInfo.GetMonthName(i), i.ToString()));
                }

                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM announcement ORDER BY YearVal DESC", con))
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

        private void BindData()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = @"SELECT a.Id, a.title, a.message, ISNULL(r.name, 'All Roles') AS role_name, 
                               a.created_at, a.status 
                               FROM announcement a
                               LEFT JOIN [role] r ON a.target_role_id = r.Id
                               WHERE 1=1";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;

                    // Universal Backend Search
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        sql += @" AND (a.title LIKE @s OR a.message LIKE @s OR ISNULL(r.name, 'All Roles') LIKE @s 
                                  OR a.status LIKE @s OR CAST(a.Id AS NVARCHAR) LIKE @s)";
                        cmd.Parameters.AddWithValue("@s", "%" + txtSearch.Text.Trim() + "%");
                    }

                    if (ddlFilterRole.SelectedValue != "All")
                    {
                        sql += " AND a.target_role_id = @target";
                        cmd.Parameters.AddWithValue("@target", ddlFilterRole.SelectedValue);
                    }

                    if (ddlFilterStatus.SelectedValue != "All")
                    {
                        sql += " AND a.status = @status";
                        cmd.Parameters.AddWithValue("@status", ddlFilterStatus.SelectedValue);
                    }

                    if (ddlFilterMonth.SelectedValue != "All")
                    {
                        sql += " AND MONTH(a.created_at) = @month";
                        cmd.Parameters.AddWithValue("@month", ddlFilterMonth.SelectedValue);
                    }

                    if (ddlFilterYear.SelectedValue != "All")
                    {
                        sql += " AND YEAR(a.created_at) = @year";
                        cmd.Parameters.AddWithValue("@year", ddlFilterYear.SelectedValue);
                    }

                    sql += ddlSortBy.SelectedValue == "Oldest" ? " ORDER BY a.created_at ASC" : " ORDER BY a.created_at DESC";

                    cmd.CommandText = sql;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ViewState["TotalAnnounceRecords"] = dt.Rows.Count;
                    gvAnnouncements.DataSource = dt;
                    gvAnnouncements.DataBind();
                }
            }
            UpdatePager();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSortBy.SelectedIndex = 0;
            ddlFilterRole.SelectedIndex = 0;
            ddlFilterStatus.SelectedIndex = 0;
            ddlFilterMonth.SelectedIndex = 0;
            ddlFilterYear.SelectedIndex = 0;
            lblMessage.Visible = false;
            gvAnnouncements.PageIndex = 0;

            BindData();
            upPanelAnnouncements.Update();
        }

        // =======================================================
        // Grid View Interactions (View Details)
        // =======================================================
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
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"SELECT a.Id, a.title, a.message, ISNULL(r.name, 'All Roles') AS role_name, 
                               a.created_at, a.status 
                               FROM announcement a
                               LEFT JOIN [role] r ON a.target_role_id = r.Id
                               WHERE a.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            litViewId.Text = "A" + rdr["Id"].ToString().PadLeft(3, '0');
                            litViewDate.Text = Convert.ToDateTime(rdr["created_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewTitle.Text = rdr["title"].ToString();
                            litViewTarget.Text = rdr["role_name"].ToString();

                            string status = rdr["status"].ToString();
                            string statusClass = status == "ACTIVE" ? "ec-status-active" : "ec-status-locked";
                            litViewStatus.Text = $"<span class='ec-status-pill {statusClass}'>{status}</span>";

                            litViewMessage.Text = rdr["message"].ToString();
                        }
                    }
                }
            }
            upPanelAnnouncements.Update();
        }

        // =======================================================
        // Compose & Delete Logic
        // =======================================================
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
                    int rawId = Convert.ToInt32(gvAnnouncements.DataKeys[row.RowIndex].Value);
                    string formattedId = "A" + rawId.ToString().PadLeft(3, '0');
                    string title = row.Cells[2].Text;
                    selectedTitles.Add($"{formattedId} - {title}");
                }
            }

            if (selectedTitles.Count > 0)
            {
                litSelectedTitles.Text = string.Join("<br/>", selectedTitles);
                upPanelAnnouncements.Update();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one announcement to remove.";
                lblMessage.CssClass = "alert alert-warning d-block mb-4";
                upPanelAnnouncements.Update();
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
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
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string[] paramNames = new string[selectedIds.Count];
                        for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;

                        string inClause = string.Join(",", paramNames);

                        string sqlDelNotif = $"DELETE FROM [notification] WHERE announcement_id IN ({inClause})";
                        string sqlDelAnn = $"DELETE FROM [announcement] WHERE Id IN ({inClause})";

                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                using (SqlCommand cmd = new SqlCommand(sqlDelNotif, conn, trans))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmd.ExecuteNonQuery();
                                }

                                using (SqlCommand cmd = new SqlCommand(sqlDelAnn, conn, trans))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmd.ExecuteNonQuery();
                                }
                                trans.Commit();
                            }
                            catch
                            {
                                trans.Rollback();
                                throw;
                            }
                        }
                    }

                    SystemLogService.Write("ADMIN_ANNOUNCEMENT_DELETED", $"Admin deleted announcement IDs: {string.Join(", ", selectedIds)}.", LogLevel.WARNING, adminId);

                    BindData();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected announcement(s) deleted permanently.";
                    lblMessage.CssClass = "alert alert-success d-block mb-4 fw-bold";
                    upPanelAnnouncements.Update();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                }
                catch (Exception ex)
                {
                    SystemLogService.Write("ADMIN_ANNOUNCEMENT_DELETE_ERROR", $"DB Error deleting announcements: {ex.Message}", LogLevel.ERROR, adminId);
                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block mb-4 fw-bold";
                    upPanelAnnouncements.Update();
                }
            }
        }

        // =======================================================
        // Standard Pagination and Filtering
        // =======================================================
        protected void FilterGrid_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvAnnouncements.PageIndex = 0; BindData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { lblMessage.Visible = false; gvAnnouncements.PageIndex = 0; BindData(); }
        protected void gvAnnouncements_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvAnnouncements.PageIndex = e.NewPageIndex; BindData(); }

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
            BindData();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvAnnouncements.PageIndex > 0)
            {
                gvAnnouncements.PageIndex--;
                BindData();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAnnounceRecords"] != null ? Convert.ToInt32(ViewState["TotalAnnounceRecords"]) : 0;
            if (gvAnnouncements.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvAnnouncements.PageSize) - 1))
            {
                gvAnnouncements.PageIndex++;
                BindData();
            }
        }
    }
}