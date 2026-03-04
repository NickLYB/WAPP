using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class ViewCourse : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        // Expose this so the ASPX Repeater knows which item is currently selected
        public int ActiveResourceId { get; set; } = 0;
        public string CourseId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            // Accept course ID from the URL
            CourseId = Request.QueryString["id"] ?? Request.QueryString["courseId"];

            if (string.IsNullOrEmpty(CourseId))
            {
                Response.Redirect("Teaching.aspx"); // Or your inventory page
            }

            if (!IsPostBack)
            {
                LoadCourseTitle(CourseId);

                // Check if a specific lesson was requested
                if (Request.QueryString["resourceId"] != null && int.TryParse(Request.QueryString["resourceId"], out int resId))
                {
                    ActiveResourceId = resId;
                }
                else
                {
                    // Find the very first lesson in the sequence if none is specified
                    ActiveResourceId = GetFirstLessonId(CourseId);
                }

                if (ActiveResourceId > 0)
                {
                    LoadLesson(ActiveResourceId);
                    phHasContent.Visible = true;
                    phNoContent.Visible = false;
                }
                else
                {
                    // No content in this course yet
                    phHasContent.Visible = false;
                    phNoContent.Visible = true;
                }

                LoadLessonsSidebar(CourseId);
            }
        }

        private void LoadCourseTitle(string cid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT title FROM course WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", cid);
                conn.Open();
                litCourseTitle.Text = cmd.ExecuteScalar()?.ToString() ?? "Course Preview";
            }
        }

        private int GetFirstLessonId(string cid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Grabs the first lesson based on how you sorted it in the edit page
                string query = "SELECT TOP 1 Id FROM learningResource WHERE course_id=@cid ORDER BY sequence_order ASC, created_at ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cid", cid);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private void LoadLessonsSidebar(string cid)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT Id, title, resource_type
                    FROM learningResource
                    WHERE course_id = @cid
                    ORDER BY sequence_order ASC, created_at ASC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@cid", cid);

                DataTable dt = new DataTable();
                da.Fill(dt);

                rptLessons.DataSource = dt;
                rptLessons.DataBind();
            }
        }

        private void LoadLesson(int resourceId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Fetch description (TinyMCE HTML) and the link
                string query = "SELECT title, note, resource_link, resource_type FROM learningResource WHERE Id=@rid";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@rid", resourceId);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string title = dr["title"].ToString();
                    string link = dr["resource_link"].ToString();
                    string descriptionHTML = dr["note"].ToString();
                    int typeId = Convert.ToInt32(dr["resource_type"]);

                    lblCurrentLessonName.Text = string.IsNullOrEmpty(title) ? $"Lesson ID: {resourceId}" : title;

                    // Safely render the rich HTML
                    litLessonNote.Text = string.IsNullOrEmpty(descriptionHTML) ? "No detailed notes available for this lesson." : descriptionHTML;

                    // Render Video or iFrame based on type
                    string fileUrl = ResolveClientUrl(link);

                    if (link.Contains("youtube.com") || link.Contains("youtu.be"))
                    {
                        litVideoPlayer.Text = $"<iframe src='{link}' width='100%' height='100%' style='border:none; min-height: 450px;' allowfullscreen></iframe>";
                    }
                    else if (typeId == 1) // 1 = Video
                    {
                        litVideoPlayer.Text = $@"
                            <video width='100%' height='450px' controls style='background: #000;'>
                                <source src='{fileUrl}' type='video/mp4'>
                                Your browser does not support the video tag.
                            </video>";
                    }
                    else // PDF, PPT, etc.
                    {
                        litVideoPlayer.Text = $@"
                            <iframe src='{fileUrl}' width='100%' height='600px' style='border: none;'>
                                This browser does not support inline documents. Please download the file.
                            </iframe>";
                    }
                }
            }
        }

        protected void lnkLesson_Click(object sender, EventArgs e)
        {
            int newResourceId = Convert.ToInt32((sender as LinkButton).CommandArgument);
            Response.Redirect($"ViewCourse.aspx?id={CourseId}&resourceId={newResourceId}");
        }

        // Helper Method for Sidebar Icons
        protected string GetIcon(object typeIdObj)
        {
            int typeId = Convert.ToInt32(typeIdObj);

            // Adjust based on your resourceType table IDs
            switch (typeId)
            {
                case 1: return "bi bi-play-btn-fill text-danger"; // Video
                case 2: return "bi bi-file-earmark-pdf-fill text-danger"; // PDF
                case 3: return "bi bi-file-earmark-slides-fill text-warning"; // PPT
                default: return "bi bi-file-earmark-text-fill text-primary";
            }
        }

        // Helper Method for active CSS class
        protected string GetLessonCSS(object lessonIdObj)
        {
            int lessonId = Convert.ToInt32(lessonIdObj);
            string cssClass = "lesson-btn ";

            if (lessonId == ActiveResourceId)
            {
                cssClass += "active-lesson text-decoration-none d-block";
            }
            else
            {
                cssClass += "text-decoration-none d-block";
            }

            return cssClass;
        }
    }
}