using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Staff
{
    public partial class UserManagement : System.Web.UI.Page
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
                string passedRole = Request.QueryString["role"];
                if (passedRole == "4" || passedRole == "student") ddlFilterRole.SelectedValue = "4";
                else if (passedRole == "3" || passedRole == "tutor") ddlFilterRole.SelectedValue = "3";
                else ddlFilterRole.SelectedValue = "All";

                ddlFilterStatus.SelectedValue = "All";
                BindGrid();
            }
        }

        protected string GetRoleName(object roleIdObj)
        {
            if (roleIdObj == null || roleIdObj == DBNull.Value) return "Unknown";
            int roleId = Convert.ToInt32(roleIdObj);
            switch (roleId) { case 3: return "Tutor"; case 4: return "Student"; default: return "Unknown"; }
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT Id, fname, lname, dob, contact, email, password_hash, 
                               ISNULL(is_locked, 0) as is_locked, 
                               ISNULL(role_id, 4) as role_id
                               FROM [user] 
                               WHERE role_id IN (3, 4)";

                string roleFilter = ddlFilterRole.SelectedValue;
                if (roleFilter != "All") sql += " AND role_id = @RoleFilter";

                string statusFilter = ddlFilterStatus.SelectedValue;
                if (statusFilter != "All") sql += " AND is_locked = @StatusFilter";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (roleFilter != "All") cmd.Parameters.AddWithValue("@RoleFilter", roleFilter);
                    if (statusFilter != "All") cmd.Parameters.AddWithValue("@StatusFilter", statusFilter);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        ViewState["TotalRecords"] = dt.Rows.Count;

                        gvUsers.DataSource = dt;
                        gvUsers.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalRecords"] != null ? Convert.ToInt32(ViewState["TotalRecords"]) : 0;
            int startRecord = (gvUsers.PageIndex * gvUsers.PageSize) + 1;
            int endRecord = startRecord + gvUsers.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} users";

            btnPrev.Enabled = gvUsers.PageIndex > 0;
            btnPrev.CssClass = btnPrev.Enabled ? "pager-link" : "pager-link disabled";

            btnNext.Enabled = gvUsers.PageIndex < (gvUsers.PageCount - 1);
            btnNext.CssClass = btnNext.Enabled ? "pager-link" : "pager-link disabled";
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvUsers.PageIndex > 0)
            {
                gvUsers.PageIndex--;
                BindGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (gvUsers.PageIndex < gvUsers.PageCount - 1)
            {
                gvUsers.PageIndex++;
                BindGrid();
            }
        }

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvUsers.PageIndex = 0;
            BindGrid();
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditUser")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                LoadUserDataForEdit(userId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openEditModal();", true);
            }
            else if (e.CommandName == "ToggleStatus")
            {
                try
                {
                    int userId = Convert.ToInt32(e.CommandArgument);
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string sql = "UPDATE [user] SET is_locked = CASE WHEN is_locked = 1 THEN 0 ELSE 1 END WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", userId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    BindGrid();
                    lblMessage.Visible = true;
                    lblMessage.Text = "User status updated.";
                    lblMessage.CssClass = "alert alert-success d-block";
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Error: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
        }

        private void LoadUserDataForEdit(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT * FROM [user] WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            hfEditUserId.Value = rdr["Id"].ToString();
                            txtEditID.Text = rdr["Id"].ToString();
                            txtEditFirstName.Text = rdr["fname"].ToString();
                            txtEditLastName.Text = rdr["lname"].ToString();
                            txtEditEmail.Text = rdr["email"].ToString();
                            txtEditPhone.Text = rdr["contact"].ToString();
                            if (rdr["dob"] != DBNull.Value) txtEditDOB.Text = Convert.ToDateTime(rdr["dob"]).ToString("yyyy-MM-dd");
                            txtEditPass.Text = rdr["password_hash"].ToString();
                            ddlEditRole.SelectedValue = rdr["role_id"].ToString();
                            bool isLocked = Convert.ToBoolean(rdr["is_locked"]);
                            ddlEditStatus.SelectedValue = isLocked ? "Locked" : "Active";
                        }
                    }
                }
            }
        }

        protected void btnUpdateUser_Click(object sender, EventArgs e)
        {
            // 1. Manual Validation Check
            if (string.IsNullOrWhiteSpace(txtEditFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtEditLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEditDOB.Text) ||
                string.IsNullOrWhiteSpace(txtEditPhone.Text) ||
                string.IsNullOrWhiteSpace(txtEditEmail.Text) ||
                string.IsNullOrWhiteSpace(txtEditPass.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please fill out all required fields.";
                lblMessage.CssClass = "alert alert-danger d-block";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openEditModal();", true);
                return;
            }

            if (string.IsNullOrEmpty(hfEditUserId.Value)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"UPDATE [user] SET fname=@fname, lname=@lname, dob=@dob, contact=@contact, email=@email, is_locked=@is_locked, password_hash=@pass WHERE Id=@Id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", hfEditUserId.Value);
                        cmd.Parameters.AddWithValue("@fname", txtEditFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@lname", txtEditLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@dob", txtEditDOB.Text);
                        cmd.Parameters.AddWithValue("@contact", txtEditPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEditEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtEditPass.Text.Trim());
                        cmd.Parameters.AddWithValue("@is_locked", (ddlEditStatus.SelectedValue == "Locked") ? 1 : 0);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                BindGrid();
                lblMessage.Visible = true;
                lblMessage.Text = "User updated successfully!";
                lblMessage.CssClass = "alert alert-success d-block";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeEditModal();", true);
            }
            catch (SqlException sqlEx)
            {
                lblMessage.Visible = true;
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    lblMessage.Text = "Cannot save: The email address entered is already registered to another user.";
                }
                else
                {
                    lblMessage.Text = "Database Error: " + sqlEx.Message;
                }
                lblMessage.CssClass = "alert alert-danger d-block";
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error updating user: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        protected void btnSaveUser_Click(object sender, EventArgs e)
        {
            // 1. Manual Validation Check
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtDOB.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPass.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please fill out all required fields.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            int selectedRole = Convert.ToInt32(ddlRole.SelectedValue);
            if (selectedRole == 1 || selectedRole == 2) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"INSERT INTO [user] (fname, lname, dob, contact, email, password_hash, role_id, is_locked) VALUES (@fname, @lname, @dob, @contact, @email, @pass, @role, @is_locked)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@dob", txtDOB.Text);
                        cmd.Parameters.AddWithValue("@contact", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPass.Text.Trim());
                        cmd.Parameters.AddWithValue("@role", selectedRole);
                        cmd.Parameters.AddWithValue("@is_locked", ddlStatus.SelectedValue == "Locked" ? 1 : 0);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                lblMessage.Visible = true;
                lblMessage.Text = "User added successfully!";
                lblMessage.CssClass = "alert alert-success d-block";

                txtFirstName.Text = ""; txtLastName.Text = ""; txtDOB.Text = "";
                txtPhone.Text = ""; txtEmail.Text = ""; txtPass.Text = "";

                BindGrid();

                ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseAddModal", "bootstrap.Modal.getOrCreateInstance(document.getElementById('addUserModal')).hide(); $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');", true);
            }
            catch (SqlException sqlEx)
            {
                lblMessage.Visible = true;
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    lblMessage.Text = "Cannot add user: The email address entered is already registered to another user.";
                }
                else
                {
                    lblMessage.Text = "Database Error: " + sqlEx.Message;
                }
                lblMessage.CssClass = "alert alert-danger d-block";
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }
    }
}