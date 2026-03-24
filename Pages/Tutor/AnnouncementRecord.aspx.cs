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
    public partial class AnnouncementRecord : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                LoadAnnouncements();
            }
        }

        private void LoadAnnouncements()
        {
            if (Session["UserId"] == null) return;
            int tutorId = Convert.ToInt32(Session["UserId"]);

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                // LEFT JOIN to get the course name if it exists, but won't break if course_id is NULL
                string query = @"
                    SELECT 
                        a.Id, 
                        a.title, 
                        a.message, 
                        a.created_at, 
                        c.title AS course_title
                    FROM announcement a
                    LEFT JOIN course c ON a.course_id = c.Id
                    WHERE a.created_by = @TutorId AND a.status = 'ACTIVE'
                    ORDER BY a.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptAnnouncements.DataSource = dt;
                            rptAnnouncements.DataBind();
                            lblEmpty.Visible = false;
                            rptAnnouncements.Visible = true;
                        }
                        else
                        {
                            rptAnnouncements.Visible = false;
                            lblEmpty.Visible = true;
                        }
                    }
                }
            }
        }

        protected void rptAnnouncements_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Archive")
            {
                int announcementId = Convert.ToInt32(e.CommandArgument);
                string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

                // change the status to 'ARCHIVED' rather than fully deleting it, maintaining referential integrity
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = "UPDATE announcement SET status = 'ARCHIVED' WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", announcementId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                lblMsg.ForeColor = System.Drawing.Color.Green;
                lblMsg.Text = "Announcement deleted successfully.";

                // Refresh the list
                LoadAnnouncements();
            }
        }
    }
}