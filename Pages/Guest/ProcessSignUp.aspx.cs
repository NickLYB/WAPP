using System;
using System.Configuration;
using System.Data.SqlClient;

namespace WAPP.Pages.Guest
{
    public partial class ProcessSignUp : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            string email = Session["Reg_Email"]?.ToString();

            // 1. Kick out anyone who doesn't have an active registration session
            if (string.IsNullOrEmpty(email))
            {
                Response.Redirect("Home.aspx?err=SessionExpired");
                return;
            }

            // 2. Security Check: Did VerifyOtp.aspx actually verify them?
            if (Session["OTP_Verified_" + email] != null && (bool)Session["OTP_Verified_" + email] == true)
            {
                // 3. Insert the DB records
                bool success = InsertUserIntoDatabase();

                if (success)
                {
                    // 4. Clean up all sessions so the data doesn't linger
                    Session.Remove("OTP_Verified_" + email);
                    Session.Remove("Reg_Fname");
                    Session.Remove("Reg_Lname");
                    Session.Remove("Reg_Dob");
                    Session.Remove("Reg_Contact");
                    Session.Remove("Reg_Email");
                    Session.Remove("Reg_Password");
                    Session.Remove("Reg_RoleId");
                    Session.Remove("Reg_FileName");

                    // 5. Send them home!
                    Response.Redirect("Home.aspx?msg=AccountCreated");
                }
                else
                {
                    Response.Redirect("Home.aspx?err=DbError");
                }
            }
            else
            {
                // Tried to bypass OTP verification
                Response.Redirect("Home.aspx?err=Unauthorized");
            }
        }

        private bool InsertUserIntoDatabase()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Retrieve all data from Session
                    string fname = Session["Reg_Fname"].ToString();
                    string lname = Session["Reg_Lname"].ToString();
                    string dob = Session["Reg_Dob"].ToString();
                    string contact = Session["Reg_Contact"].ToString();
                    string email = Session["Reg_Email"].ToString();
                    string password = Session["Reg_Password"].ToString(); // Note: Apply your password hashing here!
                    int roleId = Convert.ToInt32(Session["Reg_RoleId"]);
                    string fileName = Session["Reg_FileName"]?.ToString();

                    // Insert into [user] table
                    string userSql = @"INSERT INTO [dbo].[user] (fname, lname, dob, contact, email, password_hash, role_id) 
                                      VALUES (@fname, @lname, @dob, @contact, @email, @pass, @role);
                                      SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdUser = new SqlCommand(userSql, conn, transaction);
                    cmdUser.Parameters.AddWithValue("@fname", fname);
                    cmdUser.Parameters.AddWithValue("@lname", lname);
                    cmdUser.Parameters.AddWithValue("@dob", dob);
                    cmdUser.Parameters.AddWithValue("@contact", contact);
                    cmdUser.Parameters.AddWithValue("@email", email);
                    cmdUser.Parameters.AddWithValue("@pass", password);
                    cmdUser.Parameters.AddWithValue("@role", roleId);

                    int newUserId = Convert.ToInt32(cmdUser.ExecuteScalar());

                    // Insert into [tutorApplication] if applicable
                    if (roleId == 3 && !string.IsNullOrEmpty(fileName))
                    {
                        string appSql = @"INSERT INTO [dbo].[tutorApplication] (tutor_id, verification_document, submitted_at, status) 
                                         VALUES (@tid, @doc, @date, 'PENDING')";

                        SqlCommand cmdApp = new SqlCommand(appSql, conn, transaction);
                        cmdApp.Parameters.AddWithValue("@tid", newUserId);
                        cmdApp.Parameters.AddWithValue("@doc", fileName);
                        cmdApp.Parameters.AddWithValue("@date", DateTime.Now);
                        cmdApp.ExecuteNonQuery();
                    }

                    // Commit Transaction
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }
}