using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                Response.Redirect("~/Pages/Guest/Login.aspx");
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
                                           'T' + RIGHT('000' + CAST(Id AS VARCHAR(10)), 3) + '-' + fname + ' ' + lname AS FullTutorName 
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
            if (string.IsNullOrWhiteSpace(txtAddTitle.Text) ||
                string.IsNullOrWhiteSpace(txtAddDesc.Text) ||
                string.IsNullOrWhiteSpace(txtAddDuration.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please fill out all required fields.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"INSERT INTO [course] 
                                   (title, description, course_type_id, duration_minutes, skill_level, tutor_id, status) 
                                   VALUES (@title, @desc, @type, @duration, @skill, @tutor, @status)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", txtAddTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@desc", txtAddDesc.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", ddlAddCategory.SelectedValue);
                        cmd.Parameters.AddWithValue("@duration", txtAddDuration.Text.Trim());
                        cmd.Parameters.AddWithValue("@skill", ddlAddSkill.SelectedValue);
                        cmd.Parameters.AddWithValue("@tutor", ddlAddTutor.SelectedValue);
                        cmd.Parameters.AddWithValue("@status", ddlAddStatus.SelectedValue);

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