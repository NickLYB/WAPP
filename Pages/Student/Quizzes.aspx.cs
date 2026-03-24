using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Utils; // Accesses SystemLogService and LogLevel

namespace WAPP.Pages.Student
{
    public partial class Quizzes : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        public int QuizDuration = 15;

        int enrollmentId;
        int quizId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["quizId"] == null || Request.QueryString["enrollmentId"] == null)
            {
                Response.Redirect("Study.aspx");
                return;
            }

            quizId = Convert.ToInt32(Request.QueryString["quizId"]);
            enrollmentId = Convert.ToInt32(Request.QueryString["enrollmentId"]);

            if (!IsPostBack)
            {
                LoadQuizInfo(quizId);
                LoadQuestions(quizId);
                StartAttempt(quizId, enrollmentId);
            }
        }

        // Helper to grab the logged-in User's ID for our logs
        private int? GetCurrentUserId()
        {
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out int uid))
                return uid;
            return null;
        }

        private void LoadQuizInfo(int quizId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT q.title, q.duration_minutes, c.title as CourseName 
                               FROM quiz q WITH (NOLOCK) JOIN course c WITH (NOLOCK) ON q.course_id = c.Id WHERE q.Id = @qid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@qid", quizId);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    litQuizTitle.Text = dr["title"].ToString();
                    litCourseName.Text = dr["CourseName"].ToString();
                    QuizDuration = Convert.ToInt32(dr["duration_minutes"]);
                }
            }
        }

        private void LoadQuestions(int quizId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id, question_text FROM question WITH (NOLOCK) WHERE quiz_id = @qid";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@qid", quizId);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptQuestions.DataSource = dt;
                rptQuestions.DataBind();
            }
        }

        protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                int questionId = Convert.ToInt32(DataBinder.Eval(e.Item.DataItem, "Id"));
                RadioButtonList rbl = (RadioButtonList)e.Item.FindControl("rblOptions");

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = "SELECT Id, text AS option_text FROM [answerOption] WITH (NOLOCK) WHERE question_id = @qid";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@qid", questionId);
                    conn.Open();
                    rbl.DataSource = cmd.ExecuteReader();
                    rbl.DataBind();
                }
            }
        }

        private void StartAttempt(int qid, int eid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"
                IF NOT EXISTS (SELECT 1 FROM quizAttempt WITH (NOLOCK) WHERE enrollment_id=@eid AND quiz_id=@qid AND status='IN_PROGRESS')
                BEGIN
                    INSERT INTO quizAttempt (enrollment_id, quiz_id, started_at, status) 
                    VALUES (@eid, @qid, GETDATE(), 'IN_PROGRESS')
                END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", eid);
                cmd.Parameters.AddWithValue("@qid", qid);
                int rowsAffected = cmd.ExecuteNonQuery();

                // ---> LOGGING ADDED: INFO (Student started a quiz)
                // We only log if a NEW attempt was actually created (rowsAffected > 0)
                if (rowsAffected > 0)
                {
                    SystemLogService.Write("QUIZ_ATTEMPT_STARTED",
                        $"Student started an attempt for Quiz ID {qid} (Enrollment ID: {eid}).",
                        LogLevel.INFO, GetCurrentUserId());
                }
            }
        }

        private bool CheckAllQuestionsAnswered()
        {
            foreach (RepeaterItem item in rptQuestions.Items)
            {
                RadioButtonList rbl = (RadioButtonList)item.FindControl("rblOptions");
                // If any question is left completely blank, this stops the submission
                if (rbl.SelectedItem == null)
                {
                    return false;
                }
            }
            return true;
        }

        protected void btnSubmitQuiz_Click(object sender, EventArgs e)
        {
            // 1. Validation check
            if (!CheckAllQuestionsAnswered())
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "MissingAnswersAlert",
                    "alert('Please answer all questions before submitting the quiz.');", true);
                return;
            }

            try
            {
                int score = 0;
                int totalQuestions = rptQuestions.Items.Count;

                // 2. Calculate the score
                foreach (RepeaterItem item in rptQuestions.Items)
                {
                    RadioButtonList rbl = (RadioButtonList)item.FindControl("rblOptions");
                    if (rbl.SelectedItem != null && int.TryParse(rbl.SelectedValue, out int selectedOptionId))
                    {
                        if (IsCorrect(selectedOptionId)) score++;
                    }
                }

                int finalPercentage = (totalQuestions > 0) ? (score * 100) / totalQuestions : 0;

                // 3. Update the attempt to GRADED and get the Attempt ID
                int currentAttemptId = UpdateAttempt(finalPercentage);

                // 4. Save the exact answers to the database so Review page can see them
                if (currentAttemptId > 0)
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        foreach (RepeaterItem item in rptQuestions.Items)
                        {
                            RadioButtonList rbl = (RadioButtonList)item.FindControl("rblOptions");
                            HiddenField hfQid = (HiddenField)item.FindControl("hfQuestionId");

                            string query = "INSERT INTO quizAnswer (attempt_id, question_id, selected_option_id) VALUES (@aid, @qid, @sid)";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@aid", currentAttemptId);
                                cmd.Parameters.AddWithValue("@qid", hfQid.Value);

                                if (rbl.SelectedItem != null && !string.IsNullOrEmpty(rbl.SelectedValue))
                                    cmd.Parameters.AddWithValue("@sid", rbl.SelectedValue);
                                else
                                    cmd.Parameters.AddWithValue("@sid", DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // 5. Apply progression logic
                if (finalPercentage >= 50)
                {
                    MarkLessonAsCompleted();
                    CheckAndCompleteCourse();
                }

                // ---> LOGGING ADDED: INFO (Student successfully submitted a quiz)
                SystemLogService.Write("QUIZ_SUBMITTED",
                    $"Student submitted Quiz ID {quizId} and scored {finalPercentage}%.",
                    LogLevel.INFO, GetCurrentUserId());

                Response.Redirect("LessonView.aspx?resourceId=" + GetResourceIdByQuiz(quizId));
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Prevent Response.Redirect from triggering a false ERROR log
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR (Database failure during grading writeback)
                SystemLogService.Write("QUIZ_SUBMIT_ERROR",
                    $"Database error while grading Quiz ID {quizId}: {ex.Message}",
                    LogLevel.ERROR, GetCurrentUserId());

                ScriptManager.RegisterStartupScript(this, GetType(), "QuizError",
                    "alert('An error occurred while saving your quiz. Please try again.');", true);
            }
        }

        private bool IsCorrect(int optionId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT is_correct FROM [answerOption] WITH (NOLOCK) WHERE Id = @oid", conn);
                cmd.Parameters.AddWithValue("@oid", optionId);
                conn.Open();
                object res = cmd.ExecuteScalar();
                return res != null ? Convert.ToBoolean(res) : false;
            }
        }

        private int UpdateAttempt(int finalScore)
        {
            int attemptId = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string getQuery = @"SELECT TOP 1 Id FROM quizAttempt WITH (NOLOCK) 
                                    WHERE enrollment_id = @eid AND quiz_id = @qid AND status = 'IN_PROGRESS' 
                                    ORDER BY started_at DESC";
                using (SqlCommand cmdGet = new SqlCommand(getQuery, conn))
                {
                    cmdGet.Parameters.AddWithValue("@eid", enrollmentId);
                    cmdGet.Parameters.AddWithValue("@qid", quizId);
                    object res = cmdGet.ExecuteScalar();
                    if (res != null) attemptId = Convert.ToInt32(res);
                }

                if (attemptId == 0)
                {
                    string getFallback = "SELECT TOP 1 Id FROM quizAttempt WITH (NOLOCK) WHERE enrollment_id=@eid AND quiz_id=@qid ORDER BY started_at DESC";
                    using (SqlCommand cmdFallback = new SqlCommand(getFallback, conn))
                    {
                        cmdFallback.Parameters.AddWithValue("@eid", enrollmentId);
                        cmdFallback.Parameters.AddWithValue("@qid", quizId);
                        object resF = cmdFallback.ExecuteScalar();
                        if (resF != null) attemptId = Convert.ToInt32(resF);
                    }
                }

                if (attemptId > 0)
                {
                    string updateQ = "UPDATE quizAttempt SET finished_at = GETDATE(), score = @score, status = 'GRADED' WHERE Id = @aid";
                    using (SqlCommand cmdUp = new SqlCommand(updateQ, conn))
                    {
                        cmdUp.Parameters.AddWithValue("@score", finalScore);
                        cmdUp.Parameters.AddWithValue("@aid", attemptId);
                        cmdUp.ExecuteNonQuery();
                    }
                }
            }
            return attemptId;
        }

        private string GetResourceIdByQuiz(int qId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id FROM learningResource WITH (NOLOCK) WHERE quiz_id = @qid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@qid", qId);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private void MarkLessonAsCompleted()
        {
            int resId = Convert.ToInt32(GetResourceIdByQuiz(quizId));
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            IF NOT EXISTS (SELECT 1 FROM resourceProgress WITH (NOLOCK) WHERE enrollment_id=@eid AND resource_id=@rid)
                INSERT INTO resourceProgress (enrollment_id, resource_id, last_accessed, completed_at) 
                VALUES (@eid, @rid, GETDATE(), GETDATE())
            ELSE
                UPDATE resourceProgress SET last_accessed = GETDATE(), completed_at = GETDATE() 
                WHERE enrollment_id=@eid AND resource_id=@rid";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                cmd.Parameters.AddWithValue("@rid", resId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void CheckAndCompleteCourse()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    DECLARE @CourseID INT = (SELECT course_id FROM enrollment WITH (NOLOCK) WHERE Id = @eid);
                    DECLARE @TotalResources INT = (SELECT COUNT(*) FROM learningResource WITH (NOLOCK) WHERE course_id = @CourseID);
                    DECLARE @CompletedResources INT = (SELECT COUNT(*) FROM resourceProgress WITH (NOLOCK) WHERE enrollment_id = @eid AND completed_at IS NOT NULL);

                    IF (@TotalResources = @CompletedResources AND @TotalResources > 0)
                    BEGIN
                        UPDATE enrollment SET status = 'COMPLETED', completed_at = GETDATE() WHERE Id = @eid AND status != 'COMPLETED';
                    END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        protected void btnCancelQuiz_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = "DELETE FROM quizAttempt WHERE enrollment_id = @eid AND quiz_id = @qid AND status = 'IN_PROGRESS'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@eid", enrollmentId);
                    cmd.Parameters.AddWithValue("@qid", quizId);
                    conn.Open();
                    int rowsDeleted = cmd.ExecuteNonQuery();

                    // ---> LOGGING ADDED: WARNING (Tripwire for suspicious quiz bailing)
                    if (rowsDeleted > 0)
                    {
                        SystemLogService.Write("QUIZ_ATTEMPT_CANCELLED",
                            $"Student cancelled an in-progress attempt for Quiz ID {quizId}.",
                            LogLevel.WARNING, GetCurrentUserId());
                    }
                }
                Response.Redirect("LessonView.aspx?resourceId=" + GetResourceIdByQuiz(quizId));
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Prevent Response.Redirect from triggering a false ERROR log
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR (Database failure during deletion)
                SystemLogService.Write("QUIZ_CANCEL_ERROR",
                    $"Database error while cancelling Quiz ID {quizId}: {ex.Message}",
                    LogLevel.ERROR, GetCurrentUserId());
            }
        }
    }
}