using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class Quizzes : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        public int QuizDuration = 15;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["quizId"] == null || Request.QueryString["enrollmentId"] == null)
                    Response.Redirect("MyCourses.aspx");

                int quizId = Convert.ToInt32(Request.QueryString["quizId"]);
                int enrollmentId = Convert.ToInt32(Request.QueryString["enrollmentId"]);

                LoadQuizInfo(quizId);
                LoadQuestions(quizId);

                // Initialize the attempt record if one doesn't exist for this session
                StartAttempt(quizId, enrollmentId);
            }
        }

        private void LoadQuizInfo(int quizId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT q.title, q.duration_minutes, c.title as CourseName 
                               FROM quiz q JOIN course c ON q.course_id = c.Id WHERE q.Id = @qid";
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
                string query = "SELECT Id, question_text FROM question WHERE quiz_id = @qid";
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
                    string query = "SELECT Id, text AS option_text FROM [answerOption] WHERE question_id = @qid";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@qid", questionId);
                    conn.Open();
                    rbl.DataSource = cmd.ExecuteReader();
                    rbl.DataBind();
                }
            }
        }

        private void StartAttempt(int quizId, int enrollmentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string checkQuery = "SELECT COUNT(*) FROM quizAttempt WHERE enrollment_id=@eid AND quiz_id=@qid AND status='IN_PROGRESS'";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@eid", enrollmentId);
                checkCmd.Parameters.AddWithValue("@qid", quizId);

                conn.Open();
                int existing = (int)checkCmd.ExecuteScalar();

                if (existing == 0)
                {
                    string query = "INSERT INTO quizAttempt (enrollment_id, quiz_id, started_at, status) VALUES (@eid, @qid, GETDATE(), 'IN_PROGRESS')";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@eid", enrollmentId);
                    cmd.Parameters.AddWithValue("@qid", quizId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected void btnSubmitQuiz_Click(object sender, EventArgs e)
        {
            int score = 0;
            int totalQuestions = rptQuestions.Items.Count;

            foreach (RepeaterItem item in rptQuestions.Items)
            {
                RadioButtonList rbl = (RadioButtonList)item.FindControl("rblOptions");
                if (rbl.SelectedItem != null)
                {
                    int selectedOptionId = Convert.ToInt32(rbl.SelectedValue);
                    if (IsCorrect(selectedOptionId)) score++;
                }
            }

            int finalPercentage = (totalQuestions > 0) ? (score * 100) / totalQuestions : 0;
            UpdateAttempt(finalPercentage);

            // LOGIC: If they pass (>= 50%), update lesson progress
            if (finalPercentage >= 50)
            {
                MarkLessonAsCompleted();
                CheckAndCompleteCourse(); // New check for overall course completion
            }

            string resourceId = GetResourceIdByQuiz(Convert.ToInt32(Request.QueryString["quizId"]));
            Response.Redirect("LessonView.aspx?resourceId=" + resourceId);
        }

        private bool IsCorrect(int optionId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT is_correct FROM [answerOption] WHERE Id = @oid", conn);
                cmd.Parameters.AddWithValue("@oid", optionId);
                conn.Open();
                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }

        private void UpdateAttempt(int finalScore)
        {
            int enrollmentId = Convert.ToInt32(Request.QueryString["enrollmentId"]);
            int quizId = Convert.ToInt32(Request.QueryString["quizId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"UPDATE quizAttempt SET finished_at = GETDATE(), score = @score, status = 'GRADED' 
                               WHERE enrollment_id = @eid AND quiz_id = @qid AND status = 'IN_PROGRESS'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@score", finalScore);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                cmd.Parameters.AddWithValue("@qid", quizId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string GetResourceIdByQuiz(int quizId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id FROM learningResource WHERE quiz_id = @qid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@qid", quizId);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private void MarkLessonAsCompleted()
        {
            int enrollmentId = Convert.ToInt32(Request.QueryString["enrollmentId"]);
            int quizId = Convert.ToInt32(Request.QueryString["quizId"]);
            int resId = Convert.ToInt32(GetResourceIdByQuiz(quizId));

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    IF NOT EXISTS (SELECT 1 FROM resourceProgress WHERE enrollment_id=@eid AND resource_id=@rid)
                        INSERT INTO resourceProgress (enrollment_id, resource_id, completed_at,last_accessed) VALUES (@eid, @rid, GETDATE(), GETDATE())
                    ELSE
                        UPDATE resourceProgress SET last_accessed = GETDATE(),completed_at  = GETDATE() WHERE enrollment_id=@eid AND resource_id=@rid";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                cmd.Parameters.AddWithValue("@rid", resId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // NEW METHOD: Checks if all resources in the course are finished
        private void CheckAndCompleteCourse()
        {
            int enrollmentId = Convert.ToInt32(Request.QueryString["enrollmentId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Query compares total resources in the course vs completed resources for this enrollment
                string query = @"
                    DECLARE @CourseID INT = (SELECT course_id FROM enrollment WHERE Id = @eid);
                    DECLARE @TotalResources INT = (SELECT COUNT(*) FROM learningResource WHERE course_id = @CourseID);
                    DECLARE @CompletedResources INT = (SELECT COUNT(*) FROM resourceProgress WHERE enrollment_id = @eid);

                    IF (@TotalResources = @CompletedResources AND @TotalResources > 0)
                    BEGIN
                        UPDATE enrollment SET status = 'COMPLETED', completed_at = GETDATE() WHERE Id = @eid;
                    END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}