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
    public partial class Results : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        int studentId;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
            {
                Response.Redirect("~/Pages/Student/Home.aspx");
                return;
            }

            studentId = Convert.ToInt32(Session["UserId"]);

            if (!IsPostBack)
            {
                LoadEnrolledCourses();
            }
        }

        private void LoadEnrolledCourses()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Fetch all courses the student is currently enrolled in
                string query = @"
                    SELECT e.Id AS EnrollmentId, c.title AS CourseName 
                    FROM enrollment e WITH (NOLOCK)
                    JOIN course c WITH (NOLOCK) ON e.course_id = c.Id
                    WHERE e.student_id = @sid AND e.status IN ('ENROLLED', 'COMPLETED')
                    ORDER BY c.title ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptCourses.DataSource = dt;
                    rptCourses.DataBind();
                    phNoEnrollments.Visible = false;
                }
                else
                {
                    // No enrollments at all
                    phNoEnrollments.Visible = true;
                }
            }
        }

        protected void rptCourses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                HiddenField hfEnrollmentId = (HiddenField)e.Item.FindControl("hfEnrollmentId");
                Repeater rptQuizzes = (Repeater)e.Item.FindControl("rptQuizzes");
                PlaceHolder phNoQuizzes = (PlaceHolder)e.Item.FindControl("phNoQuizzes");

                int enrollmentId = Convert.ToInt32(hfEnrollmentId.Value);

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // THE MAGIC QUERY: Grabs ONLY the most recent 'GRADED' attempt for each quiz!
                    string query = @"
                        WITH LatestAttempts AS (
                            SELECT quiz_id, score, finished_at, enrollment_id,
                                   ROW_NUMBER() OVER(PARTITION BY quiz_id ORDER BY finished_at DESC) as rn
                            FROM quizAttempt WITH (NOLOCK)
                            WHERE enrollment_id = @eid AND status = 'GRADED'
                        )
                        SELECT la.quiz_id, la.score, la.finished_at, la.enrollment_id, 
                               q.title AS QuizTitle, lr.Id AS ResourceId
                        FROM LatestAttempts la
                        JOIN quiz q WITH (NOLOCK) ON la.quiz_id = q.Id
                        JOIN learningResource lr WITH (NOLOCK) ON q.Id = lr.quiz_id
                        WHERE la.rn = 1
                        ORDER BY la.finished_at DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@eid", enrollmentId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtQuizzes = new DataTable();
                    da.Fill(dtQuizzes);

                    if (dtQuizzes.Rows.Count > 0)
                    {
                        // They have finished quizzes, bind them to the inner table!
                        rptQuizzes.DataSource = dtQuizzes;
                        rptQuizzes.DataBind();
                        rptQuizzes.Visible = true;
                        phNoQuizzes.Visible = false;
                    }
                    else
                    {
                        // Enrolled, but haven't taken any quizzes yet
                        rptQuizzes.Visible = false;
                        phNoQuizzes.Visible = true;
                    }
                }
            }
        }

        protected string GetStatusClass(object scoreObj)
        {
            if (scoreObj != null && scoreObj != DBNull.Value)
            {
                int score = Convert.ToInt32(scoreObj);
                if (score >= 50)
                    return "ec-status-pill ec-status-active shadow-sm"; // Green Pill
                else
                    return "ec-status-pill ec-status-danger shadow-sm"; // Red Pill
            }
            return "ec-status-pill ec-status-locked"; // Grey Pill
        }

        protected string GetStatusText(object scoreObj)
        {
            if (scoreObj != null && scoreObj != DBNull.Value)
            {
                int score = Convert.ToInt32(scoreObj);
                return score >= 50 ? "Passed" : "Failed";
            }
            return "Unknown";
        }
    }
}