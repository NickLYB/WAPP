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
    public partial class Study : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            string status = Session["TutorAppStatus"] as string;
            if (status != "APPROVED")
            {
                Response.Redirect("~/Pages/Tutor/Home.aspx?err=unverified");
                return;
            }
            if (!IsPostBack) 
            { 
                LoadCourses();
            }
        }

        private void LoadCourses()
        {
            // adjust based on how you store tutor id in Session
            string tutorId = Session["UserId"]?.ToString(); // e.g., "T001"
            if (string.IsNullOrEmpty(tutorId))
            {
                //lblEmpty.Text = "Tutor not logged in.";
                //lblEmpty.Visible = true;
                return;
            }

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT Id, title, description, created_at, course_type_id, duration_minutes, skill_level, tutor_id, status, image_path
        FROM course
        WHERE tutor_id = @tutor_id
        ORDER BY created_at DESC;", con))
            {
                cmd.Parameters.AddWithValue("@tutor_id", tutorId);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        //lblEmpty.Visible = true;
                        rptCourses.Visible = false;
                        return;
                    }

                    rptCourses.DataSource = dt;
                    rptCourses.DataBind();
                }
            }
        }

        protected string GetStatusClass(object statusObj)
        {
            string s = (statusObj ?? "").ToString().ToUpperInvariant();

            // map your DB values to badge styles
            switch (s)
            {
                case "PUBLISHED": return "badge-published";
                case "PRIVATE": return "badge-private";
                case "PENDING": return "badge-pending";
                case "REJECT":
                case "REJECTED": return "badge-reject";
                default: return "badge-default";
            }
        }
        protected string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value) return "-";
            if (DateTime.TryParse(dateObj.ToString(), out DateTime dt))
                return dt.ToString("dd/MM/yyyy");
            return dateObj.ToString();
        }
        protected string FormatDuration(object minutesObj)
        {
            if (minutesObj == null || minutesObj == DBNull.Value) return "-";

            if (!int.TryParse(minutesObj.ToString(), out int minutes)) return minutesObj.ToString();

            // 40320 minutes = 28 days -> show as "28d" or "672h"
            if (minutes >= 1440)
                return $"{minutes / 1440}d";
            if (minutes >= 60)
                return $"{minutes / 60}h {minutes % 60}m";
            return $"{minutes}m";
        }

        protected string GetCourseImage(object imagePathObj)
        {
            string path = (imagePathObj ?? "").ToString().Trim();

            // If no image saved in DB -> use placeholder
            if (string.IsNullOrEmpty(path))
                return ResolveUrl("~/Images/course-placeholder.png"); // put a placeholder image here

            // If DB stored like "~/Uploads/Courses/xxx.png"
            if (path.StartsWith("~/"))
                return ResolveUrl(path);

            // If DB stored relative like "Uploads/Courses/xxx.png"
            if (!path.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !path.StartsWith("/"))
                return ResolveUrl("~/" + path);

            // Absolute url or "/Uploads/..."
            return path;
        }
    }
}