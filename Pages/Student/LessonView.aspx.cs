using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                Response.Redirect("~/Pages/Guest/Home.aspx");
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
                LoadLessons(courseId);
                LoadLesson(resourceId);

                UpdateProgress(enrollmentId, resourceId);

                UpdateOverallProgress();

                CheckExistingFeedback();
                LoadCommunityFeedback(resourceId);
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

        private void LoadLessons(string courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // NEW SQL LOGIC: 
                // 1. If there is NO quiz (quiz_id IS NULL), then simply viewing it (resourceProgress) counts as complete.
                // 2. If there IS a quiz, it only counts as complete if they have a 'GRADED' attempt with a score >= 50.
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
            ORDER BY lr.created_at ASC";

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

        private void LoadLesson(int resourceId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // UPDATED QUERY: Directly selects the quiz_id from your new column
                string query = "SELECT resource_link, note, quiz_id FROM learningResource WHERE Id=@rid";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@rid", resourceId);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

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

                    // UPDATED LOGIC: Shows the button ONLY if quiz_id is NOT NULL
                    if (dr["quiz_id"] != DBNull.Value)
                    {
                        phQuizTrigger.Visible = true;
                        int qid = Convert.ToInt32(dr["quiz_id"]);
                        ViewState["CurrentQuizId"] = qid;

                        DisplayQuizResult(qid);
                    }
                }
                else
                {
                    phQuizTrigger.Visible = false;
                }
            }
        }

        private void DisplayQuizResult(int qid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Get the highest score/latest attempt for this student enrollment
                string query = @"SELECT TOP 1 score, status FROM quizAttempt 
                        WHERE quiz_id = @qid AND enrollment_id = @eid 
                        ORDER BY finished_at DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@qid", qid);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    pnlQuizResult.Visible = true;
                    pnlQuizPrompt.Visible = false;
                    btnReviewQuiz.Visible = true;

                    int score = Convert.ToInt32(dr["score"]);
                    string status = dr["status"].ToString();

                    litScoreText.Text = score + "%";

                    if (score >= 50 && status == "GRADED")
                    {
                        spanStatus.InnerText = "Passed";
                        spanStatus.Attributes["class"] = "status-badge status-pass";
                        litResultMessage.Text = "Great job! You've mastered this lesson.";
                        btnProceedToQuiz.Visible = false; // Already passed
                        btnRetakeQuiz.Visible = false;
                    }
                    else if (status == "GRADED") // Failed
                    {
                        spanStatus.InnerText = "Failed";
                        spanStatus.Attributes["class"] = "status-badge status-fail";
                        litResultMessage.Text = "You didn't reach the 50% pass mark.";
                        btnProceedToQuiz.Visible = false;
                        btnRetakeQuiz.Visible = true; // Show retake option
                    }
                }
                else
                {
                    // No attempts yet
                    pnlQuizResult.Visible = false;
                    pnlQuizPrompt.Visible = true;
                    btnProceedToQuiz.Visible = true;
                    btnRetakeQuiz.Visible = false;
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

        private void UpdateProgress(int enrollmentId, int resourceId)
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

        private void UpdateOverallProgress()
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
                string query = "SELECT course_id FROM learningResource WHERE Id=@rid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@rid", resourceId);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private void CheckExistingFeedback()
        {
            int resId = Convert.ToInt32(Request.QueryString["resourceId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT rating, comment FROM feedback WHERE student_id=@sid AND resource_id=@rid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@rid", resId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    ddlRating.SelectedValue = dr["rating"].ToString();
                    txtComment.Text = dr["comment"].ToString();
                    litModalTitle.Text = "Edit Your Feedback";
                    btnSubmitFeedback.Text = "Update Feedback";
                    pnlRemoveFeedback.Visible = true;
                }
                else
                {
                    litModalTitle.Text = "Submit New Feedback";
                    btnSubmitFeedback.Text = "Submit Feedback";
                    pnlRemoveFeedback.Visible = false;
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
                DECLARE @tid INT = (SELECT tutor_id FROM learningResource WHERE Id=@rid)
                INSERT INTO feedback (student_id, tutor_id, resource_id, rating, comment, status)
                VALUES (@sid, @tid, @rid, @rating, @comment, 'APPROVED')
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
                Response.Redirect($"QuizReview.aspx?quizId={qid}");
            }
        }
    }
}