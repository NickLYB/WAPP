<%@ Page Title="Student Dashboard" Language="C#" MasterPageFile="~/Masters/Student.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Student.Home" %>
<%@ Register Src="~/Pages/Student/StudentCalendar.ascx" TagPrefix="uc" TagName="StudentCalendar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField ID="hfMyId" runat="server" ClientIDMode="Static" />
    <div class="page-layout-split">

        <div class="layout-main-70">
            
            <div class="ec-hero ec-hero-student">
                <span class="ec-status-pill ec-status-active mb-3">DASHBOARD</span>
                <h2 class="fw-bold mb-2">Welcome back, <asp:Label ID="lblStudentName" runat="server">Student</asp:Label>!</h2>
                <p class="mb-4">Ready to continue your learning journey? Pick up right where you left off.</p>
                <a href="Study.aspx" class="btn btn-primary rounded-pill px-4 py-2 fw-bold shadow-sm">
                    <i class="bi bi-rocket-takeoff me-2"></i>Explore Courses
                </a>
            </div>

           <h5 class="fw-bold mb-3 mt-4"><i class="bi bi-activity text-primary me-2"></i>Recent Activity</h5>
           <div class="row g-3 mb-4">
                <asp:Repeater ID="rptRecentActivity" runat="server">
                    <ItemTemplate>
                        <div class="col-md-4">
                            <div class="ec-glass-card h-100 p-3" style="border-left: 4px solid var(--ec-primary);">
                                <div class="d-flex align-items-center mb-2">
                                    <div class='<%# GetActivityIconClass(Eval("ActivityType"), Eval("Score")) %>' style="width: 35px; height: 35px; font-size: 1.1rem;">
                                        <i class='<%# GetActivityIcon(Eval("ActivityType")) %>'></i>
                                    </div>
                                    <small class="text-muted ms-2 fw-bold" style="font-size: 0.75rem; text-transform: uppercase;">
                                        <%# Eval("ActivityType") %>
                                    </small>
                                </div>
                    
                                <h6 class="fw-bold text-main text-truncate mb-1" title='<%# Eval("Title") %>'><%# Eval("Title") %></h6>
                    
                                <div class="d-flex justify-content-between align-items-center mt-2">
                                    <small class='<%# GetActivityTextClass(Eval("ActivityType"), Eval("Score")) %> fw-bold' style="font-size: 0.8rem;">
                                        <%# GetActivityDescription(Eval("ActivityType"), Eval("Score")) %>
                                    </small>
                                    <small class="text-secondary" style="font-size: 0.7rem;">
                                        <%# Eval("ActivityDate", "{0:MMM dd}") %>
                                    </small>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:PlaceHolder ID="phNoActivity" runat="server" Visible="false">
                    <div class="col-12">
                        <div class="text-center text-muted p-4 ec-glass-card">No recent activity. Start learning!</div>
                    </div>
                </asp:PlaceHolder>
            </div>

            <div class="layout-grid-half mt-2">
                
                <div class="ec-glass-card d-flex flex-column">
                    <div class="ec-glass-card-header border-0 pb-0">
                        <h5 class="fw-bold m-0"><i class="bi bi-bookmark-star-fill text-warning me-2"></i>Jump Back In</h5>
                    </div>
                    <div class="ec-glass-card-body pt-3 d-flex flex-column flex-grow-1">
                        <asp:Repeater ID="rptRecentCourses" runat="server">
                            <ItemTemplate>
                                <div class="ec-course-box p-4 text-center border rounded-4 shadow-sm d-flex flex-column flex-grow-1 justify-content-between">
                                    <div class="my-auto">
                                        <i class="bi bi-play-circle-fill text-primary mb-3" style="font-size: 3rem;"></i>
                                        <h5 class="fw-bold text-main text-truncate mb-1"><%# Eval("CourseTitle") %></h5>
                                        <p class="text-muted small mb-4">Last Accessed: <%# Eval("LastAccess", "{0:MMM dd}") %></p>
                                    </div>
                                    <a href='LessonView.aspx?resourceId=<%# Eval("LastResourceId") %>' class="btn-main btn-pill w-100 mt-auto fw-bold btn-sm">Resume Lesson</a>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        
                        <asp:PlaceHolder ID="phNoCourses" runat="server" Visible="false">
                            <div class="text-center text-muted p-4 m-auto">You haven't started any courses yet.</div>
                        </asp:PlaceHolder>
                    </div>
                </div>

                <div class="ec-glass-card d-flex flex-column">
                    <div class="ec-glass-card-header border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                        <h5 class="fw-bold m-0"><i class="bi bi-chat-dots-fill text-info me-2"></i>Recent Messages</h5>
                        <asp:HyperLink ID="lnkViewAllChat" runat="server" Text="View All &rarr;" NavigateUrl="~/Pages/Shared/Chat.aspx" CssClass="ec-link-primary m-0 small fw-bold" />
                    </div>
                
                    <div class="ec-glass-card-body p-0 d-flex flex-column flex-grow-1">
                        <asp:UpdatePanel ID="upRecentMessages" runat="server" UpdateMode="Conditional" style="height: 100%;">
                            <ContentTemplate>
                                <asp:Button ID="btnSignalRUpdate" runat="server" ClientIDMode="Static" OnClick="btnSignalRUpdate_Click" style="display:none;" />
                                
                                <div class="d-flex flex-column flex-grow-1 h-100 w-100">
                                    
                                    <div class="w-100">
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
                                    </div>

                                    <asp:Label ID="lblNoUnreadMessages" runat="server" Visible="false" CssClass="text-muted text-center w-100 m-auto py-4">
                                        <i class="bi bi-check2-all d-block mb-2" style="font-size: 2.5rem; color: #198754;"></i>
                                        <span class="d-block">All caught up!</span>
                                        <span class="d-block">No unread messages.</span>
                                    </asp:Label>

                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>

            </div>
        </div>

        <div class="layout-sidebar-30">
            <div class="ec-glass-card">
                 <div class="ec-glass-card-header border-0 pb-0 mb-2">
                    <h5 class="fw-bold m-0">Calendar</h5>
                </div>
                <uc:StudentCalendar runat="server" ID="ucCalendar" />
            </div>
            <div class="ec-glass-card notifications d-flex flex-column h-100 mt-4">
                <asp:UpdatePanel ID="upNotifications" runat="server" UpdateMode="Conditional" class="d-flex flex-column h-100 flex-grow-1">
                    <ContentTemplate>
                        <div class="ec-glass-card-header border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                            <h5 class="fw-bold m-0"><i class="bi bi-bell-fill text-danger me-2"></i>Notifications</h5>
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
                    <asp:HyperLink ID="lnkAllNotifications" runat="server" Text="View All Notifications &rarr;" CssClass="ec-link-primary m-0 small fw-bold" NavigateUrl="~/Pages/Student/Notifications.aspx" />
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
                console.log("Student Dashboard: SignalR Connected for real-time updates!");
            });
        });
    </script>
</asp:Content>