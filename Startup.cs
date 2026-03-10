using Hangfire;
using Microsoft.Owin;
using Owin;
using System;
using System.Configuration;
using System.Threading.Tasks;

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

            // 5. Schedule our recurring job to run every minute
            RecurringJob.AddOrUpdate(
                "process-scheduled-announcements",
                () => ProcessAnnouncementsJob(),
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
              AND u.role_id = a.target_role_id -- Only notify the target role (Students)
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
                      SELECT 1 
                      FROM [dbo].[enrollment] e 
                      WHERE e.course_id = a.course_id 
                        AND e.student_id = u.Id 
                        AND e.status = 'ENROLLED'
                  )
              );

            UPDATE [dbo].[announcement]
            SET is_published = 1
            WHERE scheduled_at <= GETDATE() AND status = 'ACTIVE' AND is_published = 0;";

                using (System.Data.SqlClient.SqlCommand updateCmd = new System.Data.SqlClient.SqlCommand(updateQuery, conn))
                {
                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
