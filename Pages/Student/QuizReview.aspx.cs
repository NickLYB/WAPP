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
        string sourcePage = "lesson";
        bool isFromResults = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["quizId"] == null)
            {
                Response.Redirect("Study.aspx");
                return;
            }
            enrollmentId = Convert.ToInt32(Request.QueryString["enrollmentId"]);
            int quizId = Convert.ToInt32(Request.QueryString["quizId"]);

            if (Request.QueryString["source"] != null)
            {
                sourcePage = Request.QueryString["source"].ToLower();
            }

            // 1. Find the latest attempt ID for this user/quiz before loading questions
            latestAttemptId = GetLatestAttemptId(quizId);

            if (!IsPostBack)
            {
                LoadReviewData(quizId);
                UpdateBackButtonUI();
            }
        }

        private void UpdateBackButtonUI()
        {
            if (sourcePage == "results")
            {
                btnBack.Text = "<i class='bi bi-arrow-left me-2'></i>Back to My Results";
            }
            else
            {
                btnBack.Text = "<i class='bi bi-arrow-left me-2'></i>Back to Lesson";
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Request.QueryString["source"] != null && Request.QueryString["source"].ToLower() == "results")
            {
                isFromResults = true;
            }

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

            string quizIdStr = ctx.Request.QueryString["quizId"];
            string enrollmentIdStr = ctx.Request.QueryString["enrollmentId"];

            if (!int.TryParse(quizIdStr, out int qId)) return null;

            // 1. Fetch Course Details
            GetCourseDetailsByQuizId(qId, out string courseTitle, out int resourceId);

            // 2. Create the ultimate destination node (The Quiz Review page itself)
            SiteMapNode targetNode = new SiteMapNode(provider, "QuizReview",
                $"~/Pages/Student/QuizReview.aspx?quizId={quizIdStr}&enrollmentId={enrollmentIdStr}", "Review Answers");

            // 3. Create the Home node (Root of all paths)
            SiteMapNode rootNode = new SiteMapNode(provider, "Home", "~/Pages/Student/Home.aspx", "Home");

            // --- BUILD DYNAMIC PATHS BASED ON SOURCE ---

            if (isFromResults)
            {
                // Path 3: Home -> My Results -> Quiz Review
                SiteMapNode resultsNode = new SiteMapNode(provider, "Results", "~/Pages/Student/Results.aspx", "My Grades");

                resultsNode.ParentNode = rootNode;
                targetNode.ParentNode = resultsNode;
            }
            else if (sourcePage == "course") // Let's assume you pass "?source=course" from CourseDetail
            {
                // Path 1: Home -> Explore -> Course Overview -> Quiz Review
                SiteMapNode exploreNode = new SiteMapNode(provider, "Explore", "~/Pages/Student/Study.aspx", "Explore");

                // Get the Course ID to link back to Course Detail
                string cId = GetCourseIdByResource(resourceId);
                SiteMapNode courseNode = new SiteMapNode(provider, "CourseDetail", $"~/Pages/Student/CourseDetail.aspx?id={cId}", courseTitle);

                exploreNode.ParentNode = rootNode;
                courseNode.ParentNode = exploreNode;
                targetNode.ParentNode = courseNode;
            }
            else
            {
                // Path 2 (Default): Home -> My Learning -> Classroom -> Quiz Review
                SiteMapNode myLearningNode = new SiteMapNode(provider, "MyLearning", "~/Pages/Student/MyCourses.aspx", "My Learning");
                SiteMapNode lessonNode = new SiteMapNode(provider, "LessonView", $"~/Pages/Student/LessonView.aspx?resourceId={resourceId}", courseTitle);

                myLearningNode.ParentNode = rootNode;
                lessonNode.ParentNode = myLearningNode;
                targetNode.ParentNode = lessonNode;
            }

            return targetNode;
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

        private string GetCourseIdByResource(int resourceId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT course_id FROM learningResource WITH (NOLOCK) WHERE Id=@rid";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@rid", resourceId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? result.ToString() : "";
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
            if (sourcePage == "results")
            {
                // Go back to the Results page
                Response.Redirect("Results.aspx");
            }
            else
            {
                // Go back to the specific Lesson View
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = "SELECT Id FROM learningResource WITH (NOLOCK) WHERE quiz_id = @qid";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@qid", Request.QueryString["quizId"]);
                    conn.Open();
                    object rid = cmd.ExecuteScalar();

                    // Maintain the source trail back to the lesson!
                    string redirectUrl = $"LessonView.aspx?resourceId={rid}";
                    if (!string.IsNullOrEmpty(sourcePage) && sourcePage != "lesson")
                    {
                        redirectUrl += $"&source={sourcePage}";
                    }

                    Response.Redirect(redirectUrl);
                }
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