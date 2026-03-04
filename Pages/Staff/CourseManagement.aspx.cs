using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Staff
{
    public partial class CourseManagement : System.Web.UI.Page
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
                if (Request.QueryString["msg"] == "success")
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "New course added successfully!";
                    lblMessage.CssClass = "alert alert-success d-block";
                }

                BindDropdowns();
                BindGrid();
            }
        }

        private void BindDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                using (SqlCommand cmdCat = new SqlCommand("SELECT Id, name FROM [courseType]", conn))
                {
                    using (SqlDataReader rdrCat = cmdCat.ExecuteReader())
                    {
                        DataTable dtCat = new DataTable();
                        dtCat.Load(rdrCat);

                        ddlFilterCategory.DataSource = dtCat;
                        ddlFilterCategory.DataTextField = "name";
                        ddlFilterCategory.DataValueField = "Id";
                        ddlFilterCategory.DataBind();
                        ddlFilterCategory.Items.Insert(0, new ListItem("All", "All"));
                    }
                }

                using (SqlCommand cmdTutor = new SqlCommand("SELECT Id, ('T' + RIGHT('000'+CAST(Id AS VARCHAR), 3) + '-' + fname) as FullTutorName FROM [user] WHERE role_id = 3", conn))
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

                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM [course] ORDER BY YearVal DESC", conn))
                {
                    using (SqlDataReader rdrYear = cmdYear.ExecuteReader())
                    {
                        ddlFilterYear.DataSource = rdrYear;
                        ddlFilterYear.DataTextField = "YearVal";
                        ddlFilterYear.DataValueField = "YearVal";
                        ddlFilterYear.DataBind();
                        ddlFilterYear.Items.Insert(0, new ListItem("All", "All"));
                    }
                }
            }
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT c.Id, c.title, c.description, ct.name AS category_name, c.skill_level, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname) AS tutor_name,
                                      c.created_at, c.status, c.average_rating
                               FROM [course] c
                               INNER JOIN [courseType] ct ON c.course_type_id = ct.Id
                               INNER JOIN [user] u ON c.tutor_id = u.Id
                               WHERE 1=1";

                string cat = ddlFilterCategory.SelectedValue;
                string tutor = ddlFilterTutor.SelectedValue;
                string status = ddlFilterStatus.SelectedValue;
                string rating = ddlFilterRating.SelectedValue;
                string month = ddlFilterMonth.SelectedValue;
                string year = ddlFilterYear.SelectedValue;
                string sort = ddlSortBy.SelectedValue;

                if (cat != "All") sql += " AND c.course_type_id = @cat";
                if (tutor != "All") sql += " AND c.tutor_id = @tutor";
                if (status != "All") sql += " AND c.status = @status";
                if (rating != "All") sql += " AND c.average_rating = @rating";
                if (month != "All") sql += " AND MONTH(c.created_at) = @month";
                if (year != "All") sql += " AND YEAR(c.created_at) = @year";

                if (sort == "Oldest") sql += " ORDER BY c.created_at ASC";
                else if (sort == "TutorID") sql += " ORDER BY c.tutor_id ASC, c.created_at DESC";
                else sql += " ORDER BY c.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (cat != "All") cmd.Parameters.AddWithValue("@cat", Convert.ToInt32(cat));
                    if (tutor != "All") cmd.Parameters.AddWithValue("@tutor", Convert.ToInt32(tutor));
                    if (status != "All") cmd.Parameters.AddWithValue("@status", status);
                    if (rating != "All") cmd.Parameters.AddWithValue("@rating", Convert.ToInt32(rating));
                    if (month != "All") cmd.Parameters.AddWithValue("@month", Convert.ToInt32(month));
                    if (year != "All") cmd.Parameters.AddWithValue("@year", Convert.ToInt32(year));

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        ViewState["TotalCourseRecords"] = dt.Rows.Count;

                        gvCourses.DataSource = dt;
                        gvCourses.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        // --- NEW PERMANENT PAGER LOGIC ---
        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalCourseRecords"] != null ? Convert.ToInt32(ViewState["TotalCourseRecords"]) : 0;
            int startRecord = (gvCourses.PageIndex * gvCourses.PageSize) + 1;
            int endRecord = startRecord + gvCourses.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} courses";

            btnPrev.Enabled = gvCourses.PageIndex > 0;
            btnPrev.CssClass = btnPrev.Enabled ? "pager-link" : "pager-link disabled";

            // Use Math.Ceiling to determine total pages accurately
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvCourses.PageSize);
            btnNext.Enabled = gvCourses.PageIndex < (totalPages - 1);
            btnNext.CssClass = btnNext.Enabled ? "pager-link" : "pager-link disabled";
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvCourses.PageIndex > 0)
            {
                gvCourses.PageIndex--;
                BindGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalCourseRecords"] != null ? Convert.ToInt32(ViewState["TotalCourseRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvCourses.PageSize);

            if (gvCourses.PageIndex < totalPages - 1)
            {
                gvCourses.PageIndex++;
                BindGrid();
            }
        }
        // ---------------------------------

        protected void gvCourses_DataBound(object sender, EventArgs e)
        {
            // Empty placeholder to fulfill the asp:GridView OnDataBound property requirement
        }

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvCourses.PageIndex = 0; // Reset to page 1 on filter
            BindGrid();
        }

        protected string GetStatusText(object statusObj)
        {
            if (statusObj == null) return "Pending";
            string status = statusObj.ToString().ToUpper();
            if (status == "APPROVED" || status == "PUBLISHED") return "Approved";
            if (status == "REJECT") return "Rejected";
            return "Pending";
        }

        protected string GetStatusDotClass(object statusObj)
        {
            if (statusObj == null) return "status-dot dot-draft";
            string status = statusObj.ToString().ToUpper();
            if (status == "APPROVED" || status == "PUBLISHED") return "status-dot dot-active";
            if (status == "REJECT") return "status-dot dot-archived";
            return "status-dot dot-draft";
        }

        protected void btnAddCourseRedirect_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddCourse.aspx");
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            System.Collections.Generic.List<string> selectedTitles = new System.Collections.Generic.List<string>();

            foreach (GridViewRow row in gvCourses.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedTitles.Add($"'{row.Cells[1].Text}'");
                }
            }

            if (selectedTitles.Count > 0)
            {
                litSelectedTitles.Text = string.Join(", ", selectedTitles);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one course to remove.";
                lblMessage.CssClass = "alert alert-warning d-block";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            System.Collections.Generic.List<int> selectedIds = new System.Collections.Generic.List<int>();

            foreach (GridViewRow row in gvCourses.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(Convert.ToInt32(gvCourses.DataKeys[row.RowIndex].Value));
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

                        string sql = $"DELETE FROM [course] WHERE Id IN ({string.Join(",", paramNames)})";

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
                    lblMessage.Text = "Selected course(s) removed successfully.";
                    lblMessage.CssClass = "alert alert-success d-block";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Cannot delete course: It is tied to existing enrollments. Error: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
        }
    }
}