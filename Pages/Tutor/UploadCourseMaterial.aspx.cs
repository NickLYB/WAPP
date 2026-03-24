using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using WAPP.Utils;

namespace WAPP.Pages.Tutor
{
    public partial class UploadCourseMaterial : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
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
            if (!path.EndsWith("/UploadCourseMaterial.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/UploadCourseMaterial", StringComparison.OrdinalIgnoreCase))
                return SiteMap.CurrentNode;

            SiteMapNode current = SiteMap.CurrentNode;
            if (current == null) return null;

            SiteMapNode clone = current.Clone(true);

            if (!int.TryParse(ctx.Request.QueryString["id"], out int courseId))
                return clone;

            string courseTitle = GetCourseName(courseId);
            if (string.IsNullOrWhiteSpace(courseTitle))
                return clone;

            clone.Url += $"?id={courseId}";

            if (clone.ParentNode != null)
            {
                clone.ParentNode.Title = $"Edit - {courseTitle}";
                clone.ParentNode.Url += $"?id={courseId}";
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

        protected void Button1_Click(object sender, EventArgs e)
        {
            // 1. Basic Validation
            lblMsg.ForeColor = System.Drawing.Color.Red;

            if (string.IsNullOrWhiteSpace(title.Text))
            {
                lblMsg.Text = "Please enter a material title.";
                return;
            }
            if (resource_type.SelectedIndex == 0)
            {
                lblMsg.Text = "Please select a resource type.";
                return;
            }
            if (!int.TryParse(Request.QueryString["id"], out int courseId))
            {
                lblMsg.Text = "Error: Cannot find the Course ID.";
                return;
            }

            // Verify Session
            if (Session["UserId"] == null)
            {
                lblMsg.Text = "Session expired. Please log in again.";
                return;
            }
            int tutorId = Convert.ToInt32(Session["UserId"]);

            try
            {
                string dbFilePath = "";
                bool requiresFileUpload = false;
                string expectedExtension = "";

                // 2. Determine validation based on selected input types
                if (resource_type.SelectedValue == "PDF")
                {
                    requiresFileUpload = true;
                    expectedExtension = ".pdf";
                }
                else if (resource_type.SelectedValue == "Video" && rblVideoSource.SelectedValue == "File")
                {
                    requiresFileUpload = true;
                    expectedExtension = ".mp4";
                }
                else if (resource_type.SelectedValue == "Video" && rblVideoSource.SelectedValue == "Link")
                {
                    requiresFileUpload = false;

                    if (string.IsNullOrWhiteSpace(txtYoutubeLink.Text))
                    {
                        lblMsg.Text = "Please enter a YouTube link.";
                        return;
                    }

                    string link = txtYoutubeLink.Text.Trim();

                    // Simple validation to ensure it's actually a YouTube URL
                    if (!link.Contains("youtube.com") && !link.Contains("youtu.be"))
                    {
                        lblMsg.Text = "Please provide a valid YouTube URL (must contain youtube.com or youtu.be).";
                        return;
                    }
                    dbFilePath = ConvertToYouTubeEmbed(link);
                }

                // 3. Handle File Uploads (if applicable)
                if (requiresFileUpload)
                {
                    if (!FileUpload1.HasFile)
                    {
                        lblMsg.Text = $"Please select a {expectedExtension} file to upload.";
                        return;
                    }

                    // Strict Extension Checking
                    string fileExtension = Path.GetExtension(FileUpload1.FileName).ToLower();
                    if (fileExtension != expectedExtension)
                    {
                        SystemLogService.Write("UPLOAD_INVALID_FORMAT",
                            $"Tutor attempted to upload invalid file type '{fileExtension}' for resource '{resource_type.SelectedValue}'.",
                            LogLevel.WARNING, tutorId);

                        lblMsg.Text = $"Invalid file format! You selected {resource_type.SelectedValue}, so you must upload a {expectedExtension} file.";
                        return;
                    }

                    string originalFileName = Path.GetFileName(FileUpload1.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + originalFileName;
                    string folderPath = Server.MapPath("~/Uploads/CourseMaterials/");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string savePath = Path.Combine(folderPath, uniqueFileName);
                    FileUpload1.SaveAs(savePath);

                    dbFilePath = "~/Uploads/CourseMaterials/" + uniqueFileName;
                }

                // 4. Insert into Database
                string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
                string query = @"
            INSERT INTO learningResource 
            (course_id, created_at, resource_type, resource_link, tutor_id, title, note)
            VALUES 
            (@CourseId, SYSDATETIME(), 
            (SELECT TOP 1 Id FROM resourceType WHERE name = @ResourceTypeName), 
            @ResourceLink, @TutorId, @Title, @Description)";

                using (SqlConnection con = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@ResourceTypeName", resource_type.SelectedValue);
                    cmd.Parameters.AddWithValue("@ResourceLink", dbFilePath);
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    cmd.Parameters.AddWithValue("@Title", title.Text.Trim());

                    if (string.IsNullOrWhiteSpace(description.Text))
                        cmd.Parameters.AddWithValue("@Description", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Description", description.Text.Trim());

                    con.Open();
                    cmd.ExecuteNonQuery();

                    // ---> LOGGING ADDED: INFO (Successful upload metric)
                    SystemLogService.Write("COURSE_MATERIAL_UPLOADED",
                        $"Tutor added '{resource_type.SelectedValue}' material: '{title.Text.Trim()}' to Course ID {courseId}.",
                        LogLevel.INFO, tutorId);
                }

                // 5. Success Feedback
                lblMsg.ForeColor = System.Drawing.Color.Green;
                lblMsg.Text = "Course material uploaded successfully!";

                // Clear fields
                title.Text = "";
                description.Text = "";
                resource_type.SelectedIndex = 0;
                txtYoutubeLink.Text = "";
                rblVideoSource.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // ---> LOGGING ADDED: ERROR (Catches both Disk Write IO issues and DB failures)
                SystemLogService.Write("COURSE_MATERIAL_ERROR",
                    $"Error uploading material '{title.Text}' to Course ID {Request.QueryString["id"]}: {ex.Message}",
                    LogLevel.ERROR, Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : (int?)null);

                // Replaced raw ex.Message with a safe user-facing message
                lblMsg.ForeColor = System.Drawing.Color.Red;
                lblMsg.Text = "An unexpected error occurred while saving the material. Please try again later.";
            }
        }

        private string ConvertToYouTubeEmbed(string rawUrl)
        {
            try
            {
                string videoId = string.Empty;

                // 1. Extract everything after the unique markers
                if (rawUrl.Contains("youtu.be/"))
                {
                    videoId = rawUrl.Substring(rawUrl.IndexOf("youtu.be/") + 9);
                }
                else if (rawUrl.Contains("v="))
                {
                    videoId = rawUrl.Substring(rawUrl.IndexOf("v=") + 2);
                }
                else if (rawUrl.Contains("embed/"))
                {
                    videoId = rawUrl.Substring(rawUrl.IndexOf("embed/") + 6);
                }

                // 2. Chop off any tracking codes (?si= or &t=)
                if (videoId.Contains("?"))
                {
                    videoId = videoId.Substring(0, videoId.IndexOf("?"));
                }
                if (videoId.Contains("&"))
                {
                    videoId = videoId.Substring(0, videoId.IndexOf("&"));
                }

                videoId = videoId.Trim();

                // 3. YouTube IDs are always exactly 11 characters long. 
                // If found it, return the perfect embed link.
                if (videoId.Length == 11)
                {
                    return $"https://www.youtube.com/embed/{videoId}";
                }
            }
            catch
            {

            }

            return rawUrl;
        }
    }
}