using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Staff
{
    public partial class AddResource : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["role_id"] == null || (int)Session["role_id"] != 2)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                BindDropdowns();
                ToggleInputRows();
            }
        }

        private void BindDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Bind Resource Types
                using (SqlCommand cmdType = new SqlCommand("SELECT Id, name FROM [resourceType]", conn))
                {
                    using (SqlDataReader rdrType = cmdType.ExecuteReader())
                    {
                        ddlAddType.DataSource = rdrType;
                        ddlAddType.DataTextField = "name";
                        ddlAddType.DataValueField = "Id";
                        ddlAddType.DataBind();
                    }
                }

                // 2. Bind Tutors 
                using (SqlCommand cmdTutor = new SqlCommand(@"SELECT Id, 
                                                                     ('T' + RIGHT('000'+CAST(Id AS VARCHAR), 3) + '-' + fname + ' ' + lname) as FullTutorName 
                                                              FROM [user] 
                                                              WHERE role_id = 3", conn))
                {
                    using (SqlDataReader rdrTutor = cmdTutor.ExecuteReader())
                    {
                        ddlAddTutor.DataSource = rdrTutor;
                        ddlAddTutor.DataTextField = "FullTutorName";
                        ddlAddTutor.DataValueField = "Id";
                        ddlAddTutor.DataBind();
                        // Insert a default placeholder
                        ddlAddTutor.Items.Insert(0, new ListItem("-- Select a Course First --", ""));
                    }
                }

                // 3. Bind Courses
                using (SqlCommand cmdCourse = new SqlCommand("SELECT Id, title FROM [course]", conn))
                {
                    using (SqlDataReader rdrCourse = cmdCourse.ExecuteReader())
                    {
                        ddlAddCourse.DataSource = rdrCourse;
                        ddlAddCourse.DataTextField = "title";
                        ddlAddCourse.DataValueField = "Id";
                        ddlAddCourse.DataBind();
                        ddlAddCourse.Items.Insert(0, new ListItem("-- Select Course --", ""));
                    }
                }
            }
        }

        protected void ddlAddCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlAddCourse.SelectedValue))
            {
                ddlAddTutor.ClearSelection();
                ddlAddTutor.SelectedIndex = 0;
                ddlAddTutor.Enabled = false;
                return;
            }

            int selectedCourseId = Convert.ToInt32(ddlAddCourse.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT tutor_id FROM [course] WHERE Id = @CourseId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", selectedCourseId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string tutorId = result.ToString();

                        if (ddlAddTutor.Items.FindByValue(tutorId) != null)
                        {
                            ddlAddTutor.ClearSelection();
                            ddlAddTutor.Items.FindByValue(tutorId).Selected = true;
                            ddlAddTutor.Enabled = false;
                        }
                    }
                }
            }
        }

        // Fired when the user changes between Video, PDF, Document, etc.
        protected void ddlAddType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToggleInputRows();
        }

        private void ToggleInputRows()
        {
            string selectedText = ddlAddType.SelectedItem.Text.ToLower();

            // If the user selects PDF or Document, hide the Link TextBox and show the File Upload
            if (selectedText.Contains("pdf") || selectedText.Contains("document"))
            {
                rowVideoLink.Visible = false;
                rowPdfUpload.Visible = true;
            }
            else
            {
                rowVideoLink.Visible = true;
                rowPdfUpload.Visible = false;
            }
        }

        protected void btnSaveResource_Click(object sender, EventArgs e)
        {
            // 1. Validation Checks
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please provide a Resource Title.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            if (string.IsNullOrEmpty(ddlAddCourse.SelectedValue))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select a Course.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            string resourceLinkToSave = "";
            string selectedTypeText = ddlAddType.SelectedItem.Text.ToLower();

            // Determine if we are saving a URL or a File
            if (selectedTypeText.Contains("pdf") || selectedTypeText.Contains("document"))
            {
                // PDF UPLOAD LOGIC
                if (fuPdfResource.HasFile)
                {
                    string ext = Path.GetExtension(fuPdfResource.FileName).ToLower();
                    if (ext != ".pdf")
                    {
                        lblMessage.Visible = true;
                        lblMessage.Text = "Only PDF files are allowed for this resource type.";
                        lblMessage.CssClass = "alert alert-danger d-block";
                        return;
                    }

                    string folder = Server.MapPath("~/Uploads/Resources/");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string fileName = Guid.NewGuid().ToString("N") + ".pdf";
                    string fullPath = Path.Combine(folder, fileName);
                    fuPdfResource.SaveAs(fullPath);

                    // The path saved to the database
                    resourceLinkToSave = "~/Uploads/Resources/" + fileName;
                }
                else
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Please select a PDF file to upload.";
                    lblMessage.CssClass = "alert alert-danger d-block";
                    return;
                }
            }
            else
            {
                // VIDEO LINK LOGIC
                if (string.IsNullOrWhiteSpace(txtAddLink.Text))
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Please provide a valid Video Link.";
                    lblMessage.CssClass = "alert alert-danger d-block";
                    return;
                }

                string link = txtAddLink.Text.Trim();

                // Convert YouTube links to embed format automatically
                if (link.Contains("youtube.com") || link.Contains("youtu.be"))
                {
                    resourceLinkToSave = ConvertToYouTubeEmbed(link);
                }
                else
                {
                    resourceLinkToSave = link;
                }
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"INSERT INTO [learningResource] 
                                   (title, course_id, created_at, resource_type, resource_link, tutor_id) 
                                   VALUES (@title, @course, GETDATE(), @type, @link, @tutor)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@course", ddlAddCourse.SelectedValue);
                        cmd.Parameters.AddWithValue("@type", ddlAddType.SelectedValue);
                        cmd.Parameters.AddWithValue("@link", resourceLinkToSave);
                        cmd.Parameters.AddWithValue("@tutor", ddlAddTutor.SelectedValue);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                Response.Redirect("LearningResourceManagement.aspx?msg=success");
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error adding resource: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        // Helper Method to convert raw YouTube links to Embed format
        private string ConvertToYouTubeEmbed(string rawUrl)
        {
            try
            {
                string videoId = string.Empty;

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

                if (videoId.Contains("?"))
                {
                    videoId = videoId.Substring(0, videoId.IndexOf("?"));
                }
                if (videoId.Contains("&"))
                {
                    videoId = videoId.Substring(0, videoId.IndexOf("&"));
                }

                videoId = videoId.Trim();

                if (videoId.Length == 11)
                {
                    return $"https://www.youtube.com/embed/{videoId}";
                }
            }
            catch
            {
                // Fail silently and return original if something goes wrong
            }

            return rawUrl;
        }
    }
}