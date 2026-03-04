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
    public partial class Announcement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            if (!IsPostBack)
            {
                lblMsg.Visible = false;
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
        private SiteMapNode SiteMap_Resolve(object sender, SiteMapResolveEventArgs e)
        {
            var ctx = e.Context;
            if (ctx?.Request == null) return SiteMap.CurrentNode;

            // only apply to Announcement page
            string path = ctx.Request.Path;
            if (!path.EndsWith("/Announcement.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/Announcement", StringComparison.OrdinalIgnoreCase))
                return SiteMap.CurrentNode;

            SiteMapNode current = SiteMap.CurrentNode;
            if (current == null) return null;

            // Clone(true) clones the current node AND its ancestors (parents)
            SiteMapNode clone = current.Clone(true);

            // get course id
            if (!int.TryParse(ctx.Request.QueryString["id"], out int courseId))
                return clone;

            string courseTitle = GetCourseName(courseId);
            if (string.IsNullOrWhiteSpace(courseTitle))
                return clone;

            // Optional: Append ID to current node's URL to be perfectly consistent
            clone.Url += $"?id={courseId}";

            // Walk up: Announcement -> Edit -> Courses
            if (clone.ParentNode != null)
            {
                // Update the title
                clone.ParentNode.Title = $"Edit - {courseTitle}";

                // CRITICAL FIX: Append the ID to the Parent Node's URL!
                // This ensures the breadcrumb link actually sends you back to the right course.
                clone.ParentNode.Url += $"?id={courseId}";
            }

            return clone;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            lblMsg.ForeColor = System.Drawing.Color.Red;

            if (string.IsNullOrWhiteSpace(title.Text))
            {
                lblMsg.Text = "Please enter an announcement title.";
                lblMsg.Visible = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(description.Text))
            {
                lblMsg.Text = "Please enter the announcement message.";
                lblMsg.Visible = true;
                return;
            }

            // 2. Ensure Tutor is logged in
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            int createdBy = Convert.ToInt32(Session["UserId"]);

            int targetRoleId = 4;

            // 3. Insert into Database
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            // Added course_id to the query
            string query = @"
    INSERT INTO announcement (target_role_id, course_id, title, message, created_by, status) 
    VALUES (@targetRoleId, @courseId, @title, @message, @createdBy, 'ACTIVE')";

            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@targetRoleId", targetRoleId);
                    cmd.Parameters.AddWithValue("@title", title.Text.Trim());
                    cmd.Parameters.AddWithValue("@message", description.Text.Trim());
                    cmd.Parameters.AddWithValue("@createdBy", createdBy);

                    // Check if we are inside a specific course or making a general announcement
                    string courseIdStr = Request.QueryString["id"];
                    if (!string.IsNullOrEmpty(courseIdStr) && int.TryParse(courseIdStr, out int courseId))
                    {
                        cmd.Parameters.AddWithValue("@courseId", courseId);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@courseId", DBNull.Value); // Saves as NULL in database
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                lblMsg.ForeColor = System.Drawing.Color.Green;
                lblMsg.Text = "Announcement sent successfully!";
                lblMsg.Visible = true;

                // Clear the fields
                title.Text = "";
                description.Text = "";
            }
            catch (Exception ex)
            {
                lblMsg.Text = "An error occurred: " + ex.Message;
                lblMsg.Visible = true;
            }
        }
    }
}