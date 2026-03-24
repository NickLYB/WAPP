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
                Response.Redirect("~/Pages/Guest/Home.aspx");
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

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSortBy.SelectedValue = "ID_ASC";
            ddlFilterRole.SelectedValue = "All";
            ddlFilterStatus.SelectedValue = "All";
            lblMessage.Visible = false;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ClearSearch", "document.getElementById('" + txtSearch.ClientID + "').value = ''; SearchTable();", true);

            gvUsers.PageIndex = 0;
            BindGrid();
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

                if (ddlSortBy.SelectedValue == "ID_ASC") sql += " ORDER BY Id ASC";
                else if (ddlSortBy.SelectedValue == "ID_DESC") sql += " ORDER BY Id DESC";
                else sql += " ORDER BY Id DESC";

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

            int totalPages = (int)Math.Ceiling((double)totalRecords / gvUsers.PageSize);
            if (totalPages == 0) totalPages = 1;

            int startRecord = (gvUsers.PageIndex * gvUsers.PageSize) + 1;
            int endRecord = startRecord + gvUsers.Rows.Count - 1;

            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord} to {endRecord} of {totalRecords} users";
            txtPageJump.Text = (gvUsers.PageIndex + 1).ToString();

            btnPrev.Enabled = gvUsers.PageIndex > 0;
            btnNext.Enabled = gvUsers.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalRecords"] != null ? Convert.ToInt32(ViewState["TotalRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvUsers.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages)
                {
                    gvUsers.PageIndex = pageNum - 1;
                }
                else if (pageNum < 1)
                {
                    gvUsers.PageIndex = 0;
                }
                else
                {
                    gvUsers.PageIndex = totalPages - 1;
                }
            }
            BindGrid();
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
            int totalRecords = ViewState["TotalRecords"] != null ? Convert.ToInt32(ViewState["TotalRecords"]) : 0;
            if (gvUsers.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvUsers.PageSize) - 1))
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

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvUsers, "EditRow$" + e.Row.RowIndex);
            }
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditUser" || e.CommandName == "EditRow")
            {
                int userId = -1;

                if (e.CommandName == "EditRow")
                {
                    int rowIndex = Convert.ToInt32(e.CommandArgument);
                    userId = Convert.ToInt32(gvUsers.DataKeys[rowIndex].Value);
                }
                else
                {
                    userId = Convert.ToInt32(e.CommandArgument);
                }

                LoadUserDataForEdit(userId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openEditModal();", true);
            }
            else if (e.CommandName == "ConfirmToggleStatus")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                hfToggleUserId.Value = userId.ToString();

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT fname, lname, is_locked FROM [user] WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                string fullName = rdr["fname"].ToString() + " " + rdr["lname"].ToString();
                                bool isLocked = Convert.ToBoolean(rdr["is_locked"]);

                                string formattedId = "U" + userId.ToString().PadLeft(3, '0');

                                if (isLocked)
                                {
                                    litLockModalTitle.Text = "Are you sure you want to <b>unlock</b> this user?";
                                    litLockUserDetails.Text = $"<b>{formattedId} - {fullName}</b>";
                                    btnConfirmToggleStatus.Text = "Unlock User";
                                    btnConfirmToggleStatus.CssClass = "btn btn-success px-4 fw-bold rounded-pill";
                                }
                                else
                                {
                                    litLockModalTitle.Text = "Are you sure you want to <b>lock</b> this user?";
                                    litLockUserDetails.Text = $"<b>{formattedId} - {fullName}</b>";
                                    btnConfirmToggleStatus.Text = "Lock User";
                                    btnConfirmToggleStatus.CssClass = "btn btn-danger px-4 fw-bold rounded-pill";
                                }

                                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenLockModal", "openLockModal();", true);
                            }
                        }
                    }
                }
            }
        }

        // NEW: Method to handle the actual database update after confirmation
        protected void btnConfirmToggleStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfToggleUserId.Value)) return;

            try
            {
                int userId = Convert.ToInt32(hfToggleUserId.Value);
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
                lblMessage.Text = "User status updated successfully.";
                lblMessage.CssClass = "alert alert-success d-block";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseLockModal", "closeLockModal();", true);
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
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
            if (string.IsNullOrWhiteSpace(txtEditFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtEditLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEditDOB.Text) ||
                string.IsNullOrWhiteSpace(txtEditPhone.Text) ||
                string.IsNullOrWhiteSpace(txtEditEmail.Text))
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
                    string sql = @"UPDATE [user] SET fname=@fname, lname=@lname, dob=@dob, contact=@contact, email=@email, is_locked=@is_locked WHERE Id=@Id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", hfEditUserId.Value);
                        cmd.Parameters.AddWithValue("@fname", txtEditFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@lname", txtEditLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@dob", txtEditDOB.Text);
                        cmd.Parameters.AddWithValue("@contact", txtEditPhone.Text.Trim()); // NO DASH logic formatting required here anymore.
                        cmd.Parameters.AddWithValue("@email", txtEditEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@is_locked", (ddlEditStatus.SelectedValue == "Locked") ? 1 : 0);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                BindGrid();
                lblMessage.Visible = true;
                lblMessage.Text = "User updated successfully!";
                lblMessage.CssClass = "alert alert-success d-block mb-3";

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
                lblMessage.CssClass = "alert alert-danger d-block mb-3";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openEditModal();", true);
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error updating user: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block mb-3";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openEditModal();", true);
            }
        }

        protected void btnSaveUser_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtDOB.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPass.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please fill out all required fields.";
                lblMessage.CssClass = "alert alert-danger d-block mb-3";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openAddModal();", true);
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
                        cmd.Parameters.AddWithValue("@contact", txtPhone.Text.Trim()); // Raw straight from textbox
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPass.Text.Trim());
                        cmd.Parameters.AddWithValue("@role", selectedRole);
                        cmd.Parameters.AddWithValue("@is_locked", ddlStatus.SelectedValue == "Locked" ? 1 : 0);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                txtFirstName.Text = ""; txtLastName.Text = ""; txtDOB.Text = "";
                txtPhone.Text = ""; txtEmail.Text = ""; txtPass.Text = "";

                BindGrid();

                lblMessage.Visible = true;
                lblMessage.Text = "User added successfully!";
                lblMessage.CssClass = "alert alert-success d-block mb-3";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseAddModal", "closeAddModal();", true);
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
                lblMessage.CssClass = "alert alert-danger d-block mb-3";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openAddModal();", true);
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block mb-3";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openAddModal();", true);
            }
        }
    }
}