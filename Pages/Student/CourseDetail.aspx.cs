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
using WAPP.Masters;
using WAPP.Utils; // Accesses SystemLogService and LogLevel

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

                    int enrollmentId = GetEnrollmentId(studentId, courseId);
                    int progress = GetCourseProgress(enrollmentId, courseId);

                    if (progress >= 100)
                    {
                        // Course is Finished
                        btnStart.Text = "Review Course";
                        btnStart.CssClass = "btn btn-success w-100 btn-start shadow-sm"; // You can use green here!
                    }
                    else
                    {
                        // Course is In Progress
                        btnStart.Text = "Continue Learning";
                        btnStart.CssClass = "btn btn-primary w-100 btn-start shadow-sm"; // Keeps the standard primary blue
                    }
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

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var provider = SiteMap.Providers["StudentMap"];
            if (provider != null)
            {
                provider.SiteMapResolve += SiteMap_Resolve;
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            var provider = SiteMap.Providers["StudentMap"];
            if (provider != null)
            {
                provider.SiteMapResolve -= SiteMap_Resolve;
            }
            base.OnUnload(e);
        }

        private SiteMapNode SiteMap_Resolve(object sender, SiteMapResolveEventArgs e)
        {
            var ctx = e.Context;
            if (ctx?.Request == null) return null;

            string path = ctx.Request.Path;
            if (!path.EndsWith("/CourseDetail.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/CourseDetail", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            SiteMapProvider provider = (SiteMapProvider)sender;
            string courseIdStr = ctx.Request.QueryString["id"];
            string sourcePage = ctx.Request.QueryString["source"]?.ToLower();

            if (string.IsNullOrEmpty(courseIdStr)) return null;

            // Base nodes
            SiteMapNode rootNode = new SiteMapNode(provider, "Home", "~/Pages/Student/Home.aspx", "Home");
            SiteMapNode targetNode = new SiteMapNode(provider, "CourseDetail",
                $"~/Pages/Student/CourseDetail.aspx?id={courseIdStr}", "Course Overview");

            // Build dynamic path
            if (sourcePage == "tutor")
            {
                // Path: Home -> Tutor Profile -> Course Overview
                string tId = GetTutorIdByCourse(courseIdStr);
                SiteMapNode tutorNode = new SiteMapNode(provider, "TutorProfile", $"~/Pages/Student/TutorProfile.aspx?id={tId}", "Tutor Profile");

                tutorNode.ParentNode = rootNode;
                targetNode.ParentNode = tutorNode;
            }
            else
            {
                // Default Path: Home -> Explore -> Course Overview
                SiteMapNode exploreNode = new SiteMapNode(provider, "Explore", "~/Pages/Student/Study.aspx", "Explore");
                exploreNode.ParentNode = rootNode;
                targetNode.ParentNode = exploreNode;
            }

            return targetNode;
        }

        private string GetTutorIdByCourse(string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT tutor_id FROM course WITH (NOLOCK) WHERE Id=@cid";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@cid", courseId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? result.ToString() : "";
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
                    c.tutor_id,
                    ct.name as Category, 
                    (u.fname + ' ' + u.lname) as TutorName,
                    (SELECT COUNT(*)
                     FROM enrollment e
                     WHERE e.course_id = c.Id AND e.status = 'ENROLLED') 
                     as EnrolledCount
                FROM course c
                JOIN [user] u ON c.tutor_id = u.Id
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

                    litSidebarTutorName.Text = litTutorName.Text;
                    int tutorId = Convert.ToInt32(dr["tutor_id"]);
                    hlTutorNameLink.NavigateUrl = $"TutorProfile.aspx?id={tutorId}";
                    hlTutorProfileBtn.NavigateUrl = $"TutorProfile.aspx?id={tutorId}";

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
            AND status IN ('ENROLLED', 'COMPLETED')";

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
                // Changed ORDER BY to sequence_order
                string query = @"
            SELECT TOP 3 lr.Id, rt.name as TypeName, lr.resource_type
            FROM learningResource lr
            JOIN resourceType rt ON lr.resource_type = rt.Id
            WHERE lr.course_id = @id
            ORDER BY lr.sequence_order ASC, lr.created_at ASC";

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
                // Changed ORDER BY to sequence_order
                string query = @"
            SELECT lr.Id, rt.name as TypeName, lr.resource_type
            FROM learningResource lr
            JOIN resourceType rt ON lr.resource_type = rt.Id
            WHERE lr.course_id = @id
            ORDER BY lr.sequence_order ASC, lr.created_at ASC";

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
                string query = @"
            SELECT f.rating, f.comment, (u.fname + ' ' + u.lname) as StudentName, f.created_at
            FROM feedback f
            JOIN [user] u ON f.student_id = u.Id
            WHERE f.course_id = @id AND f.resource_id IS NULL AND f.status IN ('APPROVED','PENDING')
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
                case "3": return "bi bi-file-earmark-pdf-fill text-danger";
                case "4": return "bi bi-file-earmark-ppt-fill text-danger";
                default: return "bi bi-file-text-fill text-secondary";
            }
        }

        protected void btnStart_Click(object sender, EventArgs e)
        {
            string courseId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(courseId)) return;

            int studentId = Convert.ToInt32(Session["UserId"]);

            try
            {
                bool isEnrolled = IsStudentEnrolled(courseId, studentId);

                if (!isEnrolled)
                {
                    EnrollStudent(studentId, courseId);
                }

                int enrollmentId = GetEnrollmentId(studentId, courseId);
                int resourceId = GetContinueResource(enrollmentId, courseId);

                string currentSource = Request.QueryString["source"]?.ToLower();
                string sourceToPass = (currentSource == "tutor") ? "tutor" : "course";

                Response.Redirect($"LessonView.aspx?resourceId={resourceId}&source={sourceToPass}");
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                SystemLogService.Write("COURSE_START_ERROR",
                    $"Error starting/continuing Course [{courseId}]: {ex.Message}",
                    LogLevel.ERROR, studentId);
            }
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
                // Ensures we only insert if the student isn't already enrolled
                string query = @"
                IF NOT EXISTS (SELECT 1 FROM enrollment WHERE student_id = @sid AND course_id = @cid)
                BEGIN
                    INSERT INTO enrollment (student_id, course_id, status)
                    VALUES (@sid, @cid, 'ENROLLED')
                END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@sid", SqlDbType.Int).Value = studentId;
                cmd.Parameters.Add("@cid", SqlDbType.Int).Value = courseId;

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // ---> LOGGING ADDED: INFO (Business Metric: Course Enrolled)
                    if (rowsAffected > 0)
                    {
                        SystemLogService.Write("STUDENT_ENROLLED",
                            $"Student successfully enrolled in Course ID [{courseId}].",
                            LogLevel.INFO, studentId);
                    }
                }
                catch (SqlException ex)
                {
                    // ---> LOGGING ADDED: ERROR (Database failure during enrollment)
                    SystemLogService.Write("ENROLLMENT_DB_ERROR",
                        $"DB Error enrolling student in Course [{courseId}]: {ex.Message}",
                        LogLevel.ERROR, studentId);
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
                                 AND status IN ('ENROLLED', 'COMPLETED')";

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
            // 1. Check if course is 100% complete first!
            int progress = GetCourseProgress(enrollmentId, courseId);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // If 100% complete, ALWAYS start from Lesson 1
                if (progress >= 100)
                {
                    string firstQuery = @"SELECT TOP 1 Id FROM learningResource 
                                          WHERE course_id=@cid 
                                          ORDER BY sequence_order ASC, created_at ASC";
                    SqlCommand cmdFirst = new SqlCommand(firstQuery, conn);
                    cmdFirst.Parameters.AddWithValue("@cid", courseId);
                    object firstLesson = cmdFirst.ExecuteScalar();
                    return firstLesson != null && firstLesson != DBNull.Value ? Convert.ToInt32(firstLesson) : 0;
                }

                // Otherwise, resume where they left off
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

                // If NO progress found at all, get the VERY FIRST lesson
                string fallbackQuery = @"SELECT TOP 1 Id FROM learningResource WHERE course_id=@cid ORDER BY sequence_order ASC, created_at ASC";
                SqlCommand cmd2 = new SqlCommand(fallbackQuery, conn);
                cmd2.Parameters.AddWithValue("@cid", courseId);
                object fallbackLesson = cmd2.ExecuteScalar();
                return fallbackLesson != null && fallbackLesson != DBNull.Value ? Convert.ToInt32(fallbackLesson) : 0;
            }
        }

        private int GetCourseProgress(int enrollmentId, string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        (SELECT COUNT(*) FROM learningResource lr WHERE lr.course_id = @cid) as TotalLessons,
                        (SELECT COUNT(*) FROM resourceProgress rp
                         JOIN learningResource lr2 ON rp.resource_id = lr2.Id
                         WHERE rp.enrollment_id = @eid AND rp.completed_at IS NOT NULL AND lr2.course_id = @cid) as CompletedLessons";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cid", courseId);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        int total = Convert.ToInt32(dr["TotalLessons"]);
                        int completed = Convert.ToInt32(dr["CompletedLessons"]);

                        if (total > 0)
                        {
                            return (completed * 100) / total;
                        }
                    }
                }
                return 0;
            }
        }
    }
}