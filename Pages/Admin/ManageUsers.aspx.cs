using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Admin
{
    public partial class ManageUsers : System.Web.UI.Page
    {
        // Global connection string
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
                    LoadRoles(ddlRoleFilter, true);
                    LoadRoles(ddlAddRole, false);
                    LoadRoles(ddlEditRole, false);
                    BindUserData();
                }
            }
        }

        private void LoadRoles(DropDownList ddl, bool addAllOption)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT id, name FROM [role]", con);
                con.Open();
                ddl.DataSource = cmd.ExecuteReader();
                ddl.DataTextField = "name";
                ddl.DataValueField = "id";
                ddl.DataBind();
                if (addAllOption) ddl.Items.Insert(0, new ListItem("All", "All"));
            }
        }

        private void BindUserData()
        {
            // JOIN user and role tables. Escaping [user] is mandatory.
            string sql = @"SELECT u.Id, u.fname, u.lname, u.dob, u.contact, u.email, u.password_hash,
                           r.name AS RoleName, u.is_locked,
                           CASE WHEN u.is_locked = 1 THEN 0 ELSE 1 END AS IsActive,
                           CASE WHEN u.is_locked = 1 THEN 'Locked' ELSE 'Active' END AS StatusText
                           FROM [user] u
                           INNER JOIN [role] r ON u.role_id = r.id
                           WHERE 1=1";

            if (!string.IsNullOrEmpty(txtSearch.Text))
                sql += " AND (u.fname LIKE @search OR u.email LIKE @search)";
            if (ddlRoleFilter.SelectedValue != "All")
                sql += " AND u.role_id = @roleId";
            if (ddlStatusFilter.SelectedValue != "All")
                sql += " AND u.is_locked = @status";

            sql += " ORDER BY u.Id " + ddlSort.SelectedValue;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text + "%");
                cmd.Parameters.AddWithValue("@roleId", ddlRoleFilter.SelectedValue);
                cmd.Parameters.AddWithValue("@status", ddlStatusFilter.SelectedValue);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
                UpdatePagingLabels(dt.Rows.Count);
            }
        }

        // --- Row Command (Edit User) ---
        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditUser")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                PopulateEditModal(userId);
                // Force modal to show after data is loaded
                ScriptManager.RegisterStartupScript(this, GetType(), "showEdit", "openEditModal();", true);
            }
        }

        private void PopulateEditModal(int userId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = "SELECT fname, lname, email, contact, dob, role_id FROM [user] WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", userId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    hfEditUserId.Value = userId.ToString();
                    txtEditFname.Text = dr["fname"].ToString();
                    txtEditLname.Text = dr["lname"].ToString();
                    txtEditEmail.Text = dr["email"].ToString();
                    txtEditPhone.Text = dr["contact"].ToString();
                    txtEditDob.Text = Convert.ToDateTime(dr["dob"]).ToString("yyyy-MM-dd");
                    ddlEditRole.SelectedValue = dr["role_id"].ToString();
                }
            }
        }

        // --- Actions (Update/Save/Toggle) ---
        protected void btnUpdateUser_Click(object sender, EventArgs e)
        {
            string sql = "UPDATE [user] SET fname=@f, lname=@l, email=@e, contact=@p, dob=@d, role_id=@r WHERE Id=@id";
            SqlParameter[] p = {
                new SqlParameter("@f", txtEditFname.Text), new SqlParameter("@l", txtEditLname.Text),
                new SqlParameter("@e", txtEditEmail.Text), new SqlParameter("@p", txtEditPhone.Text),
                new SqlParameter("@d", txtEditDob.Text), new SqlParameter("@r", ddlEditRole.SelectedValue),
                new SqlParameter("@id", hfEditUserId.Value)
            };
            ExecuteNonQuery(sql, p);
            BindUserData();
            ScriptManager.RegisterStartupScript(this, GetType(), "cEdit", "closeEditModal();", true);
        }

        protected void btnSaveUser_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO [user] (fname, lname, email, contact, dob, password_hash, role_id) VALUES (@f, @l, @e, @p, @d, @pw, @r)";
            SqlParameter[] p = {
                new SqlParameter("@f", txtAddFname.Text), new SqlParameter("@l", txtAddLname.Text),
                new SqlParameter("@e", txtAddEmail.Text), new SqlParameter("@p", txtAddPhone.Text),
                new SqlParameter("@d", txtAddDob.Text), new SqlParameter("@pw", txtAddPassword.Text),
                new SqlParameter("@r", ddlAddRole.SelectedValue)
            };
            ExecuteNonQuery(sql, p);

            // UX Fix: Clear the inputs so the modal is empty next time it opens
            txtAddFname.Text = string.Empty;
            txtAddLname.Text = string.Empty;
            txtAddEmail.Text = string.Empty;
            txtAddPhone.Text = string.Empty;
            txtAddDob.Text = string.Empty;
            txtAddPassword.Text = string.Empty;
            ddlAddRole.SelectedIndex = 0;

            BindUserData();
            ScriptManager.RegisterStartupScript(this, GetType(), "cAdd", "closeAddModal();", true);
        }

        protected void btnConfirmStatusChange_Click(object sender, EventArgs e)
        {
            // Security: Safe parsing of the hidden field values
            if (int.TryParse(hfConfirmUserId.Value, out int userId) && int.TryParse(hfConfirmNewStatus.Value, out int newIsLocked))
            {
                string sql = "UPDATE [user] SET is_locked = @stat WHERE Id = @id";
                ExecuteNonQuery(sql, new SqlParameter("@stat", newIsLocked), new SqlParameter("@id", userId));
                BindUserData();
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "cStat", "closeStatusConfirmModal();", true);
        }

        // --- Helper Methods ---
        private void ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(parameters);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        protected void Filter_Changed(object sender, EventArgs e) { gvUsers.PageIndex = 0; BindUserData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { gvUsers.PageIndex = 0; BindUserData(); }
        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvUsers.PageIndex = e.NewPageIndex; BindUserData(); }
        protected void btnPrev_Click(object sender, EventArgs e) { if (gvUsers.PageIndex > 0) { gvUsers.PageIndex--; BindUserData(); } }
        protected void btnNext_Click(object sender, EventArgs e) { if (gvUsers.PageIndex < gvUsers.PageCount - 1) { gvUsers.PageIndex++; BindUserData(); } }

        private void UpdatePagingLabels(int total)
        {
            int start = (gvUsers.PageIndex * gvUsers.PageSize) + 1;
            int end = Math.Min((gvUsers.PageIndex + 1) * gvUsers.PageSize, total);
            lblShowing.Text = total == 0 ? "Showing 0 users" : $"Showing {start}-{end} of {total} users";
        }
    }
}