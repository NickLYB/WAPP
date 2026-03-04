using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Admin
{
    public partial class ManageAnnouncements : System.Web.UI.Page
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

            // 2. Load Data on Initial Visit
            if (!IsPostBack)
            {
                LoadRoles();
                BindData();
            }
        }

        private void LoadRoles()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT id, name FROM [role]", con);
                con.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                // Populate Filter Dropdown
                ddlTargetFilter.DataSource = dt;
                ddlTargetFilter.DataTextField = "name";
                ddlTargetFilter.DataValueField = "id";
                ddlTargetFilter.DataBind();
                ddlTargetFilter.Items.Insert(0, new ListItem("All Roles", "All"));

                // Populate Compose Radio Buttons
                rblTarget.DataSource = dt;
                rblTarget.DataTextField = "name";
                rblTarget.DataValueField = "id";
                rblTarget.DataBind();
                rblTarget.Items.Insert(0, new ListItem("Broadcast (All)", "0")); // 0 for broadcast logic
                rblTarget.SelectedIndex = 0;
            }
        }

        private void BindData()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // Note: Make sure "created_by" exists in your DB or remove it if not needed.
                // Assuming Admin role ID is 1, or that Admin can see all announcements.
                string sql = @"SELECT a.Id, a.title, a.message, ISNULL(r.name, 'Broadcast (All)') AS RoleName, 
                               a.created_at, a.status
                               FROM announcement a
                               LEFT JOIN [role] r ON a.target_role_id = r.Id
                               WHERE 1=1";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;

                    if (!string.IsNullOrEmpty(txtSearch.Text))
                    {
                        sql += " AND (a.title LIKE @s OR a.message LIKE @s)";
                        cmd.Parameters.AddWithValue("@s", "%" + txtSearch.Text.Trim() + "%");
                    }

                    if (ddlTargetFilter.SelectedValue != "All")
                    {
                        sql += " AND a.target_role_id = @target";
                        cmd.Parameters.AddWithValue("@target", ddlTargetFilter.SelectedValue);
                    }

                    if (ddlStatusFilter.SelectedValue != "All")
                    {
                        sql += " AND a.status = @status";
                        cmd.Parameters.AddWithValue("@status", ddlStatusFilter.SelectedValue);
                    }

                    // Append order by at the very end
                    sql += " ORDER BY a.created_at DESC"; // Defaulting to DESC for recent first

                    cmd.CommandText = sql;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvAnnouncements.DataSource = dt;
                    gvAnnouncements.DataBind();

                    UpdatePagingLabels(dt.Rows.Count);
                }
            }
        }

        protected void btnSendNow_Click(object sender, EventArgs e)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('Title and Message are required.');", true);
                return;
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                // Added created_by so the system knows who posted it
                string sql = @"INSERT INTO announcement (title, message, target_role_id, status, created_by, created_at) 
                               VALUES (@t, @m, @tg, 'ACTIVE', @cb, @cd)";
                SqlCommand cmd = new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@t", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@m", txtMessage.Text.Trim());
                cmd.Parameters.AddWithValue("@cb", Convert.ToInt32(Session["UserId"])); // Admin's ID
                cmd.Parameters.AddWithValue("@cd", DateTime.Now);

                // If "Broadcast (All)" is selected (value 0), insert NULL into DB
                if (rblTarget.SelectedValue == "0")
                {
                    cmd.Parameters.AddWithValue("@tg", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@tg", rblTarget.SelectedValue);
                }

                con.Open();
                cmd.ExecuteNonQuery();
            }

            // Clear the form inputs after successful insertion
            txtTitle.Text = string.Empty;
            txtMessage.Text = string.Empty;
            rblTarget.SelectedIndex = 0;

            BindData();

            // Trigger the new Bootstrap modal closing script
            ScriptManager.RegisterStartupScript(this, GetType(), "closeCompose", "closeComposeModal();", true);
        }

        protected void Filter_Changed(object sender, EventArgs e) { gvAnnouncements.PageIndex = 0; BindData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { gvAnnouncements.PageIndex = 0; BindData(); }
        protected void gvAnnouncements_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvAnnouncements.PageIndex = e.NewPageIndex; BindData(); }

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
            if (gvAnnouncements.PageIndex < gvAnnouncements.PageCount - 1)
            {
                gvAnnouncements.PageIndex++;
                BindData();
            }
        }

        private void UpdatePagingLabels(int total)
        {
            if (total == 0)
            {
                lblShowing.Text = "Showing 0 announcements";
                btnPrev.Enabled = false;
                btnNext.Enabled = false;
                btnPrev.CssClass = "ec-pager-link disabled";
                btnNext.CssClass = "ec-pager-link disabled";
                return;
            }

            int start = (gvAnnouncements.PageIndex * gvAnnouncements.PageSize) + 1;
            int end = Math.Min((gvAnnouncements.PageIndex + 1) * gvAnnouncements.PageSize, total);
            lblShowing.Text = $"Showing {start}-{end} of {total} announcements";

            btnPrev.Enabled = gvAnnouncements.PageIndex > 0;
            btnNext.Enabled = gvAnnouncements.PageIndex < gvAnnouncements.PageCount - 1;

            btnPrev.CssClass = btnPrev.Enabled ? "ec-pager-link" : "ec-pager-link disabled";
            btnNext.CssClass = btnNext.Enabled ? "ec-pager-link" : "ec-pager-link disabled";
        }
    }
}