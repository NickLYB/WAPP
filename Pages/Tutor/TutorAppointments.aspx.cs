using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using WAPP.Utils; // Accesses SystemLogService and LogLevel

namespace WAPP.Pages.Tutor
{
    public partial class TutorAppointments : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        int tutorId;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure the user is logged in and is a Tutor (role_id = 3)
            if (Session["UserId"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            tutorId = Convert.ToInt32(Session["UserId"]);

            if (!IsPostBack)
            {
                LoadAppointments();
            }
        }

        private void LoadAppointments()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 1. Fetch PENDING Appointments
                string pendingQuery = @"
                    SELECT a.Id, a.appointment_date, a.start_time, a.end_time, a.subject, (u.fname + ' ' + u.lname) AS StudentName 
                    FROM appointment a
                    JOIN [user] u ON a.student_id = u.Id
                    WHERE a.tutor_id = @tid AND a.status = 'PENDING'
                    ORDER BY a.appointment_date ASC, a.start_time ASC";

                SqlCommand cmdPending = new SqlCommand(pendingQuery, conn);
                cmdPending.Parameters.AddWithValue("@tid", tutorId);
                SqlDataAdapter daPending = new SqlDataAdapter(cmdPending);
                DataTable dtPending = new DataTable();
                daPending.Fill(dtPending);

                dtPending.Columns.Add("TimeRange", typeof(string));
                foreach (DataRow row in dtPending.Rows)
                {
                    TimeSpan start = (TimeSpan)row["start_time"];
                    TimeSpan end = (TimeSpan)row["end_time"];
                    row["TimeRange"] = $"{DateTime.Today.Add(start).ToString("hh:mm tt")} - {DateTime.Today.Add(end).ToString("hh:mm tt")}";
                }

                rptPending.DataSource = dtPending;
                rptPending.DataBind();
                phNoPending.Visible = (dtPending.Rows.Count == 0);

                // 2. Fetch UPCOMING (APPROVED) Appointments
                string upcomingQuery = @"
                    SELECT a.Id, a.appointment_date, a.start_time, a.end_time, a.subject, (u.fname + ' ' + u.lname) AS StudentName 
                    FROM appointment a
                    JOIN [user] u ON a.student_id = u.Id
                    WHERE a.tutor_id = @tid AND a.status = 'APPROVED' AND a.appointment_date >= CAST(GETDATE() AS DATE)
                    ORDER BY a.appointment_date ASC, a.start_time ASC";

                SqlCommand cmdUpcoming = new SqlCommand(upcomingQuery, conn);
                cmdUpcoming.Parameters.AddWithValue("@tid", tutorId);
                SqlDataAdapter daUpcoming = new SqlDataAdapter(cmdUpcoming);
                DataTable dtUpcoming = new DataTable();
                daUpcoming.Fill(dtUpcoming);

                dtUpcoming.Columns.Add("TimeRange", typeof(string));
                foreach (DataRow row in dtUpcoming.Rows)
                {
                    TimeSpan start = (TimeSpan)row["start_time"];
                    TimeSpan end = (TimeSpan)row["end_time"];
                    row["TimeRange"] = $"{DateTime.Today.Add(start).ToString("hh:mm tt")} - {DateTime.Today.Add(end).ToString("hh:mm tt")}";
                }

                rptUpcoming.DataSource = dtUpcoming;
                rptUpcoming.DataBind();
                phNoUpcoming.Visible = (dtUpcoming.Rows.Count == 0);
            }
        }

        protected void rptPending_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int appointmentId = Convert.ToInt32(e.CommandArgument);
            string newStatus = e.CommandName == "Approve" ? "APPROVED" : "REJECTED";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // 1. Get Appointment Details & Student Details
                    string getApptQuery = @"
                        SELECT a.student_id, a.appointment_date, a.start_time, a.end_time, a.subject, 
                               u.fname, u.lname, u.email 
                        FROM appointment a
                        JOIN [user] u ON a.student_id = u.Id
                        WHERE a.Id = @appid";

                    SqlCommand cmdGet = new SqlCommand(getApptQuery, conn);
                    cmdGet.Parameters.AddWithValue("@appid", appointmentId);

                    int studentId = 0;
                    string dateStr = "";
                    string timeStr = "";
                    string studentEmail = "";
                    string studentName = "";
                    string apptSubject = "";
                    string apptDuration = "";
                    string fullApptDate = "";

                    using (SqlDataReader dr = cmdGet.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            studentId = Convert.ToInt32(dr["student_id"]);
                            dateStr = Convert.ToDateTime(dr["appointment_date"]).ToString("MMM dd, yyyy");
                            TimeSpan start = (TimeSpan)dr["start_time"];
                            TimeSpan end = (TimeSpan)dr["end_time"];
                            timeStr = DateTime.Today.Add(start).ToString("hh:mm tt");

                            fullApptDate = dateStr + " at " + timeStr;
                            apptDuration = (end - start).TotalMinutes.ToString();
                            apptSubject = dr["subject"].ToString();
                            studentEmail = dr["email"].ToString();
                            studentName = dr["fname"].ToString() + " " + dr["lname"].ToString();
                        }
                    }

                    // 2. Update the Appointment Status
                    string updateQuery = "UPDATE appointment SET status = @status WHERE Id = @appid";
                    SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn);
                    cmdUpdate.Parameters.AddWithValue("@status", newStatus);
                    cmdUpdate.Parameters.AddWithValue("@appid", appointmentId);
                    cmdUpdate.ExecuteNonQuery();

                    // 3. Automatically mark the Tutor's original notification as READ
                    string markReadQuery = "UPDATE notification SET status = 'READ', read_at = GETDATE() WHERE appointment_id = @appid AND user_id = @tid AND status = 'UNREAD'";
                    SqlCommand cmdMarkRead = new SqlCommand(markReadQuery, conn);
                    cmdMarkRead.Parameters.AddWithValue("@appid", appointmentId);
                    cmdMarkRead.Parameters.AddWithValue("@tid", tutorId);
                    cmdMarkRead.ExecuteNonQuery();

                    // 4. Send Internal Notification to the Student
                    string notifContent = newStatus == "APPROVED"
                        ? $"Your appointment request for {dateStr} at {timeStr} has been APPROVED by the tutor."
                        : $"Unfortunately, your appointment request for {dateStr} at {timeStr} was REJECTED. Please try reaching out to the tutor via the chat system to arrange an alternative schedule.";

                    string insertNotifQuery = @"
                        INSERT INTO notification (user_id, appointment_id, content, status)
                        VALUES (@uid, @appid, @content, 'UNREAD')";
                    SqlCommand cmdNotif = new SqlCommand(insertNotifQuery, conn);
                    cmdNotif.Parameters.AddWithValue("@uid", studentId);
                    cmdNotif.Parameters.AddWithValue("@appid", appointmentId);
                    cmdNotif.Parameters.AddWithValue("@content", notifContent);
                    cmdNotif.ExecuteNonQuery();

                    if (newStatus == "APPROVED")
                    {
                        SystemLogService.Write("APPOINTMENT_APPROVED", 
                            $"Tutor approved appointment ID {appointmentId} for Student ID {studentId}.", 
                            LogLevel.INFO, tutorId);
                    }
                    else
                    {
                        SystemLogService.Write("APPOINTMENT_REJECTED", 
                            $"Tutor rejected appointment ID {appointmentId} for Student ID {studentId}.", 
                            LogLevel.NOTICE, tutorId);
                    }

                    // 5. SEND EMAIL TO STUDENT
                    string emailSubject = newStatus == "APPROVED" ? "Session Approved!" : "Session Request Declined";
                    string emailMsg = newStatus == "APPROVED"
                        ? "Great news! Your tutor has approved your session request. Please make sure to be on time."
                        : "Unfortunately, your tutor is unable to accommodate this session request. Please try booking a different time slot or reaching out via chat.";

                    WAPP.Utils.EmailHelper.SendAppointmentEmail(
                        toEmail: studentEmail,
                        recipientName: studentName,
                        subject: emailSubject,
                        appointmentTopic: apptSubject,
                        appointmentDate: fullApptDate,
                        appointmentDuration: apptDuration,
                        appointmentStatus: newStatus,
                        customMessage: emailMsg
                    );
                }

                lblActionMessage.Text = newStatus == "APPROVED" ? "Appointment successfully approved!" : "Appointment rejected.";
                lblActionMessage.ForeColor = newStatus == "APPROVED" ? System.Drawing.Color.Green : System.Drawing.Color.Red;

                LoadAppointments(); // Reload grids
            }
            catch (Exception ex)
            {
                SystemLogService.Write("APPOINTMENT_UPDATE_ERROR", 
                    $"Error processing appointment ID {appointmentId}: {ex.Message}", 
                    LogLevel.ERROR, tutorId);

                lblActionMessage.Text = "An error occurred while processing the appointment.";
                lblActionMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
        protected void rptUpcoming_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Cancel")
            {
                int appointmentId = Convert.ToInt32(e.CommandArgument);

                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();

                        // 1. Get Appointment Details & Student Details
                        string getApptQuery = @"
                            SELECT a.student_id, a.appointment_date, a.start_time, a.end_time, a.subject, 
                                   u.fname, u.lname, u.email 
                            FROM appointment a
                            JOIN [user] u ON a.student_id = u.Id
                            WHERE a.Id = @appid";

                        SqlCommand cmdGet = new SqlCommand(getApptQuery, conn);
                        cmdGet.Parameters.AddWithValue("@appid", appointmentId);

                        int studentId = 0;
                        string dateStr = "";
                        string timeStr = "";
                        string studentEmail = "";
                        string studentName = "";
                        string apptSubject = "";
                        string apptDuration = "";
                        string fullApptDate = "";

                        using (SqlDataReader dr = cmdGet.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                studentId = Convert.ToInt32(dr["student_id"]);
                                dateStr = Convert.ToDateTime(dr["appointment_date"]).ToString("MMM dd, yyyy");
                                TimeSpan start = (TimeSpan)dr["start_time"];
                                TimeSpan end = (TimeSpan)dr["end_time"];
                                timeStr = DateTime.Today.Add(start).ToString("hh:mm tt");

                                fullApptDate = dateStr + " at " + timeStr;
                                apptDuration = (end - start).TotalMinutes.ToString();
                                apptSubject = dr["subject"].ToString();
                                studentEmail = dr["email"].ToString();
                                studentName = dr["fname"].ToString() + " " + dr["lname"].ToString();
                            }
                        }

                        // 2. Update Status to CANCELLED
                        string cancelQuery = "UPDATE appointment SET status = 'CANCELLED' WHERE Id = @appid";
                        SqlCommand cmdCancel = new SqlCommand(cancelQuery, conn);
                        cmdCancel.Parameters.AddWithValue("@appid", appointmentId);
                        cmdCancel.ExecuteNonQuery();

                        // 3. Send Internal Notification to Student
                        string notifContent = $"URGENT: Your upcoming appointment on {dateStr} at {timeStr} has been CANCELLED by the tutor. Please reach out to them to reschedule.";
                        string insertNotifQuery = @"
                            INSERT INTO notification (user_id, appointment_id, content, status)
                            VALUES (@uid, @appid, @content, 'UNREAD')";
                        SqlCommand cmdNotif = new SqlCommand(insertNotifQuery, conn);
                        cmdNotif.Parameters.AddWithValue("@uid", studentId);
                        cmdNotif.Parameters.AddWithValue("@appid", appointmentId);
                        cmdNotif.Parameters.AddWithValue("@content", notifContent);
                        cmdNotif.ExecuteNonQuery();

                        SystemLogService.Write("APPOINTMENT_CANCELLED", 
                            $"Tutor cancelled approved appointment ID {appointmentId} for Student ID {studentId}.", 
                            LogLevel.WARNING, tutorId);

                        // 4. SEND CANCELLATION EMAIL TO STUDENT
                        WAPP.Utils.EmailHelper.SendAppointmentEmail(
                            toEmail: studentEmail,
                            recipientName: studentName,
                            subject: "URGENT: Session Cancelled",
                            appointmentTopic: apptSubject,
                            appointmentDate: fullApptDate,
                            appointmentDuration: apptDuration,
                            appointmentStatus: "Cancelled",
                            customMessage: "Your upcoming appointment has been CANCELLED by the tutor. Please log in to your dashboard to book an alternative time slot or reach out via chat."
                        );
                    }

                    lblActionMessage.Text = "Appointment has been cancelled and the student has been notified.";
                    lblActionMessage.ForeColor = System.Drawing.Color.Orange;

                    LoadAppointments(); // Refresh the grid
                }
                catch (Exception ex)
                {
                    SystemLogService.Write("APPOINTMENT_CANCEL_ERROR", 
                        $"Error cancelling appointment ID {appointmentId}: {ex.Message}", 
                        LogLevel.ERROR, tutorId);

                    lblActionMessage.Text = "An error occurred while attempting to cancel the appointment.";
                    lblActionMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }
    }
}