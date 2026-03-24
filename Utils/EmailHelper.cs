using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace WAPP.Utils
{
    public static class EmailHelper
    {
        public static void SendOtpEmail(string toEmail, string otpCode, string firstName)
        {
            // Get credentials from Web.config
            string senderEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            string senderPassword = ConfigurationManager.AppSettings["SmtpPassword"];

            try
            {
                // Set up the MailMessage
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, "EduConnect Verification");
                mail.To.Add(toEmail);
                mail.Subject = "Your EduConnect Verification Code";
                mail.IsBodyHtml = true;

                // Create a nice HTML body for the email
                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
                        <div style='background-color: #0d6efd; padding: 20px; text-align: center;'>
                            <h2 style='color: white; margin: 0;'>EduConnect Registration</h2>
                        </div>
                        <div style='padding: 30px; background-color: #ffffff;'>
                            <p style='font-size: 16px; color: #334155;'>Hello <strong>{firstName}</strong>,</p>
                            <p style='font-size: 16px; color: #334155;'>Thank you for joining EduConnect! To complete your registration, please enter the following 6-digit verification code:</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #0d6efd; background-color: #f8fafc; padding: 15px 30px; border-radius: 8px; border: 1px dashed #cbd5e1;'>
                                    {otpCode}
                                </span>
                            </div>
                            
                            <p style='font-size: 14px; color: #64748b;'>This code will expire in 10 minutes. If you did not request this code, please ignore this email.</p>
                        </div>
                    </div>";

                // Set up the Gmail SMTP Client
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtp.EnableSsl = true; 
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                // In a real app, log this error. For now, we throw it so the signup modal catches it.
                throw new Exception("Failed to send OTP email: " + ex.Message);
            }
        }
        public static void SendNotificationEmail(string toEmail, string firstName, string subject, string notificationMessage)
        {
            string senderEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            string senderPassword = ConfigurationManager.AppSettings["SmtpPassword"];

            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, "EduConnect Notifications");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.IsBodyHtml = true;

                // A cleaner, generalized HTML template for standard alerts
                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
                        <div style='background-color: #0d6efd; padding: 20px; text-align: center;'>
                            <h2 style='color: white; margin: 0;'>EduConnect</h2>
                        </div>
                        <div style='padding: 30px; background-color: #ffffff;'>
                            <p style='font-size: 16px; color: #334155;'>Hello <strong>{firstName}</strong>,</p>
                            
                            <div style='font-size: 16px; color: #334155; line-height: 1.6;'>
                                {notificationMessage}
                            </div>
                            
                            <p style='font-size: 12px; color: #94a3b8; margin-top: 40px; border-top: 1px solid #e2e8f0; padding-top: 20px; text-align: center;'>
                                This is an automated message from EduConnect. Please do not reply to this email.
                            </p>
                        </div>
                    </div>";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send notification email: " + ex.Message);
            }
        }

        public static void SendAppointmentEmail(string toEmail, string recipientName, string subject, string appointmentTopic, string appointmentDate, string appointmentDuration, string appointmentStatus, string customMessage)
        {
            string senderEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            string senderPassword = ConfigurationManager.AppSettings["SmtpPassword"];

            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, "EduConnect Appointments");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.IsBodyHtml = true;

                // Determine a highlight color based on the status (Pending = Yellow/Orange, Approved = Green, Rejected = Red)
                string statusColor = "#64748b"; // default slate
                if (appointmentStatus.ToUpper() == "PENDING") statusColor = "#f59e0b"; // Amber
                else if (appointmentStatus.ToUpper() == "APPROVED" || appointmentStatus.ToUpper() == "ACCEPTED") statusColor = "#10b981"; // Emerald Green
                else if (appointmentStatus.ToUpper() == "REJECTED" || appointmentStatus.ToUpper() == "CANCELLED") statusColor = "#ef4444"; // Red

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
                        <div style='background-color: #0a58ca; padding: 20px; text-align: center;'>
                            <h2 style='color: white; margin: 0;'>Session Update</h2>
                        </div>
                        <div style='padding: 30px; background-color: #ffffff;'>
                            <p style='font-size: 16px; color: #334155;'>Hello <strong>{recipientName}</strong>,</p>
                            
                            <p style='font-size: 16px; color: #334155; line-height: 1.6;'>
                                {customMessage}
                            </p>

                            <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-left: 4px solid {statusColor}; border-radius: 6px; padding: 20px; margin: 25px 0;'>
                                <h3 style='margin-top: 0; color: #1e293b; font-size: 18px;'>Appointment Details</h3>
                                
                                <table style='width: 100%; border-collapse: collapse;'>
                                    <tr>
                                        <td style='padding: 8px 0; color: #64748b; width: 30%;'><strong>Topic:</strong></td>
                                        <td style='padding: 8px 0; color: #0f172a;'>{appointmentTopic}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px 0; color: #64748b;'><strong>Date & Time:</strong></td>
                                        <td style='padding: 8px 0; color: #0f172a;'>{appointmentDate}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px 0; color: #64748b;'><strong>Duration:</strong></td>
                                        <td style='padding: 8px 0; color: #0f172a;'>{appointmentDuration} mins</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px 0; color: #64748b;'><strong>Status:</strong></td>
                                        <td style='padding: 8px 0;'>
                                            <span style='background-color: {statusColor}; color: white; padding: 4px 10px; border-radius: 50px; font-size: 12px; font-weight: bold;'>
                                                {appointmentStatus.ToUpper()}
                                            </span>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            
                            <p style='font-size: 16px; color: #334155;'>Log in to your EduConnect dashboard to manage your schedule and view more details.</p>

                            <p style='font-size: 12px; color: #94a3b8; margin-top: 40px; border-top: 1px solid #e2e8f0; padding-top: 20px; text-align: center;'>
                                This is an automated message from EduConnect. Please do not reply to this email.
                            </p>
                        </div>
                    </div>";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send appointment email: " + ex.Message);
            }
        }
    }
}