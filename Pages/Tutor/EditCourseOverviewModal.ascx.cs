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
    public partial class EditCourseOverviewModal : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadCourseTypes();
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

        private void ShowModal(string msg = "", bool isError = false)
        {
            lblMsg.Text = msg;
            lblMsg.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;

            string script = @"
                setTimeout(function() {
                    var m = new bootstrap.Modal(document.getElementById('editCourseModal'));
                    m.show();
                }, 50);";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditModal", script, true);
        }

        // ✅ public method: parent page calls this
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
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE course
                SET title=@title,
                    description=@desc,
                    course_type_id=@typeId,
                    duration_minutes=@mins,
                    skill_level=@level
                WHERE Id=@id", con))
            {
                cmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@desc", txtDesc.Text.Trim());
                cmd.Parameters.AddWithValue("@typeId", Convert.ToInt32(ddlType.SelectedValue));
                cmd.Parameters.AddWithValue("@mins", minutes);
                cmd.Parameters.AddWithValue("@level", ddlSkill.SelectedValue);
                cmd.Parameters.AddWithValue("@id", courseId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            ShowModal("Saved successfully.", false);

            // optional: refresh parent page
            // simplest: reload page
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
                        // ⚠ Delete child data first (important to avoid FK errors)

                        // Delete lessons
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM learningResource WHERE course_id = @Id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", courseId);
                            cmd.ExecuteNonQuery();
                        }

                        // Delete quizzes
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM quiz WHERE course_id = @Id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", courseId);
                            cmd.ExecuteNonQuery();
                        }

                        // Finally delete course
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM course WHERE Id = @Id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", courseId);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        return;
                    }
                }
            }

            // Redirect to teaching page after delete
            Response.Redirect("~/Pages/Tutor/Teaching.aspx");
        }
    }
}