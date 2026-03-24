using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class Study : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();
                LoadCourses(); // Loads all courses initially
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id, name FROM courseType ORDER BY name ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                ddlCategory.DataSource = cmd.ExecuteReader();
                ddlCategory.DataTextField = "name";
                ddlCategory.DataValueField = "Id";
                ddlCategory.DataBind();
                ddlCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("All Categories", "0"));
            }
        }

        private void LoadCourses(string searchText = "", string categoryId = "0")
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT c.Id, c.title, c.duration_minutes, c.skill_level, c.average_rating, c.image_path, 
                           ct.name as TypeName, 
                           (u.fname + ' ' + u.lname) as TutorFullName
                    FROM course c WITH (NOLOCK)
                    INNER JOIN courseType ct WITH (NOLOCK) ON c.course_type_id = ct.Id
                    INNER JOIN [user] u WITH (NOLOCK) ON c.tutor_id = u.Id
                    WHERE c.status = 'PUBLISHED'";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " AND (c.title LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search)";
                }

                if (categoryId != "0")
                {
                    query += " AND c.course_type_id = @catId";
                }

                query += " ORDER BY c.created_at DESC";

                SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrEmpty(searchText))
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                if (categoryId != "0")
                    cmd.Parameters.AddWithValue("@catId", categoryId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptCourses.DataSource = dt;
                rptCourses.DataBind();

                lblCount.Text = dt.Rows.Count.ToString();

                if (dt.Rows.Count == 0)
                {
                    rptCourses.Visible = false;
                    phNoCourses.Visible = true;
                }
                else
                {
                    rptCourses.Visible = true;
                    phNoCourses.Visible = false;
                }
            }
        }

        // Fired instantly when user types OR changes the category dropdown
        protected void Search_Changed(object sender, EventArgs e)
        {
            LoadCourses(txtSearch.Text.Trim(), ddlCategory.SelectedValue);
        }
    }
}