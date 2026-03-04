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
                BindDropdowns();
                BindGrid();
            }
        }

        private void BindDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                using (SqlCommand cmdTutor = new SqlCommand(@"SELECT DISTINCT u.Id, ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname) as FullTutorName 
                                                              FROM [feedback] f 
                                                              INNER JOIN [user] u ON f.tutor_id = u.Id", conn))
                {
                    using (SqlDataReader rdrTutor = cmdTutor.ExecuteReader())
                    {
                        DataTable dtTutor = new DataTable();
                        dtTutor.Load(rdrTutor);
                        ddlFilterTutor.DataSource = dtTutor;
                        ddlFilterTutor.DataTextField = "FullTutorName";
                        ddlFilterTutor.DataValueField = "Id";
                        ddlFilterTutor.DataBind();
                        ddlFilterTutor.Items.Insert(0, new ListItem("All", "All"));
                    }
                }

                using (SqlCommand cmdResource = new SqlCommand(@"SELECT DISTINCT lr.Id, c.title 
                                                                 FROM [feedback] f 
                                                                 INNER JOIN [learningResource] lr ON f.resource_id = lr.Id
                                                                 INNER JOIN [course] c ON lr.course_id = c.Id", conn))
                {
                    using (SqlDataReader rdrResource = cmdResource.ExecuteReader())
                    {
                        DataTable dtResource = new DataTable();
                        dtResource.Load(rdrResource);
                        ddlFilterResource.DataSource = dtResource;
                        ddlFilterResource.DataTextField = "title";
                        ddlFilterResource.DataValueField = "Id";
                        ddlFilterResource.DataBind();
                        ddlFilterResource.Items.Insert(0, new ListItem("All", "All"));
                    }
                }
            }
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT f.Id, f.created_at, f.rating, f.comment, f.status, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname) AS TutorName,
                                      c.title AS ResourceTitle
                               FROM [feedback] f
                               INNER JOIN [user] u ON f.tutor_id = u.Id
                               INNER JOIN [learningResource] lr ON f.resource_id = lr.Id
                               INNER JOIN [course] c ON lr.course_id = c.Id
                               WHERE 1=1";

                string tutor = ddlFilterTutor.SelectedValue;
                string resource = ddlFilterResource.SelectedValue;
                string rating = ddlFilterRating.SelectedValue;
                string status = ddlFilterStatus.SelectedValue;
                string sort = ddlSortBy.SelectedValue;

                if (tutor != "All") sql += " AND f.tutor_id = @tutor";
                if (resource != "All") sql += " AND f.resource_id = @resource";
                if (rating != "All") sql += " AND f.rating = @rating";

                if (status == "PENDING") sql += " AND f.status = 'PENDING'";
                else if (status == "VIEWED") sql += " AND f.status IN ('APPROVED', 'REJECTED')";

                if (sort == "Oldest") sql += " ORDER BY f.created_at ASC";
                else if (sort == "RatingDesc") sql += " ORDER BY f.rating DESC, f.created_at DESC";
                else if (sort == "RatingAsc") sql += " ORDER BY f.rating ASC, f.created_at DESC";
                else sql += " ORDER BY f.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (tutor != "All") cmd.Parameters.AddWithValue("@tutor", Convert.ToInt32(tutor));
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

        // --- NEW PERMANENT PAGER LOGIC ---
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

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvFeedbacks.PageIndex > 0)
            {
                gvFeedbacks.PageIndex--;
                BindGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalFeedbackRecords"] != null ? Convert.ToInt32(ViewState["TotalFeedbackRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvFeedbacks.PageSize);

            if (gvFeedbacks.PageIndex < totalPages - 1)
            {
                gvFeedbacks.PageIndex++;
                BindGrid();
            }
        }
        // ---------------------------------

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvFeedbacks.PageIndex = 0; // Reset to page 1 on filter
            BindGrid();
        }

        protected void gvFeedbacks_DataBound(object sender, EventArgs e)
        {
            // Empty placeholder to fulfill the asp:GridView OnDataBound property requirement
        }

        protected string GetStatusText(object statusObj)
        {
            if (statusObj == null) return "Newest";
            string status = statusObj.ToString().ToUpper();
            if (status == "PENDING") return "Newest";
            return "Viewed";
        }

        protected string GetStatusDotClass(object statusObj)
        {
            if (statusObj == null) return "status-dot dot-draft";
            string status = statusObj.ToString().ToUpper();
            if (status == "PENDING") return "status-dot dot-draft";
            return "status-dot dot-active";
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
                    selectedIdsText.Add($"[{row.Cells[1].Text}]");
                }
            }

            if (selectedIdsText.Count > 0)
            {
                litSelectedTitles.Text = string.Join(", ", selectedIdsText);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
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
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
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