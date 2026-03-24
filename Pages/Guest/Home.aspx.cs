using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Guest
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPlatformStats();
                BindPopularCourses();
                BindTestimonials();
            }
        }

        // Triggered by the "Get Started Free" button in the Hero Section
        protected void btnGetStarted_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Login.aspx");
        }

        // Triggered by the "Explore Courses" button in the Hero Section
        protected void btnExploreCourses_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Guest/Courses.aspx");
        }

        // Triggered by the "Join Now - It's Free!" button in the CTA Section at the bottom
        protected void btnJoinNow_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Login.aspx");
        }

        private void BindPlatformStats()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            string sql = @"
                SELECT 
                    (SELECT COUNT(u.Id) FROM [user] u INNER JOIN [role] r ON u.role_id = r.Id WHERE r.name = 'Student') AS StudentCount,
                    (SELECT COUNT(c.Id) FROM course c WHERE c.status = 'PUBLISHED') AS CourseCount,
                    (SELECT COUNT(u.Id) FROM [user] u INNER JOIN [role] r ON u.role_id = r.Id WHERE r.name = 'Tutor') AS TutorCount";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    try
                    {
                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                litStudentCount.Text = dr["StudentCount"].ToString();
                                litCourseCount.Text = dr["CourseCount"].ToString();
                                litTutorCount.Text = dr["TutorCount"].ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        litStudentCount.Text = "50";
                        litCourseCount.Text = "10";
                        litTutorCount.Text = "5";
                    }
                }
            }
        }

        private void BindPopularCourses()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            string sql = @"
        SELECT TOP 3 
            c.Id, 
            c.title, 
            c.description, 
            c.duration_minutes AS duration,
            c.image_path,
            COUNT(e.Id) AS EnrollmentCount
        FROM course c
        LEFT JOIN enrollment e ON c.Id = e.course_id
        WHERE c.status = 'PUBLISHED'
        GROUP BY c.Id, c.title, c.description, c.duration_minutes, c.image_path, c.created_at
        ORDER BY EnrollmentCount DESC, c.created_at DESC";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptPopularCourses.DataSource = dt;
                            rptPopularCourses.DataBind();
                            lblNoCourses.Visible = false;
                        }
                        else
                        {
                            rptPopularCourses.Visible = false;
                            lblNoCourses.Visible = true;
                            lblNoCourses.Text = "Check back soon for exciting new courses!";
                        }
                    }
                    catch (Exception ex)
                    {
                        rptPopularCourses.Visible = false;
                        lblNoCourses.Visible = true;
                        lblNoCourses.Text = "Error loading courses: " + ex.Message;
                    }
                }
            }
        }

        private void BindTestimonials()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            // Gets the top 3 approved feedbacks with 4 or 5 stars, sorted randomly
            string sql = @"
                SELECT TOP 3 
                    f.comment, 
                    f.rating, 
                    u.fname, 
                    u.lname, 
                    UPPER(SUBSTRING(u.fname, 1, 1) + SUBSTRING(u.lname, 1, 1)) AS initials
                FROM feedback f
                INNER JOIN [user] u ON f.student_id = u.Id
                WHERE f.status = 'APPROVED' 
                  AND f.rating >= 4 
                  AND f.comment IS NOT NULL 
                  AND DATALENGTH(f.comment) > 0
                ORDER BY NEWID()";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptTestimonials.DataSource = dt;
                            rptTestimonials.DataBind();
                            lblNoTestimonials.Visible = false;
                        }
                        else
                        {
                            rptTestimonials.Visible = false;
                            lblNoTestimonials.Visible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        rptTestimonials.Visible = false;
                        lblNoTestimonials.Visible = true;
                        lblNoTestimonials.Text = "Error loading testimonials.";
                    }
                }
            }
        }

        protected string GetStars(int rating)
        {
            string stars = "";
            for (int i = 1; i <= 5; i++)
            {
                if (i <= rating)
                    stars += "<i class='bi bi-star-fill'></i>";
                else
                    stars += "<i class='bi bi-star'></i>";
            }
            return stars;
        }
    }
}