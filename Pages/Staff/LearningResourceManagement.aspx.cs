using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace WAPP.Pages.Staff
{
    public partial class LearningResourcesManagement : System.Web.UI.Page
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
                    lblMessage.Text = "New learning resource added successfully!";
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

                using (SqlCommand cmdType = new SqlCommand("SELECT Id, name FROM [resourceType]", conn))
                {
                    using (SqlDataReader rdrType = cmdType.ExecuteReader())
                    {
                        DataTable dtType = new DataTable();
                        dtType.Load(rdrType);

                        ddlFilterType.DataSource = dtType;
                        ddlFilterType.DataTextField = "name";
                        ddlFilterType.DataValueField = "Id";
                        ddlFilterType.DataBind();
                        ddlFilterType.Items.Insert(0, new ListItem("All", "All"));
                    }
                }

                // FIXED: SQL query now formats as "T004-John Doe"
                using (SqlCommand cmdTutor = new SqlCommand(@"SELECT Id, 
                                                                     ('T' + RIGHT('000'+CAST(Id AS VARCHAR), 3) + '-' + fname + ' ' + lname) as FullTutorName 
                                                              FROM [user] 
                                                              WHERE role_id = 3", conn))
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

                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM [learningResource] ORDER BY YearVal DESC", conn))
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
                // FIXED: SQL query now formats as "T004-John Doe" for the data table
                string sql = @"SELECT lr.Id, c.title AS CourseTitle, lr.created_at, rt.name AS ResourceTypeName, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname + ' ' + u.lname) AS TutorName,
                                      lr.resource_link
                               FROM [learningResource] lr
                               INNER JOIN [course] c ON lr.course_id = c.Id
                               INNER JOIN [resourceType] rt ON lr.resource_type = rt.Id
                               INNER JOIN [user] u ON lr.tutor_id = u.Id
                               WHERE 1=1";

                string type = ddlFilterType.SelectedValue;
                string tutor = ddlFilterTutor.SelectedValue;
                string month = ddlFilterMonth.SelectedValue;
                string year = ddlFilterYear.SelectedValue;
                string sort = ddlSortBy.SelectedValue;

                if (type != "All") sql += " AND lr.resource_type = @type";
                if (tutor != "All") sql += " AND lr.tutor_id = @tutor";
                if (month != "All") sql += " AND MONTH(lr.created_at) = @month";
                if (year != "All") sql += " AND YEAR(lr.created_at) = @year";

                if (sort == "Oldest") sql += " ORDER BY lr.created_at ASC";
                else if (sort == "TutorID") sql += " ORDER BY lr.tutor_id ASC, lr.created_at DESC";
                else sql += " ORDER BY lr.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (type != "All") cmd.Parameters.AddWithValue("@type", Convert.ToInt32(type));
                    if (tutor != "All") cmd.Parameters.AddWithValue("@tutor", Convert.ToInt32(tutor));
                    if (month != "All") cmd.Parameters.AddWithValue("@month", Convert.ToInt32(month));
                    if (year != "All") cmd.Parameters.AddWithValue("@year", Convert.ToInt32(year));

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        ViewState["TotalResourceRecords"] = dt.Rows.Count;

                        gvResources.DataSource = dt;
                        gvResources.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        // --- NEW PERMANENT PAGER LOGIC ---
        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalResourceRecords"] != null ? Convert.ToInt32(ViewState["TotalResourceRecords"]) : 0;
            int startRecord = (gvResources.PageIndex * gvResources.PageSize) + 1;
            int endRecord = startRecord + gvResources.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} resources";

            btnPrev.Enabled = gvResources.PageIndex > 0;
            btnPrev.CssClass = btnPrev.Enabled ? "pager-link" : "pager-link disabled";

            int totalPages = (int)Math.Ceiling((double)totalRecords / gvResources.PageSize);
            btnNext.Enabled = gvResources.PageIndex < (totalPages - 1);
            btnNext.CssClass = btnNext.Enabled ? "pager-link" : "pager-link disabled";
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (gvResources.PageIndex > 0)
            {
                gvResources.PageIndex--;
                BindGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalResourceRecords"] != null ? Convert.ToInt32(ViewState["TotalResourceRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvResources.PageSize);

            if (gvResources.PageIndex < totalPages - 1)
            {
                gvResources.PageIndex++;
                BindGrid();
            }
        }
        // ---------------------------------

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvResources.PageIndex = 0; // Reset to page 1 on filter
            BindGrid();
        }

        protected void btnAddResourceRedirect_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddResource.aspx");
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<string> selectedTitles = new List<string>();

            foreach (GridViewRow row in gvResources.Rows)
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
                lblMessage.Text = "Please select at least one resource to remove.";
                lblMessage.CssClass = "alert alert-warning d-block";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvResources.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add(Convert.ToInt32(gvResources.DataKeys[row.RowIndex].Value));
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

                        string sql = $"DELETE FROM [learningResource] WHERE Id IN ({string.Join(",", paramNames)})";

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
                    lblMessage.Text = "Selected resource(s) removed successfully.";
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