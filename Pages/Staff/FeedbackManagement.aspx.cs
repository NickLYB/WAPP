using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace WAPP.Pages.Staff
{
    public partial class FeedbackManagement : System.Web.UI.Page
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
                BindGrid();
            }
        }

        private void BindDropdowns()
        {
            ddlFilterMonth.Items.Clear();
            ddlFilterMonth.Items.Add(new ListItem("All Months", "All"));
            ddlFilterMonth.Items.Add(new ListItem("January", "1"));
            ddlFilterMonth.Items.Add(new ListItem("February", "2"));
            ddlFilterMonth.Items.Add(new ListItem("March", "3"));
            ddlFilterMonth.Items.Add(new ListItem("April", "4"));
            ddlFilterMonth.Items.Add(new ListItem("May", "5"));
            ddlFilterMonth.Items.Add(new ListItem("June", "6"));
            ddlFilterMonth.Items.Add(new ListItem("July", "7"));
            ddlFilterMonth.Items.Add(new ListItem("August", "8"));
            ddlFilterMonth.Items.Add(new ListItem("September", "9"));
            ddlFilterMonth.Items.Add(new ListItem("October", "10"));
            ddlFilterMonth.Items.Add(new ListItem("November", "11"));
            ddlFilterMonth.Items.Add(new ListItem("December", "12"));

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                using (SqlCommand cmdTutor = new SqlCommand(@"SELECT Id, ('T' + RIGHT('000'+CAST(Id AS VARCHAR), 3) + '-' + fname + ' ' + ISNULL(lname,'')) as FullTutorName FROM [user] WHERE role_id = 3", conn))
                {
                    using (SqlDataReader rdr = cmdTutor.ExecuteReader())
                    {
                        ddlFilterTutor.DataSource = rdr;
                        ddlFilterTutor.DataTextField = "FullTutorName";
                        ddlFilterTutor.DataValueField = "Id";
                        ddlFilterTutor.DataBind();
                        ddlFilterTutor.Items.Insert(0, new ListItem("All Tutors", "All"));
                    }
                }

                using (SqlCommand cmdCourse = new SqlCommand("SELECT Id, title FROM [course]", conn))
                {
                    using (SqlDataReader rdr = cmdCourse.ExecuteReader())
                    {
                        ddlFilterCourse.DataSource = rdr;
                        ddlFilterCourse.DataTextField = "title";
                        ddlFilterCourse.DataValueField = "Id";
                        ddlFilterCourse.DataBind();
                        ddlFilterCourse.Items.Insert(0, new ListItem("All Courses", "All"));
                    }
                }

                ddlFilterResource.Items.Insert(0, new ListItem("All Resources", "All"));
                ddlFilterResource.Enabled = false;

                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM [feedback] ORDER BY YearVal DESC", conn))
                {
                    using (SqlDataReader rdr = cmdYear.ExecuteReader())
                    {
                        ddlFilterYear.DataSource = rdr;
                        ddlFilterYear.DataTextField = "YearVal";
                        ddlFilterYear.DataValueField = "YearVal";
                        ddlFilterYear.DataBind();
                        ddlFilterYear.Items.Insert(0, new ListItem("All Years", "All"));
                    }
                }
            }
        }

        protected void ddlFilterCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlFilterCourse.SelectedValue == "All")
            {
                ddlFilterResource.Items.Clear();
                ddlFilterResource.Items.Insert(0, new ListItem("All Resources", "All"));
                ddlFilterResource.Enabled = false;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT Id, title FROM [learningResource] WHERE course_id = @courseId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseId", ddlFilterCourse.SelectedValue);
                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            ddlFilterResource.DataSource = rdr;
                            ddlFilterResource.DataTextField = "title";
                            ddlFilterResource.DataValueField = "Id";
                            ddlFilterResource.DataBind();
                        }
                    }
                }
                ddlFilterResource.Items.Insert(0, new ListItem("All Resources", "All"));
                ddlFilterResource.Enabled = true;
            }

            gvFeedbacks.PageIndex = 0;
            BindGrid();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSortBy.SelectedValue = "Newest";
            ddlFilterTutor.SelectedValue = "All";

            ddlFilterCourse.SelectedValue = "All";
            ddlFilterResource.Items.Clear();
            ddlFilterResource.Items.Insert(0, new ListItem("All Resources", "All"));
            ddlFilterResource.Enabled = false;

            ddlFilterRating.SelectedValue = "All";
            ddlFilterMonth.SelectedValue = "All";
            ddlFilterYear.SelectedValue = "All";
            lblMessage.Visible = false;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ClearSearch", "document.getElementById('" + txtSearch.ClientID + "').value = ''; SearchTable();", true);

            gvFeedbacks.PageIndex = 0;
            BindGrid();
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // UPDATED SQL: Handle NULL courses/resources gracefully for tutor-only feedback.
                string sql = @"SELECT f.Id, f.created_at, f.rating, f.comment, f.status,
                                     ('T' + RIGHT('000'+CAST(t.Id AS VARCHAR), 3) + '-' + t.fname + ' ' + ISNULL(t.lname,'')) AS TutorName,
                                     CASE 
                                        WHEN f.course_id IS NULL THEN 'General Tutor Feedback'
                                        ELSE (c.title + ' - ' + ISNULL(lr.title, 'Course Overall Feedback')) 
                                     END AS CourseAndResource
                               FROM [feedback] f
                               INNER JOIN [user] t ON f.tutor_id = t.Id
                               LEFT JOIN [course] c ON f.course_id = c.Id
                               LEFT JOIN [learningResource] lr ON f.resource_id = lr.Id
                               WHERE 1=1";

                if (ddlFilterTutor.SelectedValue != "All") sql += " AND f.tutor_id = @tutor";
                if (ddlFilterCourse.SelectedValue != "All") sql += " AND f.course_id = @course";
                if (ddlFilterResource.SelectedValue != "All") sql += " AND f.resource_id = @resource";
                if (ddlFilterRating.SelectedValue != "All") sql += " AND f.rating = @rating";
                if (ddlFilterMonth.SelectedValue != "All") sql += " AND MONTH(f.created_at) = @month";
                if (ddlFilterYear.SelectedValue != "All") sql += " AND YEAR(f.created_at) = @year";

                string sortStr = ddlSortBy.SelectedValue;
                if (sortStr == "Newest") sql += " ORDER BY f.created_at DESC";
                else if (sortStr == "Oldest") sql += " ORDER BY f.created_at ASC";
                else if (sortStr == "RatingDesc") sql += " ORDER BY f.rating DESC";
                else if (sortStr == "RatingAsc") sql += " ORDER BY f.rating ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (ddlFilterTutor.SelectedValue != "All") cmd.Parameters.AddWithValue("@tutor", ddlFilterTutor.SelectedValue);
                    if (ddlFilterCourse.SelectedValue != "All") cmd.Parameters.AddWithValue("@course", ddlFilterCourse.SelectedValue);
                    if (ddlFilterResource.SelectedValue != "All") cmd.Parameters.AddWithValue("@resource", ddlFilterResource.SelectedValue);
                    if (ddlFilterRating.SelectedValue != "All") cmd.Parameters.AddWithValue("@rating", ddlFilterRating.SelectedValue);
                    if (ddlFilterMonth.SelectedValue != "All") cmd.Parameters.AddWithValue("@month", ddlFilterMonth.SelectedValue);
                    if (ddlFilterYear.SelectedValue != "All") cmd.Parameters.AddWithValue("@year", ddlFilterYear.SelectedValue);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ViewState["TotalFbkRecords"] = dt.Rows.Count;
                        gvFeedbacks.DataSource = dt;
                        gvFeedbacks.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalFbkRecords"] != null ? Convert.ToInt32(ViewState["TotalFbkRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvFeedbacks.PageSize);
            if (totalPages == 0) totalPages = 1;

            int startRecord = (gvFeedbacks.PageIndex * gvFeedbacks.PageSize) + 1;
            int endRecord = startRecord + gvFeedbacks.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} feedbacks";
            txtPageJump.Text = (gvFeedbacks.PageIndex + 1).ToString();

            btnPrev.Enabled = gvFeedbacks.PageIndex > 0;
            btnNext.Enabled = gvFeedbacks.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalFbkRecords"] != null ? Convert.ToInt32(ViewState["TotalFbkRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvFeedbacks.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvFeedbacks.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvFeedbacks.PageIndex = 0;
                else gvFeedbacks.PageIndex = totalPages - 1;
            }
            BindGrid();
        }

        protected void btnPrev_Click(object sender, EventArgs e) { if (gvFeedbacks.PageIndex > 0) { gvFeedbacks.PageIndex--; BindGrid(); } }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalFbkRecords"] != null ? Convert.ToInt32(ViewState["TotalFbkRecords"]) : 0;
            if (gvFeedbacks.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvFeedbacks.PageSize) - 1)) { gvFeedbacks.PageIndex++; BindGrid(); }
        }

        protected void FilterGrid_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvFeedbacks.PageIndex = 0; BindGrid(); }

        protected void gvFeedbacks_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvFeedbacks, "View$" + e.Row.RowIndex);

                string dbStatus = DataBinder.Eval(e.Row.DataItem, "status").ToString();
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    if (dbStatus == "PENDING")
                    {
                        lblStatus.Text = "NEW";
                        lblStatus.CssClass = "badge bg-danger rounded-pill";
                    }
                    else
                    {
                        lblStatus.Text = "VIEWED";
                        lblStatus.CssClass = "badge bg-success rounded-pill opacity-75";
                    }
                }
            }
        }

        protected void gvFeedbacks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int feedbackId = Convert.ToInt32(gvFeedbacks.DataKeys[rowIndex].Value);
                LoadFeedbackDetails(feedbackId);

                UpdateFeedbackStatusToViewed(feedbackId);

                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void UpdateFeedbackStatusToViewed(int feedbackId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "UPDATE [feedback] SET status = 'APPROVED' WHERE Id = @Id AND status = 'PENDING'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", feedbackId);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        BindGrid();
                    }
                }
            }
        }

        private void LoadFeedbackDetails(int id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // UPDATED SQL: Use LEFT JOINs for Course and Resource to support tutor-only feedback logic
                string sql = @"SELECT f.Id, f.created_at, f.rating, f.comment, f.status,
                                     ('S' + RIGHT('000'+CAST(s.Id AS VARCHAR), 3) + '-' + s.fname + ' ' + ISNULL(s.lname,'')) AS StudentName,
                                     ('T' + RIGHT('000'+CAST(t.Id AS VARCHAR), 3) + '-' + t.fname + ' ' + ISNULL(t.lname,'')) AS TutorName,
                                     CASE 
                                        WHEN f.course_id IS NULL THEN 'General Tutor Feedback'
                                        ELSE (c.title + ' - ' + ISNULL(lr.title, 'Course Overall Feedback')) 
                                     END AS CourseAndResource
                               FROM [feedback] f
                               INNER JOIN [user] s ON f.student_id = s.Id
                               INNER JOIN [user] t ON f.tutor_id = t.Id
                               LEFT JOIN [course] c ON f.course_id = c.Id
                               LEFT JOIN [learningResource] lr ON f.resource_id = lr.Id
                               WHERE f.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            litViewId.Text = "F" + rdr["Id"].ToString().PadLeft(3, '0');
                            litViewDate.Text = Convert.ToDateTime(rdr["created_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewStudent.Text = rdr["StudentName"].ToString();
                            litViewTutor.Text = rdr["TutorName"].ToString();
                            litViewResource.Text = rdr["CourseAndResource"].ToString();
                            litViewRating.Text = rdr["rating"].ToString();
                            litViewComment.Text = string.IsNullOrWhiteSpace(rdr["comment"].ToString()) ? "<i>No comment provided</i>" : rdr["comment"].ToString().Replace("\n", "<br/>");
                        }
                    }
                }
            }
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<string> selectedTitles = new List<string>();
            foreach (GridViewRow row in gvFeedbacks.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    string formattedId = "F" + gvFeedbacks.DataKeys[row.RowIndex].Value.ToString().PadLeft(3, '0');
                    selectedTitles.Add($"{formattedId}");
                }
            }

            if (selectedTitles.Count > 0)
            {
                litSelectedTitles.Text = string.Join("<br/>", selectedTitles);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one feedback to remove.";
                lblMessage.CssClass = "alert alert-warning d-block mb-3";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = new List<int>();
            foreach (GridViewRow row in gvFeedbacks.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked) selectedIds.Add(Convert.ToInt32(gvFeedbacks.DataKeys[row.RowIndex].Value));
            }

            if (selectedIds.Count > 0)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string[] paramNames = new string[selectedIds.Count];
                        for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;
                        string sql = $"DELETE FROM [feedback] WHERE Id IN ({string.Join(",", paramNames)})";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    BindGrid();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected feedback(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block mb-3";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block mb-3";
                }
            }
        }
    }
}