using Hangfire;
using Microsoft.Owin;
using Owin;
using System;
using System.Configuration;

[assembly: OwinStartup(typeof(WAPP.Startup))]

namespace WAPP
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();

            // 1. Get your connection string
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            // 2. Configure Hangfire to use your SQL Server
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connString);

            // 3. Start the Hangfire Server (this processes the background jobs)
            app.UseHangfireServer();

            // 4. Start the Hangfire Dashboard (UI to monitor jobs)
            app.UseHangfireDashboard("/hangfire");

            // 5. Schedule our recurring job to run every minute for Announcements
            RecurringJob.AddOrUpdate(
                "process-scheduled-announcements",
                () => ProcessAnnouncementsJob(),
                Cron.Minutely);

            // 6. Schedule our recurring job to run every minute for Appointment Reminders
            RecurringJob.AddOrUpdate(
                "process-appointment-reminders",
                () => ProcessAppointmentRemindersJob(),
                Cron.Minutely);
        }

        public void ProcessAnnouncementsJob()
        {
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
            {
                conn.Open();

                // 1. SELECT the data first so we can send the emails
                string selectQuery = @"
            SELECT a.title, a.message, u.email, u.fname
            FROM [dbo].[announcement] a
            CROSS JOIN [dbo].[user] u
            WHERE a.scheduled_at <= GETDATE() 
              AND a.status = 'ACTIVE' 
              AND a.is_published = 0
              AND u.Id != a.created_by -- Do not notify the tutor who created it
              AND (a.target_role_id IS NULL OR u.role_id = a.target_role_id)
              AND (
                  -- SCENARIO A: It's a general announcement
                  a.course_id IS NULL 
                  
                  -- SCENARIO B: It's a course announcement, ONLY send to enrolled students
                  OR EXISTS (
                      SELECT 1 
                      FROM [dbo].[enrollment] e 
                      WHERE e.course_id = a.course_id 
                        AND e.student_id = u.Id 
                        AND e.status = 'ENROLLED'
                  )
              )";

                using (System.Data.SqlClient.SqlCommand selectCmd = new System.Data.SqlClient.SqlCommand(selectQuery, conn))
                {
                    using (System.Data.SqlClient.SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["title"].ToString();
                            string message = reader["message"].ToString();
                            string email = reader["email"].ToString();
                            string fname = reader["fname"].ToString();

                            try
                            {
                                // Send the email using your helper!
                                WAPP.Utils.EmailHelper.SendNotificationEmail(email, fname, "New Announcement: " + title, message);
                            }
                            catch (Exception ex)
                            {
                                // Log the error, but DO NOT throw it. 
                                // If one student's email is invalid, we don't want the whole job to crash.
                                System.Diagnostics.Debug.WriteLine($"Failed to send announcement email to {email}: {ex.Message}");
                            }
                        }
                    }
                }

                // 2. Execute your original bulk INSERT and UPDATE to lock the notifications in the database
                string updateQuery = @"
            INSERT INTO [dbo].[notification] (announcement_id, user_id, status, created_at)
            SELECT a.Id, u.Id, 'UNREAD', GETDATE()
            FROM [dbo].[announcement] a
            CROSS JOIN [dbo].[user] u
            WHERE a.scheduled_at <= GETDATE() 
              AND a.status = 'ACTIVE' 
              AND a.is_published = 0
              AND u.Id != a.created_by
              AND u.role_id = a.target_role_id
              AND (
                  a.course_id IS NULL 
                  OR EXISTS (
                      SELECT 1 FROM [dbo].[enrollment] e 
                      WHERE e.course_id = a.course_id AND e.student_id = u.Id AND e.status = 'ENROLLED'
                  )
              );

            UPDATE [dbo].[announcement]
            SET is_published = 1
            WHERE scheduled_at <= GETDATE() AND status = 'ACTIVE' AND is_published = 0;";

                using (System.Data.SqlClient.SqlCommand updateCmd = new System.Data.SqlClient.SqlCommand(updateQuery, conn))
                {
                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    // 3. SIGNALR TRIGGER
                    // Only send the real-time signal if we ACTUALLY published something just now
                    if (rowsAffected > 0)
                    {
                        var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<WAPP.Hubs.NotificationHub>();
                        hubContext.Clients.All.receiveNotification(0); // 0 = broadcast to everyone to check their unread counts
                    }
                }
            }
        }

        public void ProcessAppointmentRemindersJob()
        {
            string connString = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
            {
                conn.Open();

                // DATEDIFF checks if the combined Date + Time is exactly 10 minutes away from GETDATE()
                string query = @"
                    SELECT 
                        a.subject, 
                        a.start_time,
                        s.email AS StudentEmail, 
                        s.fname AS StudentName,
                        t.email AS TutorEmail, 
                        t.fname AS TutorName
                    FROM [dbo].[appointment] a
                    INNER JOIN [dbo].[user] s ON a.student_id = s.Id
                    INNER JOIN [dbo].[user] t ON a.tutor_id = t.Id
                    WHERE a.status = 'APPROVED'
                      AND DATEDIFF(minute, GETDATE(), CAST(CONCAT(a.appointment_date, ' ', a.start_time) AS DATETIME2)) = 10";

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string subject = reader["subject"].ToString();
                            string startTimeStr = reader["start_time"].ToString();

                            // Format the time nicely for the email body
                            if (TimeSpan.TryParse(startTimeStr, out TimeSpan parsedTime))
                            {
                                startTimeStr = DateTime.Today.Add(parsedTime).ToString("h:mm tt");
                            }

                            string studentEmail = reader["StudentEmail"].ToString();
                            string studentName = reader["StudentName"].ToString();
                            string tutorEmail = reader["TutorEmail"].ToString();
                            string tutorName = reader["TutorName"].ToString();

                            string emailSubject = $"Reminder: Appointment '{subject}' starts in 10 minutes!";

                            try
                            {
                                // 1. Notify Student
                                string studentMsg = $"Hi {studentName}, your appointment with {tutorName} for '{subject}' is starting shortly at {startTimeStr}. Please get ready!";
                                WAPP.Utils.EmailHelper.SendNotificationEmail(studentEmail, studentName, emailSubject, studentMsg);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to send reminder to student {studentEmail}: {ex.Message}");
                            }

                            try
                            {
                                // 2. Notify Tutor
                                string tutorMsg = $"Hi {tutorName}, your appointment with {studentName} for '{subject}' is starting shortly at {startTimeStr}. Please get ready!";
                                WAPP.Utils.EmailHelper.SendNotificationEmail(tutorEmail, tutorName, emailSubject, tutorMsg);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to send reminder to tutor {tutorEmail}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
    }
}