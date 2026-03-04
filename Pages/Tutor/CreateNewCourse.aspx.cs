using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;


namespace WAPP.Pages.Tutor
{
    public partial class CreateNewCourse : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                LoadCourseTypes();
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // 1) Basic validation
            if (string.IsNullOrWhiteSpace(title.Text) ||
                string.IsNullOrWhiteSpace(description.Text) ||
                string.IsNullOrWhiteSpace(duration.Text) ||
                type.SelectedValue == "" ||
                skill.SelectedValue == "")
            {
                // show message if you want (label on page)
                lblMsg.Text = "Please fill in all fields.";
                return;
            }

            if (!int.TryParse(duration.Text.Trim(), out int durationMinutes) || durationMinutes <= 0)
            {
                 lblMsg.Text = "Duration must be a positive number.";
                return;
            }

            // 2) Get tutor id from session (you should have this from login)
            if (Session["UserId"] == null)
            {
                // not logged in / session expired
                Response.Redirect("~/Pages/Login.aspx");
                return;
            }

            int tutorId = Convert.ToInt32(Session["UserId"]);

            // 3) (Optional) handle image upload
            string imagePath = null;
            if (FileUpload1.HasFile)
            {
                string ext = System.IO.Path.GetExtension(FileUpload1.FileName).ToLower();
                string[] allowed = { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowed.Contains(ext))
                {
                     lblMsg.Text = "Only JPG/PNG/GIF files are allowed.";
                    return;
                }

                string fileName = Guid.NewGuid().ToString("N") + ext;

                // Save into ~/Uploads/Courses/ (create folder if not exist)
                string folder = Server.MapPath("~/Images/Courses/");
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                string fullPath = System.IO.Path.Combine(folder, fileName);
                FileUpload1.SaveAs(fullPath);

                imagePath = "~/Images/Courses/" + fileName; // store this in DB if you have a column
            }

            // 4) Insert to DB
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // If your table has an image column, add it here.
                // If not, remove image_path parts.
                string sql = @"INSERT INTO course (title, description, course_type_id, duration_minutes, skill_level, tutor_id, status, image_path) VALUES (@title, @description, @course_type_id, @duration_minutes, @skill_level, @tutor_id, @status, @image_path); SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title.Text.Trim());
                    cmd.Parameters.AddWithValue("@description", description.Text.Trim());
                    cmd.Parameters.AddWithValue("@course_type_id", Convert.ToInt32(type.SelectedValue));
                    cmd.Parameters.AddWithValue("@duration_minutes", durationMinutes);
                    cmd.Parameters.AddWithValue("@skill_level", skill.SelectedValue.ToUpper());
                    cmd.Parameters.AddWithValue("@tutor_id", tutorId);
                    cmd.Parameters.AddWithValue("@status", "PENDING");
                    cmd.Parameters.AddWithValue("@image_path", (object)imagePath ?? DBNull.Value);

                    try
                    {
                        conn.Open();

                        // returns new course id
                        int newCourseId = (int)cmd.ExecuteScalar();

                        Session["NewCourseId"] = newCourseId;

                        // Redirect to your Teaching/My Course page
                        Response.Redirect("Teaching.aspx");
                    }
                    catch (Exception ex)
                    {
                        // For debugging (don't expose in production)
                        lblMsg.Text = "Error: " + ex.Message;
                    }
                }
            }
        }


        protected void Button2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Tutor/Teaching.aspx");
        }

        private void LoadCourseTypes()
        {
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT id, name FROM courseType";
                // change table name if yours is different

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    type.DataSource = reader;
                    type.DataTextField = "name";   // what user sees
                    type.DataValueField = "id";    // what gets stored
                    type.DataBind();
                }
            }

            // Optional default item
            type.Items.Insert(0, new ListItem("-- Select Type --", ""));
        }
    }
}