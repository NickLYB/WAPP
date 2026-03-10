using System;
using System.IO;
using System.Configuration;
using System.Web.UI;
using WAPP.Utils; // Ensure your OtpHelper and EmailHelper are here

namespace WAPP.Controls
{
    public partial class SignUpModal : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Form.Attributes.Add("enctype", "multipart/form-data");
        }

        protected void rblRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlTutorDocs.Visible = (rblRole.SelectedValue == "3");
        }

        protected void btnSignUp_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            int roleId = int.Parse(rblRole.SelectedValue);

            // 1. Validation 
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(email))
            {
                ShowModalAndError("Please fill in all required fields.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDob.Text))
            {
                ShowModalAndError("Please select a Date of Birth.");
                return;
            }
            if (password != txtConfirmPassword.Text)
            {
                ShowModalAndError("Passwords do not match.");
                return;
            }
            if (roleId == 3 && !fileVerification.HasFile)
            {
                ShowModalAndError("Tutors must upload a verification document.");
                return;
            }

            // 2. Handle File Upload BEFORE DB creation
            string savedFileName = null;
            if (roleId == 3)
            {
                try
                {
                    savedFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileVerification.FileName);
                    string folderPath = Server.MapPath("~/Uploads/Verification/");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    fileVerification.SaveAs(folderPath + savedFileName);
                }
                catch (Exception ex)
                {
                    ShowModalAndError("File upload failed: " + ex.Message);
                    return;
                }
            }

            // 3. Hash Password and Store Data in Session temporarily
            PasswordManager pwdManager = new PasswordManager();
            string hashedPassword = pwdManager.HashPassword(password);

            Session["Reg_Fname"] = txtFirstName.Text.Trim();
            Session["Reg_Lname"] = txtLastName.Text.Trim();
            Session["Reg_Dob"] = txtDob.Text;
            Session["Reg_Contact"] = txtContact.Text.Trim();
            Session["Reg_Email"] = email;

            // SECURITY UPGRADE: Store the securely hashed password, never the plain text!
            Session["Reg_Password"] = hashedPassword;

            Session["Reg_RoleId"] = roleId;
            Session["Reg_FileName"] = savedFileName;

            // 4. Generate OTP and Redirect
            try
            {
                string otp = OtpHelper.GenerateNumericOtp();
                string secret = ConfigurationManager.AppSettings["OtpSecret"];
                string otpHash = OtpHelper.HmacOtp(otp, secret);

                // Save OTP to DB tied to EMAIL, not UserId
                OtpHelper.SaveOtpToDb(email, otpHash, DateTime.Now.AddMinutes(10));

                // Send OTP email
                EmailHelper.SendOtpEmail(email, otp, txtFirstName.Text.Trim());

                // Redirect to universal OTP page, passing the next destination
                string redirectUrl = $"~/Pages/Guest/VerifyOtp.aspx?email={Server.UrlEncode(email)}&next=ProcessSignUp.aspx";
                Response.Redirect(redirectUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                ShowModalAndError("Failed to initiate verification: " + ex.Message);
            }
        }

        private void ShowModalAndError(string errorMsg)
        {
            lblError.Text = errorMsg;
            string script = @"
                setTimeout(function() { 
                    var myModal = new bootstrap.Modal(document.getElementById('signUpModal')); 
                    myModal.show(); 
                }, 100);";

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "KeepSignUpModalOpen", script, true);
        }
    }
}