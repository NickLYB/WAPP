using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Student
{
    public partial class TutorProfile : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        int tutorId;
        int studentId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["role_id"] == null || (int)Session["role_id"] != 4)
            {
                Response.Redirect("~/Pages/Student/Home.aspx");
                return;
            }

            if (Request.QueryString["id"] == null || !int.TryParse(Request.QueryString["id"], out tutorId))
            {
                Response.Redirect("Study.aspx");
                return;
            }

            studentId = Convert.ToInt32(Session["UserId"]);

            if (!IsPostBack)
            {
                SetTutorBackground(tutorId);
                LoadTutorDetails();
                LoadTutorCourses();
                LoadTutorReviews();
                CheckIfEligibleToRate();
            }
        }

        private void LoadTutorDetails()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT u.fname, u.lname, u.email, u.contact, u.created_at, ta.status AS AppStatus
                    FROM [user] u WITH (NOLOCK)
                    LEFT JOIN tutorApplication ta WITH (NOLOCK) ON u.Id = ta.tutor_id
                    WHERE u.Id = @tid AND u.role_id = 3";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tid", tutorId);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string fname = dr["fname"].ToString();
                        string lname = dr["lname"].ToString();

                        litTutorName.Text = fname + " " + lname;
                        litEmail.Text = dr["email"].ToString();
                        litJoinedDate.Text = Convert.ToDateTime(dr["created_at"]).ToString("MMMM yyyy");

                        litInitials.Text = (fname.Substring(0, 1) + lname.Substring(0, 1)).ToUpper();

                        if (dr["AppStatus"] != DBNull.Value && dr["AppStatus"].ToString() == "APPROVED")
                        {
                            phVerified.Visible = true;
                        }
                    }
                    else
                    {
                        Response.Redirect("Study.aspx");
                    }
                }

                // Calculate Average Rating
                string avgQuery = @"
                    SELECT AVG(CAST(rating AS DECIMAL(3,1))) 
                    FROM feedback WITH (NOLOCK) 
                    WHERE tutor_id = @tid AND course_id IS NULL AND resource_id IS NULL AND status = 'APPROVED'";

                using (SqlCommand cmdAvg = new SqlCommand(avgQuery, conn))
                {
                    cmdAvg.Parameters.AddWithValue("@tid", tutorId);
                    object avgRes = cmdAvg.ExecuteScalar();

                    if (avgRes != DBNull.Value && avgRes != null)
                    {
                        decimal avg = Convert.ToDecimal(avgRes);
                        litAvgRating.Text = avg.ToString("0.0");
                        litStarRating.Text = GenerateStars(avg); // Generates actual dynamic stars!
                    }
                    else
                    {
                        litAvgRating.Text = "0.0";
                        litStarRating.Text = GenerateStars(0); // Renders 5 empty stars
                    }
                }
            }
        }

        private void LoadTutorCourses()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // ADDED c.image_path to the SELECT statement
                string query = @"
                    SELECT c.Id, c.title, ct.name AS category, c.image_path 
                    FROM course c WITH (NOLOCK) 
                    JOIN courseType ct WITH (NOLOCK) ON c.course_type_id = ct.Id 
                    WHERE c.tutor_id = @tid AND c.status = 'PUBLISHED'"; 

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tid", tutorId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptCourses.DataSource = dt;
                rptCourses.DataBind();
            }
        }

        protected string GetCourseImage(object imgObj)
        {
            // If the database has an image, use it. Otherwise, use a default placeholder!
            if (imgObj == null || imgObj == DBNull.Value || string.IsNullOrWhiteSpace(imgObj.ToString()))
            {
                return "~/Images/logo.png"; // should be default image
            }
            return imgObj.ToString();
        }

        private void CheckIfEligibleToRate()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string checkEnrollmentQuery = @"
                    SELECT TOP 1 1 FROM enrollment e WITH (NOLOCK)
                    JOIN course c WITH (NOLOCK) ON e.course_id = c.Id
                    WHERE e.student_id = @sid AND c.tutor_id = @tid AND e.status IN ('ENROLLED', 'COMPLETED')";

                SqlCommand cmdCheck = new SqlCommand(checkEnrollmentQuery, conn);
                cmdCheck.Parameters.AddWithValue("@sid", studentId);
                cmdCheck.Parameters.AddWithValue("@tid", tutorId);

                conn.Open();
                object isEligible = cmdCheck.ExecuteScalar();

                if (isEligible != null)
                {
                    string checkFeedbackQuery = "SELECT rating, comment FROM feedback WITH (NOLOCK) WHERE student_id = @sid AND tutor_id = @tid AND course_id IS NULL AND resource_id IS NULL";
                    SqlCommand cmdFeedback = new SqlCommand(checkFeedbackQuery, conn);
                    cmdFeedback.Parameters.AddWithValue("@sid", studentId);
                    cmdFeedback.Parameters.AddWithValue("@tid", tutorId);

                    using (SqlDataReader dr = cmdFeedback.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            ddlRating.SelectedValue = dr["rating"].ToString();
                            txtComment.Text = dr["comment"].ToString();
                            btnSubmitReview.Text = "Update Review";
                        }
                    }
                    phWriteReview.Visible = true;
                }
                else
                {
                    phWriteReview.Visible = false;
                }
            }
        }

        private void LoadTutorReviews()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT f.rating, f.comment, f.created_at, u.fname, u.lname 
                    FROM feedback f WITH (NOLOCK)
                    JOIN [user] u WITH (NOLOCK) ON f.student_id = u.Id 
                    WHERE f.tutor_id = @tid AND f.course_id IS NULL AND f.resource_id IS NULL AND f.status = 'APPROVED'
                    ORDER BY f.created_at DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tid", tutorId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptReviews.DataSource = dt;
                    rptReviews.DataBind();
                    phNoReviews.Visible = false;
                }
                else
                {
                    phNoReviews.Visible = true;
                }
            }
        }

        protected void btnSubmitReview_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    IF EXISTS (SELECT 1 FROM feedback WHERE student_id=@sid AND tutor_id=@tid AND course_id IS NULL AND resource_id IS NULL)
                    BEGIN
                        UPDATE feedback SET rating=@rating, comment=@comment, created_at=GETDATE() 
                        WHERE student_id=@sid AND tutor_id=@tid AND course_id IS NULL AND resource_id IS NULL
                    END
                    ELSE
                    BEGIN
                        INSERT INTO feedback (student_id, tutor_id, course_id, resource_id, rating, comment, status)
                        VALUES (@sid, @tid, NULL, NULL, @rating, @comment, 'APPROVED')
                    END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", studentId);
                cmd.Parameters.AddWithValue("@tid", tutorId);
                cmd.Parameters.AddWithValue("@rating", ddlRating.SelectedValue);
                cmd.Parameters.AddWithValue("@comment", txtComment.Text);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            Response.Redirect(Request.RawUrl);
        }

        private string GenerateStars(decimal rating)
        {
            string stars = "";
            int fullStars = (int)Math.Floor(rating);
            bool hasHalfStar = (rating - fullStars) >= 0.5m;
            int emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

            // Add full stars
            for (int i = 0; i < fullStars; i++)
                stars += "<i class='bi bi-star-fill text-warning'></i> ";

            // Add half star if applicable
            if (hasHalfStar)
                stars += "<i class='bi bi-star-half text-warning'></i> ";

            // Add remaining empty stars
            for (int i = 0; i < emptyStars; i++)
                stars += "<i class='bi bi-star text-warning opacity-50'></i> ";

            return stars;
        }

        protected void txtApptDate_TextChanged(object sender, EventArgs e)
        {
            lblApptMessage.Text = "";
            ddlTimeSlots.Items.Clear();

            if (string.IsNullOrEmpty(txtApptDate.Text))
            {
                ddlTimeSlots.Items.Add(new ListItem("-- Please select a date first --", ""));
                return;
            }

            DateTime selectedDate = DateTime.Parse(txtApptDate.Text);
            int durationMins = int.Parse(ddlDuration.SelectedValue);

            // Set Tutor working hours (9:00 AM to 5:00 PM)
            TimeSpan workStart = new TimeSpan(9, 0, 0);
            TimeSpan workEnd = new TimeSpan(17, 0, 0);

            // Fetch existing appointments for this tutor on this date
            DataTable dtExistingAppts = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT start_time, end_time FROM appointment WHERE tutor_id = @tid AND appointment_date = @date AND status != 'REJECTED'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tid", tutorId);
                cmd.Parameters.AddWithValue("@date", selectedDate.Date);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtExistingAppts);
            }

            // Loop through possible time slots every 30 mins
            TimeSpan currentSlot = workStart;
            bool foundSlots = false;

            while (currentSlot.Add(TimeSpan.FromMinutes(durationMins)) <= workEnd)
            {
                TimeSpan proposedEnd = currentSlot.Add(TimeSpan.FromMinutes(durationMins));
                bool isSlotAvailable = true;

                foreach (DataRow row in dtExistingAppts.Rows)
                {
                    TimeSpan existStart = (TimeSpan)row["start_time"];
                    TimeSpan existEnd = (TimeSpan)row["end_time"];

                    // ENFORCE 30-MINUTE BUFFER RULE
                    // The proposed slot must end at least 30 mins before the existing starts
                    // OR it must start at least 30 mins after the existing ends.
                    bool violatesBuffer = !(proposedEnd <= existStart.Subtract(TimeSpan.FromMinutes(30)) ||
                                            currentSlot >= existEnd.Add(TimeSpan.FromMinutes(30)));

                    if (violatesBuffer)
                    {
                        isSlotAvailable = false;
                        break;
                    }
                }

                // If date is today, block past times
                if (selectedDate.Date == DateTime.Today.Date && currentSlot <= DateTime.Now.TimeOfDay)
                {
                    isSlotAvailable = false;
                }

                if (isSlotAvailable)
                {
                    string timeText = $"{DateTime.Today.Add(currentSlot).ToString("hh:mm tt")} - {DateTime.Today.Add(proposedEnd).ToString("hh:mm tt")}";
                    ddlTimeSlots.Items.Add(new ListItem(timeText, currentSlot.ToString()));
                    foundSlots = true;
                }

                currentSlot = currentSlot.Add(TimeSpan.FromMinutes(30)); // Move forward 30 mins
            }

            if (!foundSlots)
            {
                ddlTimeSlots.Items.Add(new ListItem("-- No slots available on this date --", ""));
                btnConfirmBooking.Enabled = false;
            }
            else
            {
                btnConfirmBooking.Enabled = true;
            }
        }

        protected void btnConfirmBooking_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtApptDate.Text) || string.IsNullOrEmpty(ddlTimeSlots.SelectedValue))
            {
                lblApptMessage.Text = "Please select a valid date and time.";
                lblApptMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            DateTime apptDate = DateTime.Parse(txtApptDate.Text);
            TimeSpan startTime = TimeSpan.Parse(ddlTimeSlots.SelectedValue);
            int durationMins = int.Parse(ddlDuration.SelectedValue);
            TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(durationMins));

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // --- 1. SPAM PREVENTION: CHECK FOR DUPLICATES ---
                string checkQuery = @"
                    SELECT COUNT(*) FROM appointment 
                    WHERE student_id = @sid AND tutor_id = @tid 
                    AND appointment_date = @date AND start_time = @start AND status != 'REJECTED'";

                using (SqlCommand cmdCheck = new SqlCommand(checkQuery, conn))
                {
                    cmdCheck.Parameters.AddWithValue("@sid", studentId);
                    cmdCheck.Parameters.AddWithValue("@tid", tutorId);
                    cmdCheck.Parameters.AddWithValue("@date", apptDate.Date);
                    cmdCheck.Parameters.AddWithValue("@start", startTime);

                    int existingCount = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (existingCount > 0)
                    {
                        lblApptMessage.Text = "You have already requested this exact time slot.";
                        lblApptMessage.ForeColor = System.Drawing.Color.Red;
                        return; // Stop execution, don't insert again
                    }
                }

                // --- 2. Insert Appointment and get the new ID ---
                string queryAppt = @"
                    INSERT INTO appointment (student_id, tutor_id, appointment_date, start_time, end_time, status, subject)
                    VALUES (@sid, @tid, @date, @start, @end, 'PENDING', @subject);
                    SELECT SCOPE_IDENTITY();";

                SqlCommand cmdAppt = new SqlCommand(queryAppt, conn);
                cmdAppt.Parameters.AddWithValue("@sid", studentId);
                cmdAppt.Parameters.AddWithValue("@tid", tutorId);
                cmdAppt.Parameters.AddWithValue("@date", apptDate.Date);
                cmdAppt.Parameters.AddWithValue("@start", startTime);
                cmdAppt.Parameters.AddWithValue("@end", endTime);
                cmdAppt.Parameters.AddWithValue("@subject", ddlSubject.SelectedValue);

                int newApptId = Convert.ToInt32(cmdAppt.ExecuteScalar());

                // --- 3. Insert Notification for the Tutor ---
                string queryNotif = @"
                    INSERT INTO notification (user_id, appointment_id, content, status)
                    VALUES (@uid, @appid, @content, 'UNREAD')";

                SqlCommand cmdNotif = new SqlCommand(queryNotif, conn);
                cmdNotif.Parameters.AddWithValue("@uid", tutorId);
                cmdNotif.Parameters.AddWithValue("@appid", newApptId);
                cmdNotif.Parameters.AddWithValue("@content", $"You have a new appointment request for {apptDate.ToString("MMM dd")} at {DateTime.Today.Add(startTime).ToString("hh:mm tt")}.");
                cmdNotif.ExecuteNonQuery();

                // --- 4. FETCH TUTOR DETAILS FOR THE EMAIL ---
                string tutorEmailAddress = "";
                string tutorName = "";
                string queryTutor = "SELECT email, fname, lname FROM [user] WHERE Id = @tid";
                using (SqlCommand cmdTutor = new SqlCommand(queryTutor, conn))
                {
                    cmdTutor.Parameters.AddWithValue("@tid", tutorId);
                    using (SqlDataReader dr = cmdTutor.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            tutorEmailAddress = dr["email"].ToString();
                            tutorName = dr["fname"].ToString() + " " + dr["lname"].ToString();
                        }
                    }
                }

                // --- 5. SEND THE EMAIL ---
                WAPP.Utils.EmailHelper.SendAppointmentEmail(
                    toEmail: tutorEmailAddress,
                    recipientName: tutorName,
                    subject: "New Session Request: Action Required",
                    appointmentTopic: ddlSubject.SelectedValue,
                    appointmentDate: txtApptDate.Text + " at " + ddlTimeSlots.SelectedItem.Text,
                    appointmentDuration: ddlDuration.SelectedValue,
                    appointmentStatus: "Pending",
                    customMessage: $"You have received a new session request from {Session["UserName"]}. Please log in to your dashboard to review and approve this request."
                );
            }

            // --- 6. RESET UI & CLOSE MODAL ---
            ddlTimeSlots.Items.Clear();
            txtApptDate.Text = "";
            lblApptMessage.Text = "";
            btnConfirmBooking.Enabled = false;

            // Trigger JavaScript to close the modal and show a nice success alert
            string script = @"
                var myModalEl = document.getElementById('bookAppointmentModal');
                var modal = bootstrap.Modal.getInstance(myModalEl);
                if (modal) { modal.hide(); }
                alert('Appointment request sent successfully! You will be notified when the tutor responds.');
            ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseModalScript", script, true);
        }

        private void SetTutorBackground(int tutorId)
        {
            // A collection of beautiful, modern CSS gradients
            string[,] gradients = new string[,] {
                { "#4facfe", "#00f2fe" }, // Cool Blue
                { "#43e97b", "#38f9d7" }, // Mint Green
                { "#fa709a", "#fee140" }, // Sunset Pink/Yellow
                { "#a18cd1", "#fbc2eb" }, // Soft Purple
                { "#ff9a9e", "#fecfef" }, // Rose Water
                { "#84fab0", "#8fd3f4" }, // Ocean
                { "#fccb90", "#d57eeb" }  // Peach/Purple
            };

            // Use the Tutor's ID to always pick the exact same color for them
            int index = tutorId % gradients.GetLength(0);
            string color1 = gradients[index, 0];
            string color2 = gradients[index, 1];

            // Apply the gradient directly to the div
            divCoverPhoto.Style["background"] = $"linear-gradient(135deg, {color1} 0%, {color2} 100%)";
        }
    }
}