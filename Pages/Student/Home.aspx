<%@ Page Title="Student Dashboard" Language="C#" MasterPageFile="~/Masters/Student.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Student.Home" %>
<%@ Register Src="~/Pages/Student/StudentCalendar.ascx" TagPrefix="uc" TagName="StudentCalendar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField ID="hfMyId" runat="server" ClientIDMode="Static" />
    <div class="page-layout-split">

        <div class="layout-main-70">
            
            <div class="ec-hero mb-4">
                <asp:Image ID="imgStudent" runat="server" ImageUrl="~/Images/profile_m.png" CssClass="ec-hero-avatar" />
                <div class="d-flex flex-column justify-content-center">
                    <h2 class="ec-hero-text m-0 mb-2">Welcome back, <asp:Label ID="lblStudentName" runat="server">Student</asp:Label>!</h2>
                    <p class="mb-3">Ready to continue your learning journey? Pick up right where you left off.</p>
                    <div>
                        <a href="Study.aspx" class="btn btn-primary rounded-pill px-4 py-2 fw-bold shadow-sm" style="display: inline-flex; align-items: center;">
                            <i class="bi bi-rocket-takeoff me-2"></i>Explore Courses
                        </a>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card mb-4">
                <div class="ec-glass-card-header border-0 pb-0 mb-3 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0"><i class="bi bi-activity text-primary me-2"></i>Recent Activity</h5>
                    <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseRecentActivity">
                        <i class="bi bi-dash"></i>
                    </button>
                </div>
     
                <div class="collapse show" id="collapseRecentActivity">
                    <div class="row g-3">
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
                                <div class="text-center text-muted p-3">No recent activity. Start learning!</div>
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </div>
            </div>

            <div class="layout-grid-half mb-4" style="align-items: start;">
                
                <div class="ec-glass-card w-100 align-self-start">
                    <div class="ec-glass-card-header border-0 pb-0 d-flex justify-content-between align-items-center mb-3">
                        <h5 class="fw-bold m-0"><i class="bi bi-bookmark-star-fill text-warning me-2"></i>Jump Back In</h5>
                        <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseJumpBackIn">
                            <i class="bi bi-dash"></i>
                        </button>
                    </div>
            
                    <div class="collapse show" id="collapseJumpBackIn">
                        <div class="ec-glass-card-body pt-0 d-flex flex-column" style="height: 300px;">
                            
                            <div class="m-auto w-100">
                                <asp:Repeater ID="rptRecentCourses" runat="server">
                                    <ItemTemplate>
                                        <div class="ec-course-box p-3 border rounded-4 shadow-sm d-flex flex-column align-items-center justify-content-center gap-3 bg-white text-center">
                                            
                                            <div>
                                                <i class="bi bi-play-circle-fill text-primary mb-2 d-block" style="font-size: 2.8rem;"></i>
                                                <h5 class="fw-bold text-main m-0 mb-1"><%# Eval("CourseTitle") %></h5>
                                                <p class="text-muted small m-0">Last Accessed: <%# Eval("LastAccess", "{0:MMM dd}") %></p>
                                            </div>

                                            <div class="w-100 mt-2">
                                                <a href='LessonView.aspx?resourceId=<%# Eval("TargetResource") %>' 
                                                   class='<%# Convert.ToBoolean(Eval("IsCompleted")) ? "btn btn-success rounded-pill fw-bold shadow-sm w-100 py-2" : "btn btn-primary rounded-pill fw-bold shadow-sm w-100 py-2" %>'>
                                                    <%# Convert.ToBoolean(Eval("IsCompleted")) ? "Review Course" : "Resume Lesson" %>
                                                </a>
                                            </div>

                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                    
                                <asp:PlaceHolder ID="phNoCourses" runat="server" Visible="false">
                                    <div class="text-center text-muted p-4 m-auto">You haven't started any courses yet.</div>
                                </asp:PlaceHolder>
                            </div>

                        </div>
                    </div>
                </div>

                <div class="ec-glass-card w-100 align-self-start">
                    <div class="ec-glass-card-header border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                        <h5 class="fw-bold m-0"><i class="bi bi-chat-dots-fill text-info me-2"></i>Recent Messages</h5>
                        <div class="d-flex align-items-center gap-3">
                            <asp:HyperLink ID="lnkViewAllChat" runat="server" Text="View All &rarr;" NavigateUrl="~/Pages/Shared/Chat.aspx" CssClass="ec-link-primary m-0 small fw-bold" />
                            <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseMessages">
                                <i class="bi bi-dash"></i>
                            </button>
                        </div>
                    </div>

                    <div class="collapse show" id="collapseMessages">
                        <div class="ec-glass-card-body p-0" style="height: 300px;">
                            <asp:UpdatePanel ID="upRecentMessages" runat="server" UpdateMode="Conditional" style="height: 100%;">
                                <ContentTemplate>
                                    <asp:Button ID="btnSignalRUpdate" runat="server" ClientIDMode="Static" OnClick="btnSignalRUpdate_Click" style="display:none;" />
                                    
                                    <div class="d-flex flex-column h-100 w-100">
                                        <div class="w-100 flex-grow-1 overflow-auto" style="max-height: 290px;">
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
        </div>

        <div class="layout-sidebar-30">
            <div class="ec-glass-card mb-4">
                 <div class="ec-glass-card-header border-0 pb-0 mb-2">
                    <h5 class="fw-bold m-0">Calendar</h5>
                </div>
                <uc:StudentCalendar runat="server" ID="ucCalendar" />
            </div>

            <div class="ec-glass-card notifications mb-4">
                <asp:UpdatePanel ID="upNotifications" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="ec-glass-card-header border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                            <div class="d-flex align-items-center gap-2">
                                <h5 class="fw-bold m-0"><i class="bi bi-bell-fill text-danger me-2"></i>Notifications</h5>
                                <asp:Label ID="lblNotificationCount" runat="server" CssClass="badge bg-danger rounded-pill"></asp:Label>
                            </div>
                            <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseNotifications">
                                <i class="bi bi-dash"></i>
                            </button>
                        </div>

                        <div class="collapse show" id="collapseNotifications">
                            <div class="ec-glass-card-body overflow-auto" style="max-height: 300px;">
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
                            <div class="ec-card-actions mt-auto pt-3 border-top text-center">
                                <asp:HyperLink ID="lnkAllNotifications" runat="server" Text="View All Notifications &rarr;" CssClass="ec-link-primary m-0 small fw-bold" NavigateUrl="~/Pages/Student/Notifications.aspx" />
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>

    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            var collapseElements = document.querySelectorAll('.collapse');

            collapseElements.forEach(function (collapseEl) {
                var id = collapseEl.id;
                var btnIcon = document.querySelector('[data-bs-target="#' + id + '"] i');

                var isMinimized = localStorage.getItem(id + "_state") === "minimized";
                if (isMinimized) {
                    collapseEl.classList.remove('show');
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-dash');
                        btnIcon.classList.add('bi-plus');
                    }
                }

                collapseEl.addEventListener('shown.bs.collapse', function () {
                    localStorage.setItem(id + "_state", "maximized");
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-plus');
                        btnIcon.classList.add('bi-dash');
                    }
                });

                collapseEl.addEventListener('hidden.bs.collapse', function () {
                    localStorage.setItem(id + "_state", "minimized");
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-dash');
                        btnIcon.classList.add('bi-plus');
                    }
                });
            });
        });
    </script>
</asp:Content>