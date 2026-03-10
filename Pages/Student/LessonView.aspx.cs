using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class LessonView : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        int studentId;
        int resourceId;
        string courseId;
        int enrollmentId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
            {
                Response.Redirect("~/Pages/Student/Home.aspx");
            }
            else{
                studentId = Convert.ToInt32(Session["UserId"]);
            }
            if (Request.QueryString["resourceId"] == null || !int.TryParse(Request.QueryString["resourceId"], out resourceId))
                Response.Redirect("Study.aspx");

            courseId = GetCourseIdByResource(resourceId);
            if (courseId == null)
                Response.Redirect("Study.aspx");

            enrollmentId = GetEnrollmentId(studentId, courseId);
            if (enrollmentId == 0)
                Response.Redirect("CourseDetail.aspx?id=" + courseId);

            if (!IsPostBack)
            {
                
                LoadCourseTitle(courseId);
                LoadLessonList(courseId);
                LoadLessonContent(resourceId);

                TrackLessonVisit(enrollmentId, resourceId);

                UpdateProgressBar();

                CheckExistingCourseFeedback();
                LoadCommunityFeedback(resourceId);
            }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // 1. Explicitly bind to the StudentMap provider instead of the default SiteMap
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
            if (!path.EndsWith("/LessonView.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/LessonView", StringComparison.OrdinalIgnoreCase))
            {
                return null; // Return null to let the provider handle other pages normally
            }

            SiteMapProvider provider = (SiteMapProvider)sender;

            // 2. Try to get CurrentNode. If it's null (due to query strings or .aspx), force it to find the base URL.
            SiteMapNode current = provider.CurrentNode ?? provider.FindSiteMapNode("~/Pages/Student/LessonView");

            if (current == null) return null;

            // Clone the node and its ancestors so we don't overwrite the global sitemap
            SiteMapNode clone = current.Clone(true);

            if (int.TryParse(ctx.Request.QueryString["resourceId"], out int currentResourceId))
            {
                string cId = GetCourseIdByResource(currentResourceId);
                if (!string.IsNullOrEmpty(cId))
                {
                    string courseTitle = GetCourseNameForBreadcrumb(cId);
                    if (!string.IsNullOrWhiteSpace(courseTitle))
                    {
                        // 3. Dynamically update the title and ensure the URL keeps the resourceId
                        clone.Title = courseTitle;
                        clone.Url = $"~/Pages/Student/LessonView.aspx?resourceId={currentResourceId}";
                    }
                }
            }

            return clone;
        }

        // Helper method to fetch the course title safely
        private string GetCourseNameForBreadcrumb(string cId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT title FROM course WITH (NOLOCK) WHERE Id=@id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", cId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? result.ToString() : null;
                }
            }
        }
        private void LoadCourseTitle(string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT title FROM course WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", courseId);
                conn.Open();
                litCourseTitle.Text = cmd.ExecuteScalar()?.ToString() ?? "Course";
            }
        }

        private void LoadLessonList(string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // UPDATED SQL: Added lr.sequence_order to the ORDER BY clause
                string query = @"
            SELECT lr.Id, rt.name as TypeName,
                   CASE 
                       WHEN lr.quiz_id IS NULL AND rp.resource_id IS NOT NULL THEN 1
                       WHEN lr.quiz_id IS NOT NULL AND EXISTS (
                           SELECT 1 FROM quizAttempt qa 
                           WHERE qa.quiz_id = lr.quiz_id 
                           AND qa.enrollment_id = @eid 
                           AND qa.status = 'GRADED' 
                           AND qa.score >= 50
                       ) THEN 1
                       ELSE 0 
                   END AS IsCompleted
            FROM learningResource lr
            JOIN resourceType rt ON lr.resource_type = rt.Id
            LEFT JOIN resourceProgress rp ON rp.resource_id = lr.Id AND rp.enrollment_id = @eid
            WHERE lr.course_id = @cid
            ORDER BY lr.sequence_order ASC, lr.created_at ASC"; // Sorted by sequence first

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@cid", courseId);
                da.SelectCommand.Parameters.AddWithValue("@eid", enrollmentId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (!dt.Columns.Contains("IsAccessible"))
                    dt.Columns.Add("IsAccessible", typeof(bool));

                // Logic to unlock the next lesson only if the current one is completed
                bool previousCompleted = true;
                foreach (DataRow row in dt.Rows)
                {
                    row["IsAccessible"] = previousCompleted;
                    bool isCompleted = Convert.ToBoolean(row["IsCompleted"]);

                    if (!isCompleted)
                        previousCompleted = false;
                }

                rptLessons.DataSource = dt;
                rptLessons.DataBind();
            }
        }

        private void LoadLessonContent(int resourceId)
        {
            int? pendingQuizId = null; // Store the quiz ID to load it AFTER we close the connection

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT resource_link, note, quiz_id FROM learningResource WHERE Id=@rid";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@rid", resourceId);
                    conn.Open();

                    // USING block ensures the DataReader is instantly closed when finished
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string link = dr["resource_link"].ToString();
                            string note = dr["note"].ToString();

                            if (link.Contains("youtube.com") || link.Contains("youtu.be"))
                                litVideoPlayer.Text = $"<iframe src='{link}' allowfullscreen></iframe>";
                            else
                                litVideoPlayer.Text = $"<div class='p-5 text-center'><a href='{link}' class='btn btn-primary rounded-pill'>Download File</a></div>";

                            litLessonNote.Text = string.IsNullOrEmpty(note) ? "No detailed notes available for this lesson." : note;
                            lblCurrentLessonName.Text = "Lesson ID: " + resourceId;

                            if (dr["quiz_id"] != DBNull.Value)
                            {
                                phQuizTrigger.Visible = true;
                                pendingQuizId = Convert.ToInt32(dr["quiz_id"]);
                                ViewState["CurrentQuizId"] = pendingQuizId;
                            }
                            else
                            {
                                phQuizTrigger.Visible = false;
                            }
                        }
                    } 
                }
            } 

            if (pendingQuizId.HasValue)
            {
                DisplayQuizResult(pendingQuizId.Value);
            }
        }

        private void DisplayQuizResult(int qid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT TOP 1 score, status FROM quizAttempt WITH (NOLOCK) 
                        WHERE quiz_id = @qid AND enrollment_id = @eid AND status = 'GRADED' 
                        ORDER BY finished_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@qid", qid);
                    cmd.Parameters.AddWithValue("@eid", enrollmentId);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            // A GRADED attempt was found!
                            pnlQuizResult.Visible = true;
                            pnlQuizPrompt.Visible = false;
                            btnReviewQuiz.Visible = true;

                            int score = Convert.ToInt32(dr["score"]);
                            litScoreText.Text = score + "%";

                            if (score >= 50)
                            {
                                spanStatus.InnerText = "Passed";
                                spanStatus.Attributes["class"] = "ec-status-pill ec-status-active mb-2";
                                litResultMessage.Text = "Great job! You've mastered this lesson.";
                                btnProceedToQuiz.Visible = false;
                                btnRetakeQuiz.Visible = false;
                            }
                            else
                            {
                                spanStatus.InnerText = "Failed";
                                spanStatus.Attributes["class"] = "ec-status-pill ec-status-danger mb-2";
                                litResultMessage.Text = "You didn't reach the 50% pass mark.";
                                btnProceedToQuiz.Visible = false;
                                btnRetakeQuiz.Visible = true;
                            }
                        }
                        else
                        {
                            // No graded attempts found. Show the start prompt.
                            pnlQuizResult.Visible = false;
                            pnlQuizPrompt.Visible = true;
                            btnProceedToQuiz.Visible = true;
                            btnRetakeQuiz.Visible = false;
                            btnReviewQuiz.Visible = false; // Explicitly hide the review button just in case!
                        }
                    }
                }
            }
        }


        protected void btnProceedToQuiz_Click(object sender, EventArgs e)
        {
            if (ViewState["CurrentQuizId"] != null)
            {
                string qid = ViewState["CurrentQuizId"].ToString();
                // Check if enrollmentId is actually assigned here!
                Response.Redirect($"Quizzes.aspx?quizId={qid}&enrollmentId={enrollmentId}");
            }
        }

        protected string GetIcon(object isAccessible, object isCompleted)
        {
            bool accessible = (isAccessible != null && isAccessible != DBNull.Value) && Convert.ToBoolean(isAccessible);
            bool completed = (isCompleted != null && isCompleted != DBNull.Value) && Convert.ToBoolean(isCompleted);

            if (!accessible)
                return "bi bi-lock-fill text-secondary opacity-50";

            if (completed)
                return "bi bi-check-circle-fill text-success";

            return "bi bi-play-circle-fill text-primary";
        }

        protected string GetLessonCSS(object isAccessible, object lessonId)
        {
            bool accessible = isAccessible != DBNull.Value && Convert.ToBoolean(isAccessible);
            string currentResourceId = Request.QueryString["resourceId"];

            string cssClass = "lesson-btn ";

            if (!accessible)
                cssClass += "lesson-locked ";

            if (lessonId.ToString() == currentResourceId)
                cssClass += "active-lesson ";

            return cssClass;
        }

        protected void lnkLesson_Click(object sender, EventArgs e)
        {
            int newResourceId = Convert.ToInt32((sender as LinkButton).CommandArgument);
            Response.Redirect("LessonView.aspx?resourceId=" + newResourceId);
        }

        private void TrackLessonVisit(int enrollmentId, int resourceId)
        {
            bool hasQuiz = LessonHasQuiz(resourceId);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // CHANGE: If there's NO quiz, we set completed_at immediately.
                // If there IS a quiz, we leave completed_at as NULL (Quizzes.aspx will handle it later).
                string query = @"
            IF NOT EXISTS (SELECT 1 FROM resourceProgress WHERE enrollment_id=@eid AND resource_id=@rid)
                INSERT INTO resourceProgress (enrollment_id, resource_id, last_accessed, completed_at) 
                VALUES (@eid, @rid, GETDATE(), @comp)
            ELSE
                UPDATE resourceProgress 
                SET last_accessed = GETDATE(), 
                    completed_at = ISNULL(completed_at, @comp)
                WHERE enrollment_id=@eid AND resource_id=@rid";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                cmd.Parameters.AddWithValue("@rid", resourceId);

                // If no quiz, comp = current date. If has quiz, comp = DBNull (staying null).
                cmd.Parameters.AddWithValue("@comp", hasQuiz ? (object)DBNull.Value : DateTime.Now);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Helper method to check if we should update progress bar immediately
        private bool LessonHasQuiz(int rid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT quiz_id FROM learningResource WHERE Id=@rid", conn);
                cmd.Parameters.AddWithValue("@rid", rid);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != DBNull.Value && result != null;
            }
        }

        private void UpdateProgressBar()
        {
            int totalLessons = 0;
            int completedLessons = 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                // 1. Total number of lessons in this course
                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM learningResource WHERE course_id=@cid", conn);
                cmdTotal.Parameters.AddWithValue("@cid", courseId);
                totalLessons = (int)cmdTotal.ExecuteScalar();

                // 2. ONLY count rows where the student has actually finished (completed_at is not null)
                SqlCommand cmdCompleted = new SqlCommand(@"
                SELECT COUNT(*) FROM resourceProgress 
                WHERE enrollment_id=@eid AND completed_at IS NOT NULL", conn);
                cmdCompleted.Parameters.AddWithValue("@eid", enrollmentId);
                completedLessons = (int)cmdCompleted.ExecuteScalar();
            }

            // 3. Update the UI
            int progress = (totalLessons > 0) ? (int)((double)completedLessons / totalLessons * 100) : 0;
            progressBar.Style["width"] = progress + "%";
            lblProgressPercent.Text = progress + "%";

            if (progress >= 100)
            {
                lblCompletionMessage.Text = "Congratulations! You have completed this course.";
                lblCompletionMessage.Visible = true;
                btnRateCourse.Visible = true;
            }
        }

        private int GetEnrollmentId(int studentId, string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT Id FROM enrollment WHERE student_id=@sid AND course_id=@cid AND status='ENROLLED'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@cid", courseId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private string GetCourseIdByResource(int resourceId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // FIX 1: Added WITH (NOLOCK) to prevent database timeouts
                string query = "SELECT course_id FROM learningResource WITH (NOLOCK) WHERE Id=@rid";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@rid", resourceId);
                    conn.Open();

                    // FIX 2: Replaced the '?.ToString()' with a safe, traditional check
                    // This works on ALL versions of C# and prevents object reference errors
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return result.ToString();
                    }

                    return null; // Return null safely if nothing was found
                }
            }
        }

        private void CheckExistingCourseFeedback()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT rating, comment FROM feedback WHERE student_id=@sid AND course_id=@cid AND resource_id IS NULL";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@cid", courseId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    ddlCourseRating.SelectedValue = dr["rating"].ToString();
                    txtCourseComment.Text = dr["comment"].ToString();
                    btnSubmitCourseFeedback.Text = "Update Course Review";
                    pnlRemoveCourseFeedback.Visible = true;
                }
                else
                {
                    btnSubmitCourseFeedback.Text = "Submit Course Review";
                    pnlRemoveCourseFeedback.Visible = false;
                }
            }
        }

        protected void btnSubmitFeedback_Click(object sender, EventArgs e)
        {
            int resId = Convert.ToInt32(Request.QueryString["resourceId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    IF EXISTS (SELECT 1 FROM feedback WHERE student_id=@sid AND resource_id=@rid)
                    BEGIN
                        UPDATE feedback SET rating=@rating, comment=@comment, created_at=GETDATE() 
                        WHERE student_id=@sid AND resource_id=@rid
                    END
                    ELSE
                    BEGIN
                        -- Dynamically fetch both tutor and course ID to save alongside the resource!
                        DECLARE @tid INT = (SELECT tutor_id FROM learningResource WHERE Id=@rid)
                        DECLARE @cid INT = (SELECT course_id FROM learningResource WHERE Id=@rid)
        
                        INSERT INTO feedback (student_id, tutor_id, course_id, resource_id, rating, comment, status)
                        VALUES (@sid, @tid, @cid, @rid, @rating, @comment, 'APPROVED')
                    END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@rid", resId);
                cmd.Parameters.AddWithValue("@rating", ddlRating.SelectedValue);
                cmd.Parameters.AddWithValue("@comment", txtComment.Text);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            Response.Redirect(Request.RawUrl);
        }

        protected void btnDeleteFeedback_Click(object sender, EventArgs e)
        {
            int resId = Convert.ToInt32(Request.QueryString["resourceId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM feedback WHERE student_id=@sid AND resource_id=@rid", conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@rid", resId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            Response.Redirect(Request.RawUrl);
        }

        protected void btnSubmitCourseFeedback_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. Get the Tutor ID for this course
                string getTutorQuery = "SELECT tutor_id FROM course WHERE Id=@cid";
                SqlCommand tutorCmd = new SqlCommand(getTutorQuery, conn);
                tutorCmd.Parameters.AddWithValue("@cid", courseId);
                int tutorId = Convert.ToInt32(tutorCmd.ExecuteScalar());

                // 2. Save the feedback (resource_id is set to NULL)
                string query = @"
                    IF EXISTS (SELECT 1 FROM feedback WHERE student_id=@sid AND course_id=@cid AND resource_id IS NULL)
                    BEGIN
                        UPDATE feedback SET rating=@rating, comment=@comment, created_at=GETDATE() 
                        WHERE student_id=@sid AND course_id=@cid AND resource_id IS NULL
                    END
                    ELSE
                    BEGIN
                        INSERT INTO feedback (student_id, tutor_id, course_id, resource_id, rating, comment, status)
                        VALUES (@sid, @tid, @cid, NULL, @rating, @comment, 'APPROVED')
                    END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@tid", tutorId);
                cmd.Parameters.AddWithValue("@cid", courseId);
                cmd.Parameters.AddWithValue("@rating", ddlCourseRating.SelectedValue);
                cmd.Parameters.AddWithValue("@comment", txtCourseComment.Text);

                cmd.ExecuteNonQuery();
            }

            // 3. Recalculate Average Rating for the Course!
            RecalculateCourseAverageRating(courseId);

            Response.Redirect(Request.RawUrl);
        }

        protected void btnDeleteCourseFeedback_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Delete ONLY the course-level feedback (resource_id IS NULL)
                string query = "DELETE FROM feedback WHERE student_id=@sid AND course_id=@cid AND resource_id IS NULL";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@cid", courseId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Recalculate Average Rating since a review was deleted!
            RecalculateCourseAverageRating(courseId);

            Response.Redirect(Request.RawUrl);
        }

        private void RecalculateCourseAverageRating(string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Calculate the average ONLY from course-level feedback (resource_id IS NULL)
                string query = @"
                    DECLARE @AvgRating DECIMAL(3,2);
            
                    SELECT @AvgRating = AVG(CAST(rating AS DECIMAL(3,2))) 
                    FROM feedback 
                    WHERE course_id = @cid AND resource_id IS NULL AND status = 'APPROVED';

                    -- Update the course table if there is an average
                    IF @AvgRating IS NOT NULL
                    BEGIN
                        UPDATE course SET average_rating = @AvgRating WHERE Id = @cid;
                    END
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cid", courseId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadCommunityFeedback(int resId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT f.rating, f.comment, f.created_at, u.fname, u.lname 
            FROM feedback f 
            JOIN [user] u ON f.student_id = u.Id 
            WHERE f.resource_id = @rid AND f.status = 'APPROVED'
            ORDER BY f.created_at DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@rid", resId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptPublicFeedback.DataSource = dt;
                    rptPublicFeedback.DataBind();
                    phNoReviews.Visible = false;
                }
                else
                {
                    phNoReviews.Visible = true;
                }
            }
        }
        protected void btnReviewQuiz_Click(object sender, EventArgs e)
        {
            if (ViewState["CurrentQuizId"] != null)
            {
                string qid = ViewState["CurrentQuizId"].ToString();

                // Added the enrollmentId to the QueryString so QuizReview.aspx can catch it!
                Response.Redirect($"QuizReview.aspx?quizId={qid}&enrollmentId={enrollmentId}");
            }
        }
    }
}