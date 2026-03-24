using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using WAPP.Utils; // Accesses PasswordManager and SystemLogService

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
                string sql = "SELECT Id, fname, role_id, password_hash, ISNULL(is_locked, 0) AS is_locked, profile_pic FROM [user] WHERE email = @email";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@email", email);

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int currentUserId = Convert.ToInt32(reader["Id"]);

                            // --- CHECK IF ACCOUNT IS LOCKED ---
                            bool isLocked = Convert.ToBoolean(reader["is_locked"]);
                            if (isLocked)
                            {
                                // ---> UPDATED TO AUDIT
                                SystemLogService.Write("AUTH_LOGIN_LOCKED", $"Locked account attempted login: {email}", LogLevel.WARNING, currentUserId, "AUDIT");
                                lblError.Text = "Your account has been locked. Please contact support.";
                                KeepModalOpen();
                                return; // Stop the login process immediately
                            }

                            // 1. Get the securely stored hash from the database
                            string storedHash = reader["password_hash"].ToString();

                            // 2. Use the PasswordManager to verify the entered password
                            PasswordManager pwdManager = new PasswordManager();
                            bool isPasswordCorrect = pwdManager.VerifyPassword(enteredPassword, storedHash);

                            if (isPasswordCorrect)
                            {
                                // ---> LOGGING: INFO (Successful login) - Already AUDIT
                                SystemLogService.Write("AUTH_LOGIN_SUCCESS", "User logged in successfully.", LogLevel.INFO, currentUserId, "AUDIT");

                                // --- REAL-TIME SIGNALR TRIGGER ---
                                try
                                {
                                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<WAPP.Hubs.NotificationHub>();
                                    hubContext.Clients.All.receiveNotification(0);
                                }
                                catch {}

                                // 3. Passwords match! Store identity in Session
                                Session["UserId"] = currentUserId.ToString();
                                Session["UserName"] = reader["fname"].ToString();

                                if (reader["profile_pic"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["profile_pic"].ToString()))
                                {
                                    Session["ProfilePic"] = reader["profile_pic"].ToString();
                                }
                                else
                                {
                                    Session["ProfilePic"] = null;
                                }

                                int roleId = Convert.ToInt32(reader["role_id"]);
                                Session["role_id"] = roleId;

                                // Navigate based on the integer ID
                                switch (roleId)
                                {
                                    case 1: // Admin
                                        Response.Redirect("~/Pages/Admin/Home.aspx", false);
                                        break;
                                    case 2: // Staff
                                        Response.Redirect("~/Pages/Staff/StaffDashboard.aspx", false);
                                        break;
                                    case 3: // Tutor
                                        Response.Redirect("~/Pages/Tutor/Home.aspx", false);
                                        break;
                                    case 4: // Student
                                        Response.Redirect("~/Pages/Student/Home.aspx", false);
                                        break;
                                    default:
                                        lblError.Text = "Account has no assigned role.";
                                        KeepModalOpen();
                                        break;
                                }
                                Context.ApplicationInstance.CompleteRequest();
                            }
                            else
                            {
                                // ---> UPDATED TO AUDIT
                                SystemLogService.Write("AUTH_LOGIN_FAILED", "Failed login attempt (Wrong password).", LogLevel.WARNING, currentUserId, "AUDIT");

                                lblError.Text = "Invalid email or password.";
                                KeepModalOpen();
                            }
                        }
                        else
                        {
                            // ---> UPDATED TO AUDIT
                            SystemLogService.Write("AUTH_LOGIN_FAILED", $"Failed login attempt (Email not found: {email}).", LogLevel.WARNING, null, "AUDIT");

                            lblError.Text = "Invalid email or password.";
                            KeepModalOpen();
                        }
                    }
                }
                catch (System.Threading.ThreadAbortException){}
                catch (Exception ex)
                {
                    SystemLogService.Write("AUTH_LOGIN_DB_ERROR", $"Database error during login for email {email}: {ex.Message}", LogLevel.ERROR, null, "OPEN");

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