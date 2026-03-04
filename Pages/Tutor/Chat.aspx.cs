using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    public partial class Chat : System.Web.UI.Page
    {
        public int LoggedInUserId
        {
            get { return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0; }
        }

        public int ActiveContactId
        {
            get { return ViewState["ActiveContactId"] != null ? Convert.ToInt32(ViewState["ActiveContactId"]) : 0; }
            set { ViewState["ActiveContactId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LoggedInUserId == 0)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                LoadContacts(""); // Blank search term loads history by default
                hfMyId.Value = LoggedInUserId.ToString();
            }
        }

        // Triggered when the Tutor types a name and presses Enter
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

                if (string.IsNullOrEmpty(searchTerm))
                {
                    // DEFAULT: Only load users you already have a conversation with
                    query = @"
                        SELECT Id AS UserId, (fname + ' ' + lname) AS UserName 
                        FROM [user] 
                        WHERE Id IN (
                            SELECT sender_id FROM chatMessage WHERE receiver_id = @Me
                            UNION 
                            SELECT receiver_id FROM chatMessage WHERE sender_id = @Me
                        )";

                    cmd.Parameters.AddWithValue("@Me", LoggedInUserId);
                }
                else
                {
                    // SEARCH: Load any user matching the search term
                    query = @"
                        SELECT Id AS UserId, (fname + ' ' + lname) AS UserName 
                        FROM [user] 
                        WHERE (fname LIKE @Search OR lname LIKE @Search) 
                        AND Id != @Me";

                    cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                    cmd.Parameters.AddWithValue("@Me", LoggedInUserId);
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

        private void LoadChatHistory()
        {
            if (ActiveContactId == 0) return;

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT m.sender_id, m.message_text, m.created_at, u.fname AS SenderName
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

            // If this was the very first message sent to this user, we should refresh the sidebar
            // so they permanently appear in the history list, then update the panel.
            LoadContacts(txtSearch.Text.Trim());
            upContacts.Update();

            LoadChatHistory();
        }

        protected void btnRefreshChat_Click(object sender, EventArgs e)
        {
            LoadChatHistory();

            // Refresh sidebar in case a new student messaged us
            LoadContacts(txtSearch.Text.Trim());
            upContacts.Update();
        }

        protected void btnSearchTrigger_Click(object sender, EventArgs e)
        {
            // Search the database
            LoadContacts(txtSearch.Text.Trim());

            // Only refresh the sidebar list, keeping the user's cursor safely in the search box
            upContacts.Update();
        }
    }
}