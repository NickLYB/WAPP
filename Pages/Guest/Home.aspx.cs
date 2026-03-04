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
                // Any initial page load logic for the guest home page can go here.
                // For a static landing page, this can usually be left empty.
                BindPlatformStats();
                BindPopularCourses();
            }
        }

        // Triggered by the "Get Started Free" button in the Hero Section
        protected void btnGetStarted_Click(object sender, EventArgs e)
        {
            // Redirect the user to your registration or login page.
            // If your login modal is on the master page, you could alternatively 
            // use a client-side trigger, but since this is a server button, redirecting is standard.
            Response.Redirect("~/Login.aspx");
        }

        // Triggered by the "Explore Courses" button in the Hero Section
        protected void btnExploreCourses_Click(object sender, EventArgs e)
        {
            // Redirect the user to a public course catalog page if you have one,
            // or route them to login if they must be authenticated to browse.
            Response.Redirect("~/Pages/Guest/Courses.aspx");
        }

        // Triggered by the "Join Now - It's Free!" button in the CTA Section at the bottom
        protected void btnJoinNow_Click(object sender, EventArgs e)
        {
            // Redirect the user to your registration page
            Response.Redirect("~/Login.aspx");
        }
        private void BindPlatformStats()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            // We use subqueries to get all 3 counts in a single database trip for maximum performance.
            // It joins the user and role tables to dynamically find the correct counts for Students and Tutors.
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
                                // Assign the database counts to the frontend literals
                                litStudentCount.Text = dr["StudentCount"].ToString();
                                litCourseCount.Text = dr["CourseCount"].ToString();
                                litTutorCount.Text = dr["TutorCount"].ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fallback to baseline numbers if the database fails to prevent a blank UI
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

            // 1. We use LEFT JOIN so courses with 0 enrollments are still returned.
            // 2. We use duration_minutes AS duration so the front-end Eval("duration") still works perfectly.
            // 3. We check for status = 'PUBLISHED' matching your exact CHECK constraint.
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
                        // Temporarily display the error so you know if something is wrong with the SQL
                        rptPopularCourses.Visible = false;
                        lblNoCourses.Visible = true;
                        lblNoCourses.Text = "Error loading courses: " + ex.Message;
                    }
                }
            }
        }
    }
}