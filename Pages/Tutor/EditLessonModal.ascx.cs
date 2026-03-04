using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace WAPP.Pages.Tutor
{
    public partial class EditLessonModal : System.Web.UI.UserControl
    {
        public void LoadLessonData(int lessonId)
        {
            hfLessonId.Value = lessonId.ToString();
            lblMsg.Text = "";

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT lr.title, lr.note, lr.resource_link, rt.name as type_name
                    FROM learningResource lr
                    INNER JOIN resourceType rt ON lr.resource_type = rt.Id
                    WHERE lr.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", lessonId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtTitle.Text = reader["title"].ToString();
                            txtDescription.Text = reader["note"].ToString();
                            string link = reader["resource_link"].ToString();

                            // Select Resource Type (Video or PDF)
                            string typeName = reader["type_name"].ToString();
                            if (ddlResourceType.Items.FindByText(typeName) != null)
                            {
                                ddlResourceType.ClearSelection();
                                ddlResourceType.Items.FindByText(typeName).Selected = true;
                            }

                            // If it's a Video, figure out if it's a YouTube link or a Local File
                            if (typeName == "Video")
                            {
                                if (link.Contains("youtube.com") || link.Contains("youtu.be"))
                                {
                                    rblEditVideoSource.SelectedValue = "Link";
                                    txtEditYoutubeLink.Text = link;
                                }
                                else
                                {
                                    rblEditVideoSource.SelectedValue = "File";
                                    txtEditYoutubeLink.Text = "";
                                }
                            }
                            else
                            {
                                rblEditVideoSource.SelectedValue = "File";
                                txtEditYoutubeLink.Text = "";
                            }
                        }
                    }
                }
            }

            // Open the modal via Javascript
            string script = @"
        setTimeout(function() { 
            var myModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('editLessonModal')); 
            myModal.show(); 
            toggleEditInputs(); // Force UI update
        }, 100);";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditLesson", script, true);
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            int lessonId = Convert.ToInt32(hfLessonId.Value);
            string dbFilePath = null;
            lblMsg.Text = "";

            bool isVideo = ddlResourceType.SelectedValue == "Video";
            bool isYoutubeLink = isVideo && rblEditVideoSource.SelectedValue == "Link";

            // 1. Determine if we are replacing the resource link
            if (isYoutubeLink)
            {
                // If they provided a new youtube link, update it
                if (!string.IsNullOrWhiteSpace(txtEditYoutubeLink.Text))
                {
                    string link = txtEditYoutubeLink.Text.Trim();
                    if (!link.Contains("youtube.com") && !link.Contains("youtu.be"))
                    {
                        lblMsg.Text = "Please provide a valid YouTube URL.";

                        // Keep modal open
                        ScriptManager.RegisterStartupScript(this, GetType(), "KeepOpen", "var myModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('editLessonModal')); myModal.show();", true);
                        return;
                    }
                    dbFilePath = ConvertToYouTubeEmbed(link);
                }
            }
            else
            {
                // It's a File (PDF or MP4). Check if they uploaded a new one.
                if (fileUploadEdit.HasFile)
                {
                    string expectedExt = isVideo ? ".mp4" : ".pdf";
                    string fileExtension = Path.GetExtension(fileUploadEdit.FileName).ToLower();

                    if (fileExtension != expectedExt)
                    {
                        lblMsg.Text = $"Invalid file format. You selected {ddlResourceType.SelectedValue}, so you must upload a {expectedExt} file.";
                        ScriptManager.RegisterStartupScript(this, GetType(), "KeepOpen", "var myModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('editLessonModal')); myModal.show();", true);
                        return;
                    }

                    string originalFileName = Path.GetFileName(fileUploadEdit.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + originalFileName;
                    string folderPath = Server.MapPath("~/Uploads/CourseMaterials/");

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    fileUploadEdit.SaveAs(Path.Combine(folderPath, uniqueFileName));
                    dbFilePath = "~/Uploads/CourseMaterials/" + uniqueFileName;
                }
            }

            // 2. Update Database
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string updateSql = @"
                    UPDATE learningResource SET 
                        title = @Title, 
                        note = @Note, 
                        resource_type = (SELECT TOP 1 Id FROM resourceType WHERE name = @TypeName)"
                    + (dbFilePath != null ? ", resource_link = @Link " : " ") +
                    "WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(updateSql, con))
                {
                    cmd.Parameters.AddWithValue("@Id", lessonId);
                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@TypeName", ddlResourceType.SelectedValue);

                    if (dbFilePath != null)
                        cmd.Parameters.AddWithValue("@Link", dbFilePath);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Refresh the main page to show the updated data
            Response.Redirect(Request.RawUrl);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int lessonId = Convert.ToInt32(hfLessonId.Value);
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM learningResource WHERE Id = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", lessonId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            Response.Redirect(Request.RawUrl);
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
                if (videoId.Length == 11)
                {
                    return $"https://www.youtube.com/embed/{videoId}";
                }
            }
            catch
            {
                // Fail silently and return original if something goes completely wrong
            }

            return rawUrl;
        }
    }
}