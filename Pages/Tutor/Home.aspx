<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Tutor.Home" %>
<%@ Register Src="~/Pages/Tutor/Calendar.ascx" TagPrefix="uc" TagName="Calendar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:HiddenField ID="hfMyId" runat="server" ClientIDMode="Static" />

    <div class="page-layout-split">
        <div class="layout-main-70">
            <div class="ec-hero" style="display: flex; align-items: center; gap: 20px;">
                <asp:Image ID="imgTutor" runat="server" ImageUrl="~/Images/profile_f.png" CssClass="ec-hero-avatar" />
        
                <div style="display: flex; flex-direction: column; justify-content: center;">
                    <h2 style="margin-bottom: 8px; margin-top: 0;">Welcome back, <asp:Label ID="lblTutorName" runat="server"></asp:Label>!</h2>
            
                    <asp:Panel ID="pnlApplicationStatus" runat="server">
                        <i id="iconStatus" runat="server" class="bi"></i>
                        <span style="font-weight: 600;"><asp:Label ID="lblStatusText" runat="server"></asp:Label></span>
                    </asp:Panel>
                </div>
            </div>
            
            <div class="layout-grid-half mb-4">
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="fw-bold m-0">Teaching Overview</h5>
                    </div>
                    
                    <div class="ec-glass-card-body">
                        <div class="d-flex justify-content-between text-center mb-4 mt-2" style="gap: 15px;">
                            
                            <div class="ec-stat-box flex-fill">
                                <asp:Label ID="lblPublishedCourses" runat="server" Text="0" CssClass="ec-stat-value text-success"></asp:Label>
                                <span class="ec-stat-desc">Published<br>Courses</span>
                            </div>

                            <div class="ec-stat-box flex-fill">
                                <asp:Label ID="lblPendingCourses" runat="server" Text="0" CssClass="ec-stat-value text-warning"></asp:Label>
                                <span class="ec-stat-desc">Pending<br>Approval</span>
                            </div>

                            <div class="ec-stat-box flex-fill">
                                <asp:Label ID="lblTotalResources" runat="server" Text="0" CssClass="ec-stat-value text-primary"></asp:Label>
                                <span class="ec-stat-desc">Learning<br>Resources</span>
                            </div>
                            
                        </div>

                        <div class="ec-card-actions mt-auto border-top pt-3">
                            <asp:HyperLink ID="lnkCreateCourse" runat="server" Text="Go to Courses" CssClass="btn-main w-100 rounded-pill" NavigateUrl="~/Pages/Tutor/Teaching.aspx" />
                        </div>
                    </div>
                </div>
                
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="fw-bold m-0">Recent Announcements</h5>
                    </div>
                    
                    <div class="ec-glass-card-body d-flex flex-column h-100">
                        <div class="flex-grow-1">
                            <asp:Repeater ID="rptRecentAnnouncements" runat="server">
                                <ItemTemplate>
                                    <div class="ec-feed-item flex-column align-items-start mb-3 pb-3">
                                        <div class="d-flex justify-content-between w-100 mb-1">
                                            <span class="fw-bold text-dark"><%# Eval("title") %></span>
                                            <span class="text-muted small"><%# FormatDate(Eval("created_at")) %></span>
                                        </div>
                                        <div class="text-secondary small" style="line-height: 1.5;">
                                            <%# TruncateMessage(Eval("message"), 80) %>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            
                            <asp:Label ID="lblNoAnnouncements" runat="server" Visible="false" Text="No recent announcements." CssClass="text-muted text-center d-block mt-4"></asp:Label>
                        </div>
                        
                        <div class="ec-card-actions border-top pt-3 mt-auto">
                            <asp:HyperLink ID="lnkViewAllAnnouncements" runat="server" Text="View All Announcements" CssClass="btn-main w-100 rounded-pill" NavigateUrl="~/Pages/Tutor/AnnouncementRecord.aspx" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card mb-4">
                <div class="ec-glass-card-header border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Session Appointments</h5>
                    <asp:HyperLink ID="lnkManageAppointments" runat="server" Text="Manage Schedule &rarr;" NavigateUrl="~/Pages/Tutor/TutorAppointments.aspx" CssClass="ec-link-primary m-0 small fw-bold" />
                </div>
                <div class="ec-glass-card-body">
                    <div class="p-3 bg-light rounded-3 border d-flex align-items-center gap-3">
                        <div class="bg-danger bg-opacity-10 text-danger rounded-circle d-flex align-items-center justify-content-center shadow-sm" style="width: 55px; height: 55px;">
                            <i class="bi bi-calendar-event fs-4"></i>
                        </div>
                        <div>
                            <h4 class="fw-bold text-dark m-0"><asp:Label ID="lblPendingAppts" runat="server" Text="0" CssClass="text-danger"></asp:Label></h4>
                            <span class="text-muted small fw-medium">Pending Session Requests</span>
                        </div>
                    </div>
                </div>
            </div>
            <div class="ec-glass-card">
                <div class="ec-glass-card-header border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Recent Messages</h5>
                    <asp:HyperLink ID="HyperLink1" runat="server" Text="View All Chat &rarr;" NavigateUrl="~/Pages/Shared/Chat.aspx" CssClass="ec-link-primary m-0 small fw-bold" />
                </div>
                
                <asp:UpdatePanel ID="upRecentMessages" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                            <asp:Button ID="btnSignalRUpdate" runat="server" ClientIDMode="Static" OnClick="btnSignalRUpdate_Click" style="display:none;" />
                        <div class="ec-glass-card-body p-0">
                            <asp:Repeater ID="rptUnreadMessages" runat="server">
                                <ItemTemplate>
                                    <a href='<%# ResolveUrl("~/Pages/Shared/Chat.aspx") %>' class="d-flex align-items-center p-3 border-bottom text-decoration-none" style="transition: background-color 0.2s ease;">
                                        
                                        <div class="me-3">
                                            <i class="bi bi-person-circle text-primary" style="font-size: 2.2rem;"></i>
                                        </div>
                                        
                                        <div class="flex-grow-1 overflow-hidden">
                                            <div class="d-flex justify-content-between align-items-center mb-1">
                                                <span class="fw-bold text-dark text-truncate"><%# Eval("SenderName") %></span>
                                                <span class="text-muted small" style="font-size: 0.75rem;"><%# FormatDate(Eval("LastMessageTime")) %></span>
                                            </div>
                                            <div class="d-flex justify-content-between align-items-center">
                                                <span class="text-secondary small text-truncate" style="max-width: 80%;"><%# TruncateMessage(Eval("LastMessage"), 40) %></span>
                                                <span class="badge bg-danger rounded-pill" style="font-size: 0.7rem;"><%# Eval("UnreadCount") %></span>
                                            </div>
                                        </div>

                                    </a>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:Label ID="lblNoUnreadMessages" runat="server" Visible="false" CssClass="text-muted d-block text-center mt-4 mb-3 small">
                                <i class="bi bi-check2-all d-block mb-2" style="font-size: 2rem; color: #198754;"></i>
                                All caught up!<br />No unread messages.
                            </asp:Label>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>

        <div class="layout-sidebar-30">
            <div class="ec-glass-card">
                <div class="ec-glass-card-header border-0 pb-0 mb-2">
                    <h5 class="fw-bold m-0">Calendar</h5>
                </div>
                <uc:Calendar runat="server" ID="ucCalendar" />
            </div>
            
            <div class="ec-glass-card notifications d-flex flex-column h-100">
                <asp:UpdatePanel ID="upNotifications" runat="server" UpdateMode="Conditional" class="d-flex flex-column h-100 flex-grow-1">
                    <ContentTemplate>
                        <div class="ec-glass-card-header border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                            <h5 class="fw-bold m-0">Notifications</h5>
                            <asp:Label ID="lblNotificationCount" runat="server" CssClass="badge bg-danger rounded-pill"></asp:Label>
                        </div>

                        <div class="ec-glass-card-body flex-grow-1 overflow-auto" style="max-height: 300px;">
                            <asp:Repeater ID="rptNotifications" runat="server">
                                <ItemTemplate>
                                    <div class="notification-item mb-3 pb-2 border-bottom">
                                        <div class="d-flex justify-content-between align-items-start mb-1">
                                            <span class="fw-bold text-dark" style="font-size: 0.9rem;"><%# Eval("title") %></span>
                                            <span class="text-muted small" style="font-size: 0.75rem;"><%# FormatDate(Eval("created_at")) %></span>
                                        </div>
                                        <div class="text-secondary small" style="line-height: 1.4;">
                                            <%# TruncateMessage(Eval("message"), 60) %>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            
                            <asp:Label ID="lblNoNotifications" runat="server" Visible="false" Text="You're all caught up!" CssClass="text-muted text-center d-block mt-4 small"></asp:Label>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <div class="ec-card-actions mt-auto pt-3 border-top text-center">
                    <asp:HyperLink ID="lnkAllNotifications" runat="server" Text="View All Notifications &rarr;" CssClass="ec-link-primary m-0 small fw-bold" NavigateUrl="~/Pages/Tutor/Notifications.aspx" />
                </div>
            </div>
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="../../Scripts/jquery.signalR-2.4.3.min.js"></script>
    <script src="../../signalr/hubs"></script>

    <script type="text/javascript">
        $(function () {
            var chat = $.connection.chatHub;

            chat.client.receiveNewMessage = function (senderId, receiverId) {
                var myId = $('#hfMyId').val();

                // If the message is meant for the logged-in user, refresh the panels!
                if (receiverId == myId) {
                    $('#btnSignalRUpdate').click();
                }
            };

            $.connection.hub.start().done(function () {
                console.log("Home Page: SignalR Connected for real-time updates!");
            });
        });
    </script>
</asp:Content>