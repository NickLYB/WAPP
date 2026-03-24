using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Hubs;
using WAPP.Utils; // Accesses SystemLogService and LogLevel

namespace WAPP.Pages.Staff
{
    public partial class LearningResourcesManagement : System.Web.UI.Page
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
                if (Request.QueryString["msg"] == "success")
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Learning resource added successfully!";
                    lblMessage.CssClass = "alert alert-success d-block mb-3";
                }

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

                using (SqlCommand cmdType = new SqlCommand("SELECT Id, name FROM [resourceType]", conn))
                {
                    using (SqlDataReader rdr = cmdType.ExecuteReader())
                    {
                        ddlFilterType.DataSource = rdr;
                        ddlFilterType.DataTextField = "name";
                        ddlFilterType.DataValueField = "Id";
                        ddlFilterType.DataBind();
                        ddlFilterType.Items.Insert(0, new ListItem("All Types", "All"));
                    }
                }

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

                using (SqlCommand cmdYear = new SqlCommand("SELECT DISTINCT YEAR(created_at) as YearVal FROM [learningResource] ORDER BY YearVal DESC", conn))
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

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSortBy.SelectedValue = "Latest";
            ddlFilterType.SelectedValue = "All";
            ddlFilterTutor.SelectedValue = "All";
            ddlFilterMonth.SelectedValue = "All";
            ddlFilterYear.SelectedValue = "All";
            lblMessage.Visible = false;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ClearSearch", "document.getElementById('" + txtSearch.ClientID + "').value = ''; SearchTable();", true);

            gvResources.PageIndex = 0;
            BindGrid();
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT lr.Id, 
                                      (c.title + ' - ' + ISNULL(lr.title, 'Unnamed Resource')) AS CourseAndResource,
                                      lr.created_at, 
                                      rt.name AS ResourceTypeName, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname + ' ' + ISNULL(u.lname,'')) AS TutorName,
                                      lr.resource_link
                               FROM [learningResource] lr
                               INNER JOIN [course] c ON lr.course_id = c.Id
                               INNER JOIN [resourceType] rt ON lr.resource_type = rt.Id
                               INNER JOIN [user] u ON lr.tutor_id = u.Id
                               WHERE 1=1";

                if (ddlFilterType.SelectedValue != "All") sql += " AND lr.resource_type = @type";
                if (ddlFilterTutor.SelectedValue != "All") sql += " AND lr.tutor_id = @tutor";
                if (ddlFilterMonth.SelectedValue != "All") sql += " AND MONTH(lr.created_at) = @month";
                if (ddlFilterYear.SelectedValue != "All") sql += " AND YEAR(lr.created_at) = @year";

                string sortStr = ddlSortBy.SelectedValue;
                if (sortStr == "Latest") sql += " ORDER BY lr.created_at DESC";
                else if (sortStr == "Oldest") sql += " ORDER BY lr.created_at ASC";
                else if (sortStr == "TutorID") sql += " ORDER BY lr.tutor_id ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (ddlFilterType.SelectedValue != "All") cmd.Parameters.AddWithValue("@type", ddlFilterType.SelectedValue);
                    if (ddlFilterTutor.SelectedValue != "All") cmd.Parameters.AddWithValue("@tutor", ddlFilterTutor.SelectedValue);
                    if (ddlFilterMonth.SelectedValue != "All") cmd.Parameters.AddWithValue("@month", ddlFilterMonth.SelectedValue);
                    if (ddlFilterYear.SelectedValue != "All") cmd.Parameters.AddWithValue("@year", ddlFilterYear.SelectedValue);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ViewState["TotalLRRecords"] = dt.Rows.Count;
                        gvResources.DataSource = dt;
                        gvResources.DataBind();
                    }
                }
            }
            UpdatePager();
        }

        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalLRRecords"] != null ? Convert.ToInt32(ViewState["TotalLRRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvResources.PageSize);
            if (totalPages == 0) totalPages = 1;

            int startRecord = (gvResources.PageIndex * gvResources.PageSize) + 1;
            int endRecord = startRecord + gvResources.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} resources";
            txtPageJump.Text = (gvResources.PageIndex + 1).ToString();

            btnPrev.Enabled = gvResources.PageIndex > 0;
            btnNext.Enabled = gvResources.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalLRRecords"] != null ? Convert.ToInt32(ViewState["TotalLRRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvResources.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvResources.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvResources.PageIndex = 0;
                else gvResources.PageIndex = totalPages - 1;
            }
            BindGrid();
        }

        protected void btnPrev_Click(object sender, EventArgs e) { if (gvResources.PageIndex > 0) { gvResources.PageIndex--; BindGrid(); } }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalLRRecords"] != null ? Convert.ToInt32(ViewState["TotalLRRecords"]) : 0;
            if (gvResources.PageIndex < ((int)Math.Ceiling((double)totalRecords / gvResources.PageSize) - 1)) { gvResources.PageIndex++; BindGrid(); }
        }

        protected void FilterGrid_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvResources.PageIndex = 0; BindGrid(); }

        protected void btnAddResourceRedirect_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddResource.aspx");
        }

        protected void gvResources_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvResources, "View$" + e.Row.RowIndex);
            }
        }

        protected void gvResources_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int resourceId = Convert.ToInt32(gvResources.DataKeys[rowIndex].Value);
                LoadResourceDetails(resourceId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void LoadResourceDetails(int id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT lr.Id, 
                                      (c.title + ' - ' + ISNULL(lr.title, 'Unnamed Resource')) AS CourseAndResource,
                                      lr.created_at, rt.name AS ResourceTypeName, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname + ' ' + ISNULL(u.lname,'')) AS TutorName
                               FROM [learningResource] lr
                               INNER JOIN [course] c ON lr.course_id = c.Id
                               INNER JOIN [resourceType] rt ON lr.resource_type = rt.Id
                               INNER JOIN [user] u ON lr.tutor_id = u.Id
                               WHERE lr.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            litViewId.Text = "R" + rdr["Id"].ToString().PadLeft(3, '0');
                            litViewDate.Text = Convert.ToDateTime(rdr["created_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewTitle.Text = rdr["CourseAndResource"].ToString();
                            litViewType.Text = rdr["ResourceTypeName"].ToString();
                            litViewTutor.Text = rdr["TutorName"].ToString();
                        }
                    }
                }
            }
        }

        // UPDATED: Pre-gather Course details and Student Enrollment numbers for the confirmation modal
        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
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
                List<string> displayTitles = new List<string>();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string[] paramNames = new string[selectedIds.Count];
                    for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;

                    string sql = $@"
                        SELECT lr.Id, 
                               ISNULL(lr.title, 'Unnamed Resource') AS ResourceTitle,
                               c.title AS CourseTitle,
                               (SELECT COUNT(*) FROM [enrollment] e WHERE e.course_id = lr.course_id AND e.status = 'ENROLLED') as EnrolledCount 
                        FROM [learningResource] lr 
                        INNER JOIN [course] c ON lr.course_id = c.Id
                        WHERE lr.Id IN ({string.Join(",", paramNames)})";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        for (int i = 0; i < selectedIds.Count; i++)
                            cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);

                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string formattedId = "R" + rdr["Id"].ToString().PadLeft(3, '0');
                                string title = rdr["ResourceTitle"].ToString();
                                string courseTitle = rdr["CourseTitle"].ToString();
                                int enrolledCount = Convert.ToInt32(rdr["EnrolledCount"]);

                                displayTitles.Add($"<div class='mb-3 text-start bg-light p-3 border rounded shadow-sm'><span class='fw-bold text-dark fs-6'>{formattedId} - {title}</span><br/><span class='small text-muted'>Course: {courseTitle}<br/>Affected Students: <strong class='text-danger'>{enrolledCount}</strong></span></div>");
                            }
                        }
                    }
                }

                litSelectedTitles.Text = string.Join("", displayTitles);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenModal", "openRemoveModal();", true);
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select at least one resource to remove.";
                lblMessage.CssClass = "alert alert-warning d-block mb-3";
            }
        }

        // UPDATED: Prepares notifications for Tutor and Enrolled Students, triggers SignalR, and cascades
        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            int staffId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in gvResources.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked) selectedIds.Add(Convert.ToInt32(gvResources.DataKeys[row.RowIndex].Value));
            }

            if (selectedIds.Count > 0)
            {
                try
                {
                    List<int> usersToNotify = new List<int>();

                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                string[] paramNames = new string[selectedIds.Count];
                                for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;
                                string inClause = string.Join(",", paramNames);

                                // 1. Gather Notification Data before deletion
                                var notificationsToInsert = new List<Tuple<int, string>>();

                                // Query Tutors
                                string sqlTutors = $@"
                                    SELECT lr.tutor_id, 
                                           ISNULL(lr.title, 'Unnamed Resource') AS ResourceTitle, 
                                           c.title AS CourseTitle 
                                    FROM [learningResource] lr 
                                    INNER JOIN [course] c ON lr.course_id = c.Id 
                                    WHERE lr.Id IN ({inClause})";

                                using (SqlCommand cmd = new SqlCommand(sqlTutors, conn, trans))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            int tId = Convert.ToInt32(rdr["tutor_id"]);
                                            string resTitle = rdr["ResourceTitle"].ToString();
                                            string courseTitle = rdr["CourseTitle"].ToString();
                                            notificationsToInsert.Add(new Tuple<int, string>(tId, $"Your learning resource '{resTitle}' in course '{courseTitle}' has been permanently removed by the staff."));
                                            if (!usersToNotify.Contains(tId)) usersToNotify.Add(tId);
                                        }
                                    }
                                }

                                // Query Students
                                string sqlStudents = $@"
                                    SELECT e.student_id, 
                                           ISNULL(lr.title, 'Unnamed Resource') AS ResourceTitle, 
                                           c.title AS CourseTitle
                                    FROM [enrollment] e 
                                    INNER JOIN [learningResource] lr ON e.course_id = lr.course_id
                                    INNER JOIN [course] c ON e.course_id = c.Id
                                    WHERE lr.Id IN ({inClause}) AND e.student_id IS NOT NULL AND e.status = 'ENROLLED'";

                                using (SqlCommand cmd = new SqlCommand(sqlStudents, conn, trans))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            int sId = Convert.ToInt32(rdr["student_id"]);
                                            string resTitle = rdr["ResourceTitle"].ToString();
                                            string courseTitle = rdr["CourseTitle"].ToString();
                                            notificationsToInsert.Add(new Tuple<int, string>(sId, $"A learning resource '{resTitle}' from your enrolled course '{courseTitle}' has been removed by the staff."));
                                            if (!usersToNotify.Contains(sId)) usersToNotify.Add(sId);
                                        }
                                    }
                                }

                                // Insert Notifications into the DB
                                foreach (var notif in notificationsToInsert)
                                {
                                    using (SqlCommand cmdNotif = new SqlCommand("INSERT INTO [notification] (user_id, content, status) VALUES (@uid, @content, 'UNREAD')", conn, trans))
                                    {
                                        cmdNotif.Parameters.AddWithValue("@uid", notif.Item1);
                                        cmdNotif.Parameters.AddWithValue("@content", notif.Item2);
                                        cmdNotif.ExecuteNonQuery();
                                    }
                                }

                                // 2. Set Feedback resource_id to NULL to prevent FK constraint error
                                string sqlUpdateFeedback = $"UPDATE [feedback] SET resource_id = NULL WHERE resource_id IN ({inClause})";
                                using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdateFeedback, conn, trans))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmdUpdate.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmdUpdate.ExecuteNonQuery();
                                }

                                // 3. Safely Delete the Learning Resource
                                string sqlDeleteResource = $"DELETE FROM [learningResource] WHERE Id IN ({inClause})";
                                using (SqlCommand cmdDelete = new SqlCommand(sqlDeleteResource, conn, trans))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmdDelete.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmdDelete.ExecuteNonQuery();
                                }

                                trans.Commit();
                            }
                            catch (Exception)
                            {
                                trans.Rollback();
                                throw;
                            }
                        }

                        // 4. Trigger SignalR for affected users (outside of transaction)
                        if (usersToNotify.Count > 0)
                        {
                            try
                            {
                                var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                                foreach (int uid in usersToNotify)
                                {
                                    hubContext.Clients.All.receiveNotification(uid);
                                }
                            }
                            catch (Exception ex)
                            {
                                SystemLogService.Write("SIGNALR_ERROR", $"Failed to send real-time notification upon resource deletion: {ex.Message}", LogLevel.WARNING, staffId);
                            }
                        }

                        BindGrid();
                        lblMessage.Visible = true;
                        lblMessage.Text = "Selected learning resource(s) removed successfully. Notifications sent.";
                        lblMessage.CssClass = "alert alert-success d-block mb-3";
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                    }
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