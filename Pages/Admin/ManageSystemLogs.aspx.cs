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
    public partial class ManageSystemLogs : System.Web.UI.Page
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
                LoadSeverities();
                LoadDateFilters();
                BindLogData();
            }
        }

        private void LoadSeverities()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT id, name FROM logSeverity", con);
                con.Open();
                ddlSeverityFilter.DataSource = cmd.ExecuteReader();
                ddlSeverityFilter.DataTextField = "name";
                ddlSeverityFilter.DataValueField = "id";
                ddlSeverityFilter.DataBind();
                ddlSeverityFilter.Items.Insert(0, new ListItem("All Severities", "All"));
            }
        }

        private void LoadDateFilters()
        {
            ddlFilterMonth.Items.Clear();
            ddlFilterMonth.Items.Add(new ListItem("All Months", "All"));
            for (int i = 1; i <= 12; i++)
            {
                ddlFilterMonth.Items.Add(new ListItem(System.Globalization.DateTimeFormatInfo.InvariantInfo.GetMonthName(i), i.ToString()));
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM systemLog ORDER BY YearVal DESC", con))
                {
                    con.Open();
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

        private void BindLogData()
        {
            string sql = @"SELECT sl.Id, (ISNULL(u.fname, '') + ' ' + ISNULL(u.lname, '')) AS UserName, 
                           ls.name AS SeverityName, sl.action_type, sl.description, 
                           sl.status, sl.created_at
                           FROM systemLog sl
                           LEFT JOIN [user] u ON sl.user_id = u.Id
                           INNER JOIN logSeverity ls ON sl.severity_id = ls.Id
                           WHERE 1=1";

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;

                // Universal Backend Search
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    sql += @" AND (sl.description LIKE @s OR sl.action_type LIKE @s OR ls.name LIKE @s 
                              OR sl.status LIKE @s OR CAST(sl.Id AS NVARCHAR) LIKE @s 
                              OR (ISNULL(u.fname, '') + ' ' + ISNULL(u.lname, '')) LIKE @s)";
                    cmd.Parameters.AddWithValue("@s", "%" + txtSearch.Text.Trim() + "%");
                }

                if (ddlSeverityFilter.SelectedValue != "All")
                {
                    sql += " AND sl.severity_id = @sev";
                    cmd.Parameters.AddWithValue("@sev", ddlSeverityFilter.SelectedValue);
                }

                if (ddlStatusFilter.SelectedValue != "All")
                {
                    sql += " AND sl.status = @stat";
                    cmd.Parameters.AddWithValue("@stat", ddlStatusFilter.SelectedValue);
                }

                if (ddlFilterMonth.SelectedValue != "All")
                {
                    sql += " AND MONTH(sl.created_at) = @month";
                    cmd.Parameters.AddWithValue("@month", ddlFilterMonth.SelectedValue);
                }

                if (ddlFilterYear.SelectedValue != "All")
                {
                    sql += " AND YEAR(sl.created_at) = @year";
                    cmd.Parameters.AddWithValue("@year", ddlFilterYear.SelectedValue);
                }

                sql += " ORDER BY sl.created_at " + ddlSort.SelectedValue;
                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ViewState["TotalLogRecords"] = dt.Rows.Count;

                gvLogs.DataSource = dt;
                gvLogs.DataBind();

                UpdatePagingLabels(dt.Rows.Count);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSort.SelectedIndex = 0;
            ddlSeverityFilter.SelectedIndex = 0;
            ddlStatusFilter.SelectedIndex = 0;
            ddlFilterMonth.SelectedIndex = 0;
            ddlFilterYear.SelectedIndex = 0;
            lblMessage.Visible = false;
            gvLogs.PageIndex = 0;

            BindLogData();
            upPanelSystemLogs.Update();
        }

        // =======================================================
        // Grid View Interactions (Row Click & View Details)
        // =======================================================
        protected void gvLogs_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvLogs, "ViewLog$" + e.Row.RowIndex);
            }
        }

        protected void gvLogs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewLog")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int logId = Convert.ToInt32(gvLogs.DataKeys[rowIndex].Value);
                LoadLogDetails(logId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void LoadLogDetails(int id)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = @"SELECT sl.Id, (ISNULL(u.fname, '') + ' ' + ISNULL(u.lname, '')) AS UserName, 
                               ls.name AS SeverityName, sl.action_type, sl.description, sl.status, sl.created_at
                               FROM systemLog sl
                               LEFT JOIN [user] u ON sl.user_id = u.Id
                               INNER JOIN logSeverity ls ON sl.severity_id = ls.Id
                               WHERE sl.Id = @Id";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    con.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            litViewId.Text = "LOG" + rdr["Id"].ToString().PadLeft(4, '0');
                            litViewDate.Text = Convert.ToDateTime(rdr["created_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewUser.Text = string.IsNullOrWhiteSpace(rdr["UserName"].ToString()) ? "System" : rdr["UserName"].ToString();
                            litViewSeverity.Text = rdr["SeverityName"].ToString();
                            litViewAction.Text = rdr["action_type"].ToString();

                            string status = rdr["status"].ToString();
                            string statusClass = status == "OPEN" ? "bg-danger" : (status == "RESOLVED" ? "bg-success" : "bg-secondary");
                            litViewStatus.Text = $"<span class='badge px-3 py-2 ec-status-badge {statusClass}'>{status}</span>";

                            litViewMessage.Text = rdr["description"].ToString().Replace("\n", "<br/>");
                        }
                    }
                }
            }
            upPanelSystemLogs.Update();
        }

        // =======================================================
        // Actions
        // =======================================================
        protected void btnConfirmChange_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserId"]);
            if (int.TryParse(hfLogId.Value, out int logId))
            {
                string newStatus = ddlModalStatus.SelectedValue;

                try
                {
                    using (SqlConnection con = new SqlConnection(connStr))
                    {
                        SqlCommand cmd = new SqlCommand("UPDATE systemLog SET status = @status WHERE Id = @id", con);
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", logId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    SystemLogService.Write("ADMIN_LOG_STATUS_CHANGED", $"Admin changed status of Log ID {logId} to {newStatus}.", LogLevel.INFO, adminId);

                    BindLogData();
                }
                catch (Exception ex)
                {
                    SystemLogService.Write("ADMIN_LOG_UPDATE_ERROR", $"DB Error changing status for Log ID {logId}: {ex.Message}", LogLevel.ERROR, adminId);
                }
            }
            upPanelSystemLogs.Update();
            ScriptManager.RegisterStartupScript(this, GetType(), "hideModal", "closeLogConfirmModal();", true);
        }

        protected void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvLogs.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(Convert.ToInt32(gvLogs.DataKeys[row.RowIndex].Value));
                }
            }

            if (selectedIds.Count > 0)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string[] paramNames = new string[selectedIds.Count];
                        for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;
                        string sql = $"DELETE FROM systemLog WHERE Id IN ({string.Join(",", paramNames)})";

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

                    string idList = string.Join(", ", selectedIds);
                    SystemLogService.Write("ADMIN_LOGS_DELETED", $"Admin permanently deleted system log IDs: {idList}.", LogLevel.WARNING, adminId);

                    BindLogData();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected log(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block mb-4 fw-bold";
                }
                catch (Exception ex)
                {
                    SystemLogService.Write("ADMIN_LOGS_DELETE_ERROR", $"DB Error deleting logs: {ex.Message}", LogLevel.ERROR, adminId);
                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block mb-4 fw-bold";
                }
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one log to remove.";
                lblMessage.CssClass = "alert alert-warning d-block mb-4 fw-bold";
            }
            upPanelSystemLogs.Update();
        }

        protected void Filter_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvLogs.PageIndex = 0; BindLogData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { lblMessage.Visible = false; gvLogs.PageIndex = 0; BindLogData(); }
        protected void gvLogs_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvLogs.PageIndex = e.NewPageIndex; BindLogData(); }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvLogs.PageIndex > 0)
            {
                gvLogs.PageIndex--;
                BindLogData();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalLogRecords"] != null ? Convert.ToInt32(ViewState["TotalLogRecords"]) : 0;
            if (gvLogs.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvLogs.PageSize) - 1))
            {
                gvLogs.PageIndex++;
                BindLogData();
            }
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalLogRecords"] != null ? Convert.ToInt32(ViewState["TotalLogRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvLogs.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvLogs.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvLogs.PageIndex = 0;
                else gvLogs.PageIndex = totalPages - 1;
            }
            BindLogData();
        }

        private void UpdatePagingLabels(int total)
        {
            if (total == 0)
            {
                lblShowing.Text = "Showing 0 logs";
                btnPrev.Enabled = false;
                btnNext.Enabled = false;
                btnPrev.CssClass = "btn btn-outline-primary btn-sm fw-bold px-3 disabled";
                btnNext.CssClass = "btn btn-outline-primary btn-sm fw-bold px-3 disabled";
                txtPageJump.Text = "1";
                return;
            }

            int start = (gvLogs.PageIndex * gvLogs.PageSize) + 1;
            int end = Math.Min((gvLogs.PageIndex + 1) * gvLogs.PageSize, total);
            lblShowing.Text = $"Showing {start}-{end} of {total} logs";

            txtPageJump.Text = (gvLogs.PageIndex + 1).ToString();

            int totalPages = (int)Math.Ceiling((double)total / gvLogs.PageSize);

            btnPrev.Enabled = gvLogs.PageIndex > 0;
            btnNext.Enabled = gvLogs.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }
    }
}