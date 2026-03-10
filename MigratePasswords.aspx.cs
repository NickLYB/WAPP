using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using WAPP.Utils; // Make sure this matches where you put PasswordManager.cs!

namespace WAPP
{
    public partial class MigratePasswords : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Optional Security: Uncomment the lines below to ensure only Admins can access this page
            /*
            if (Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 1)
            {
                Response.Write("Access Denied.");
                Response.End();
            }
            */
        }

        protected void btnMigrate_Click(object sender, EventArgs e)
        {
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            PasswordManager pwdManager = new PasswordManager();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // 1. Find all passwords that are NOT BCrypt hashes
                    string selectSql = "SELECT Id, password_hash FROM [user] WHERE password_hash NOT LIKE '$2%'";
                    SqlCommand selectCmd = new SqlCommand(selectSql, conn);
                    conn.Open();

                    var usersToUpdate = new List<Tuple<int, string>>();

                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int userId = Convert.ToInt32(reader["Id"]);
                            string plainText = reader["password_hash"].ToString();

                            if (!string.IsNullOrWhiteSpace(plainText))
                            {
                                usersToUpdate.Add(new Tuple<int, string>(userId, plainText));
                            }
                        }
                    }

                    // 2. Hash and update
                    string updateSql = "UPDATE [user] SET password_hash = @hash WHERE Id = @id";
                    int updatedCount = 0;

                    foreach (var user in usersToUpdate)
                    {
                        string newHash = pwdManager.HashPassword(user.Item2);

                        using (SqlCommand updateCmd = new SqlCommand(updateSql, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@hash", newHash);
                            updateCmd.Parameters.AddWithValue("@id", user.Item1);
                            updateCmd.ExecuteNonQuery();
                            updatedCount++;
                        }
                    }

                    // 3. Display success message
                    lblMessage.Text = $"✅ Success! {updatedCount} plain-text passwords were converted.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                    btnMigrate.Enabled = false; // Disable button to prevent double clicks
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}