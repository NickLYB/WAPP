using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class CourseDetail : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            if (!IsPostBack)
            {
                string courseId = Request.QueryString["id"];
                if (string.IsNullOrEmpty(courseId)) Response.Redirect("Study.aspx");

                int studentId = Convert.ToInt32(Session["UserId"]);

                LoadCourseOverview(courseId);
                LoadFeedback(courseId);

                bool isEnrolled = IsStudentEnrolled(courseId, studentId);

                if (isEnrolled)
                {
                    LoadResources(courseId);

                    btnStart.Text = "Continue Learning";
                    btnStart.CssClass = "btn btn-success w-100 btn-start shadow-sm";
                }
                else
                {
                    ShowPreviewResources(courseId);
                    pnlLockedMessage.Visible = true;

                    btnStart.Text = "Start Learning Now";
                    btnStart.CssClass = "btn btn-primary w-100 btn-start shadow-sm";
                }

            }
        }

        private void LoadCourseOverview(string id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Join course with courseType and user for complete metadata
                string query = @"
                SELECT
                    c.title, 
                    c.description, 
                    c.duration_minutes,
                    c.skill_level,
                    c.average_rating,
                    ct.name as Category, 
                    (u.fname + ' ' + u.lname) as TutorName,
                    (SELECT COUNT(*)
                     FROM enrollment e
                     WHERE e.course_id = c.Id AND e.status = 'ENROLLED') 
                     as EnrolledCount
                FROM course c
                JOIN[user] u ON c.tutor_id = u.Id
                JOIN courseType ct ON c.course_type_id = ct.Id
                WHERE c.Id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    litCourseTitle.Text = dr["title"].ToString();
                    litTutorName.Text = dr["TutorName"].ToString();
                    litCategory.Text = dr["Category"].ToString();
                    litDescription.Text = dr["description"].ToString();

                    int minutes = Convert.ToInt32(dr["duration_minutes"]);
                    litDuration.Text = FormatDuration(minutes);

                    litSkillLevel.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dr["skill_level"].ToString().ToLower());
                    // Enrolled count
                    litEnrolledCount.Text = dr["EnrolledCount"].ToString();

                    // Average rating stars
                    if (dr["average_rating"] != DBNull.Value)
                    {
                        int rating = Convert.ToInt32(dr["average_rating"]);
                        litRatingStars.Text = GenerateStars(rating);
                    }
                    else
                    {
                        litRatingStars.Text = "No ratings yet";
                    }

                }
            }
        }

        private bool IsStudentEnrolled(string courseId, int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT COUNT(*) 
            FROM enrollment 
            WHERE student_id = @sid 
            AND course_id = @cid 
            AND status = 'ENROLLED'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@cid", courseId);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }

        private void ShowPreviewResources(string id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT TOP 3 lr.Id, rt.name as TypeName, lr.resource_type
            FROM learningResource lr
            JOIN resourceType rt ON lr.resource_type = rt.Id
            WHERE lr.course_id = @id
            ORDER BY lr.created_at ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptResources.DataSource = dt;
                rptResources.DataBind();
            }
        }



        private void LoadResources(string id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT lr.Id, rt.name as TypeName, lr.resource_type
                    FROM learningResource lr
                    JOIN resourceType rt ON lr.resource_type = rt.Id
                    WHERE lr.course_id = @id
                    ORDER BY lr.created_at ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptResources.DataSource = dt;
                rptResources.DataBind();
            }
        }

        private void LoadFeedback(string id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Pulls approved feedback linked to any resource in this course
                string query = @"
                    SELECT f.rating, f.comment, (u.fname + ' ' + u.lname) as StudentName, f.created_at
                    FROM feedback f
                    JOIN [user] u ON f.student_id = u.Id
                    JOIN learningResource lr ON f.resource_id = lr.Id
                    WHERE lr.course_id = @id AND f.status = 'APPROVED'
                    ORDER BY f.created_at DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptFeedback.DataSource = dt;
                rptFeedback.DataBind();
            }
        }

        private string FormatDuration(int minutes)
        {
            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;

            if (hours > 0)
                return $"{hours}h {remainingMinutes}m";
            else
                return $"{remainingMinutes}m";
        }

        private string GenerateStars(int rating)
        {
            string stars = "";

            for (int i = 1; i <= 5; i++)
            {
                if (i <= rating)
                    stars += "<i class='bi bi-star-fill text-warning'></i>";
                else
                    stars += "<i class='bi bi-star text-warning'></i>";
            }

            return stars;
        }

        protected string GetIcon(string typeId)
        {
            switch (typeId)
            {
                case "1": return "bi bi-play-circle-fill text-primary";
                case "3": return "bi bi-patch-question-fill text-warning";
                case "4": return "bi bi-file-earmark-ppt-fill text-danger";
                default: return "bi bi-file-text-fill text-secondary";
            }
        }

        protected void btnStart_Click(object sender, EventArgs e)
        {
            string courseId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(courseId)) return;

            int studentId = Convert.ToInt32(Session["UserId"]);

            bool isEnrolled = IsStudentEnrolled(courseId, studentId);

            if (!isEnrolled)
            {
                EnrollStudent(studentId, courseId);
            }

            int enrollmentId = GetEnrollmentId(studentId, courseId);
            int resourceId = GetContinueResource(enrollmentId, courseId);

            Response.Redirect("LessonView.aspx?resourceId=" + resourceId);
        }
        private void EnrollStudent(int studentId, string courseIdStr)
        {
            // 1. Convert courseId string to int safely
            if (!int.TryParse(courseIdStr, out int courseId))
            {
                // Handle error: course ID is not a valid number
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 2. Use 'IF NOT EXISTS' to prevent unique constraint crashes
                // This ensures we only insert if the student isn't already enrolled
                string query = @"
            IF NOT EXISTS (SELECT 1 FROM enrollment WHERE student_id = @sid AND course_id = @cid)
            BEGIN
                INSERT INTO enrollment (student_id, course_id, status)
                VALUES (@sid, @cid, 'ENROLLED')
            END";

                SqlCommand cmd = new SqlCommand(query, conn);

                // 3. Explicitly define parameter types to match the database
                cmd.Parameters.Add("@sid", SqlDbType.Int).Value = studentId;
                cmd.Parameters.Add("@cid", SqlDbType.Int).Value = courseId;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    // 4. Redirect to the actual learning view after successful enrollment
                    Response.Redirect("LessonView.aspx?id=" + courseId);
                }
                catch (SqlException ex)
                {
                    // Log error (ex.Message)
                }
            }
        }
        private int GetEnrollmentId(int studentId, string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT Id FROM enrollment
                                 WHERE student_id=@sid 
                                 AND course_id=@cid 
                                 AND status='ENROLLED'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@cid", courseId);

                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        private int GetContinueResource(int enrollmentId, string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Last accessed
                string lastQuery = @"SELECT TOP 1 rp.resource_id
                                     FROM resourceProgress rp
                                     JOIN learningResource lr ON rp.resource_id = lr.Id
                                     WHERE rp.enrollment_id=@eid
                                     AND lr.course_id=@cid
                                     ORDER BY rp.last_accessed DESC";

                SqlCommand cmd = new SqlCommand(lastQuery, conn);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                cmd.Parameters.AddWithValue("@cid", courseId);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return Convert.ToInt32(result);

                // If never accessed → first lesson
                string firstQuery = @"SELECT TOP 1 Id
                                      FROM learningResource
                                      WHERE course_id=@cid
                                      ORDER BY created_at ASC";

                SqlCommand cmd2 = new SqlCommand(firstQuery, conn);
                cmd2.Parameters.AddWithValue("@cid", courseId);

                object firstLesson = cmd2.ExecuteScalar();
                return firstLesson != null && firstLesson != DBNull.Value ? Convert.ToInt32(firstLesson) : 0;
            }
        }
    }
}