using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class Calendar : System.Web.UI.UserControl
    {
        string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        DataTable dtAppointments = new DataTable();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                // Load data on every postback to fix the disappearing month bug!
                LoadAppointments();
            }
        }

        private void LoadAppointments()
        {
            int tutorId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Fetch appointments AND the Student's name
                string query = @"
                    SELECT a.appointment_date, a.start_time, a.status, a.subject, (u.fname + ' ' + u.lname) AS StudentName
                    FROM appointment a
                    JOIN [user] u ON a.student_id = u.Id
                    WHERE a.tutor_id = @tid AND a.status IN ('PENDING', 'APPROVED')";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tid", tutorId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtAppointments);
            }
        }

        protected void Calendar1_DayRender(object sender, DayRenderEventArgs e)
        {
            if (dtAppointments == null || dtAppointments.Rows.Count == 0) return;

            foreach (DataRow row in dtAppointments.Rows)
            {
                DateTime apptDate = Convert.ToDateTime(row["appointment_date"]);

                if (e.Day.Date == apptDate.Date)
                {
                    string status = row["status"].ToString();
                    string subject = row["subject"].ToString();
                    string studentName = row["StudentName"].ToString();
                    TimeSpan startTime = (TimeSpan)row["start_time"];
                    string timeStr = DateTime.Today.Add(startTime).ToString("hh:mm tt");

                    string badgeClass = status == "APPROVED" ? "bg-success bg-opacity-25 text-success border border-success" : "bg-warning bg-opacity-25 text-dark border border-warning";

                    // Call the JS function passing the STUDENT name
                    string htmlContent = $@"
                        <div class='appt-badge {badgeClass}' onclick=""showApptModal(event, '{studentName}', '{subject}', '{timeStr}', '{status}')"" title='Click for details'>
                            <i class='bi bi-circle-fill me-1' style='font-size: 0.4rem;'></i>{timeStr}
                        </div>";

                    LiteralControl apptBadge = new LiteralControl(htmlContent);
                    e.Cell.Controls.Add(apptBadge);
                }
            }
        }
    }
}