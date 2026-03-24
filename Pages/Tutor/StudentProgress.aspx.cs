using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    // Helper classes to structure our data cleanly
    public class LessonSegment
    {
        public int ResourceId { get; set; }
        public bool IsCompleted { get; set; }
        public string Tooltip { get; set; }
    }

    public class StudentProgressData
    {
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; }
        public string Initials { get; set; }
        public int ProgressPercentage { get; set; }
        public List<LessonSegment> Segments { get; set; }
    }

    public partial class StudentProgress : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                string courseId = Request.QueryString["id"];
                if (string.IsNullOrEmpty(courseId))
                {
                    Response.Redirect("Teaching.aspx");
                }

                lnkBack.NavigateUrl = $"~/Pages/Tutor/EditCourse.aspx?id={courseId}";
                LoadCourseTitle(courseId);
                LoadStudentProgress(courseId);
            }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SiteMap.SiteMapResolve += SiteMap_Resolve;
        }
        protected override void OnUnload(EventArgs e)
        {
            SiteMap.SiteMapResolve -= SiteMap_Resolve;
            base.OnUnload(e);
        }
        private SiteMapNode SiteMap_Resolve(object sender, SiteMapResolveEventArgs e)
        {
            var ctx = e.Context;
            if (ctx?.Request == null) return SiteMap.CurrentNode;

            string path = ctx.Request.Path;
            if (!path.EndsWith("/StudentProgress.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/StudentProgress", StringComparison.OrdinalIgnoreCase))
                return SiteMap.CurrentNode;

            SiteMapNode current = SiteMap.CurrentNode;
            if (current == null) return null;

            SiteMapNode clone = current.Clone(true);

            if (!int.TryParse(ctx.Request.QueryString["id"], out int courseId))
                return clone;

            // Fetch the course title
            string courseTitle = "";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT title FROM course WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", courseId);
                con.Open();
                courseTitle = cmd.ExecuteScalar()?.ToString() ?? "Course";
            }

            clone.Url += $"?id={courseId}";

            if (clone.ParentNode != null)
            {
                clone.ParentNode.Title = $"Edit - {courseTitle}";
                clone.ParentNode.Url += $"?id={courseId}";
            }

            return clone;
        }

        private void LoadCourseTitle(string cid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT title FROM course WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", cid);
                conn.Open();
                object title = cmd.ExecuteScalar();
                litCourseTitle.Text = title != null ? title.ToString() : "Unknown Course";
            }
        }
        private void LoadStudentProgress(string courseId)
        {
            // 1. Fetch the exact sequence of lessons for this course
            List<LessonSegment> courseLessons = new List<LessonSegment>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sqlLessons = @"
                    SELECT Id, title 
                    FROM learningResource 
                    WHERE course_id = @cid 
                    ORDER BY ISNULL(sequence_order, 999) ASC, created_at ASC";

                SqlCommand cmd = new SqlCommand(sqlLessons, conn);
                cmd.Parameters.AddWithValue("@cid", courseId);
                conn.Open();

                int index = 1;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        courseLessons.Add(new LessonSegment
                        {
                            ResourceId = Convert.ToInt32(dr["Id"]),
                            IsCompleted = false, // Default state
                            Tooltip = $"Lesson {index}: {dr["title"]}"
                        });
                        index++;
                    }
                }
            }

            // If there are no lessons, don't attempt to calculate math
            int totalLessons = courseLessons.Count;

            // 2. Fetch all enrolled students and their completed resources
            List<StudentProgressData> students = new List<StudentProgressData>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sqlStudents = @"
                    SELECT 
                        e.Id AS EnrollmentId, 
                        u.fname, 
                        u.lname,
                        (SELECT STRING_AGG(rp.resource_id, ',') 
                         FROM resourceProgress rp 
                         WHERE rp.enrollment_id = e.Id AND rp.completed_at IS NOT NULL) AS CompletedResourceIds
                    FROM enrollment e
                    INNER JOIN [user] u ON e.student_id = u.Id
                    WHERE e.course_id = @cid AND e.status IN ('ENROLLED', 'COMPLETED')
                    ORDER BY u.fname ASC";

                SqlCommand cmd = new SqlCommand(sqlStudents, conn);
                cmd.Parameters.AddWithValue("@cid", courseId);
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string fname = dr["fname"].ToString();
                        string lname = dr["lname"].ToString();
                        string completedRaw = dr["CompletedResourceIds"].ToString();

                        // Parse completed IDs into a list for easy checking
                        HashSet<string> completedSet = new HashSet<string>(completedRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                        // Create a fresh copy of the segments for this specific student
                        List<LessonSegment> studentSegments = new List<LessonSegment>();
                        int completedCount = 0;

                        foreach (var lesson in courseLessons)
                        {
                            bool isDone = completedSet.Contains(lesson.ResourceId.ToString());
                            if (isDone) completedCount++;

                            studentSegments.Add(new LessonSegment
                            {
                                ResourceId = lesson.ResourceId,
                                Tooltip = lesson.Tooltip + (isDone ? " (Completed)" : " (Pending)"),
                                IsCompleted = isDone
                            });
                        }

                        // Calculate percentage securely
                        int percentage = (totalLessons > 0) ? (int)Math.Round((double)completedCount / totalLessons * 100) : 0;

                        students.Add(new StudentProgressData
                        {
                            EnrollmentId = Convert.ToInt32(dr["EnrollmentId"]),
                            StudentName = $"{fname} {lname}",
                            Initials = (fname.Substring(0, 1) + lname.Substring(0, 1)).ToUpper(),
                            ProgressPercentage = percentage,
                            Segments = studentSegments
                        });
                    }
                }
            }

            // 3. Bind to UI
            lblTotalStudents.Text = students.Count.ToString();

            if (students.Count > 0)
            {
                rptStudents.DataSource = students;
                rptStudents.DataBind();
                phNoStudents.Visible = false;
            }
            else
            {
                phNoStudents.Visible = true;
            }
        }

        protected void rptStudents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Repeater rptSegments = (Repeater)e.Item.FindControl("rptSegments");
                StudentProgressData studentData = (StudentProgressData)e.Item.DataItem;

                if (rptSegments != null && studentData != null)
                {
                    rptSegments.DataSource = studentData.Segments;
                    rptSegments.DataBind();
                }
            }
        }
    }
}