using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using WAPP.Utils; // Accesses OtpHelper, EmailHelper, and SystemLogService

namespace WAPP.Controls
{
    public partial class ForgotPasswordModal : System.Web.UI.UserControl
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSendOtp_Click(object sender, EventArgs e)
        {
            string email = txtResetEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowModalAndError("Please enter your email address.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // 1. Check if the email actually exists in the database
                string sql = "SELECT fname FROM [user] WHERE email = @email";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@email", email);

                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string firstName = result.ToString();

                        // 2. Generate OTP
                        string otp = OtpHelper.GenerateNumericOtp();
                        string secret = ConfigurationManager.AppSettings["OtpSecret"];
                        string otpHash = OtpHelper.HmacOtp(otp, secret);

                        // 3. Save OTP to Database
                        OtpHelper.SaveOtpToDb(email, otpHash, DateTime.Now.AddMinutes(10));

                        // 4. Send OTP Email
                        EmailHelper.SendOtpEmail(email, otp, firstName);

                        // 5. Log the action
                        SystemLogService.Write("AUTH_PASSWORD_RESET_REQ", $"Password reset requested for {email}.", LogLevel.INFO);

                        // 6. Redirect to universal OTP page. 
                        string redirectUrl = $"~/Pages/Guest/VerifyOtp.aspx?email={Server.UrlEncode(email)}&next=ResetPassword.aspx";
                        Response.Redirect(redirectUrl, false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        // Email not found
                        ShowModalAndError("We couldn't find an account with that email address.");
                    }
                }
                catch (Exception ex)
                {
                    ShowModalAndError("An error occurred: " + ex.Message);
                }
            }
        }

        // Helper method to keep the Bootstrap modal open if there is an error
        private void ShowModalAndError(string errorMsg)
        {
            lblError.Text = errorMsg;
            string script = @"
                setTimeout(function() { 
                    var myModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('forgotPasswordModal')); 
                    myModal.show(); 
                }, 100);";

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "KeepForgotModalOpen", script, true);
        }
    }
}