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
    public partial class Home : System.Web.UI.Page
    {
         string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }
            else
            {
                lblTutorName.Text = Session["UserName"].ToString();
                int tutorId = Convert.ToInt32(Session["UserId"]);

                CheckTutorStatus(tutorId);
                LoadTeachingOverview();
                LoadRecentAnnouncements();

                // NEW: Check if they were redirected here from the Teaching dashboard
                if (!IsPostBack && Request.QueryString["err"] == "unverified")
                {
                    string script = "alert('Access Denied: You must be a fully verified tutor to access the teaching dashboard. Please wait for your application to be approved.');";
                    ClientScript.RegisterStartupScript(this.GetType(), "AccessDeniedAlert", script, true);
                }
            }
        }
        private void CheckTutorStatus(int tutorId)
        {
            string status = "UNKNOWN";

            using (SqlConnection conn = new SqlConnection(cs))
            {
                string sql = "SELECT TOP 1 status FROM tutorApplication WHERE tutor_id = @tutorId ORDER BY submitted_at DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tutorId", tutorId);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        status = result.ToString();
                    }
                }
            }

            pnlApplicationStatus.Visible = true;
            Session["TutorAppStatus"] = status;

            // Base styling for the sleek pill look
            string baseStyle = "display: inline-flex; align-items: center; gap: 6px; padding: 4px 12px; border-radius: 50px; font-size: 0.85rem; width: fit-content; ";

            switch (status.ToUpper())
            {
                case "APPROVED":
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #d1e7dd; color: #0f5132; border: 1px solid #badbcc;";
                    iconStatus.Attributes["class"] = "bi bi-check-circle-fill";
                    lblStatusText.Text = "Verified";
                    // Leave the button alone, it works normally
                    break;

                case "PENDING":
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #fff3cd; color: #664d03; border: 1px solid #ffecb5;";
                    iconStatus.Attributes["class"] = "bi bi-hourglass-split";
                    lblStatusText.Text = "Pending";
                    DisableCourseButton();
                    break;

                case "REJECTED":
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #f8d7da; color: #842029; border: 1px solid #f5c2c7;";
                    iconStatus.Attributes["class"] = "bi bi-x-octagon-fill";
                    lblStatusText.Text = "Rejected";
                    DisableCourseButton();
                    break;

                default:
                    pnlApplicationStatus.Attributes["style"] = baseStyle + "background-color: #e2e3e5; color: #41464b; border: 1px solid #d3d6d8;";
                    iconStatus.Attributes["class"] = "bi bi-question-circle-fill";
                    lblStatusText.Text = "Action Required";
                    DisableCourseButton();
                    break;
            }
        }

        private void DisableCourseButton()
        {
            lnkCreateCourse.Enabled = false;
            lnkCreateCourse.CssClass = "btn-main w-100 rounded-pill disabled";

            // Force it to look gray and stop pointer clicks via CSS
            lnkCreateCourse.Attributes.Add("style", "background-color: #6c757d !important; border-color: #6c757d !important; color: white !important; opacity: 0.6; pointer-events: none;");

            lnkViewAllAnnouncements.Enabled = false;
            lnkViewAllAnnouncements.CssClass = "btn-main w-100 rounded-pill disabled";

            // Force it to look gray and stop pointer clicks via CSS
            lnkViewAllAnnouncements.Attributes.Add("style", "background-color: #6c757d !important; border-color: #6c757d !important; color: white !important; opacity: 0.6; pointer-events: none;");
        }
        private void LoadTeachingOverview()
        {
            if (Session["UserId"] == null) return;
            int tutorId = Convert.ToInt32(Session["UserId"]);

            // 1. Get Course Counts (Published & Pending)
            using (SqlConnection con = new SqlConnection(cs))
            {
                string courseQuery = @"
            SELECT 
                SUM(CASE WHEN status = 'PUBLISHED' THEN 1 ELSE 0 END) AS PublishedCount,
                SUM(CASE WHEN status = 'PENDING' THEN 1 ELSE 0 END) AS PendingCount
            FROM course 
            WHERE tutor_id = @TutorId";

                using (SqlCommand cmd = new SqlCommand(courseQuery, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblPublishedCourses.Text = reader["PublishedCount"] != DBNull.Value ? reader["PublishedCount"].ToString() : "0";
                            lblPendingCourses.Text = reader["PendingCount"] != DBNull.Value ? reader["PendingCount"].ToString() : "0";
                        }
                    }
                }
            } // Connection for query 1 is closed and returned to the pool here

            // 2. Get Total Learning Resources Count
            using (SqlConnection con = new SqlConnection(cs))
            {
                string resourceQuery = "SELECT COUNT(Id) FROM learningResource WHERE tutor_id = @TutorId";
                using (SqlCommand cmd = new SqlCommand(resourceQuery, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    lblTotalResources.Text = result != DBNull.Value ? result.ToString() : "0";
                }
            } // Connection for query 2 is closed and returned to the pool here
        }

        private void LoadRecentAnnouncements()
        {
            if (Session["UserId"] == null) return;
            int tutorId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT TOP 2 title, message, created_at 
                    FROM announcement 
                    WHERE created_by = @TutorId AND status = 'ACTIVE' 
                    ORDER BY created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TutorId", tutorId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptRecentAnnouncements.DataSource = dt;
                            rptRecentAnnouncements.DataBind();
                            lblNoAnnouncements.Visible = false;
                        }
                        else
                        {
                            rptRecentAnnouncements.Visible = false;
                            lblNoAnnouncements.Visible = true;
                        }
                    }
                }
            }
        }

        // Helper Method: Formats the date to look like "Feb 14, 2026"
        protected string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value) return "";
            if (DateTime.TryParse(dateObj.ToString(), out DateTime dt))
            {
                return dt.ToString("MMM dd, yyyy");
            }
            return dateObj.ToString();
        }

        // Helper Method: Truncates long messages so they don't break the UI card layout
        protected string TruncateMessage(object messageObj, int maxLength)
        {
            if (messageObj == null || messageObj == DBNull.Value) return "";
            string msg = messageObj.ToString();

            if (msg.Length > maxLength)
            {
                return msg.Substring(0, maxLength) + "...";
            }
            return msg;
        }
    }
}