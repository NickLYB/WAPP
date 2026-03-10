using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Staff
{
    public partial class AddResource : System.Web.UI.Page
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

                using (SqlCommand cmdType = new SqlCommand("SELECT Id, name FROM [resourceType]", conn))
                {
                    using (SqlDataReader rdrType = cmdType.ExecuteReader())
                    {
                        ddlAddType.DataSource = rdrType;
                        ddlAddType.DataTextField = "name";
                        ddlAddType.DataValueField = "Id";
                        ddlAddType.DataBind();
                    }
                }

                // FIXED: SQL query now formats as "T004-John Doe"
                using (SqlCommand cmdTutor = new SqlCommand(@"SELECT Id, 
                                                                     ('T' + RIGHT('000'+CAST(Id AS VARCHAR), 3) + '-' + fname + ' ' + lname) as FullTutorName 
                                                              FROM [user] 
                                                              WHERE role_id = 3", conn))
                {
                    using (SqlDataReader rdrTutor = cmdTutor.ExecuteReader())
                    {
                        ddlAddTutor.DataSource = rdrTutor;
                        ddlAddTutor.DataTextField = "FullTutorName";
                        ddlAddTutor.DataValueField = "Id";
                        ddlAddTutor.DataBind();
                    }
                }

                using (SqlCommand cmdCourse = new SqlCommand("SELECT Id, title FROM [course]", conn))
                {
                    using (SqlDataReader rdrCourse = cmdCourse.ExecuteReader())
                    {
                        ddlAddCourse.DataSource = rdrCourse;
                        ddlAddCourse.DataTextField = "title";
                        ddlAddCourse.DataValueField = "Id";
                        ddlAddCourse.DataBind();
                    }
                }
            }
        }

        protected void btnSaveResource_Click(object sender, EventArgs e)
        {
            // 1. Manual Server-Side Validation Check
            if (string.IsNullOrWhiteSpace(txtAddLink.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please provide a valid Resource Link.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"INSERT INTO [learningResource] 
                                   (course_id, created_at, resource_type, resource_link, tutor_id) 
                                   VALUES (@course, GETDATE(), @type, @link, @tutor)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@course", ddlAddCourse.SelectedValue);
                        cmd.Parameters.AddWithValue("@type", ddlAddType.SelectedValue);
                        cmd.Parameters.AddWithValue("@link", txtAddLink.Text.Trim());
                        cmd.Parameters.AddWithValue("@tutor", ddlAddTutor.SelectedValue);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                Response.Redirect("LearningResourceManagement.aspx?msg=success");
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error adding resource: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }
    }
}