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
                conn.Open();

                using (SqlCommand cmdCat = new SqlCommand("SELECT Id, name FROM [courseType]", conn))
                {
                    using (SqlDataReader rdrCat = cmdCat.ExecuteReader())
                    {
                        ddlAddCategory.DataSource = rdrCat;
                        ddlAddCategory.DataTextField = "name";
                        ddlAddCategory.DataValueField = "Id";
                        ddlAddCategory.DataBind();
                    }
                }

                using (SqlCommand cmdTutor = new SqlCommand("SELECT Id, ('T' + RIGHT('000'+CAST(Id AS VARCHAR), 3) + '-' + fname) as FullTutorName FROM [user] WHERE role_id = 3", conn))
                {
                    using (SqlDataReader rdrTutor = cmdTutor.ExecuteReader())
                    {
                        ddlAddTutor.DataSource = rdrTutor;
                        ddlAddTutor.DataTextField = "FullTutorName";
                        ddlAddTutor.DataValueField = "Id";
                        ddlAddTutor.DataBind();
                    }
                }
            }
        }

        protected void btnSaveCourse_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

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