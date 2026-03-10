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
                     SELECT e.course_id, c.title, e.status, c.image_path,
                        (SELECT COUNT(*) FROM learningResource lr WHERE lr.course_id = e.course_id) as TotalLessons,
                        (SELECT COUNT(*) FROM resourceProgress rp
                        JOIN learningResource lr2 ON rp.resource_id = lr2.Id
                        WHERE rp.enrollment_id = e.Id AND rp.completed_at IS NOT NULL) as CompletedLessons
                    FROM enrollment e
                    JOIN course c ON e.course_id = c.Id
                    WHERE e.student_id = @sid AND e.status = 'ENROLLED'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", currentStudentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dt.Columns.Add("Progress", typeof(int));
                foreach (DataRow row in dt.Rows)
                {
                    int total = Convert.ToInt32(row["TotalLessons"]);
                    int completed = Convert.ToInt32(row["CompletedLessons"]);
                    row["Progress"] = total > 0 ? (completed * 100 / total) : 0;
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
    }
}