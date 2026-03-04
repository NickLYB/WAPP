<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="Chat.aspx.cs" Inherits="WAPP.Pages.Tutor.Chat" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        <div class="ec-item-gap">
                <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                    PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
            </div>
        <div class="ec-section-gap">
            <h1 class="ec-page-title">Messages</h1>
            <p class="ec-page-subtitle">Communicate with your students in real-time.</p>
        </div>

        <div class="ec-chat-layout">
            
            <div class="ec-chat-sidebar">
    
                <div class="p-3 border-bottom bg-white">
                    <asp:TextBox ID="txtSearch" runat="server" ClientIDMode="Static" CssClass="login-input m-0" 
                        placeholder="Search students..." style="padding: 10px 15px; font-size: 0.9rem;"
                        onkeyup="handleLiveSearch(event)"></asp:TextBox>
        
                    <asp:Button ID="btnSearchTrigger" runat="server" ClientIDMode="Static" OnClick="btnSearchTrigger_Click" style="display:none;" />
                </div>
    
                <div class="ec-chat-list">
                    <asp:UpdatePanel ID="upContacts" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Repeater ID="rptContacts" runat="server" OnItemCommand="rptContacts_ItemCommand">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkContact" runat="server" 
                                        CommandName="SelectContact" 
                                        CommandArgument='<%# Eval("UserId") + "|" + Eval("UserName") %>'
                                        CssClass='<%# Convert.ToInt32(Eval("UserId")) == ActiveContactId ? "ec-chat-contact active" : "ec-chat-contact" %>'>
                                        <div class="small fw-bold"><%# Eval("UserName") %></div>
                                        <small class="text-muted">Tap to message</small>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:Repeater>
                
                            <asp:Label ID="lblNoContacts" runat="server" Visible="false" CssClass="text-muted small d-block text-center mt-4">
                                No conversations found.
                            </asp:Label>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>

            <div class="ec-chat-window">
                <asp:UpdatePanel ID="upChatArea" runat="server" UpdateMode="Conditional" style="height: 100%; display: flex; flex-direction: column;">
                    <ContentTemplate>
                        <asp:HiddenField ID="hfMyId" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hfCurrentContactId" runat="server" ClientIDMode="Static" />
                        <asp:Button ID="btnRefreshChat" runat="server" OnClick="btnRefreshChat_Click" style="display:none;" />

                        <div class="p-3 border-bottom bg-white d-flex justify-content-between align-items-center">
                            <h6 class="fw-bold m-0 text-dark">
                                <i class="bi bi-person-circle me-2 text-primary"></i>
                                <asp:Label ID="lblChatHeader" runat="server" Text="SELECT A CONVERSATION"></asp:Label>
                            </h6>
                        </div>

                        <div class="ec-chat-messages" id="chatMessagesContainer">
                            <asp:Repeater ID="rptMessages" runat="server">
                                <ItemTemplate>
                                    <div class='<%# Convert.ToInt32(Eval("sender_id")) == LoggedInUserId ? "ec-msg-wrapper sent" : "ec-msg-wrapper received" %>'>
                                        <div class="ec-msg-bubble shadow-sm">
                                            <%# Eval("message_text") %>
                                        </div>
                                        <small class="text-muted mt-1" style="font-size: 0.7rem;">
                                            <%# Eval("SenderName") %> • <%# Convert.ToDateTime(Eval("created_at")).ToString("MMM dd, hh:mm tt") %>
                                        </small>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:PlaceHolder ID="phNoMessages" runat="server" Visible="false">
                                <div class="ec-empty-state">
                                    <i class="bi bi-chat-dots"></i>
                                    <p>Start a conversation with this student.</p>
                                </div>
                            </asp:PlaceHolder>
                        </div>

                        <div class="ec-chat-footer">
                            <asp:TextBox ID="txtMessage" runat="server" CssClass="login-input m-0" 
                                placeholder="Write your message..." Enabled="false"></asp:TextBox>
                            <asp:Button ID="btnSend" runat="server" Text="Send" 
                                CssClass="btn-main btn-pill px-4" OnClick="btnSend_Click" Enabled="false" />
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="../../Scripts/jquery.signalR-2.4.3.min.js"></script>
    <script src="../../signalr/hubs"></script>

    <script type="text/javascript">
        var typingTimer;

        function handleLiveSearch(event) {
            // Prevent page reload if they accidentally press 'Enter'
            if (event.keyCode === 13) {
                event.preventDefault();
                clearTimeout(typingTimer);
                document.getElementById('btnSearchTrigger').click();
                return false;
            }

            // Ignore arrow keys, shift, ctrl, etc. to prevent useless searches
            var key = event.keyCode || event.charCode;
            if (key >= 37 && key <= 40) return;

            // Clear the timer on every keystroke
            clearTimeout(typingTimer);

            // Start a new 400ms timer. If they stop typing for 0.4 seconds, it triggers the search!
            typingTimer = setTimeout(function () {
                document.getElementById('btnSearchTrigger').click();
            }, 400);
        }
        $(function () {
            var chat = $.connection.chatHub;
            chat.client.receiveNewMessage = function (senderId, receiverId) {
                var myId = $('#hfMyId').val();
                var currentContactId = $('#hfCurrentContactId').val();
                if ((receiverId == myId && senderId == currentContactId) ||
                    (senderId == myId && receiverId == currentContactId)) {
                    $('#<%= btnRefreshChat.ClientID %>').click();
                }
            };
            $.connection.hub.start().done(function () {
                console.log("Connected!");
            });
        });

        function scrollToBottom() {
            var chatContainer = document.getElementById("chatMessagesContainer");
            if (chatContainer) { chatContainer.scrollTop = chatContainer.scrollHeight; }
        }
        document.addEventListener("DOMContentLoaded", scrollToBottom);
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(scrollToBottom);
        }
    </script>
</asp:Content>
