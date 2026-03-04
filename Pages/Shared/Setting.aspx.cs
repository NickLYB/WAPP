using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace WAPP.Pages.Shared
{
    public partial class Settings : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        // Dynamic Master Page
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

        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int userId = Convert.ToInt32(Session["Id"]);
            string currentPass = txtCurrentPass.Text;
            string newPass = txtNewPass.Text;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Verify old password
                string verifySql = "SELECT COUNT(*) FROM [user] WHERE Id = @Id AND password_hash = @oldPass";
                using (SqlCommand cmdVerify = new SqlCommand(verifySql, conn))
                {
                    cmdVerify.Parameters.AddWithValue("@Id", userId);
                    cmdVerify.Parameters.AddWithValue("@oldPass", currentPass);
                    conn.Open();
                    int exists = (int)cmdVerify.ExecuteScalar();

                    if (exists > 0)
                    {
                        // Proceed to update
                        string updateSql = "UPDATE [user] SET password_hash = @newPass WHERE Id = @Id";
                        using (SqlCommand cmdUpdate = new SqlCommand(updateSql, conn))
                        {
                            cmdUpdate.Parameters.AddWithValue("@newPass", newPass);
                            cmdUpdate.Parameters.AddWithValue("@Id", userId);
                            cmdUpdate.ExecuteNonQuery();

                            lblMessage.Visible = true;
                            lblMessage.Text = "Password changed successfully!";
                            lblMessage.CssClass = "alert alert-success d-block";

                            txtCurrentPass.Text = ""; txtNewPass.Text = ""; txtConfirmPass.Text = "";
                        }
                    }
                    else
                    {
                        lblMessage.Visible = true;
                        lblMessage.Text = "Incorrect current password.";
                        lblMessage.CssClass = "alert alert-danger d-block";
                    }
                }
            }
        }

        protected void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["Id"]);
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Note: If you have foreign key constraints in Course/Enrollment tables, 
                    // a direct DELETE might fail. If so, a soft delete is safer:
                    // "UPDATE [user] SET is_locked = 1, role_id = NULL WHERE Id = @Id"

                    string sql = "DELETE FROM [user] WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Log the user out after deletion
                Session.Clear();
                Session.Abandon();
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Could not delete account. Ensure you have no active courses linked. Error: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }
    }
}