using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WAPP.Pages.Shared
{
    public partial class EditProfile : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        // Dynamically attach the correct Master Page based on the logged-in role
        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (Session["role_id"] == null)
            {
                Response.Redirect("~/Pages/Guest/Login.aspx");
                return;
            }

            int roleId = Convert.ToInt32(Session["role_id"]);
            switch (roleId)
            {
                case 1: this.MasterPageFile = "~/Masters/Admin.Master"; break;
                case 2: this.MasterPageFile = "~/Masters/Staff.Master"; break;
                case 3: this.MasterPageFile = "~/Masters/Tutor.Master"; break;
                case 4: this.MasterPageFile = "~/Masters/Student.Master"; break;
                default: Response.Redirect("~/Pages/Guest/Login.aspx"); break;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUserData();
            }
        }

        private void LoadUserData()
        {
            if (Session["UserId"] == null) return;
            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT fname, lname, email, contact, dob, role_id FROM [user] WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                txtFname.Text = rdr["fname"].ToString();
                                txtLname.Text = rdr["lname"].ToString();
                                txtEmail.Text = rdr["email"].ToString();
                                txtPhone.Text = rdr["contact"].ToString();

                                if (rdr["dob"] != DBNull.Value)
                                {
                                    txtDob.Text = Convert.ToDateTime(rdr["dob"]).ToString("yyyy-MM-dd");
                                }

                                litFullName.Text = $"{rdr["fname"]} {rdr["lname"]}";
                                litRole.Text = GetRoleName(Convert.ToInt32(rdr["role_id"]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error loading profile: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        private string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case 1: return "Administrator";
                case 2: return "Staff";
                case 3: return "Tutor";
                case 4: return "Student";
                default: return "User";
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("UpdateProfile");
            if (!Page.IsValid) return;

            if (Session["UserId"] == null) return;
            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Exact same clean SQL structure as your UserManagement file!
                    string sql = @"UPDATE [user] SET fname=@fname, lname=@lname, dob=@dob, contact=@contact, email=@email WHERE Id=@Id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        cmd.Parameters.AddWithValue("@fname", txtFname.Text.Trim());
                        cmd.Parameters.AddWithValue("@lname", txtLname.Text.Trim());
                        cmd.Parameters.AddWithValue("@dob", txtDob.Text); // No complex parsing needed
                        cmd.Parameters.AddWithValue("@contact", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Update the session name so the Top Navbar changes instantly
                Session["UserName"] = txtFname.Text.Trim() + " " + txtLname.Text.Trim();

                lblMessage.Visible = true;
                lblMessage.Text = "Profile updated successfully!";
                lblMessage.CssClass = "alert alert-success d-block";

                LoadUserData(); // Refresh the labels below the avatar
            }
            catch (SqlException sqlEx)
            {
                lblMessage.Visible = true;
                // Error 2627 and 2601 mean "Unique Constraint Violation" (Email already exists)
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    lblMessage.Text = "The email address you entered is already registered to another account.";
                }
                else
                {
                    lblMessage.Text = "Database Error: " + sqlEx.Message;
                }
                lblMessage.CssClass = "alert alert-danger d-block";
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error updating profile: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }
    }
}