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
    public partial class ManageUsers : System.Web.UI.Page
    {
        // Global connection string
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Security Check: Ensure user is logged in AND has role_id 1 (Admin)
            // Redirect to ~/Pages/Guest/Home.aspx if unauthorized
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 1)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadRoles(ddlRoleFilter, true);
                LoadRoles(ddlAddRole, false);
                LoadRoles(ddlEditRole, false);
                BindUserData();
            }
        }

        private void LoadRoles(DropDownList ddl, bool addAllOption)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // Exclude Admin (id = 1) from all role dropdowns
                SqlCommand cmd = new SqlCommand("SELECT id, name FROM [role] WHERE id != 1", con);
                con.Open();
                ddl.DataSource = cmd.ExecuteReader();
                ddl.DataTextField = "name";
                ddl.DataValueField = "id";
                ddl.DataBind();
                if (addAllOption) ddl.Items.Insert(0, new ListItem("All Roles", "All"));
            }
        }

        private void BindUserData()
        {
            // Exclude Admin (role_id = 1) from the grid
            string sql = @"SELECT u.Id, u.fname, u.lname, u.dob, u.contact, u.email,
                           r.name AS RoleName, u.is_locked,
                           CASE WHEN u.is_locked = 1 THEN 0 ELSE 1 END AS IsActive,
                           CASE WHEN u.is_locked = 1 THEN 'Locked' ELSE 'Active' END AS StatusText
                           FROM [user] u
                           INNER JOIN [role] r ON u.role_id = r.id
                           WHERE u.role_id != 1";

            // Universal Backend Search
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                sql += @" AND (u.fname LIKE @search OR u.lname LIKE @search OR u.email LIKE @search 
                          OR u.contact LIKE @s OR r.name LIKE @s OR CAST(u.Id AS NVARCHAR) LIKE @s 
                          OR (CASE WHEN u.is_locked = 1 THEN 'Locked' ELSE 'Active' END) LIKE @s)";
            }

            if (ddlRoleFilter.SelectedValue != "All")
                sql += " AND u.role_id = @roleId";
            if (ddlStatusFilter.SelectedValue != "All")
                sql += " AND u.is_locked = @status";

            sql += " ORDER BY u.Id " + ddlSort.SelectedValue;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(sql, con);
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@s", "%" + txtSearch.Text.Trim() + "%");
                }

                if (ddlRoleFilter.SelectedValue != "All")
                    cmd.Parameters.AddWithValue("@roleId", ddlRoleFilter.SelectedValue);

                if (ddlStatusFilter.SelectedValue != "All")
                    cmd.Parameters.AddWithValue("@status", ddlStatusFilter.SelectedValue);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ViewState["TotalUserRecords"] = dt.Rows.Count;

                gvUsers.DataSource = dt;
                gvUsers.DataBind();
                UpdatePagingLabels(dt.Rows.Count);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSort.SelectedIndex = 0;
            ddlRoleFilter.SelectedIndex = 0;
            ddlStatusFilter.SelectedIndex = 0;
            lblMessage.Visible = false;
            gvUsers.PageIndex = 0;

            BindUserData();
            upPanelUsers.Update();
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
            lblMessage.Visible = false;

            if (e.CommandName == "EditUser" || e.CommandName == "EditRow")
            {
                int rowIndex = (e.CommandName == "EditRow") ? Convert.ToInt32(e.CommandArgument) : -1;
                int userId = (e.CommandName == "EditRow") ? Convert.ToInt32(gvUsers.DataKeys[rowIndex].Value) : Convert.ToInt32(e.CommandArgument);

                PopulateEditModal(userId);
                ScriptManager.RegisterStartupScript(this, GetType(), "showEdit", "openEditModal();", true);
            }
        }

        private void PopulateEditModal(int userId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = "SELECT fname, lname, email, contact, dob, role_id, is_locked FROM [user] WHERE Id = @id";
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

                    if (dr["dob"] != DBNull.Value)
                        txtEditDob.Text = Convert.ToDateTime(dr["dob"]).ToString("yyyy-MM-dd");
                    else
                        txtEditDob.Text = "";

                    ddlEditRole.SelectedValue = dr["role_id"].ToString();

                    bool isLocked = Convert.ToBoolean(dr["is_locked"]);
                    ddlEditStatus.SelectedValue = isLocked ? "Locked" : "Active";

                    txtEditPass.Text = "";
                }
            }
            upPanelUsers.Update();
        }

        protected void btnUpdateUser_Click(object sender, EventArgs e)
        {
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
            string targetUserId = hfEditUserId.Value;

            try
            {
                int oldRoleId = 0;
                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("SELECT role_id FROM [user] WHERE Id = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", targetUserId);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null) oldRoleId = Convert.ToInt32(result);
                }

                int newRoleId = Convert.ToInt32(ddlEditRole.SelectedValue);
                int isLocked = ddlEditStatus.SelectedValue == "Locked" ? 1 : 0;
                string newPassword = txtEditPass.Text.Trim();

                string sql = "UPDATE [user] SET fname=@f, lname=@l, email=@e, contact=@p, dob=@d, role_id=@r, is_locked=@lck";

                if (!string.IsNullOrEmpty(newPassword))
                {
                    sql += ", password_hash=@pw";
                }

                sql += " WHERE Id=@id";

                DateTime? dob = null;
                if (DateTime.TryParse(txtEditDob.Text.Trim(), out DateTime parsedDob))
                    dob = parsedDob;

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@f", txtEditFname.Text.Trim()),
                    new SqlParameter("@l", txtEditLname.Text.Trim()),
                    new SqlParameter("@e", txtEditEmail.Text.Trim()),
                    new SqlParameter("@p", txtEditPhone.Text.Trim()),
                    new SqlParameter("@d", dob.HasValue ? (object)dob.Value : DBNull.Value),
                    new SqlParameter("@r", newRoleId),
                    new SqlParameter("@lck", isLocked),
                    new SqlParameter("@id", targetUserId)
                };

                if (!string.IsNullOrEmpty(newPassword))
                {
                    PasswordManager pwdManager = new PasswordManager();
                    parameters.Add(new SqlParameter("@pw", pwdManager.HashPassword(newPassword)));
                }

                ExecuteNonQuery(sql, parameters.ToArray());

                if (oldRoleId != 0 && oldRoleId != newRoleId)
                {
                    SystemLogService.Write("ADMIN_ROLE_CHANGED",
                        $"Admin changed role for User ID {targetUserId} from Role ID {oldRoleId} to Role ID {newRoleId}.",
                        LogLevel.WARNING, adminId);
                }

                SystemLogService.Write("ADMIN_USER_UPDATED", $"Admin updated profile details for User ID {targetUserId}.", LogLevel.NOTICE, adminId);

                BindUserData();
                ShowMessage("User updated successfully!", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "cEdit", "closeEditModal();", true);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    ShowMessage("Cannot update: The email address is already registered.", false);
                else
                    ShowMessage("Database Error: " + sqlEx.Message, false);

                ScriptManager.RegisterStartupScript(this, GetType(), "ReopenEditErr", "openEditModal();", true);
            }
            catch (Exception ex)
            {
                SystemLogService.Write("ADMIN_UPDATE_ERROR", $"DB Error updating User ID {targetUserId}: {ex.Message}", LogLevel.ERROR, adminId);
                ShowMessage("An error occurred while updating the user.", false);
                ScriptManager.RegisterStartupScript(this, GetType(), "ReopenEditErr", "openEditModal();", true);
            }
        }

        protected void btnSaveUser_Click(object sender, EventArgs e)
        {
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
            int selectedRole = Convert.ToInt32(ddlAddRole.SelectedValue);
            int isLocked = ddlStatus.SelectedValue == "Locked" ? 1 : 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            int newUserId = 0;

                            string sqlUser = @"
                                INSERT INTO [user] (fname, lname, email, contact, dob, password_hash, role_id, is_locked) 
                                VALUES (@f, @l, @e, @p, @d, @pw, @r, @lck);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            using (SqlCommand cmd = new SqlCommand(sqlUser, conn, trans))
                            {
                                cmd.Parameters.Add("@f", SqlDbType.NVarChar, 50).Value = txtAddFname.Text.Trim();
                                cmd.Parameters.Add("@l", SqlDbType.NVarChar, 50).Value = txtAddLname.Text.Trim();
                                cmd.Parameters.Add("@e", SqlDbType.NVarChar, 100).Value = txtAddEmail.Text.Trim();
                                cmd.Parameters.Add("@p", SqlDbType.NVarChar, 15).Value = txtAddPhone.Text.Trim();

                                DateTime dob;
                                if (DateTime.TryParse(txtAddDob.Text.Trim(), out dob))
                                    cmd.Parameters.Add("@d", SqlDbType.Date).Value = dob;
                                else
                                    cmd.Parameters.Add("@d", SqlDbType.Date).Value = DBNull.Value;

                                PasswordManager pwdManager = new PasswordManager();
                                string hashedPassword = pwdManager.HashPassword(txtAddPassword.Text.Trim());

                                cmd.Parameters.Add("@pw", SqlDbType.NVarChar).Value = hashedPassword;
                                cmd.Parameters.Add("@r", SqlDbType.Int).Value = selectedRole;
                                cmd.Parameters.Add("@lck", SqlDbType.TinyInt).Value = isLocked;

                                newUserId = (int)cmd.ExecuteScalar();
                            }

                            if (selectedRole == 3 && newUserId > 0)
                            {
                                string sqlApp = @"
                                    INSERT INTO [tutorApplication] 
                                    (tutor_id, verification_document, reviewed_by, submitted_at, verified_at, status)
                                    VALUES 
                                    (@tutor_id, @doc, @reviewer, GETDATE(), GETDATE(), 'APPROVED')";

                                using (SqlCommand appCmd = new SqlCommand(sqlApp, conn, trans))
                                {
                                    appCmd.Parameters.Add("@tutor_id", SqlDbType.Int).Value = newUserId;
                                    appCmd.Parameters.Add("@doc", SqlDbType.NVarChar).Value = "Manually Created & Verified by Admin";
                                    appCmd.Parameters.Add("@reviewer", SqlDbType.Int).Value = adminId;

                                    appCmd.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }

                SystemLogService.Write("ADMIN_USER_CREATED",
                    $"Admin manually created a new account for {txtAddEmail.Text} with Role ID {selectedRole}.",
                    LogLevel.INFO, adminId);

                txtAddFname.Text = string.Empty;
                txtAddLname.Text = string.Empty;
                txtAddEmail.Text = string.Empty;
                txtAddPhone.Text = string.Empty;
                txtAddDob.Text = string.Empty;
                txtAddPassword.Text = string.Empty;
                ddlAddRole.SelectedIndex = 0;
                ddlStatus.SelectedIndex = 0;

                BindUserData();
                ShowMessage("User added successfully!", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "cAdd", "closeAddModal();", true);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    ShowMessage("Cannot add user: The email address is already registered.", false);
                else
                {
                    SystemLogService.Write("ADMIN_CREATE_ERROR", $"DB Error creating user {txtAddEmail.Text}: {sqlEx.Message}", LogLevel.ERROR, adminId);
                    ShowMessage("Database Error: " + sqlEx.Message, false);
                }
                ScriptManager.RegisterStartupScript(this, GetType(), "ReopenAddErr", "openAddModal();", true);
            }
            catch (Exception ex)
            {
                SystemLogService.Write("ADMIN_CREATE_ERROR", $"Error creating user {txtAddEmail.Text}: {ex.Message}", LogLevel.ERROR, adminId);
                ShowMessage("Error: " + ex.Message, false);
                ScriptManager.RegisterStartupScript(this, GetType(), "ReopenAddErr", "openAddModal();", true);
            }
        }

        protected void btnConfirmStatusChange_Click(object sender, EventArgs e)
        {
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;

            try
            {
                if (int.TryParse(hfConfirmUserId.Value, out int targetUserId) && int.TryParse(hfConfirmNewStatus.Value, out int newIsLocked))
                {
                    string sql = "UPDATE [user] SET is_locked = @stat WHERE Id = @id";
                    ExecuteNonQuery(sql, new SqlParameter("@stat", newIsLocked), new SqlParameter("@id", targetUserId));

                    string actionText = newIsLocked == 1 ? "LOCKED" : "UNLOCKED";
                    SystemLogService.Write($"ADMIN_{actionText}_USER",
                        $"Admin {actionText.ToLower()} account for User ID {targetUserId}.",
                        LogLevel.NOTICE, adminId);

                    BindUserData();
                    ShowMessage($"User account successfully {actionText.ToLower()}.", true);
                }
                ScriptManager.RegisterStartupScript(this, GetType(), "cStat", "closeStatusConfirmModal();", true);
            }
            catch (Exception ex)
            {
                SystemLogService.Write("ADMIN_STATUS_ERROR", $"DB Error changing lock status: {ex.Message}", LogLevel.ERROR, adminId);
                ShowMessage("Failed to change user status.", false);
            }
        }

        protected void btnDeleteUser_Click(object sender, EventArgs e)
        {
            int adminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
            string targetUserId = hfEditUserId.Value;

            if (string.IsNullOrWhiteSpace(targetUserId)) return;

            try
            {
                string sql = "DELETE FROM [user] WHERE Id = @id";
                ExecuteNonQuery(sql, new SqlParameter("@id", targetUserId));

                SystemLogService.Write("ADMIN_USER_DELETED", $"Admin permanently deleted User ID {targetUserId}.", LogLevel.WARNING, adminId);

                BindUserData();
                ShowMessage("User permanently deleted.", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "cDel", "closeDeleteConfirmModal(); closeEditModal();", true);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 547)
                {
                    ShowMessage("Cannot delete this user because they have associated records (e.g., courses, enrollments, or payments). Consider locking the account instead.", false);
                }
                else
                {
                    ShowMessage("Database Error: " + sqlEx.Message, false);
                }
                ScriptManager.RegisterStartupScript(this, GetType(), "ReopenDelErr", "closeDeleteConfirmModal(); openEditModal();", true);
            }
            catch (Exception ex)
            {
                SystemLogService.Write("ADMIN_DELETE_ERROR", $"Error deleting User ID {targetUserId}: {ex.Message}", LogLevel.ERROR, adminId);
                ShowMessage("An error occurred while deleting the user.", false);
                ScriptManager.RegisterStartupScript(this, GetType(), "ReopenDelErr", "closeDeleteConfirmModal(); openEditModal();", true);
            }
        }

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

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = isSuccess ? "alert alert-success d-block fw-bold mb-4" : "alert alert-danger d-block fw-bold mb-4";
            upPanelUsers.Update();
        }

        protected void Filter_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvUsers.PageIndex = 0; BindUserData(); }
        protected void BtnSearch_Click(object sender, EventArgs e) { lblMessage.Visible = false; gvUsers.PageIndex = 0; BindUserData(); }
        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvUsers.PageIndex = e.NewPageIndex; BindUserData(); }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvUsers.PageIndex > 0)
            {
                gvUsers.PageIndex--;
                BindUserData();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalUserRecords"] != null ? Convert.ToInt32(ViewState["TotalUserRecords"]) : 0;
            if (gvUsers.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvUsers.PageSize) - 1))
            {
                gvUsers.PageIndex++;
                BindUserData();
            }
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalUserRecords"] != null ? Convert.ToInt32(ViewState["TotalUserRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvUsers.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvUsers.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvUsers.PageIndex = 0;
                else gvUsers.PageIndex = totalPages - 1;
            }
            BindUserData();
        }

        private void UpdatePagingLabels(int total)
        {
            if (total == 0)
            {
                lblShowing.Text = "Showing 0 users";
                btnPrev.Enabled = false;
                btnNext.Enabled = false;
                btnPrev.CssClass = "ec-pager-link disabled";
                btnNext.CssClass = "ec-pager-link disabled";
                txtPageJump.Text = "1";
                return;
            }

            int start = (gvUsers.PageIndex * gvUsers.PageSize) + 1;
            int end = Math.Min((gvUsers.PageIndex + 1) * gvUsers.PageSize, total);
            lblShowing.Text = $"Showing {start}-{end} of {total} users";
            txtPageJump.Text = (gvUsers.PageIndex + 1).ToString();

            int totalPages = (int)Math.Ceiling((double)total / gvUsers.PageSize);
            btnPrev.Enabled = gvUsers.PageIndex > 0;
            btnNext.Enabled = gvUsers.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "ec-pager-link" : "ec-pager-link disabled";
            btnNext.CssClass = btnNext.Enabled ? "ec-pager-link" : "ec-pager-link disabled";
        }
    }
}