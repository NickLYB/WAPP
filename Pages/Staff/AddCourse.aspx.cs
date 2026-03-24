using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Staff
{
    public partial class AddCourse : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                BindDropdowns();
            }
        }

        private void BindDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // 1. Load Categories
                using (SqlDataAdapter sdaCat = new SqlDataAdapter("SELECT Id, name FROM [courseType]", conn))
                {
                    DataTable dtCat = new DataTable();
                    sdaCat.Fill(dtCat);
                    ddlAddCategory.DataSource = dtCat;
                    ddlAddCategory.DataTextField = "name";
                    ddlAddCategory.DataValueField = "Id";
                    ddlAddCategory.DataBind();
                }

                // 2. Load Tutors (Formatted as: T004-John Doe)
                string tutorSql = @"SELECT Id, 
                                           'T' + RIGHT('000' + CAST(Id AS VARCHAR(10)), 3) + '-' + fname + ' ' + ISNULL(lname,'') AS FullTutorName 
                                    FROM [user] 
                                    WHERE role_id = 3";
                using (SqlDataAdapter sdaTutor = new SqlDataAdapter(tutorSql, conn))
                {
                    DataTable dtTutor = new DataTable();
                    sdaTutor.Fill(dtTutor);
                    ddlAddTutor.DataSource = dtTutor;
                    ddlAddTutor.DataTextField = "FullTutorName";
                    ddlAddTutor.DataValueField = "Id";
                    ddlAddTutor.DataBind();
                }
            }
        }

        protected void btnSaveCourse_Click(object sender, EventArgs e)
        {
            // 1) Basic Validation
            if (string.IsNullOrWhiteSpace(txtAddTitle.Text) ||
                string.IsNullOrWhiteSpace(txtAddDesc.Text) ||
                string.IsNullOrWhiteSpace(txtAddDuration.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please fill out all required fields.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            if (!int.TryParse(txtAddDuration.Text.Trim(), out int durationMinutes) || durationMinutes <= 0)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Duration must be a positive number.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            // 2) Handle Image Upload
            string imagePath = null;
            if (fuCourseImage.HasFile)
            {
                string ext = System.IO.Path.GetExtension(fuCourseImage.FileName).ToLower();
                string[] allowed = { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowed.Contains(ext))
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Only JPG/PNG/GIF files are allowed for the Course Image.";
                    lblMessage.CssClass = "alert alert-danger d-block";
                    return;
                }

                // Generate unique filename and setup path
                string fileName = Guid.NewGuid().ToString("N") + ext;
                string folder = Server.MapPath("~/Images/Courses/");

                // Create directory if it doesn't exist
                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }

                // Save file to server
                string fullPath = System.IO.Path.Combine(folder, fileName);
                fuCourseImage.SaveAs(fullPath);

                // Store relative path for database
                imagePath = "~/Images/Courses/" + fileName;
            }

            // 3) Database Insert
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"INSERT INTO [course] 
                                   (title, description, course_type_id, duration_minutes, skill_level, tutor_id, status, image_path) 
                                   VALUES (@title, @desc, @type, @duration, @skill, @tutor, @status, @image_path)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", txtAddTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@desc", txtAddDesc.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", ddlAddCategory.SelectedValue);
                        cmd.Parameters.AddWithValue("@duration", durationMinutes);
                        cmd.Parameters.AddWithValue("@skill", ddlAddSkill.SelectedValue);
                        cmd.Parameters.AddWithValue("@tutor", ddlAddTutor.SelectedValue);
                        cmd.Parameters.AddWithValue("@status", ddlAddStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@image_path", (object)imagePath ?? DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                Response.Redirect("CourseManagement.aspx?msg=success");
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error adding course: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }
    }
}