using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace WAPP.Pages.Staff
{
    public partial class CourseApplications : System.Web.UI.Page
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
                ddlFilterStatus.SelectedValue = "All";
                BindGrid();
            }
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT c.Id, c.title, c.description, c.skill_level, c.tutor_id, 
                                      c.created_at, c.status, ct.name AS category_name
                               FROM [course] c
                               INNER JOIN [courseType] ct ON c.course_type_id = ct.Id
                               WHERE 1=1";

                string filter = ddlFilterStatus.SelectedValue;
                if (filter != "All") sql += " AND c.status = @FilterStatus";

                sql += " ORDER BY c.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (filter != "All") cmd.Parameters.AddWithValue("@FilterStatus", filter);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        ViewState["TotalCourseAppRecords"] = dt.Rows.Count;

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
            int totalRecords = ViewState["TotalCourseAppRecords"] != null ? Convert.ToInt32(ViewState["TotalCourseAppRecords"]) : 0;
            int startRecord = (gvCourses.PageIndex * gvCourses.PageSize) + 1;
            int endRecord = startRecord + gvCourses.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} applications";

            btnPrev.Enabled = gvCourses.PageIndex > 0;
            btnPrev.CssClass = btnPrev.Enabled ? "pager-link" : "pager-link disabled";

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
            int totalRecords = ViewState["TotalCourseAppRecords"] != null ? Convert.ToInt32(ViewState["TotalCourseAppRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvCourses.PageSize);

            if (gvCourses.PageIndex < totalPages - 1)
            {
                gvCourses.PageIndex++;
                BindGrid();
            }
        }
        // ---------------------------------

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvCourses.PageIndex = 0;
            BindGrid();
        }

        protected void gvCourses_DataBound(object sender, EventArgs e)
        {
            // Empty placeholder to fulfill the asp:GridView OnDataBound property requirement
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

        protected void gvCourses_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string status = DataBinder.Eval(e.Row.DataItem, "status").ToString().ToUpper();
                RadioButton rbVerify = (RadioButton)e.Row.FindControl("rbVerify");
                RadioButton rbReject = (RadioButton)e.Row.FindControl("rbReject");

                if (status == "APPROVED" || status == "PUBLISHED") rbVerify.Checked = true;
                else if (status == "REJECT") rbReject.Checked = true;
            }
        }

        protected void Action_Changed(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            GridViewRow row = (GridViewRow)rb.NamingContainer;
            int courseId = Convert.ToInt32(gvCourses.DataKeys[row.RowIndex].Value);
            string courseTitle = row.Cells[2].Text; // Title is exactly at index 2

            string newStatus = rb.ID == "rbVerify" ? "APPROVED" : "REJECT";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"UPDATE [course] SET status = @status WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@Id", courseId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                BindGrid();

                lblMessage.Visible = true;
                if (newStatus == "APPROVED")
                {
                    lblMessage.Text = $"Course [{courseTitle}] approved successfully!";
                    lblMessage.CssClass = "alert alert-success d-block";
                }
                else
                {
                    lblMessage.Text = $"Course [{courseTitle}] rejected.";
                    lblMessage.CssClass = "alert alert-danger d-block";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Error updating course status: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            List<string> selectedIds = new List<string>();

            foreach (GridViewRow row in gvCourses.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    selectedIds.Add("R" + gvCourses.DataKeys[row.RowIndex].Value.ToString().PadLeft(3, '0'));
                }
            }

            if (selectedIds.Count > 0)
            {
                litSelectedIds.Text = "[" + string.Join("], [", selectedIds) + "]";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one course application to remove.";
                lblMessage.CssClass = "alert alert-warning d-block";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = new List<int>();

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
                    lblMessage.Text = "Selected course application(s) removed successfully.";
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