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
        int enrollmentId;
        int latestAttemptId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["quizId"] == null)
            {
                Response.Redirect("Study.aspx");
                return;
            }
            enrollmentId = Convert.ToInt32(Request.QueryString["enrollmentId"]);
            int quizId = Convert.ToInt32(Request.QueryString["quizId"]);

            // 1. Find the latest attempt ID for this user/quiz before loading questions
            latestAttemptId = GetLatestAttemptId(quizId);

            if (!IsPostBack)
            {
                LoadReviewData(quizId);
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
            if (!path.EndsWith("/QuizReview.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/QuizReview", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            SiteMapProvider provider = (SiteMapProvider)sender;

            // Force find the node if the query strings hide it
            SiteMapNode current = provider.CurrentNode ?? provider.FindSiteMapNode("~/Pages/Student/QuizReview");
            if (current == null) return null;

            // Clone the node AND its ancestors
            SiteMapNode clone = current.Clone(true);

            string quizIdStr = ctx.Request.QueryString["quizId"];
            string enrollmentIdStr = ctx.Request.QueryString["enrollmentId"];

            if (int.TryParse(quizIdStr, out int qId))
            {
                // 1. Update the Current Node (Review Answers) to keep its query strings
                clone.Url = $"~/Pages/Student/QuizReview.aspx?quizId={quizIdStr}&enrollmentId={enrollmentIdStr}";

                // 2. Fetch the Course Title and Resource ID for the Parent Node (LessonView)
                GetCourseDetailsByQuizId(qId, out string courseTitle, out int resourceId);

                if (clone.ParentNode != null)
                {
                    // Rename "Classroom" to the actual Course Name
                    if (!string.IsNullOrWhiteSpace(courseTitle))
                    {
                        clone.ParentNode.Title = courseTitle;
                    }

                    // Ensure clicking the parent breadcrumb takes them back to the exact lesson
                    if (resourceId > 0)
                    {
                        clone.ParentNode.Url = $"~/Pages/Student/LessonView.aspx?resourceId={resourceId}";
                    }
                }
            }

            return clone;
        }

        // Helper method to fetch the Course Title and Resource ID together
        private void GetCourseDetailsByQuizId(int quizId, out string courseTitle, out int resourceId)
        {
            courseTitle = string.Empty;
            resourceId = 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Join learningResource and course to get everything we need in one go
                string query = @"SELECT c.title, lr.Id as resourceId 
                         FROM learningResource lr WITH (NOLOCK)
                         JOIN course c WITH (NOLOCK) ON lr.course_id = c.Id 
                         WHERE lr.quiz_id = @qid";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@qid", quizId);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            courseTitle = dr["title"].ToString();
                            resourceId = Convert.ToInt32(dr["resourceId"]);
                        }
                    }
                }
            }
        }
        private int GetLatestAttemptId(int qid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT TOP 1 Id FROM quizAttempt WITH (NOLOCK) 
                                 WHERE enrollment_id = @eid AND quiz_id = @qid AND status = 'GRADED'
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
                string query = "SELECT Id, question_text FROM question WITH (NOLOCK) WHERE quiz_id = @qid";
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
                    // Perfectly links the selected option directly to the Attempt ID
                    string query = @"
                        SELECT o.Id, o.text, o.is_correct, 
                            CASE 
                                WHEN (SELECT TOP 1 selected_option_id FROM quizAnswer qa WITH (NOLOCK) 
                                      WHERE qa.attempt_id = @aid AND qa.question_id = @qid) = o.Id 
                                THEN 1 
                                ELSE 0 
                            END as is_selected
                        FROM [answerOption] o WITH (NOLOCK)
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
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id FROM learningResource WITH (NOLOCK) WHERE quiz_id = @qid";
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
            bool selected = isSelected != DBNull.Value && Convert.ToInt32(isSelected) == 1;

            // 1. Correct Answer AND User Picked It (Green BG, Green Text)
            if (selected && correct)
                return "option-item p-3 mb-2 rounded bg-success bg-opacity-25 border border-success fw-bold text-success d-flex align-items-center";

            // 2. Wrong Answer AND User Picked It (Red BG, Red Text)
            if (selected && !correct)
                return "option-item p-3 mb-2 rounded bg-danger bg-opacity-25 border border-danger fw-bold text-danger d-flex align-items-center";

            // 3. Correct Answer BUT User Missed It (White BG, Green Text, Green Border)
            if (!selected && correct)
                return "option-item p-3 mb-2 rounded border border-success fw-bold text-success d-flex align-items-center";

            // 4. Default: Unselected Wrong Options (Grey Text)
            return "option-item p-3 mb-2 rounded border text-muted d-flex align-items-center";
        }

        protected string GetOptionIcon(object isCorrect, object isSelected)
        {
            bool correct = Convert.ToBoolean(isCorrect);
            bool selected = isSelected != DBNull.Value && Convert.ToInt32(isSelected) == 1;

            // 1. Solid checkmark if they got it right
            if (selected && correct) return "bi bi-check-circle-fill text-success fs-4 me-3";

            // 2. Solid X if they got it wrong
            if (selected && !correct) return "bi bi-x-circle-fill text-danger fs-4 me-3";

            // 3. Outline checkmark indicating the correct answer they missed
            if (!selected && correct) return "bi bi-check-circle text-success fs-4 me-3";

            // 4. Default empty circle
            return "bi bi-circle text-secondary opacity-25 fs-4 me-3";
        }

    }
}