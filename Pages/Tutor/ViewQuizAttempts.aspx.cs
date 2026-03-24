using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class ViewQuizAttempts : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Security Check
            if (Session["UserName"] == null || Session["role_id"] == null || Convert.ToInt32(Session["role_id"]) != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["quiz_id"] != null)
                {
                    int quizId = Convert.ToInt32(Request.QueryString["quiz_id"]);
                    LoadQuizDetails(quizId);
                    BindAttemptsData(quizId);
                }
                else
                {
                    lblEmpty.Visible = true;
                    lblEmpty.Text = "Invalid Quiz ID.";
                    pagerWrapper.Visible = false;
                }
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
            if (!path.EndsWith("/ViewQuizAttempts.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/ViewQuizAttempts", StringComparison.OrdinalIgnoreCase))
                return SiteMap.CurrentNode;

            SiteMapNode current = SiteMap.CurrentNode;
            if (current == null) return null;

            SiteMapNode clone = current.Clone(true);

            if (!int.TryParse(ctx.Request.QueryString["quiz_id"], out int quizId))
                return clone;

            // Maintain the query string for the current node
            clone.Url += $"?quiz_id={quizId}";

            // Look up the Course ID so we can fix the parent node's link
            int courseId = 0;
            string courseTitle = "";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT c.Id, c.title 
                    FROM quiz q 
                    INNER JOIN course c ON q.course_id = c.Id 
                    WHERE q.Id = @QuizId";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@QuizId", quizId);
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            courseId = Convert.ToInt32(dr["Id"]);
                            courseTitle = dr["title"].ToString();
                        }
                    }
                }
            }

            // Fix the parent node ("EditCourse") so it redirects to the correct course
            if (clone.ParentNode != null && courseId > 0)
            {
                clone.ParentNode.Title = $"Edit - {courseTitle}";
                clone.ParentNode.Url += $"?id={courseId}";
            }

            return clone;
        }

        private void LoadQuizDetails(int quizId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = "SELECT title, course_id FROM quiz WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", quizId);
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            lblQuizTitle.Text = "Attempts: " + dr["title"].ToString();
                        }
                    }
                }
            }
        }

        private void BindAttemptsData(int quizId)
        {
            string sql = @"
                SELECT 
                    qa.Id AS AttemptId,
                    (u.fname + ' ' + u.lname) AS StudentName,
                    u.email AS StudentEmail,
                    qa.started_at,
                    qa.finished_at,
                    qa.score,
                    qa.status
                FROM quizAttempt qa
                INNER JOIN enrollment e ON qa.enrollment_id = e.Id
                INNER JOIN [user] u ON e.student_id = u.Id
                WHERE qa.quiz_id = @QuizId";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                sql += " AND (u.fname LIKE @Search OR u.lname LIKE @Search OR u.email LIKE @Search)";

            if (ddlStatusFilter.SelectedValue != "All")
                sql += " AND qa.status = @Status";

            sql += " ORDER BY qa.started_at DESC";

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@QuizId", quizId);

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    cmd.Parameters.AddWithValue("@Search", "%" + txtSearch.Text.Trim() + "%");

                if (ddlStatusFilter.SelectedValue != "All")
                    cmd.Parameters.AddWithValue("@Status", ddlStatusFilter.SelectedValue);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvAttempts.DataSource = dt;
                gvAttempts.DataBind();

                UpdatePagingLabels(dt.Rows.Count);

                lblEmpty.Visible = (dt.Rows.Count == 0);
                lblEmpty.Text = dt.Rows.Count == 0 ? "No attempts found for this quiz matching your criteria." : "";
                pagerWrapper.Visible = (dt.Rows.Count > 0);
            }
        }

        protected string GetStatusBadge(string status)
        {
            switch (status.ToUpper())
            {
                case "GRADED":
                    return "<span class='badge bg-success bg-opacity-10 text-success border border-success'>Graded</span>";
                case "SUBMITTED":
                    return "<span class='badge bg-warning bg-opacity-10 text-warning border border-warning'>Pending Grade</span>";
                case "IN_PROGRESS":
                    return "<span class='badge bg-primary bg-opacity-10 text-primary border border-primary'>In Progress</span>";
                default:
                    return $"<span class='badge bg-secondary'>{status}</span>";
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvAttempts.PageIndex = 0;
            BindAttemptsData(Convert.ToInt32(Request.QueryString["quiz_id"]));
        }

        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            gvAttempts.PageIndex = 0;
            BindAttemptsData(Convert.ToInt32(Request.QueryString["quiz_id"]));
        }

        protected void gvAttempts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAttempts.PageIndex = e.NewPageIndex;
            BindAttemptsData(Convert.ToInt32(Request.QueryString["quiz_id"]));
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvAttempts.PageIndex > 0)
            {
                gvAttempts.PageIndex--;
                BindAttemptsData(Convert.ToInt32(Request.QueryString["quiz_id"]));
            }
        }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (gvAttempts.PageIndex < gvAttempts.PageCount - 1)
            {
                gvAttempts.PageIndex++;
                BindAttemptsData(Convert.ToInt32(Request.QueryString["quiz_id"]));
            }
        }

        private void UpdatePagingLabels(int total)
        {
            int start = (gvAttempts.PageIndex * gvAttempts.PageSize) + 1;
            int end = Math.Min((gvAttempts.PageIndex + 1) * gvAttempts.PageSize, total);
            lblShowing.Text = total == 0 ? "Showing 0 attempts" : $"Showing {start}-{end} of {total} attempts";

            btnPrev.CssClass = gvAttempts.PageIndex == 0 ? "btn-sub disabled" : "btn-sub";
            btnNext.CssClass = gvAttempts.PageIndex >= (gvAttempts.PageCount - 1) || total == 0 ? "btn-sub disabled" : "btn-sub";
        }
    }
}