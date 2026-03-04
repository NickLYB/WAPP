using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Admin
{
    public partial class ManageSystemLogs : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Security Check: Ensure user is logged in AND has role_id 1 (Admin)
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 1)
            {
                // Unauthorized or session expired, kick them back to the public homepage
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            else
            {
                // 2. Load Data on Initial Visit
                if (!IsPostBack)
                {
                    LoadSeverities();
                    BindLogData();
                }
            }
        }

        private void LoadSeverities()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // Assuming logSeverity has 'id' and 'name' columns
                SqlCommand cmd = new SqlCommand("SELECT id, name FROM logSeverity", con);
                con.Open();
                ddlSeverityFilter.DataSource = cmd.ExecuteReader();
                ddlSeverityFilter.DataTextField = "name";
                ddlSeverityFilter.DataValueField = "id";
                ddlSeverityFilter.DataBind();
                ddlSeverityFilter.Items.Insert(0, new ListItem("All Severities", "All"));
            }
        }

        private void BindLogData()
        {
            // Join with user and logSeverity tables for readable data
            string sql = @"SELECT sl.Id, (u.fname + ' ' + u.lname) AS UserName, 
                           ls.name AS SeverityName, sl.action_type, sl.description, 
                           sl.status, sl.created_at
                           FROM systemLog sl
                           LEFT JOIN [user] u ON sl.user_id = u.Id
                           INNER JOIN logSeverity ls ON sl.severity_id = ls.Id
                           WHERE 1=1";

            if (!string.IsNullOrEmpty(txtSearch.Text))
                sql += " AND (sl.description LIKE @s OR sl.action_type LIKE @s)";
            if (ddlSeverityFilter.SelectedValue != "All")
                sql += " AND sl.severity_id = @sev";
            if (ddlStatusFilter.SelectedValue != "All")
                sql += " AND sl.status = @stat";

            sql += " ORDER BY sl.created_at " + ddlSort.SelectedValue;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@s", "%" + txtSearch.Text + "%");
                cmd.Parameters.AddWithValue("@sev", ddlSeverityFilter.SelectedValue);
                cmd.Parameters.AddWithValue("@stat", ddlStatusFilter.SelectedValue);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvLogs.DataSource = dt;
                gvLogs.DataBind();
                UpdatePagingLabels(dt.Rows.Count);
            }
        }

        protected void btnConfirmChange_Click(object sender, EventArgs e)
        {
            // Security: Ensure the hidden field value is a valid integer before executing the query
            if (int.TryParse(hfLogId.Value, out int logId))
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE systemLog SET status = @status WHERE Id = @id", con);
                    cmd.Parameters.AddWithValue("@status", hfNewStatus.Value);
                    cmd.Parameters.AddWithValue("@id", logId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                BindLogData();
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "hideModal", "closeLogConfirmModal();", true);
        }

        // --- Standard Pagination/Filter Handlers ---
        protected void Filter_Changed(object sender, EventArgs e) { gvLogs.PageIndex = 0; BindLogData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { gvLogs.PageIndex = 0; BindLogData(); }
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
            if (gvLogs.PageIndex < gvLogs.PageCount - 1)
            {
                gvLogs.PageIndex++;
                BindLogData();
            }
        }

        private void UpdatePagingLabels(int total)
        {
            int start = (gvLogs.PageIndex * gvLogs.PageSize) + 1;
            int end = Math.Min((gvLogs.PageIndex + 1) * gvLogs.PageSize, total);
            lblShowing.Text = total == 0 ? "Showing 0 logs" : $"Showing {start}-{end} of {total} logs";
        }
    }
}