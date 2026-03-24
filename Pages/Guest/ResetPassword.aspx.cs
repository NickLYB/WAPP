using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions; 
using WAPP.Utils;

namespace WAPP.Pages.Guest
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // SECURITY CHECK: Ensure they actually passed the OTP phase
            // If they just type the URL manually, kick them out to Home.
            if (Session["ResetEmail"] == null)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowMessage("Please fill in both fields.", false);
                return;
            }

            if (!IsPasswordStrong(newPassword))
            {
                ShowMessage("Password does not meet the minimum strength requirements.", false);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowMessage("Passwords do not match.", false);
                return;
            }

            // 2. Hash the new password
            string targetEmail = Session["ResetEmail"].ToString();
            PasswordManager pwdManager = new PasswordManager();
            string newHashedPassword = pwdManager.HashPassword(newPassword);

            // 3. Update Database
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Get the User ID first so we can log it properly
                    int userId = 0;
                    string getIdQuery = "SELECT Id FROM [user] WHERE email = @email";
                    using (SqlCommand cmdId = new SqlCommand(getIdQuery, conn))
                    {
                        cmdId.Parameters.AddWithValue("@email", targetEmail);
                        object result = cmdId.ExecuteScalar();
                        if (result != null) userId = Convert.ToInt32(result);
                    }

                    if (userId > 0)
                    {
                        // Update the password
                        string updateQuery = "UPDATE [user] SET password_hash = @hash WHERE email = @email";
                        using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn))
                        {
                            cmdUpdate.Parameters.AddWithValue("@hash", newHashedPassword);
                            cmdUpdate.Parameters.AddWithValue("@email", targetEmail);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        // ---> LOGGING ADDED: INFO (Password Successfully Changed)
                        SystemLogService.Write("USER_PASSWORD_CHANGED",
                            "User successfully reset their password via OTP.",
                            LogLevel.INFO, userId);

                        // Clear the session so they can't reuse this page
                        Session.Remove("ResetEmail");

                        // Show success message and hide the textboxes
                        ShowMessage("Password updated successfully! You can now log in.", true);
                        txtNewPassword.Visible = false;
                        txtConfirmPassword.Visible = false;
                        btnResetPassword.Visible = false;
                    }
                    else
                    {
                        ShowMessage("Account not found.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR
                SystemLogService.Write("PASSWORD_RESET_ERROR",
                    $"DB Error resetting password for {targetEmail}: {ex.Message}",
                    LogLevel.ERROR);

                ShowMessage("An error occurred while updating your password. Please try again.", false);
            }
        }

        private bool IsPasswordStrong(string password)
        {
            // Requires at least 8 chars, 1 lowercase, 1 uppercase, 1 number, and 1 special char
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        private void ShowMessage(string msg, bool isSuccess)
        {
            lblMessage.Text = msg;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}