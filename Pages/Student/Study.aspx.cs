using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
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
                LoadCourses();
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
                // Join course with courseType for names and user for tutor names
                string query = @"
                    SELECT c.Id, c.title, c.duration_minutes, c.skill_level, c.average_rating, c.image_path, 
                           ct.name as TypeName, 
                           (u.fname + ' ' + u.lname) as TutorFullName
                    FROM course c
                    INNER JOIN courseType ct ON c.course_type_id = ct.Id
                    INNER JOIN [user] u ON c.tutor_id = u.Id
                    WHERE c.status = 'PUBLISHED'";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " AND (c.title LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search)";
                }
                if (categoryId != "0")
                {
                    query += " AND c.course_type_id = @catId";
                }

                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(searchText)) cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");
                if (categoryId != "0") cmd.Parameters.AddWithValue("@catId", categoryId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptCourses.DataSource = dt;
                rptCourses.DataBind();

                // UI management for results count and empty state
                lblCount.Text = dt.Rows.Count.ToString();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadCourses(txtSearch.Text.Trim(), ddlCategory.SelectedValue);
        }
    }
}