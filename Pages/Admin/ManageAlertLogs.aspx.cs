using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Admin
{
    public partial class ManageAlertLogs : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Security Check: Ensure user is logged in AND has role_id 1 (Admin)
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 1)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindAlertData();
            }
        }

        private void BindAlertData()
        {
            // Join systemLog and logSeverity. Target High Severity (>= 2).
            string sql = @"SELECT sl.Id, ls.name AS SeverityName, sl.action_type, sl.description, 
                           sl.status, sl.created_at
                           FROM systemLog sl
                           INNER JOIN logSeverity ls ON sl.severity_id = ls.Id
                           WHERE ls.id >= 2";

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    sql += " AND (sl.description LIKE @s OR sl.action_type LIKE @s)";
                    cmd.Parameters.AddWithValue("@s", "%" + txtSearch.Text.Trim() + "%");
                }

                if (ddlStatusFilter.SelectedValue != "All")
                {
                    sql += " AND sl.status = @stat";
                    cmd.Parameters.AddWithValue("@stat", ddlStatusFilter.SelectedValue);
                }

                sql += " ORDER BY sl.created_at DESC";
                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvAlerts.DataSource = dt;
                gvAlerts.DataBind();

                UpdatePagingLabels(dt.Rows.Count);
            }
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("UPDATE systemLog SET status=@st WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@st", hfNewStatus.Value);
                cmd.Parameters.AddWithValue("@id", hfAlertId.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            BindAlertData();

            // Triggers the updated JavaScript function from the new Bootstrap frontend
            ScriptManager.RegisterStartupScript(this, GetType(), "closeConfirm", "closeConfirmModal();", true);
        }

        protected void Filter_Changed(object sender, EventArgs e) { gvAlerts.PageIndex = 0; BindAlertData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { gvAlerts.PageIndex = 0; BindAlertData(); }
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
            if (gvAlerts.PageIndex < gvAlerts.PageCount - 1)
            {
                gvAlerts.PageIndex++;
                BindAlertData();
            }
        }

        private void UpdatePagingLabels(int total)
        {
            if (total == 0)
            {
                lblShowing.Text = "Showing 0 alerts";
                btnPrev.Enabled = false;
                btnNext.Enabled = false;
                btnPrev.CssClass = "ec-pager-link disabled";
                btnNext.CssClass = "ec-pager-link disabled";
                return;
            }

            int start = (gvAlerts.PageIndex * gvAlerts.PageSize) + 1;
            int end = Math.Min((gvAlerts.PageIndex + 1) * gvAlerts.PageSize, total);
            lblShowing.Text = $"Showing {start}-{end} of {total} alerts";

            // Properly disable pagination buttons to match frontend CSS
            btnPrev.Enabled = gvAlerts.PageIndex > 0;
            btnNext.Enabled = gvAlerts.PageIndex < gvAlerts.PageCount - 1;

            btnPrev.CssClass = btnPrev.Enabled ? "ec-pager-link" : "ec-pager-link disabled";
            btnNext.CssClass = btnNext.Enabled ? "ec-pager-link" : "ec-pager-link disabled";
        }
    }
}