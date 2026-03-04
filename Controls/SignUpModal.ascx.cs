using System;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace WAPP.Controls
{
    public partial class SignUpModal : System.Web.UI.UserControl
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Forces the HTML form to support file uploads, 
            // even if the FileUpload control starts off hidden!
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
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowModalAndError("Please fill in all required fields.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDob.Text)) // Added DOB check to prevent SQL date conversion errors
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

            bool isSuccess = false; // Add a flag to track if the transaction worked

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 2. Insert into [user] table
                    string userSql = @"INSERT INTO [dbo].[user] (fname, lname, dob, contact, email, password_hash, role_id) 
                              VALUES (@fname, @lname, @dob, @contact, @email, @pass, @role);
                              SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdUser = new SqlCommand(userSql, conn, transaction);
                    cmdUser.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
                    cmdUser.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
                    cmdUser.Parameters.AddWithValue("@dob", txtDob.Text);
                    cmdUser.Parameters.AddWithValue("@contact", txtContact.Text.Trim());
                    cmdUser.Parameters.AddWithValue("@email", email);
                    cmdUser.Parameters.AddWithValue("@pass", password); // Remember to hash this later!
                    cmdUser.Parameters.AddWithValue("@role", roleId);

                    int newUserId = Convert.ToInt32(cmdUser.ExecuteScalar());

                    // 3. If Tutor, Insert into [tutorApplication]
                    if (roleId == 3)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileVerification.FileName);
                        string folderPath = Server.MapPath("~/Uploads/Verification/");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        fileVerification.SaveAs(folderPath + fileName);

                        string appSql = @"INSERT INTO [dbo].[tutorApplication] (tutor_id, verification_document, submitted_at, status) 
                                 VALUES (@tid, @doc, @date, 'PENDING')";

                        SqlCommand cmdApp = new SqlCommand(appSql, conn, transaction);
                        cmdApp.Parameters.AddWithValue("@tid", newUserId);
                        cmdApp.Parameters.AddWithValue("@doc", fileName);
                        cmdApp.Parameters.AddWithValue("@date", DateTime.Now);
                        cmdApp.ExecuteNonQuery();
                    }

                    // 4. Commit 
                    transaction.Commit();
                    isSuccess = true; // Mark as success!
                }
                catch (Exception ex)
                {
                    // Only roll back if something actually failed
                    transaction.Rollback();
                    ShowModalAndError("Registration failed: " + ex.Message);
                }
            }

            // 5. Redirect outside the try-catch block so we don't trigger rollback errors
            if (isSuccess)
            {
                // Using false prevents the ThreadAbortException
                Response.Redirect("Login.aspx?msg=RegistrationSuccess", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        // Helper Method: Shows the error text and injects JS to re-open the Bootstrap Modal
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