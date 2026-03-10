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
                Response.Redirect("~/Pages/Guest/Login.aspx");
            }

            if (!IsPostBack)
            {
                BindInitialDropdowns();
                BindResourceDropdown();
                BindGrid();
            }
        }

        private void BindInitialDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlDataAdapter sdaTutor = new SqlDataAdapter(@"SELECT DISTINCT u.Id, ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname) as FullTutorName 
                                                                      FROM [feedback] f 
                                                                      INNER JOIN [user] u ON f.tutor_id = u.Id", conn))
                {
                    DataTable dtTutor = new DataTable();
                    sdaTutor.Fill(dtTutor);
                    ddlFilterTutor.DataSource = dtTutor;
                    ddlFilterTutor.DataTextField = "FullTutorName";
                    ddlFilterTutor.DataValueField = "Id";
                    ddlFilterTutor.DataBind();
                    ddlFilterTutor.Items.Insert(0, new ListItem("All Tutors", "All"));
                }

                using (SqlDataAdapter sdaCourse = new SqlDataAdapter(@"SELECT DISTINCT c.Id, c.title 
                                                                       FROM [feedback] f 
                                                                       INNER JOIN [learningResource] lr ON f.resource_id = lr.Id
                                                                       INNER JOIN [course] c ON lr.course_id = c.Id", conn))
                {
                    DataTable dtCourse = new DataTable();
                    sdaCourse.Fill(dtCourse);
                    ddlFilterCourse.DataSource = dtCourse;
                    ddlFilterCourse.DataTextField = "title";
                    ddlFilterCourse.DataValueField = "Id";
                    ddlFilterCourse.DataBind();
                    ddlFilterCourse.Items.Insert(0, new ListItem("All Courses", "All"));
                }
            }
        }

        private void BindResourceDropdown()
        {
            string selectedCourseId = ddlFilterCourse.SelectedValue;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT DISTINCT lr.Id, ISNULL(lr.title, 'Resource ID ' + CAST(lr.Id AS VARCHAR)) AS resource_title
                               FROM [feedback] f 
                               INNER JOIN [learningResource] lr ON f.resource_id = lr.Id
                               WHERE 1=1";

                if (selectedCourseId != "All")
                {
                    sql += " AND lr.course_id = @CourseId";
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (selectedCourseId != "All")
                    {
                        cmd.Parameters.AddWithValue("@CourseId", Convert.ToInt32(selectedCourseId));
                    }

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ddlFilterResource.DataSource = dt;
                        ddlFilterResource.DataTextField = "resource_title";
                        ddlFilterResource.DataValueField = "Id";
                        ddlFilterResource.DataBind();
                        ddlFilterResource.Items.Insert(0, new ListItem("All Resources", "All"));
                    }
                }
            }
        }

        protected void ddlFilterCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindResourceDropdown();
            FilterGrid_Changed(sender, e);
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT f.Id, f.created_at, f.rating, f.comment, f.status, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname) AS TutorName,
                                      (c.title + ' - ' + ISNULL(lr.title, 'Unnamed Resource')) AS CourseAndResource
                               FROM [feedback] f
                               INNER JOIN [user] u ON f.tutor_id = u.Id
                               INNER JOIN [learningResource] lr ON f.resource_id = lr.Id
                               INNER JOIN [course] c ON lr.course_id = c.Id
                               WHERE 1=1";

                string tutor = ddlFilterTutor.SelectedValue;
                string course = ddlFilterCourse.SelectedValue;
                string resource = ddlFilterResource.SelectedValue;
                string rating = ddlFilterRating.SelectedValue;
                string sort = ddlSortBy.SelectedValue;

                if (tutor != "All") sql += " AND f.tutor_id = @tutor";
                if (course != "All") sql += " AND c.Id = @course";
                if (resource != "All") sql += " AND f.resource_id = @resource";
                if (rating != "All") sql += " AND f.rating = @rating";

                if (sort == "Oldest") sql += " ORDER BY f.created_at ASC";
                else if (sort == "RatingDesc") sql += " ORDER BY f.rating DESC, f.created_at DESC";
                else if (sort == "RatingAsc") sql += " ORDER BY f.rating ASC, f.created_at DESC";
                else sql += " ORDER BY f.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (tutor != "All") cmd.Parameters.AddWithValue("@tutor", Convert.ToInt32(tutor));
                    if (course != "All") cmd.Parameters.AddWithValue("@course", Convert.ToInt32(course));
                    if (resource != "All") cmd.Parameters.AddWithValue("@resource", Convert.ToInt32(resource));
                    if (rating != "All") cmd.Parameters.AddWithValue("@rating", Convert.ToInt32(rating));

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ViewState["TotalFeedbackRecords"] = dt.Rows.Count;
                        gvFeedbacks.DataSource = dt;
                        gvFeedbacks.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalFeedbackRecords"] != null ? Convert.ToInt32(ViewState["TotalFeedbackRecords"]) : 0;
            int startRecord = (gvFeedbacks.PageIndex * gvFeedbacks.PageSize) + 1;
            int endRecord = startRecord + gvFeedbacks.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} feedbacks";

            btnPrev.Enabled = gvFeedbacks.PageIndex > 0;
            btnPrev.CssClass = btnPrev.Enabled ? "pager-link" : "pager-link disabled";

            int totalPages = (int)Math.Ceiling((double)totalRecords / gvFeedbacks.PageSize);
            btnNext.Enabled = gvFeedbacks.PageIndex < (totalPages - 1);
            btnNext.CssClass = btnNext.Enabled ? "pager-link" : "pager-link disabled";
        }

        protected void btnPrev_Click(object sender, EventArgs e) { if (gvFeedbacks.PageIndex > 0) { gvFeedbacks.PageIndex--; BindGrid(); } }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalFeedbackRecords"] != null ? Convert.ToInt32(ViewState["TotalFeedbackRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvFeedbacks.PageSize);
            if (gvFeedbacks.PageIndex < totalPages - 1) { gvFeedbacks.PageIndex++; BindGrid(); }
        }

        protected void FilterGrid_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvFeedbacks.PageIndex = 0; BindGrid(); }

        protected void gvFeedbacks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewFeedback")
            {
                int feedbackId = Convert.ToInt32(e.CommandArgument);

                LoadFeedbackDetails(feedbackId);
                MarkFeedbackAsViewed(feedbackId);
                BindGrid();

                ScriptManager.RegisterStartupScript(upFeedbackMgmt, upFeedbackMgmt.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void LoadFeedbackDetails(int id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT f.Id, f.rating, f.comment, f.created_at, 
                                      ('T' + RIGHT('000'+CAST(u_tutor.Id AS VARCHAR), 3) + '-' + u_tutor.fname) AS TutorName,
                                      (u_student.fname + ' ' + u_student.lname) AS StudentName,
                                      (c.title + ' - ' + ISNULL(lr.title, 'Unnamed Resource')) AS CourseAndResource
                               FROM [feedback] f
                               INNER JOIN [user] u_tutor ON f.tutor_id = u_tutor.Id
                               INNER JOIN [user] u_student ON f.student_id = u_student.Id
                               INNER JOIN [learningResource] lr ON f.resource_id = lr.Id
                               INNER JOIN [course] c ON lr.course_id = c.Id
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

                            string comment = rdr["comment"].ToString();
                            litViewComment.Text = string.IsNullOrWhiteSpace(comment) ? "<i class='text-muted'>No comment provided</i>" : comment;
                        }
                    }
                }
            }
        }

        private void MarkFeedbackAsViewed(int id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "UPDATE [feedback] SET status = 'APPROVED' WHERE Id = @Id AND status = 'PENDING'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<string> selectedIdsText = new List<string>();

            foreach (GridViewRow row in gvFeedbacks.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    // BULLETPROOF FIX: We now pull the ID safely from DataKeys instead of scraping the UI cell!
                    int rawId = Convert.ToInt32(gvFeedbacks.DataKeys[row.RowIndex].Value);
                    string formattedId = "F" + rawId.ToString().PadLeft(3, '0');
                    selectedIdsText.Add($"[{formattedId}]");
                }
            }

            if (selectedIdsText.Count > 0)
            {
                litSelectedTitles.Text = string.Join(", ", selectedIdsText);
                ScriptManager.RegisterStartupScript(upFeedbackMgmt, upFeedbackMgmt.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one feedback to remove.";
                lblMessage.CssClass = "alert alert-warning d-block";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvFeedbacks.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(Convert.ToInt32(gvFeedbacks.DataKeys[row.RowIndex].Value));
                }
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
                            for (int i = 0; i < selectedIds.Count; i++)
                            {
                                cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                            }
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    BindGrid();
                    lblMessage.Visible = true;
                    lblMessage.Text = "Selected feedback(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block";
                    ScriptManager.RegisterStartupScript(upFeedbackMgmt, upFeedbackMgmt.GetType(), "CloseModal", "closeRemoveModal();", true);
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Error removing records: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
        }
    }
}