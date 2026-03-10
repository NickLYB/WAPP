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
                Response.Redirect("Home.aspx");
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            string userInputOtp = txtOtp.Text.Trim();

            if (string.IsNullOrEmpty(userInputOtp) || userInputOtp.Length != 6)
            {
                ShowMessage("Please enter a valid 6-digit code.", false);
                return;
            }

            string secret = ConfigurationManager.AppSettings["OtpSecret"];
            string hashedInputOtp = OtpHelper.HmacOtp(userInputOtp, secret);

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
                    ShowMessage("No verification code found for this email.", false);
                    return;
                }

                if (DateTime.Now > expiry)
                {
                    ShowMessage("This code has expired. Please request a new one.", false);
                    return;
                }

                if (hashedInputOtp == dbHash)
                {
                    // SUCCESS! 
                    DeleteOtpRecord(pendingEmail, conn);

                    // Grant the "Verified" ticket
                    Session["OTP_Verified_" + pendingEmail] = true;

                    // Send them to the target page (ProcessSignUp.aspx)
                    ShowMessage("Verified! Processing your request...", true);
                    ScriptManager.RegisterStartupScript(this, GetType(), "redirect",
                        $"setTimeout(function(){{ window.location.href = '{nextUrl}'; }}, 1500);", true);
                }
                else
                {
                    ShowMessage("Invalid verification code. Please try again.", false);
                }
            }
        }

        private void DeleteOtpRecord(string email, SqlConnection conn)
        {
            string query = "DELETE FROM Emailotp WHERE Email = @email";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.ExecuteNonQuery();
        }

        private void ShowMessage(string msg, bool isSuccess)
        {
            lblMessage.Text = msg;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}