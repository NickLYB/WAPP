using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class EditCourseOverviewModal : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadCourseTypes();
        }
    
        private void ShowModal(string msg = "", bool isError = false)
        {
            lblMsg.Text = msg;
            lblMsg.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;

            // Use this.Page to ensure the script registers correctly after a Full PostBack
            string script = @"
        setTimeout(function() {
            var modalEl = document.getElementById('editCourseModal');
            if(modalEl) {
                var m = new bootstrap.Modal(modalEl);
                m.show();
            }
        }, 150);";

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "ShowEditModal", script, true);
        }

        private void LoadCourseTypes()
        {
            ddlType.Items.Clear();

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand("SELECT Id, name FROM courseType ORDER BY name", con))
            {
                con.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        ddlType.Items.Add(new ListItem(r["name"].ToString(), r["Id"].ToString()));
                    }
                }
            }
            ddlType.Items.Insert(0, new ListItem("-- Select Type --", ""));
        }
        public void LoadCourse(int courseId)
        {
            hfCourseId.Value = courseId.ToString();

            // ensure types exist every time
            LoadCourseTypes();

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT title, description, course_type_id, duration_minutes, skill_level
                FROM course
                WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", courseId);
                con.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        txtTitle.Text = r["title"]?.ToString();
                        txtDesc.Text = r["description"]?.ToString();
                        txtDuration.Text = r["duration_minutes"]?.ToString();

                        string typeId = r["course_type_id"]?.ToString();
                        if (!string.IsNullOrEmpty(typeId) && ddlType.Items.FindByValue(typeId) != null)
                            ddlType.SelectedValue = typeId;

                        string level = r["skill_level"]?.ToString()?.ToUpper();
                        if (!string.IsNullOrEmpty(level) && ddlSkill.Items.FindByValue(level) != null)
                            ddlSkill.SelectedValue = level;
                    }
                }
            }

            ShowModal();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfCourseId.Value, out int courseId))
            {
                ShowModal("Invalid course.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text) ||
                string.IsNullOrWhiteSpace(txtDesc.Text) ||
                string.IsNullOrWhiteSpace(txtDuration.Text) ||
                string.IsNullOrWhiteSpace(ddlType.SelectedValue) ||
                string.IsNullOrWhiteSpace(ddlSkill.SelectedValue))
            {
                ShowModal("Please fill in all fields.", true);
                return;
            }

            if (!int.TryParse(txtDuration.Text.Trim(), out int minutes) || minutes <= 0)
            {
                ShowModal("Duration must be a positive number.", true);
                return;
            }

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            // Image Upload Logic
            string imagePathUpdateSql = "";
            string newImagePath = null;

            if (fuCourseImage.HasFile)
            {
                string ext = Path.GetExtension(fuCourseImage.FileName).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    string oldImagePath = "";
                    using (SqlConnection con = new SqlConnection(cs))
                    using (SqlCommand cmdOld = new SqlCommand("SELECT image_path FROM course WHERE Id = @Id", con))
                    {
                        cmdOld.Parameters.AddWithValue("@Id", courseId);
                        con.Open();
                        oldImagePath = cmdOld.ExecuteScalar()?.ToString();
                    }

                    string filename = Guid.NewGuid().ToString() + ext;
                    string folderPath = Server.MapPath("~/Uploads/CourseImages/");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        try
                        {
                            string oldPhysicalPath = Server.MapPath(oldImagePath);
                            if (!oldPhysicalPath.ToLower().Contains("placeholder") && File.Exists(oldPhysicalPath))
                            {
                                File.Delete(oldPhysicalPath);
                            }
                        }
                        catch { } // Ignore if locked/missing
                    }

                    string savePath = Path.Combine(folderPath, filename);
                    fuCourseImage.SaveAs(savePath);

                    newImagePath = "~/Uploads/CourseImages/" + filename;
                    imagePathUpdateSql = ", image_path = @ImagePath";
                }
                else
                {
                    ShowModal("Invalid image format. Please upload JPG or PNG.", true);
                    return;
                }
            }

            // Update the Database
            string updateQuery = $@"
        UPDATE course
        SET title=@title,
            description=@desc,
            course_type_id=@typeId,
            duration_minutes=@mins,
            skill_level=@level
            {imagePathUpdateSql}
        WHERE Id=@id";

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(updateQuery, con))
            {
                cmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@desc", txtDesc.Text.Trim());
                cmd.Parameters.AddWithValue("@typeId", Convert.ToInt32(ddlType.SelectedValue));
                cmd.Parameters.AddWithValue("@mins", minutes);
                cmd.Parameters.AddWithValue("@level", ddlSkill.SelectedValue);
                cmd.Parameters.AddWithValue("@id", courseId);

                if (newImagePath != null)
                {
                    cmd.Parameters.AddWithValue("@ImagePath", newImagePath);
                }

                con.Open();
                cmd.ExecuteNonQuery();
            }

            // Redirect immediately to refresh the parent page and show the new data!
            Page.Response.Redirect(Page.Request.RawUrl);
        }
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfCourseId.Value, out int courseId))
                return;

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                using (SqlTransaction tran = con.BeginTransaction())
                {
                    try
                    {
                        // Delete child data first (important to avoid Foreign Key errors)

                        // 1. Delete lessons
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM learningResource WHERE course_id = @Id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", courseId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Delete quizzes
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM quiz WHERE course_id = @Id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", courseId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Finally delete the course itself
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM course WHERE Id = @Id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", courseId);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        ShowModal("Failed to delete the course. It may have existing enrollments.", true);
                        return;
                    }
                }
            }

            // Redirect to teaching page after successful deletion
            Response.Redirect("~/Pages/Tutor/Teaching.aspx");
        }
    }
}