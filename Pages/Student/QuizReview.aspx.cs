using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class QuizReview : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        int enrollmentId = 1;
        int latestAttemptId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["quizId"] == null)
            {
                Response.Redirect("Study.aspx");
                return;
            }

            // 1. Find the latest attempt ID for this user/quiz before loading questions
            latestAttemptId = GetLatestAttemptId(Convert.ToInt32(Request.QueryString["quizId"]));

            if (!IsPostBack)
            {
                LoadReviewData(Convert.ToInt32(Request.QueryString["quizId"]));
            }
        }

        private int GetLatestAttemptId(int qid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Pull the ID of the most recent finished attempt
                string query = @"SELECT TOP 1 Id FROM quizAttempt 
                                WHERE enrollment_id = @eid AND quiz_id = @qid 
                                ORDER BY finished_at DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", enrollmentId);
                cmd.Parameters.AddWithValue("@qid", qid);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        private void LoadReviewData(int qid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id, question_text FROM question WHERE quiz_id = @qid";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@qid", qid);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptReview.DataSource = dt;
                rptReview.DataBind();
            }
        }
        protected void rptReview_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                int questionId = Convert.ToInt32(((HiddenField)e.Item.FindControl("hfQuestionId")).Value);
                Repeater rptOptions = (Repeater)e.Item.FindControl("rptOptions");

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // FIX: Joining quizAnswer on attempt_id instead of enrollment_id
                    string query = @"
                        SELECT o.text, o.is_correct, 
                        CASE WHEN sa.selected_option_id IS NOT NULL THEN 1 ELSE 0 END as is_selected
                        FROM [answerOption] o
                        LEFT JOIN quizAnswer sa ON o.Id = sa.selected_option_id 
                             AND sa.attempt_id = @aid
                        WHERE o.question_id = @qid";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@qid", questionId);
                    cmd.Parameters.AddWithValue("@aid", latestAttemptId);
                    conn.Open();
                    rptOptions.DataSource = cmd.ExecuteReader();
                    rptOptions.DataBind();
                }
            }
        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            // Redirects back to the lesson that owns this quiz
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id FROM learningResource WHERE quiz_id = @qid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@qid", Request.QueryString["quizId"]);
                conn.Open();
                object rid = cmd.ExecuteScalar();
                Response.Redirect("LessonView.aspx?resourceId=" + rid);
            }
        }
        protected string GetOptionClass(object isCorrect, object isSelected)
        {
            bool correct = Convert.ToBoolean(isCorrect);
            bool selected = Convert.ToBoolean(isSelected);

            // Green if it's correct (whether chosen or not)
            if (selected && correct) return "option-item is-correct-choice";
            // Red if you chose it and it's wrong
            if (selected && !correct) return "option-item is-wrong-choice";
            // Dashed Green if you MISSED the correct one
            if (!selected && correct) return "option-item is-missed-correct";

            return "option-item";
        }

        protected string GetOptionIcon(object isCorrect, object isSelected)
        {
            bool correct = Convert.ToBoolean(isCorrect);
            bool selected = Convert.ToBoolean(isSelected);

            // Always show a checkmark on the correct answer
            if (correct) return "bi bi-check-circle-fill";

            // Show an X if you picked the wrong one
            if (selected && !correct) return "bi bi-x-circle-fill";

            // Empty circle for other unselected wrong options
            return "bi bi-circle opacity-25";
        }

    }
}