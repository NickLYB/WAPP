using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class EditCourse : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                string idStr = Request.QueryString["id"];
                if (int.TryParse(idStr, out int courseId))
                {
                    courseTitle.InnerText = GetCourseName(courseId);
                    LoadCourseOverview(courseId);
                    LoadCourseContent(courseId);
                    LoadAboutCourse(courseId);
                    LoadReviews(courseId);

                    Page.DataBind();
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SiteMap.SiteMapResolve += SiteMapPath1_SiteMapResolve;
        }
        protected override void OnUnload(EventArgs e)
        {
            SiteMap.SiteMapResolve -= SiteMapPath1_SiteMapResolve;
            base.OnUnload(e);
        }
        private SiteMapNode SiteMapPath1_SiteMapResolve(object sender, SiteMapResolveEventArgs e)
        {
            var ctx = e.Context;
            if (ctx?.Request == null) return SiteMap.CurrentNode;

            // Friendly URL safe check
            string reqPath = ctx.Request.Path;
            if (!(reqPath.EndsWith("/EditCourse", StringComparison.OrdinalIgnoreCase) ||
                  reqPath.EndsWith("/EditCourse.aspx", StringComparison.OrdinalIgnoreCase)))
                return SiteMap.CurrentNode;

            var current = SiteMap.CurrentNode;
            if (current == null) return null;

            var clone = current.Clone(true);

            string idStr = ctx.Request.QueryString["id"];
            if (!int.TryParse(idStr, out int courseId))
                return clone;

            string courseName = GetCourseName(courseId);
            if (!string.IsNullOrWhiteSpace(courseName))
            {
                clone.Title = $"Edit - {courseName}";

                clone.Url += $"?id={courseId}";
            }

            return clone;
        }

        private string GetCourseName(int courseId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 title FROM course WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", courseId);
                con.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private void LoadCourseOverview(int courseId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT 
            ct.name AS course_type_name,
            c.duration_minutes,
            c.skill_level,
            c.status, -- ADDED STATUS HERE
            (SELECT COUNT(Id) FROM learningResource WHERE course_id = c.Id) AS lecture_count
        FROM course c
        INNER JOIN courseType ct ON c.course_type_id = ct.Id
        WHERE c.Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", courseId);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblCourseType.Text = reader["course_type_name"]?.ToString();

                        if (int.TryParse(reader["duration_minutes"]?.ToString(), out int minutes))
                        {
                            double weeks = minutes / 10080.0;
                            lblDuration.Text = weeks.ToString("F2") + " weeks";
                        }

                        string level = reader["skill_level"]?.ToString();
                        if (!string.IsNullOrEmpty(level))
                        {
                            level = level.ToLower();
                            lblSkillLevel.Text = char.ToUpper(level[0]) + level.Substring(1);
                        }

                        lblLectures.Text = reader["lecture_count"]?.ToString() ?? "0";
                        string status = reader["status"]?.ToString().ToUpper();
                        lblCourseStatus.Text = status;

                        // Color the badge based on status
                        switch (status)
                        {
                            case "PUBLISHED": lblCourseStatus.CssClass = "badge bg-success"; break;
                            case "PENDING": lblCourseStatus.CssClass = "badge bg-warning text-dark"; break;
                            case "APPROVED": lblCourseStatus.CssClass = "badge bg-info text-dark"; break;
                            case "REJECT": lblCourseStatus.CssClass = "badge bg-danger"; break;
                            default: lblCourseStatus.CssClass = "badge bg-secondary"; break; // PRIVATE
                        }

                        // Show/Hide Publish buttons
                        if (status == "APPROVED" || status == "PRIVATE")
                        {
                            btnPublish.Visible = true;
                            btnUnpublish.Visible = false;
                        }
                        else if (status == "PUBLISHED")
                        {
                            btnPublish.Visible = false;
                            btnUnpublish.Visible = true;
                        }
                        else
                        {
                            // If it is PENDING or REJECT, they cannot publish or unpublish
                            btnPublish.Visible = false;
                            btnUnpublish.Visible = false;
                        }
                    }
                }
            }
        }
        private void LoadCourseContent(int courseId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
            SELECT 
                L.Id AS LessonId, 
                L.title AS LessonTitle, 
                L.sequence_order,
                L.quiz_id,
                Q.title AS QuizTitle,
                Q.Id AS ActualQuizId
            FROM learningResource L
            LEFT JOIN quiz Q ON L.quiz_id = Q.Id
            WHERE L.course_id = @CourseId
            ORDER BY 
                CASE WHEN L.sequence_order IS NULL THEN 1 ELSE 0 END ASC,
                L.sequence_order ASC, 
                L.created_at ASC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);

                    rptCourseContent.DataSource = dt;
                    rptCourseContent.DataBind();
                }
            }
        }
        private void LoadAboutCourse(int courseId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 description FROM course WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", courseId);
                con.Open();

                object desc = cmd.ExecuteScalar();
                lblCourseDesc.Text = desc?.ToString() ?? "(No description yet)";
            }
        }
        private void LoadReviews(int courseId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
            SELECT 
                (u.fname + ' ' + u.lname) AS reviewer_name, 
                f.rating, 
                f.created_at, 
                f.comment 
            FROM feedback f
            JOIN [user] u ON f.student_id = u.Id
            WHERE f.course_id = @CourseId 
              AND f.resource_id IS NULL 
              AND f.status IN ('APPROVED','PENDING')
            ORDER BY f.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptReviews.DataSource = dt;
                            rptReviews.DataBind();

                            // Automatically calculate the average rating based on the fetched rows
                            double avgRating = Convert.ToDouble(dt.Compute("AVG(rating)", ""));
                            lblReviewSummary.Text = $"Course Rating: {avgRating:F1} ★ ({dt.Rows.Count} Reviews)";

                            lblNoReviews.Visible = false;
                            rptReviews.Visible = true;
                        }
                        else
                        {
                            lblNoReviews.Visible = true;
                            rptReviews.Visible = false;
                            lblReviewSummary.Text = "";
                        }
                    }
                }
            }
        }

        protected void rptCourseContent_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            // This grabs the ID of the specific Lesson or Quiz you clicked
            string itemId = e.CommandArgument.ToString();
            string courseId = Request.QueryString["id"];

            // Route the user based on which button they clicked
            switch (e.CommandName)
            {
                case "ViewLesson":
                    Response.Redirect($"~/Pages/Tutor/ViewLesson.aspx?courseId={courseId}&lessonId={itemId}");
                    break;

                case "EditLesson":
                    int lessonId = Convert.ToInt32(e.CommandArgument);
                    // Call the public method on our modal to load data and show itself!
                    EditLessonModal1.LoadLessonData(lessonId);
                    break;

                case "ViewScore":
                    Response.Redirect($"~/Pages/Tutor/QuizScores.aspx?courseId={courseId}&quizId={itemId}");
                    break;

                case "EditQuiz":
                    Response.Redirect($"~/Pages/Tutor/EditQuiz.aspx?courseId={courseId}&quizId={itemId}");
                    break;
            }
        }

        protected void btnSaveSequence_Click(object sender, EventArgs e)
        {
            // 1. Read the data from the hidden field
            string rawSequence = hfSequenceData.Value;

            // If they clicked save without moving anything, just ignore
            if (string.IsNullOrEmpty(rawSequence)) return;

            // Split into an array: ["Lesson_12", "Quiz_3", "Lesson_13"]
            string[] items = rawSequence.Split(',');

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                // Use a transaction in case something fails mid-update
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // The index represents the new sequence order (1, 2, 3, etc.)
                        for (int i = 0; i < items.Length; i++)
                        {
                            string[] parts = items[i].Split('_'); // Splits "Lesson_12" into ["Lesson", "12"]
                            string itemType = parts[0];
                            int itemId = Convert.ToInt32(parts[1]);
                            int newSequenceIndex = i + 1; // 1-based index

                            string updateSql = "";

                            if (itemType == "Lesson")
                            {
                                updateSql = "UPDATE learningResource SET sequence_order = @Seq WHERE Id = @Id";
                            }
                            else if (itemType == "Quiz")
                            {
                                updateSql = "UPDATE quiz SET sequence_order = @Seq WHERE Id = @Id";
                            }

                            if (!string.IsNullOrEmpty(updateSql))
                            {
                                using (SqlCommand cmd = new SqlCommand(updateSql, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Seq", newSequenceIndex);
                                    cmd.Parameters.AddWithValue("@Id", itemId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();

                        // Reload the content so the screen reflects the new saved order
                        int courseId = Convert.ToInt32(Request.QueryString["id"]);
                        LoadCourseContent(courseId);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }
        }
        protected void btnEditOverview_Click(object sender, EventArgs e)
        {
            if (int.TryParse(Request.QueryString["id"], out int courseId))
            {
                EditCourseOverviewModal1.LoadCourse(courseId);
            }

        }
        protected void btnPublish_Click(object sender, EventArgs e)
        {
            UpdateCourseStatus("PUBLISHED");
        }
        protected void btnUnpublish_Click(object sender, EventArgs e)
        {
            UpdateCourseStatus("PRIVATE");
        }

        private void UpdateCourseStatus(string newStatus)
        {
            if (int.TryParse(Request.QueryString["id"], out int courseId))
            {
                string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = "UPDATE course SET status = @Status WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Status", newStatus);
                        cmd.Parameters.AddWithValue("@Id", courseId);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Refresh the overview panel to reflect the new status and swap the buttons!
                LoadCourseOverview(courseId);
            }
        }
    }
}