using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Hubs;
using WAPP.Utils; 

namespace WAPP.Pages.Staff
{
    public partial class CourseManagement : System.Web.UI.Page
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
                    lblMessage.Text = "New course added successfully!";
                    lblMessage.CssClass = "alert alert-success d-block mb-4";
                }

                BindDropdowns();

                if (Request.QueryString["status"] != null)
                {
                    string passedStatus = Request.QueryString["status"].ToUpper();
                    foreach (ListItem item in ddlFilterStatus.Items)
                    {
                        if (item.Value.ToUpper() == passedStatus)
                        {
                            ddlFilterStatus.ClearSelection();
                            item.Selected = true;
                            break;
                        }
                    }
                }

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
                        ddlFilterCategory.Items.Insert(0, new ListItem("All Categories", "All"));
                    }
                }

                string tutorSql = "SELECT Id, ('T' + RIGHT('000'+CAST(Id AS VARCHAR), 3) + '-' + fname + ' ' + ISNULL(lname, '')) as FullTutorName FROM [user] WHERE role_id = 3";
                using (SqlCommand cmdTutor = new SqlCommand(tutorSql, conn))
                {
                    using (SqlDataReader rdrTutor = cmdTutor.ExecuteReader())
                    {
                        DataTable dtTutor = new DataTable();
                        dtTutor.Load(rdrTutor);
                        ddlFilterTutor.DataSource = dtTutor;
                        ddlFilterTutor.DataTextField = "FullTutorName";
                        ddlFilterTutor.DataValueField = "Id";
                        ddlFilterTutor.DataBind();
                        ddlFilterTutor.Items.Insert(0, new ListItem("All Tutors", "All"));
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
                        ddlFilterYear.Items.Insert(0, new ListItem("All Years", "All"));
                    }
                }

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
            }
        }

        protected string StripHTML(object input)
        {
            if (input == null || input == DBNull.Value) return "";
            string text = input.ToString();
            text = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", " ");
            text = text.Replace("\r", " ").Replace("\n", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
            return text;
        }

        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT c.Id, c.title, c.description, ct.name AS category_name, c.skill_level, 
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname + ' ' + ISNULL(u.lname, '')) AS tutor_name,
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

                if (rating != "All")
                {
                    if (rating == "NoRating") sql += " AND c.average_rating IS NULL";
                    else sql += " AND c.average_rating = @rating";
                }

                if (month != "All") sql += " AND MONTH(c.created_at) = @month";
                if (year != "All") sql += " AND YEAR(c.created_at) = @year";

                if (sort == "Oldest") sql += " ORDER BY c.created_at ASC";
                else if (sort == "TutorID") sql += " ORDER BY c.tutor_id ASC, c.created_at DESC";
                else sql += " ORDER BY c.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    int parsedId;
                    if (!string.IsNullOrWhiteSpace(cat) && cat != "All" && int.TryParse(cat, out parsedId)) cmd.Parameters.AddWithValue("@cat", parsedId);
                    if (!string.IsNullOrWhiteSpace(tutor) && tutor != "All" && int.TryParse(tutor, out parsedId)) cmd.Parameters.AddWithValue("@tutor", parsedId);
                    if (!string.IsNullOrWhiteSpace(status) && status != "All") cmd.Parameters.AddWithValue("@status", status);
                    if (!string.IsNullOrWhiteSpace(rating) && rating != "All" && rating != "NoRating" && int.TryParse(rating, out parsedId)) cmd.Parameters.AddWithValue("@rating", parsedId);
                    if (!string.IsNullOrWhiteSpace(month) && month != "All" && int.TryParse(month, out parsedId)) cmd.Parameters.AddWithValue("@month", parsedId);
                    if (!string.IsNullOrWhiteSpace(year) && year != "All" && int.TryParse(year, out parsedId)) cmd.Parameters.AddWithValue("@year", parsedId);

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

        private void UpdatePager()
        {
            int totalRecords = ViewState["TotalCourseRecords"] != null ? Convert.ToInt32(ViewState["TotalCourseRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvCourses.PageSize);
            if (totalPages == 0) totalPages = 1;

            int startRecord = (gvCourses.PageIndex * gvCourses.PageSize) + 1;
            int endRecord = startRecord + gvCourses.Rows.Count - 1;
            if (endRecord > totalRecords) endRecord = totalRecords;
            if (totalRecords == 0) { startRecord = 0; endRecord = 0; }

            litPagerInfo.Text = $"Showing {startRecord} to {endRecord} of {totalRecords} courses";
            txtPageJump.Text = (gvCourses.PageIndex + 1).ToString();

            btnPrev.Enabled = gvCourses.PageIndex > 0;
            btnNext.Enabled = gvCourses.PageIndex < (totalPages - 1);

            btnPrev.CssClass = btnPrev.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
            btnNext.CssClass = btnNext.Enabled ? "btn btn-outline-primary btn-sm fw-bold px-3" : "btn btn-outline-secondary btn-sm fw-bold px-3 disabled";
        }

        protected void txtPageJump_TextChanged(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalCourseRecords"] != null ? Convert.ToInt32(ViewState["TotalCourseRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvCourses.PageSize);
            if (totalPages == 0) totalPages = 1;

            if (int.TryParse(txtPageJump.Text, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages) gvCourses.PageIndex = pageNum - 1;
                else if (pageNum < 1) gvCourses.PageIndex = 0;
                else gvCourses.PageIndex = totalPages - 1;
            }
            BindGrid();
        }

        protected void btnPrev_Click(object sender, EventArgs e) { if (gvCourses.PageIndex > 0) { gvCourses.PageIndex--; BindGrid(); } }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            int totalRecords = ViewState["TotalCourseRecords"] != null ? Convert.ToInt32(ViewState["TotalCourseRecords"]) : 0;
            int totalPages = (int)Math.Ceiling((double)totalRecords / gvCourses.PageSize);
            if (gvCourses.PageIndex < totalPages - 1) { gvCourses.PageIndex++; BindGrid(); }
        }

        protected void FilterGrid_Changed(object sender, EventArgs e) { lblMessage.Visible = false; gvCourses.PageIndex = 0; BindGrid(); }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlSortBy.SelectedValue = "Latest";
            ddlFilterCategory.SelectedValue = "All";
            ddlFilterTutor.SelectedValue = "All";
            ddlFilterStatus.SelectedValue = "All";
            ddlFilterRating.SelectedValue = "All";
            ddlFilterMonth.SelectedValue = "All";
            ddlFilterYear.SelectedValue = "All";
            lblMessage.Visible = false;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ClearSearch", "document.getElementById('" + txtSearch.ClientID + "').value = ''; SearchTable();", true);

            gvCourses.PageIndex = 0;
            BindGrid();
        }

        protected void btnReviewApplications_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlFilterStatus.SelectedValue = "PENDING";
            gvCourses.PageIndex = 0;
            BindGrid();
        }

        protected void gvCourses_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.CssClass = "clickable-row";
                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(gvCourses, "View$" + e.Row.RowIndex);
            }
        }

        protected void gvCourses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int courseId = Convert.ToInt32(gvCourses.DataKeys[rowIndex].Value);
                LoadCourseDetails(courseId);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenView", "openViewModal();", true);
            }
        }

        private void LoadCourseDetails(int courseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT c.Id, c.title, c.description, ct.name AS category_name, c.skill_level, c.duration_minutes,
                                      ('T' + RIGHT('000'+CAST(u.Id AS VARCHAR), 3) + '-' + u.fname + ' ' + ISNULL(u.lname, '')) AS tutor_name,
                                      c.status, c.created_at, c.image_path
                               FROM [course] c
                               INNER JOIN [courseType] ct ON c.course_type_id = ct.Id
                               INNER JOIN [user] u ON c.tutor_id = u.Id
                               WHERE c.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", courseId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            hfActionCourseId.Value = rdr["Id"].ToString();
                            litViewId.Text = "C" + rdr["Id"].ToString().PadLeft(3, '0');
                            litViewDate.Text = Convert.ToDateTime(rdr["created_at"]).ToString("dd/MM/yyyy HH:mm");
                            litViewTitle.Text = rdr["title"].ToString();
                            litViewCategory.Text = rdr["category_name"].ToString();
                            litViewSkill.Text = rdr["skill_level"].ToString();
                            litViewTutor.Text = rdr["tutor_name"].ToString();
                            litViewDuration.Text = rdr["duration_minutes"].ToString() + " mins";
                            litViewDescription.Text = rdr["description"].ToString();

                            string imgPath = rdr["image_path"].ToString();
                            imgViewCourse.ImageUrl = string.IsNullOrWhiteSpace(imgPath) ? "~/Images/course_placeholder.jpg" : ResolveUrl(imgPath);

                            string status = rdr["status"].ToString();
                            litViewStatus.Text = $"<span class='{GetStatusDotClass(status)}'></span> {GetStatusText(status)}";

                            if (status.ToUpper() == "PENDING")
                            {
                                pnlApprovalActions.Visible = true;
                                rbVerify.Checked = false;
                                rbReject.Checked = false;
                            }
                            else
                            {
                                pnlApprovalActions.Visible = false;
                            }
                        }
                    }
                }
            }
        }

        protected void btnSubmitAction_Click(object sender, EventArgs e)
        {
            if (!rbVerify.Checked && !rbReject.Checked)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please select 'Approve' or 'Reject' before submitting.";
                lblMessage.CssClass = "alert alert-warning d-block";
                return;
            }

            int courseId = Convert.ToInt32(hfActionCourseId.Value);
            string newStatus = rbVerify.Checked ? "APPROVED" : "REJECT";
            int staffId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;

            string tutorEmail = "";
            string tutorFirstName = "";
            int tutorId = 0;
            string courseTitle = litViewTitle.Text;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    string sqlUpdate = @"UPDATE [course] SET status = @status WHERE Id = @Id";
                    using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@status", newStatus);
                        cmdUpdate.Parameters.AddWithValue("@Id", courseId);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    string sqlTutor = @"SELECT u.email, u.fname, u.Id AS tutor_id 
                                FROM [course] c 
                                INNER JOIN [user] u ON c.tutor_id = u.Id 
                                WHERE c.Id = @Id";
                    using (SqlCommand cmdTutor = new SqlCommand(sqlTutor, conn))
                    {
                        cmdTutor.Parameters.AddWithValue("@Id", courseId);
                        using (SqlDataReader rdr = cmdTutor.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                tutorEmail = rdr["email"].ToString();
                                tutorFirstName = rdr["fname"].ToString();
                                tutorId = Convert.ToInt32(rdr["tutor_id"]);
                            }
                        }
                    }

                    if (tutorId > 0)
                    {
                        string notificationContent = newStatus == "APPROVED"
                            ? $"Your course application for '{courseTitle}' has been approved! You can now publish it."
                            : $"Your course application for '{courseTitle}' has been rejected.";

                        string sqlNotif = @"INSERT INTO [notification] (user_id, content, status) VALUES (@userId, @content, 'UNREAD')";
                        using (SqlCommand cmdNotif = new SqlCommand(sqlNotif, conn))
                        {
                            cmdNotif.Parameters.AddWithValue("@userId", tutorId);
                            cmdNotif.Parameters.AddWithValue("@content", notificationContent);
                            cmdNotif.ExecuteNonQuery();
                        }

                        try
                        {
                            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                            hubContext.Clients.All.receiveNotification(tutorId);
                        }
                        catch (Exception ex)
                        {
                            SystemLogService.Write("SIGNALR_ERROR", $"Failed to send real-time notification to user {tutorId}: {ex.Message}", LogLevel.WARNING, staffId);
                        }
                    }
                }

                SystemLogService.Write("STAFF_COURSE_APP_UPDATED", $"Staff updated course ID {courseId} ('{courseTitle}') status to {newStatus}.", LogLevel.INFO, staffId);

                if (!string.IsNullOrEmpty(tutorEmail))
                {
                    string subject = newStatus == "APPROVED" ? "Course Application Approved!" : "Course Application Update";
                    string emailMessage = newStatus == "APPROVED"
                        ? $"Great news! Your course application for <strong>'{courseTitle}'</strong> has been approved by our staff. <br><br><strong>Next Step:</strong> You can now log into your EduConnect dashboard, navigate to the course overview, and click the <strong>'Publish Course'</strong> button to make it visible to students!"
                        : $"We are writing to let you know that your course application for <strong>'{courseTitle}'</strong> has been rejected. Please review our content guidelines or contact staff support if you need further clarification.";

                    EmailHelper.SendNotificationEmail(tutorEmail, tutorFirstName, subject, emailMessage);
                }

                BindGrid();
                lblMessage.Visible = true;

                if (newStatus == "APPROVED")
                {
                    lblMessage.Text = $"Course '{courseTitle}' approved! Notifications have been sent.";
                    lblMessage.CssClass = "alert alert-success d-block";
                }
                else
                {
                    lblMessage.Text = $"Course application '{courseTitle}' rejected. Notifications have been sent.";
                    lblMessage.CssClass = "alert alert-danger d-block";
                }

                ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeViewModal();", true);
            }
            catch (Exception ex)
            {
                SystemLogService.Write("STAFF_COURSE_APP_UPDATE_ERROR", $"DB Error updating course ID {courseId}: {ex.Message}", LogLevel.ERROR, staffId);

                lblMessage.Visible = true;
                lblMessage.Text = "Error updating course status or sending notifications: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block";
            }
        }

        protected string GetStatusText(object statusObj)
        {
            if (statusObj == null) return "Pending";
            string status = statusObj.ToString().ToUpper();
            if (status == "APPROVED") return "Approved";
            if (status == "PUBLISHED") return "Published";
            if (status == "PRIVATE") return "Private";
            if (status == "REJECT") return "Rejected";
            return "Pending";
        }

        protected string GetStatusDotClass(object statusObj)
        {
            if (statusObj == null) return "badge bg-warning text-dark";
            string status = statusObj.ToString().ToUpper();
            if (status == "APPROVED" || status == "PUBLISHED") return "badge bg-success";
            if (status == "REJECT" || status == "PRIVATE") return "badge bg-danger";
            return "badge bg-warning text-dark";
        }

        protected void btnAddCourseRedirect_Click(object sender, EventArgs e) { Response.Redirect("AddCourse.aspx"); }

        protected void btnTriggerRemove_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
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
                List<string> displayTitles = new List<string>();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string[] paramNames = new string[selectedIds.Count];
                    for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;

                    string sql = $@"
                        SELECT c.Id, c.title, 
                               (SELECT COUNT(*) FROM [enrollment] WHERE course_id = c.Id AND status = 'ENROLLED') as EnrolledCount 
                        FROM [course] c 
                        WHERE c.Id IN ({string.Join(",", paramNames)})";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        for (int i = 0; i < selectedIds.Count; i++)
                            cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);

                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string formattedId = "C" + rdr["Id"].ToString().PadLeft(3, '0');
                                string title = rdr["title"].ToString();
                                int enrolledCount = Convert.ToInt32(rdr["EnrolledCount"]);

                                displayTitles.Add($"<div class='mb-2'><b>{formattedId} - {title}</b> <br/><span class='small text-muted'>Enrolled Students: <b class='text-danger fs-6'>{enrolledCount}</b></span></div>");
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
                lblMessage.Text = "Please select at least one course to remove.";
                lblMessage.CssClass = "alert alert-warning d-block mb-4";
            }
        }

        protected void btnConfirmRemove_Click(object sender, EventArgs e)
        {
            int staffId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
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
                    List<int> usersToNotify = new List<int>();

                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        conn.Open();
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                string[] paramNames = new string[selectedIds.Count];
                                for (int i = 0; i < selectedIds.Count; i++) paramNames[i] = "@id" + i;
                                string inClause = string.Join(",", paramNames);

                                // 1. Gather Notification Data Before Deleting Anything
                                var notificationsToInsert = new List<Tuple<int, string>>();

                                // Query Tutors
                                string sqlTutors = $"SELECT Id, title, tutor_id FROM [course] WHERE Id IN ({inClause})";
                                using (SqlCommand cmd = new SqlCommand(sqlTutors, conn, transaction))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            int tId = Convert.ToInt32(rdr["tutor_id"]);
                                            string title = rdr["title"].ToString();
                                            notificationsToInsert.Add(new Tuple<int, string>(tId, $"Your course '{title}' has been permanently removed by the staff."));
                                            if (!usersToNotify.Contains(tId)) usersToNotify.Add(tId);
                                        }
                                    }
                                }

                                // Query Students
                                string sqlStudents = $"SELECT e.student_id, c.title FROM [enrollment] e INNER JOIN [course] c ON e.course_id = c.Id WHERE e.course_id IN ({inClause}) AND e.student_id IS NOT NULL";
                                using (SqlCommand cmd = new SqlCommand(sqlStudents, conn, transaction))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            int sId = Convert.ToInt32(rdr["student_id"]);
                                            string title = rdr["title"].ToString();
                                            notificationsToInsert.Add(new Tuple<int, string>(sId, $"The course '{title}' you were enrolled in has been removed by the staff."));
                                            if (!usersToNotify.Contains(sId)) usersToNotify.Add(sId);
                                        }
                                    }
                                }

                                // Insert Notifications into Database
                                foreach (var notif in notificationsToInsert)
                                {
                                    using (SqlCommand cmdNotif = new SqlCommand("INSERT INTO [notification] (user_id, content, status) VALUES (@uid, @content, 'UNREAD')", conn, transaction))
                                    {
                                        cmdNotif.Parameters.AddWithValue("@uid", notif.Item1);
                                        cmdNotif.Parameters.AddWithValue("@content", notif.Item2);
                                        cmdNotif.ExecuteNonQuery();
                                    }
                                }

                                // 2. Perform Cascading Deletes
                                string unattachFeedbackSql = $@"
                                    UPDATE [feedback] 
                                    SET course_id = NULL, resource_id = NULL 
                                    WHERE course_id IN ({inClause}) 
                                    OR resource_id IN (SELECT Id FROM [learningResource] WHERE course_id IN ({inClause}))";

                                using (SqlCommand cmd = new SqlCommand(unattachFeedbackSql, conn, transaction))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmd.ExecuteNonQuery();
                                }

                                string deleteEnrollmentsSql = $"DELETE FROM [enrollment] WHERE course_id IN ({inClause})";
                                using (SqlCommand cmd = new SqlCommand(deleteEnrollmentsSql, conn, transaction))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmd.ExecuteNonQuery();
                                }

                                string deleteResourceSql = $"DELETE FROM [learningResource] WHERE course_id IN ({inClause})";
                                using (SqlCommand cmd = new SqlCommand(deleteResourceSql, conn, transaction))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmd.ExecuteNonQuery();
                                }

                                string unattachAnnouncementsSql = $"UPDATE [announcement] SET course_id = NULL WHERE course_id IN ({inClause})";
                                using (SqlCommand cmd = new SqlCommand(unattachAnnouncementsSql, conn, transaction))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmd.ExecuteNonQuery();
                                }

                                string deleteCourseSql = $"DELETE FROM [course] WHERE Id IN ({inClause})";
                                using (SqlCommand cmd = new SqlCommand(deleteCourseSql, conn, transaction))
                                {
                                    for (int i = 0; i < selectedIds.Count; i++) cmd.Parameters.AddWithValue(paramNames[i], selectedIds[i]);
                                    cmd.ExecuteNonQuery();
                                }

                                transaction.Commit();

                                string idList = string.Join(", ", selectedIds);
                                SystemLogService.Write("STAFF_COURSE_MANAGEMENT_DELETED",
                                    $"Staff permanently deleted course IDs: {idList}, including all related resources and enrollments.",
                                    LogLevel.WARNING, staffId);

                                BindGrid();
                                lblMessage.Visible = true;
                                lblMessage.Text = "Selected course(s) removed successfully. Notifications sent to affected users.";
                                lblMessage.CssClass = "alert alert-success d-block mb-4";
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModal", "closeRemoveModal();", true);
                            }
                            catch (Exception trEx)
                            {
                                transaction.Rollback();
                                throw trEx;
                            }
                        }
                    }

                    // 3. Trigger SignalR for all affected Users (Outside Transaction)
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
                            SystemLogService.Write("SIGNALR_ERROR", $"Failed to send real-time notification to users upon course deletion: {ex.Message}", LogLevel.WARNING, staffId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SystemLogService.Write("STAFF_COURSE_MANAGEMENT_DELETE_ERROR",
                        $"DB Error while attempting to cascade delete courses: {ex.Message}",
                        LogLevel.ERROR, staffId);

                    lblMessage.Visible = true;
                    lblMessage.Text = "Cannot delete course. Details: " + ex.Message;
                    lblMessage.CssClass = "alert alert-danger d-block mb-4";
                }
            }
        }
    }
}