using System;
using System.Web.UI;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace WAPP.Pages.Staff
{
    public partial class StaffDashboard : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Login.aspx");
            }
            else
            {
                if (!IsPostBack)
                {
                    string fullName = Session["UserName"].ToString();
                    lblStaffName.Text = fullName.Split(' ')[0];

                    BindSystemStats();
                    BindFeedbackGrid();
                }
            }
        }

        protected void btnAddCourse_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Staff/AddCourse.aspx");
        }

        protected void btnAddResource_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Staff/AddResource.aspx");
        }

        // This method handles the redirect if you are using an <asp:LinkButton> or <asp:Button>
        protected void btnViewActiveCourses_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Staff/CourseManagement.aspx?status=PUBLISHED");
        }

        private void BindSystemStats()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Pending Tutors
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [tutorApplication] WHERE status = 'PENDING'", conn))
                    {
                        lblPendingTutors.Text = cmd.ExecuteScalar().ToString();
                    }
                }
                catch
                {
                    try
                    {
                        using (SqlCommand fbCmd = new SqlCommand("SELECT COUNT(*) FROM [user] WHERE role_id = 3 AND status = 'PENDING'", conn))
                        {
                            lblPendingTutors.Text = fbCmd.ExecuteScalar().ToString();
                        }
                    }
                    catch { lblPendingTutors.Text = "0"; }
                }

                // 2. Total Students
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [user] WHERE role_id = 4", conn))
                    {
                        lblTotalStudents.Text = cmd.ExecuteScalar().ToString();
                    }
                }
                catch { lblTotalStudents.Text = "0"; }

                // 3. Total Tutors
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [user] WHERE role_id = 3", conn))
                    {
                        lblTotalTutors.Text = cmd.ExecuteScalar().ToString();
                    }
                }
                catch { lblTotalTutors.Text = "0"; }

                // 4. Active Courses (Strictly counts 'PUBLISHED')
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [course] WHERE status = 'PUBLISHED'", conn))
                    {
                        lblActiveCourses.Text = cmd.ExecuteScalar().ToString();
                    }
                }
                catch { lblActiveCourses.Text = "0"; }
            }
        }

        private void BindFeedbackGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Pulls TOP 8 newest feedbacks and formats names beautifully
                string sql = @"SELECT TOP 8 
                                      f.created_at, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname) AS TutorName,
                                      (c.title + ' - ' + ISNULL(lr.title, 'Unnamed Resource')) AS ResourceTitle,
                                      f.rating,
                                      f.comment,
                                      f.status
                               FROM [feedback] f
                               INNER JOIN [user] u ON f.tutor_id = u.Id
                               INNER JOIN [learningResource] lr ON f.resource_id = lr.Id
                               INNER JOIN [course] c ON lr.course_id = c.Id
                               ORDER BY f.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvFeedback.DataSource = dt;
                        gvFeedback.DataBind();
                    }
                }
            }
        }
    }
}