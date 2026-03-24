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
    public partial class ManageAlertLogs : System.Web.UI.Page
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
                BindAlertData();
            }
        }

        private void LoadSeverities()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT id, name FROM logSeverity WHERE id >= 4", con);
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
                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM systemLog WHERE severity_id >= 4 ORDER BY YearVal DESC", con))
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

        private void BindAlertData()
        {
            string sql = @"SELECT sl.Id, ls.name AS SeverityName, sl.action_type, sl.description, 
                   sl.status, sl.created_at
                   FROM systemLog sl
                   INNER JOIN logSeverity ls ON sl.severity_id = ls.Id
                   WHERE sl.severity_id >= 4 
                   AND sl.status != 'AUDIT'";

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;

                // Universal Backend Search: Searches Description, Action, Status, Severity, and ID
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    sql += " AND (sl.description LIKE @s OR sl.action_type LIKE @s OR ls.name LIKE @s OR sl.status LIKE @s OR CAST(sl.Id AS NVARCHAR) LIKE @s)";
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

                ViewState["TotalAlertRecords"] = dt.Rows.Count;

                gvAlerts.DataSource = dt;
                gvAlerts.DataBind();

                UpdatePagingLabels(dt.Rows.Count);
            }
        }

        // =======================================================
        // Grid View Interactions (Row Click & View Details)
        // =======================================================
        protected void gvAlerts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvAlerts, "ViewAlert$" + e.Row.RowIndex);
            }
        }

        protected void gvAlerts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewAlert")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int alertId = Convert.ToInt32(gvAlerts.DataKeys[rowIndex].Value);
                LoadAlertDetails(alertId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void LoadAlertDetails(int id)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = @"SELECT sl.Id, ls.name AS SeverityName, sl.action_type, sl.description, sl.status, sl.created_at
                               FROM systemLog sl
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
                            litViewId.Text = "ALRT" + rdr["Id"].ToString().PadLeft(4, '0');
                            litViewDate.Text = Convert.ToDateTime(rdr["created_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewSeverity.Text = rdr["SeverityName"].ToString();
                            litViewAction.Text = rdr["action_type"].ToString();

                            string status = rdr["status"].ToString();
                            string statusClass = status == "RESOLVED" ? "ec-status-active" : (status == "IGNORED" ? "ec-status-locked" : "ec-status-pending");
                            litViewStatus.Text = $"<span class='ec-status-pill {statusClass}'>{status}</span>";

                            litViewMessage.Text = rdr["description"].ToString().Replace("\n", "<br/>");
                        }
                    }
                }
            }
            upPanelAlerts.Update();
        }

        // =======================================================
        // Actions
        // =======================================================
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserId"]);
            if (int.TryParse(hfAlertId.Value, out int logId))
            {
                string newStatus = hfNewStatus.Value;

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE systemLog SET status=@st WHERE Id=@id", con);
                    cmd.Parameters.AddWithValue("@st", newStatus);
                    cmd.Parameters.AddWithValue("@id", logId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                SystemLogService.Write("ADMIN_ALERT_STATUS_UPDATED", $"Admin updated status of Alert ID {logId} to {newStatus}.", LogLevel.INFO, adminId);

                lblMessage.Visible = false;
                BindAlertData();
                upPanelAlerts.Update();
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "closeConfirm", "closeConfirmModal();", true);
        }

        protected void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvAlerts.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(Convert.ToInt32(gvAlerts.DataKeys[row.RowIndex].Value));
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
                    SystemLogService.Write("ADMIN_ALERTS_DELETED", $"Admin permanently deleted alert IDs: {idList}.", LogLevel.WARNING, adminId);

                    BindAlertData();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected alert(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block mb-4 fw-bold";
                    upPanelAlerts.Update();
                }
                catch (Exception ex)
                {
                    SystemLogService.Write("ADMIN_ALERTS_DELETE_ERROR", $"DB Error deleting alerts: {ex.Message}", LogLevel.ERROR, adminId);

                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block mb-4 fw-bold";
                    upPanelAlerts.Update();
                }
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one alert to remove.";
                lblMessage.CssClass = "alert alert-warning d-block mb-4 fw-bold";
                upPanelAlerts.Update();
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
            gvAlerts.PageIndex = 0;

            BindAlertData();
            upPanelAlerts.Update();
        }

        protected void Filter_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvAlerts.PageIndex = 0; BindAlertData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { lblMessage.Visible = false; gvAlerts.PageIndex = 0; BindAlertData(); }
        protected void gvAlerts_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvAlerts.PageIndex = e.NewPageIndex; BindAlertData(); }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvAlerts.PageIndex > 0)
            {
                gvAlerts.PageIndex--;
                BindAlertData();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAlertRecords"] != null ? Convert.ToInt32(ViewState["TotalAlertRecords"]) : 0;
            if (gvAlerts.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvAlerts.PageSize) - 1))
            {
                gvAlerts.PageIndex++;
                BindAlertData();
            }
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalAlertRecords"] != null ? Convert.ToInt32(ViewState["TotalAlertRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvAlerts.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvAlerts.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvAlerts.PageIndex = 0;
                else gvAlerts.PageIndex = totalPages - 1;
            }
            BindAlertData();
        }

        private void UpdatePagingLabels(int total)
        {
            if (total == 0)
            {
                lblShowing.Text = "Showing 0 alerts";
                btnPrev.Enabled = false;
                btnNext.Enabled = false;
                btnPrev.CssClass = "btn btn-outline-primary btn-sm fw-bold px-3 disabled";
                btnNext.CssClass = "btn btn-outline-primary btn-sm fw-bold px-3 disabled";
                txtPageJump.Text = "1";
                return;
            }

            int start = (gvAlerts.PageIndex * gvAlerts.PageSize) + 1;
            int end = Math.Min((gvAlerts.PageIndex + 1) * gvAlerts.PageSize, total);
            lblShowing.Text = $"Showing {start}-{end} of {total} alerts";

            txtPageJump.Text = (gvAlerts.PageIndex + 1).ToString();

            int totalPages = (int)Math.Ceiling((double)total / gvAlerts.PageSize);

            btnPrev.Enabled = (gvAlerts.PageIndex > 0);
            btnNext.Enabled = (gvAlerts.PageIndex < totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }
    }
}