using System;
using System.Configuration;
using System.Data.SqlClient;

namespace WAPP.Controls
{
    public partial class LoginModal : System.Web.UI.UserControl
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT Id, fname, role_id FROM [user] WHERE email = @email AND password_hash = @pass";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@pass", txtPassword.Text.Trim());

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        // Store identity in Session
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
                                break;
                        }
                    }
                    else
                    {
                        lblError.Text = "Invalid email or password.";
                    }
                }
                catch (Exception ex)
                {
                    lblError.Text = "Database Error: " + ex.Message;
                }
            }
        }
    }
}