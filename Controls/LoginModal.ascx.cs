using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using WAPP.Utils; // Make sure to include this to access PasswordManager!

namespace WAPP.Controls
{
    public partial class LoginModal : System.Web.UI.UserControl
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string enteredPassword = txtPassword.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Fetch the user's details AND the stored hash based ONLY on their email
                string sql = "SELECT Id, fname, role_id, password_hash FROM [user] WHERE email = @email";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@email", email);

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // 1. Get the securely stored hash from the database
                            string storedHash = reader["password_hash"].ToString();

                            // 2. Use the PasswordManager to verify the entered password
                            PasswordManager pwdManager = new PasswordManager();
                            bool isPasswordCorrect = pwdManager.VerifyPassword(enteredPassword, storedHash);

                            if (isPasswordCorrect)
                            {
                                // 3. Passwords match! Store identity in Session
                                Session["UserId"] = reader["Id"].ToString();
                                Session["UserName"] = reader["fname"].ToString();

                                int roleId = Convert.ToInt32(reader["role_id"]);
                                Session["role_id"] = roleId;

                                // Navigate based on the integer ID
                                switch (roleId)
                                {
                                    case 1: // Admin
                                        Response.Redirect("~/Pages/Admin/Home.aspx");
                                        break;
                                    case 2: // Staff
                                        Response.Redirect("~/Pages/Staff/StaffDashboard.aspx");
                                        break;
                                    case 3: // Tutor
                                        Response.Redirect("~/Pages/Tutor/Home.aspx");
                                        break;
                                    case 4: // Student
                                        Response.Redirect("~/Pages/Student/Home.aspx");
                                        break;
                                    default:
                                        lblError.Text = "Account has no assigned role.";
                                        KeepModalOpen();
                                        break;
                                }
                            }
                            else
                            {
                                // Password was incorrect
                                lblError.Text = "Invalid email or password.";
                                KeepModalOpen();
                            }
                        }
                        else
                        {
                            // Email was not found in the database
                            lblError.Text = "Invalid email or password.";
                            KeepModalOpen();
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblError.Text = "Database Error: " + ex.Message;
                    KeepModalOpen();
                }
            }
        }

        // Helper method to keep the Bootstrap modal open if there is an error
        private void KeepModalOpen()
        {
            string script = @"
        setTimeout(function() { 
            var modalEl = document.getElementById('loginModal');
            
            // 1. Only initialize and show if it isn't already open
            if (!modalEl.classList.contains('show')) {
                var myModal = bootstrap.Modal.getOrCreateInstance(modalEl); 
                myModal.show(); 
            }

            // 2. Failsafe: Remove any extra backdrops just in case
            var backdrops = document.querySelectorAll('.modal-backdrop');
            while (backdrops.length > 1) {
                backdrops[1].remove();
            }
        }, 100);";

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "KeepLoginModalOpen", script, true);
        }
    }
}