using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Masters;

namespace WAPP.Pages.Student
{
    public partial class MyCourses : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        int currentStudentId;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            else
            {
                currentStudentId = Convert.ToInt32(Session["UserId"]);
                LoadEnrolledCourses();
            }
            
        }

        private void LoadEnrolledCourses()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                     SELECT e.course_id, c.title, e.status, c.image_path, e.Id as enrollmentId,
                        (SELECT COUNT(*) FROM learningResource lr WHERE lr.course_id = e.course_id) as TotalLessons,
                        
                        (SELECT COUNT(*) FROM resourceProgress rp
                        JOIN learningResource lr2 ON rp.resource_id = lr2.Id
                        WHERE rp.enrollment_id = e.Id AND rp.completed_at IS NOT NULL) as CompletedLessons,
                        
                        (SELECT TOP 1 Id FROM learningResource WHERE course_id = e.course_id ORDER BY sequence_order ASC, created_at ASC) as FirstLessonId,
                        
                        (SELECT TOP 1 rp2.resource_id FROM resourceProgress rp2 
                         JOIN learningResource lr2 ON rp2.resource_id = lr2.Id 
                         WHERE lr2.course_id = e.course_id AND rp2.enrollment_id = e.Id 
                         ORDER BY rp2.last_accessed DESC) as LastAccessedLessonId
                        
                    FROM enrollment e
                    JOIN course c ON e.course_id = c.Id
                    WHERE e.student_id = @sid AND e.status IN ('ENROLLED', 'COMPLETED')";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", currentStudentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dt.Columns.Add("Progress", typeof(int));
                dt.Columns.Add("TargetResourceId", typeof(int));
                dt.Columns.Add("DisplayStatus", typeof(string)); // NEW: Controls the badge text

                foreach (DataRow row in dt.Rows)
                {
                    int total = Convert.ToInt32(row["TotalLessons"]);
                    int completed = Convert.ToInt32(row["CompletedLessons"]);
                    int progress = total > 0 ? (completed * 100 / total) : 0;
                    row["Progress"] = progress;

                    if (progress >= 100)
                    {
                        // 100% Finished: Go to Lesson 1, and show COMPLETED badge
                        row["TargetResourceId"] = row["FirstLessonId"] != DBNull.Value ? row["FirstLessonId"] : 0;
                        row["DisplayStatus"] = "COMPLETED";
                    }
                    else
                    {
                        // Not Finished: Go to Last Accessed, and show ENROLLED badge
                        row["TargetResourceId"] = row["LastAccessedLessonId"] != DBNull.Value ? row["LastAccessedLessonId"] : (row["FirstLessonId"] != DBNull.Value ? row["FirstLessonId"] : 0);
                        row["DisplayStatus"] = "ENROLLED";
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    rptMyCourses.DataSource = dt;
                    rptMyCourses.DataBind();
                }
                else
                {
                    phEmpty.Visible = true;
                }
            }
        }

        protected int GetFirstResourceId(object courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT TOP 1 Id FROM learningResource WHERE course_id = @cid ORDER BY created_at ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cid", courseId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        protected string GetButtonText(object progressObj)
        {
            int progress = Convert.ToInt32(progressObj);

            // If they have finished all lessons
            if (progress >= 100)
                return "<i class='bi bi-check-circle-fill me-2'></i>Review Course";

            // If they are anywhere from 0% to 99%
            return "<i class='bi bi-arrow-right-circle me-2'></i>Continue Lesson";
        }

        protected string GetButtonCssClass(object progressObj)
        {
            int progress = Convert.ToInt32(progressObj);

            if (progress >= 100)
                return "btn btn-success rounded-pill px-4 fw-bold shadow-sm text-white"; // Turns Green when finished
            else
                return "btn-main rounded-pill px-4 shadow-sm text-white"; // Stays default color while learning
        }
    }
}