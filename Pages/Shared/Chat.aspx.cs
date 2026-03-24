using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Shared
{
    public partial class Chat : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (Session["role_id"] != null)
            {
                int roleId = Convert.ToInt32(Session["role_id"]);
                switch (roleId)
                {
                    case 1: // Admin
                        this.MasterPageFile = "~/Masters/Admin.Master";
                        break;
                    case 2: // Staff
                        this.MasterPageFile = "~/Masters/Staff.Master";
                        break;
                    case 3: // Tutor
                        this.MasterPageFile = "~/Masters/Tutor.Master";
                        break;
                    case 4: // Student
                        this.MasterPageFile = "~/Masters/Student.Master";
                        break;
                    default:
                        this.MasterPageFile = "~/Masters/Guest.Master";
                        break;
                }
            }
        }

        public int LoggedInUserId => Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
        public int UserRoleId => Session["role_id"] != null ? Convert.ToInt32(Session["role_id"]) : 0;

        public int ActiveContactId
        {
            get => ViewState["ActiveContactId"] != null ? Convert.ToInt32(ViewState["ActiveContactId"]) : 0;
            set => ViewState["ActiveContactId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LoggedInUserId == 0)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Dynamic SiteMapProvider Setup
                if (UserRoleId == 1) SiteMapPath1.SiteMapProvider = "AdminMap";
                else if (UserRoleId == 2) SiteMapPath1.SiteMapProvider = "StaffMap";
                else if (UserRoleId == 3) SiteMapPath1.SiteMapProvider = "TutorMap";
                else if (UserRoleId == 4) SiteMapPath1.SiteMapProvider = "StudentMap";

                LoadContacts("");
                hfMyId.Value = LoggedInUserId.ToString();
            }
        }

        // Triggered when the user types a name
        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadContacts(txtSearch.Text.Trim());
        }

        private void LoadContacts(string searchTerm)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "";
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.Parameters.AddWithValue("@Me", LoggedInUserId);

                if (string.IsNullOrEmpty(searchTerm))
                {
                    // Load users you have existing conversations with
                    query = @"
                        SELECT 
                            u.Id AS UserId, 
                            (u.fname + ' ' + u.lname) AS UserName,
                            (SELECT COUNT(Id) FROM chatMessage WHERE sender_id = u.Id AND receiver_id = @Me AND is_read = 0) AS UnreadCount
                        FROM [user] u 
                        WHERE u.Id IN (
                            SELECT sender_id FROM chatMessage WHERE receiver_id = @Me
                            UNION 
                            SELECT receiver_id FROM chatMessage WHERE sender_id = @Me
                        )
                        ORDER BY UnreadCount DESC, UserName ASC";
                }
                else
                {
                    // SEARCH Logic with Role Boundaries
                    string roleFilter = "";
                    if (UserRoleId == 4) // Student searching
                        roleFilter = " AND u.role_id = 3"; // Can only find Tutors

                    query = @"
                        SELECT 
                            u.Id AS UserId, 
                            (u.fname + ' ' + u.lname) AS UserName,
                            0 AS UnreadCount
                        FROM [user] u 
                        WHERE (u.fname LIKE @Search OR u.lname LIKE @Search) 
                        AND u.Id != @Me " + roleFilter + @"
                        ORDER BY UserName ASC";

                    cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                }

                cmd.CommandText = query;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    sda.Fill(dt);

                    rptContacts.DataSource = dt;
                    rptContacts.DataBind();

                    lblNoContacts.Visible = (dt.Rows.Count == 0);
                }
            }
        }

        protected void rptContacts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "SelectContact")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                ActiveContactId = Convert.ToInt32(args[0]);
                string contactName = args[1];

                hfCurrentContactId.Value = ActiveContactId.ToString();

                // Re-bind to apply the active CSS class
                LoadContacts(txtSearch.Text.Trim());

                lblChatHeader.Text = "CHAT WITH " + contactName.ToUpper();
                txtMessage.Enabled = true;
                btnSend.Enabled = true;

                LoadChatHistory();
            }
        }

        private void MarkMessagesAsRead()
        {
            if (ActiveContactId == 0) return;

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    UPDATE chatMessage 
                    SET is_read = 1 
                    WHERE sender_id = @Them AND receiver_id = @Me AND is_read = 0";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Them", ActiveContactId);
                    cmd.Parameters.AddWithValue("@Me", LoggedInUserId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadChatHistory()
        {
            if (ActiveContactId == 0) return;

            MarkMessagesAsRead();

            LoadContacts(txtSearch.Text.Trim());
            upContacts.Update();

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT m.sender_id, m.message_text, m.created_at, m.is_read, u.fname AS SenderName
                    FROM chatMessage m
                    INNER JOIN [user] u ON m.sender_id = u.Id
                    WHERE (m.sender_id = @Me AND m.receiver_id = @Them)
                       OR (m.sender_id = @Them AND m.receiver_id = @Me)
                    ORDER BY m.created_at ASC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Me", LoggedInUserId);
                    cmd.Parameters.AddWithValue("@Them", ActiveContactId);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptMessages.DataSource = dt;
                            rptMessages.DataBind();
                            phNoMessages.Visible = false;
                        }
                        else
                        {
                            rptMessages.DataSource = null;
                            rptMessages.DataBind();
                            phNoMessages.Visible = true;
                        }
                    }
                }
            }
            upChatArea.Update();
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message) || ActiveContactId == 0) return;

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"INSERT INTO chatMessage (sender_id, receiver_id, message_text) 
                         VALUES (@SenderId, @ReceiverId, @MessageText)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SenderId", LoggedInUserId);
                    cmd.Parameters.AddWithValue("@ReceiverId", ActiveContactId);
                    cmd.Parameters.AddWithValue("@MessageText", message);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    hubContext.Clients.All.receiveNewMessage(LoggedInUserId.ToString(), ActiveContactId.ToString());
                }
            }

            txtMessage.Text = "";

            LoadContacts(txtSearch.Text.Trim());
            upContacts.Update();
            LoadChatHistory();

            ScriptManager.RegisterStartupScript(btnSend, btnSend.GetType(), "ClearMyChatBox", "setTimeout(function() { document.getElementById('txtMessage').value = ''; }, 0);", true);
        }
        protected void btnRefreshChat_Click(object sender, EventArgs e)
        {
            LoadChatHistory();
        }
        protected void btnRefreshSidebar_Click(object sender, EventArgs e)
        {
            LoadContacts(txtSearch.Text.Trim());
            upContacts.Update();
        }
        protected void btnSearchTrigger_Click(object sender, EventArgs e)
        {
            LoadContacts(txtSearch.Text.Trim());
            upContacts.Update();
        }
    }
}