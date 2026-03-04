using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class CreateCourseModal : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCourseTypes();
            }
        }

        private void ShowModalWithMessage(string msg, bool isError = true)
        {
            lblMsg.Text = msg;
            lblMsg.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;

            // Use vanilla JavaScript for Bootstrap 5 (no jQuery needed).
            // The setTimeout ensures the DOM is fully loaded before trying to open the modal.
            string script = @"
        setTimeout(function() { 
            var myModal = new bootstrap.Modal(document.getElementById('createCourseModal')); 
            myModal.show(); 
        }, 100);";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowCreateModal", script, true);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // 1) Basic validation
            if (string.IsNullOrWhiteSpace(title.Text) ||
                string.IsNullOrWhiteSpace(description.Text) ||
                string.IsNullOrWhiteSpace(duration.Text) ||
                type.SelectedValue == "" ||
                skill.SelectedIndex == 0) // Changed to index check to catch "-- Select Skill --"
            {
                ShowModalWithMessage("Please fill in all fields.");
                return;
            }

            if (!int.TryParse(duration.Text.Trim(), out int durationMinutes) || durationMinutes <= 0)
            {
                ShowModalWithMessage("Duration must be a positive number.");
                return;
            }

            // 2) Get tutor id from session
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Login.aspx");
                return;
            }
            int tutorId = Convert.ToInt32(Session["UserId"]);

            // 3) Handle image upload
            string imagePath = null;
            if (FileUpload1.HasFile)
            {
                string ext = System.IO.Path.GetExtension(FileUpload1.FileName).ToLower();
                string[] allowed = { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowed.Contains(ext))
                {
                    ShowModalWithMessage("Only JPG/PNG/GIF files are allowed.");
                    return;
                }

                string fileName = Guid.NewGuid().ToString("N") + ext;
                string folder = Server.MapPath("~/Images/Courses/");

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                string fullPath = System.IO.Path.Combine(folder, fileName);
                FileUpload1.SaveAs(fullPath);

                imagePath = "~/Images/Courses/" + fileName;
            }

            // 4) Insert to DB
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"INSERT INTO course (title, description, course_type_id, duration_minutes, skill_level, tutor_id, status, image_path) 
                               VALUES (@title, @description, @course_type_id, @duration_minutes, @skill_level, @tutor_id, @status, @image_path); 
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title.Text.Trim());
                    cmd.Parameters.AddWithValue("@description", description.Text.Trim());
                    cmd.Parameters.AddWithValue("@course_type_id", Convert.ToInt32(type.SelectedValue));
                    cmd.Parameters.AddWithValue("@duration_minutes", durationMinutes);
                    cmd.Parameters.AddWithValue("@skill_level", skill.SelectedValue.ToUpper());
                    cmd.Parameters.AddWithValue("@tutor_id", tutorId);
                    cmd.Parameters.AddWithValue("@status", "PENDING");
                    cmd.Parameters.AddWithValue("@image_path", (object)imagePath ?? DBNull.Value);

                    try
                    {
                        conn.Open();
                        int newCourseId = (int)cmd.ExecuteScalar();
                        Session["NewCourseId"] = newCourseId;

                        // Because this is a user control on the Teaching page, you can just refresh the parent page
                        Response.Redirect(Request.RawUrl);
                    }
                    catch (Exception ex)
                    {
                        ShowModalWithMessage("Error: " + ex.Message);
                    }
                }
            }
        }

        private void LoadCourseTypes()
        {
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT id, name FROM courseType";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    type.DataSource = reader;
                    type.DataTextField = "name";
                    type.DataValueField = "id";
                    type.DataBind();
                }
            }
            type.Items.Insert(0, new ListItem("-- Select Type --", ""));
        }
    }
}