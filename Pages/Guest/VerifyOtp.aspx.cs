using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using WAPP.Utils;

namespace WAPP.Pages.Guest
{
    public partial class VerifyOtp : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        string pendingEmail = "";
        string nextUrl = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["email"] != null && Request.QueryString["next"] != null)
            {
                pendingEmail = Request.QueryString["email"];
                nextUrl = Request.QueryString["next"];
            }
            else
            {
                // ---> LOGGING ADDED: WARNING (Attempted access without required parameters)
                SystemLogService.Write("OTP_VERIFY_BYPASS", "Attempted OTP verify page access without email/next parameters.", LogLevel.WARNING);

                Response.Redirect("Home.aspx");
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            string userInputOtp = txtOtp.Text.Trim();

            if (string.IsNullOrEmpty(userInputOtp) || userInputOtp.Length != 6)
            {
                // ---> LOGGING ADDED: WARNING (Bad format attempt)
                SystemLogService.Write("OTP_VERIFY_INVALID_FORMAT", $"Invalid OTP format entered for email: {pendingEmail}", LogLevel.WARNING);

                ShowMessage("Please enter a valid 6-digit code.", false);
                return;
            }

            string secret = ConfigurationManager.AppSettings["OtpSecret"];
            string hashedInputOtp = OtpHelper.HmacOtp(userInputOtp, secret);

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = "SELECT OtpHash, ExpiryDate FROM Emailotp WHERE Email = @email";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@email", pendingEmail);

                    conn.Open();
                    string dbHash = null;
                    DateTime expiry = DateTime.MinValue;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            dbHash = dr["OtpHash"].ToString();
                            expiry = Convert.ToDateTime(dr["ExpiryDate"]);
                        }
                    }

                    if (dbHash == null)
                    {
                        // ---> LOGGING ADDED: WARNING (OTP record not found)
                        SystemLogService.Write("OTP_VERIFY_NOT_FOUND", $"No OTP record found for email: {pendingEmail}", LogLevel.WARNING);

                        ShowMessage("No verification code found for this email.", false);
                        return;
                    }

                    if (DateTime.Now > expiry)
                    {
                        // ---> LOGGING ADDED: WARNING (Expired OTP used)
                        SystemLogService.Write("OTP_VERIFY_EXPIRED", $"Expired OTP submitted for email: {pendingEmail}", LogLevel.WARNING);

                        ShowMessage("This code has expired. Please request a new one.", false);
                        return;
                    }

                    if (hashedInputOtp == dbHash)
                    {
                        // SUCCESS! 
                        // ---> LOGGING ADDED: INFO (Successful OTP verification)
                        SystemLogService.Write("OTP_VERIFY_SUCCESS", $"OTP successfully verified for email: {pendingEmail}", LogLevel.INFO);

                        DeleteOtpRecord(pendingEmail, conn);

                        // Grant the "Verified" ticket for Signups
                        Session["OTP_Verified_" + pendingEmail] = true;

                        // NEW: Grant the "Verified" ticket for Password Resets!
                        Session["ResetEmail"] = pendingEmail;

                        // Send them to the target page (ProcessSignUp.aspx OR ResetPassword.aspx)
                        ShowMessage("Verified! Processing your request...", true);
                        ScriptManager.RegisterStartupScript(this, GetType(), "redirect",
                            $"setTimeout(function(){{ window.location.href = '{nextUrl}'; }}, 1500);", true);
                    }
                    else
                    {
                        // ---> LOGGING ADDED: WARNING (Wrong OTP entered)
                        SystemLogService.Write("OTP_VERIFY_FAILED", $"Incorrect OTP entered for email: {pendingEmail}", LogLevel.WARNING);

                        ShowMessage("Invalid verification code. Please try again.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR (Database failure during OTP validation)
                SystemLogService.Write("OTP_DB_ERROR", $"Database error during OTP check for {pendingEmail}: {ex.Message}", LogLevel.ERROR);

                ShowMessage("An unexpected system error occurred. Please try again later.", false);
            }
        }

        private void DeleteOtpRecord(string email, SqlConnection conn)
        {
            try
            {
                string query = "DELETE FROM Emailotp WHERE Email = @email";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR (Failed to clean up OTP record)
                SystemLogService.Write("OTP_CLEANUP_ERROR", $"Failed to delete used OTP for {email}: {ex.Message}", LogLevel.ERROR);
            }
        }

        private void ShowMessage(string msg, bool isSuccess)
        {
            lblMessage.Text = msg;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}