using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Text.RegularExpressions;
using WAPP.Utils; 

namespace WAPP.Pages.Shared
{
    public partial class Settings : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (Session["role_id"] == null)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            int roleId = Convert.ToInt32(Session["role_id"]);
            switch (roleId)
            {
                case 1: this.MasterPageFile = "~/Masters/Admin.Master"; break;
                case 2: this.MasterPageFile = "~/Masters/Staff.Master"; break;
                case 3: this.MasterPageFile = "~/Masters/Tutor.Master"; break;
                case 4: this.MasterPageFile = "~/Masters/Student.Master"; break;
                default: Response.Redirect("~/Pages/Guest/Login.aspx"); break;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] != null && Convert.ToInt32(Session["role_id"]) == 1)
            {
                divDangerZone.Visible = false;
            }
        }

        // --- BULLETPROOF PASSWORD UPDATE ---
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            Page.Validate("PasswordGroup");
            if (!Page.IsValid) return;
            if (Session["UserId"] == null) return;

            int userId = Convert.ToInt32(Session["UserId"]);
            string currentPassTyped = txtCurrentPass.Text.Trim();
            string newPassTyped = txtNewPass.Text.Trim();

            // --- STRONG PASSWORD VALIDATION ---
            if (!IsPasswordStrong(newPassTyped))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "New password does not meet the minimum strength requirements.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            // Instantiate your PasswordManager
            PasswordManager pwdManager = new PasswordManager();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Step 1: Get the actual current password hash from the database
                    string dbPasswordHash = "";
                    string fetchSql = "SELECT password_hash FROM [user] WHERE Id = @Id";
                    using (SqlCommand cmdFetch = new SqlCommand(fetchSql, conn))
                    {
                        cmdFetch.Parameters.AddWithValue("@Id", userId);
                        object result = cmdFetch.ExecuteScalar();
                        if (result != null)
                        {
                            dbPasswordHash = result.ToString();
                        }
                    }

                    // Step 2: Compare what they typed with the database hash using BCrypt
                    if (pwdManager.VerifyPassword(currentPassTyped, dbPasswordHash))
                    {
                        // Step 3: It matches! Hash the new password and update it.
                        string newHashedPass = pwdManager.HashPassword(newPassTyped);

                        string updateSql = "UPDATE [user] SET password_hash = @newPass WHERE Id = @Id";
                        using (SqlCommand cmdUpdate = new SqlCommand(updateSql, conn))
                        {
                            // Save the HASH, not the plain text password
                            cmdUpdate.Parameters.AddWithValue("@newPass", newHashedPass);
                            cmdUpdate.Parameters.AddWithValue("@Id", userId);
                            cmdUpdate.ExecuteNonQuery();

                            // ---> LOGGING ADDED: INFO (Successful password change)
                            SystemLogService.Write("USER_PASSWORD_CHANGED", "User successfully changed their password.", LogLevel.INFO, userId);

                            lblMessage.Visible = true;
                            lblMessage.Text = "Password updated successfully!";
                            lblMessage.CssClass = "alert alert-success d-block";

                            // Clear inputs
                            txtCurrentPass.Text = "";
                            txtNewPass.Text = "";
                            txtConfirmPass.Text = "";
                        }
                    }
                    else
                    {
                        // ---> LOGGING ADDED: WARNING (Possible account takeover attempt)
                        SystemLogService.Write("USER_PASSWORD_CHANGE_FAILED", "Failed password change attempt (Incorrect current password).", LogLevel.WARNING, userId);

                        // It failed. Tell the user EXACTLY why.
                        lblMessage.Visible = true;
                        lblMessage.Text = "The current password you entered is incorrect.";
                        lblMessage.CssClass = "alert alert-danger d-block";
                    }
                }
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR (Database failure during password change)
                SystemLogService.Write("USER_PASSWORD_DB_ERROR", $"Database error during password update: {ex.Message}", LogLevel.ERROR, userId);

                lblMessage.Visible = true;
                lblMessage.Text = "Database Error: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        // --- NEW HELPER METHOD ---
        private bool IsPasswordStrong(string password)
        {
            // Requires at least 8 chars, 1 lowercase, 1 uppercase, 1 number, and 1 special char
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        // --- BULLETPROOF ACCOUNT LOCK ---
        protected void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) return;
            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // This sets is_locked to 1 (True) exactly like your UserManagement page
                    string sql = "UPDATE [user] SET is_locked = 1 WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // ---> LOGGING ADDED: NOTICE (User deactivated their own account)
                            SystemLogService.Write("USER_ACCOUNT_SELF_LOCKED", "User locked/deactivated their own account.", LogLevel.NOTICE, userId);

                            // It worked! Clear the session and kick them to login
                            Session.Clear();
                            Session.Abandon();
                            Response.Redirect("~/Pages/Guest/Home.aspx");
                        }
                        else
                        {
                            // ---> LOGGING ADDED: WARNING (Failed to find user to lock)
                            SystemLogService.Write("USER_ACCOUNT_LOCK_FAILED", "User attempted to self-lock account but DB update affected 0 rows.", LogLevel.WARNING, userId);

                            lblMessage.Visible = true;
                            lblMessage.Text = "Failed to lock account. User not found.";
                            lblMessage.CssClass = "alert alert-warning d-block";
                        }
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Prevent Response.Redirect from triggering a false ERROR log
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR (Database failure during account lock)
                SystemLogService.Write("USER_ACCOUNT_LOCK_ERROR", $"Error self-locking account: {ex.Message}", LogLevel.ERROR, userId);

                lblMessage.Visible = true;
                lblMessage.Text = "Error locking account: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }
    }
}