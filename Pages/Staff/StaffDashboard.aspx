<%@ Page Title="Staff Dashboard" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="StaffDashboard.aspx.cs" Inherits="WAPP.Pages.Staff.StaffDashboard" %>
<%@ Register Src="~/Pages/Tutor/Calendar.ascx" TagPrefix="uc" TagName="Calendar" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .btn-outline-danger-custom {
            border: 1px solid #dc3545;
            color: #dc3545;
            background-color: transparent;
            font-weight: bold;
            padding: 8px 30px;
            border-radius: 30px;
            transition: all 0.2s ease;
        }
        .btn-outline-danger-custom:hover {
            background-color: #dc3545;
            color: white;
        }
        
        /* Collapse Button Styles */
        .widget-toggle-btn {
            width: 32px;
            height: 32px;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 0;
            border: 1px solid #dee2e6;
            background-color: #f8f9fa;
            color: #495057;
            transition: all 0.2s ease;
        }
        .widget-toggle-btn:hover {
            background-color: #e9ecef;
            color: #212529;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField ID="hfMyId" runat="server" ClientIDMode="Static" />
    <div class="page-layout-split">
        
        <div class="layout-main-70">
            <div class="ec-hero mb-4">
                <asp:Image ID="imgStaff" runat="server" ImageUrl="~/Images/profile_f.png" CssClass="ec-hero-avatar" />
                <h2 class="ec-hero-text">Welcome back, <asp:Label ID="lblStaffName" runat="server"></asp:Label>!</h2>
            </div>

            <div class="ec-glass-card p-0 mb-4">
                <div class="ec-glass-card-header p-4 border-0 pb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Course & Resource Management</h5>
                    <button class="btn rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseManagement">
                        <i class="bi bi-dash-lg"></i>
                    </button>
                </div>
                
                <div class="collapse show" id="collapseManagement">
                    <div class="ec-glass-card-body px-4 pb-4 pt-2">
                        <div class="row g-4">
                            <div class="col-md-6 pe-md-4 border-end">
                                <h6 class="fw-bold text-dark mb-1">Manage Courses</h6>
                                <p class="text-muted small mb-3">Create, manage, and review system courses.</p>
                                <div class="ec-card-actions">
                                    <asp:Button ID="btnAddCourse" runat="server" Text="+ Add New Course" CssClass="btn-main btn-pill w-100 mb-2" OnClick="btnAddCourse_Click" />
                                    <div class="text-center mt-2">
                                        <asp:HyperLink ID="hlViewCourses" runat="server" NavigateUrl="~/Pages/Staff/CourseManagement.aspx" CssClass="ec-link-primary small fw-bold">View All Courses &rarr;</asp:HyperLink>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="col-md-6 ps-md-4">
                                <h6 class="fw-bold text-dark mb-1">Learning Resources</h6>
                                <p class="text-muted small mb-3">Upload and manage learning materials.</p>
                                <div class="ec-card-actions">
                                    <asp:Button ID="btnAddResource" runat="server" Text="+ Add New Resource" CssClass="btn-main btn-pill w-100 mb-2" OnClick="btnAddResource_Click" />
                                    <div class="text-center mt-2">
                                        <asp:HyperLink ID="hlViewResources" runat="server" NavigateUrl="~/Pages/Staff/LearningResourceManagement.aspx" CssClass="ec-link-primary small fw-bold">View All Resources &rarr;</asp:HyperLink>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card p-0 overflow-hidden mb-4 bg-white">
                <div class="ec-glass-card-header px-4 pt-4 border-0 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold text-dark m-0" style="color: #2c3e50 !important;">Pending Tutor Verifications</h5>
                    <button class="btn rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseTutors">
                        <i class="bi bi-dash-lg"></i>
                    </button>
                </div>
                <div class="collapse show" id="collapseTutors">
                    <div class="px-4 pb-4 text-center mt-2">
                        <h1 class="text-danger fw-bold mb-1" style="font-size: 5rem; line-height: 1;">
                            <asp:Label ID="lblPendingCount" runat="server" Text="0"></asp:Label>
                        </h1>
                        <p class="text-muted mb-4 fs-5">New applications waiting for review.</p>
                        <asp:Button ID="btnReviewTutors" runat="server" Text="Review Now" CssClass="btn-outline-danger-custom" OnClick="btnReviewTutors_Click" />
                    </div>
                </div>
            </div>

            <div class="ec-glass-card p-0 overflow-hidden mb-4">
                <div class="ec-glass-card-header px-4 pt-4 border-0 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">My Recent Announcements</h5>
                    <div class="d-flex align-items-center gap-3">
                        <asp:HyperLink ID="hlViewAllAnnouncements" runat="server" NavigateUrl="~/Pages/Staff/AnnouncementManagement.aspx" CssClass="ec-link-primary m-0 small fw-bold">View All &rarr;</asp:HyperLink>
                        <button class="btn rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseAnnouncements">
                            <i class="bi bi-dash-lg"></i>
                        </button>
                    </div>
                </div>
                <div class="collapse show" id="collapseAnnouncements">
                    <div class="px-4 pb-4">
                        <asp:UpdatePanel ID="upDashboardAnnouncements" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <ul class="list-unstyled m-0">
                                    <asp:Repeater ID="rptRecentAnnouncements" runat="server">
                                        <ItemTemplate>
                                            <li class="ec-feed-item py-3">
                                                <div class="w-100">
                                                    <div class="d-flex justify-content-between align-items-center mb-1">
                                                        <span class="text-dark fw-bold fs-6">
                                                            <i class="bi bi-megaphone-fill text-primary me-2"></i>
                                                            <%# Eval("TargetRole") %>: <%# Eval("title") %>
                                                        </span>
                                                        <small class="text-muted"><%# Convert.ToDateTime(Eval("created_at")).ToString("dd MMM yyyy, h:mm tt") %></small>
                                                    </div>
                                                    <div class="text-muted small ms-4 ec-desc-truncate" style="max-width: 100%;"><%# Eval("message") %></div>
                                                </div>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                                <asp:Label ID="lblNoAnnouncements" runat="server" Visible="false" CssClass="ec-empty-state d-block py-3">No recent announcements created by you.</asp:Label>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
            
            <div class="ec-glass-card p-0 overflow-hidden mt-4">
                <div class="ec-glass-card-header px-4 pt-4 border-0 pb-2 mb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Recent Messages</h5>
                    <div class="d-flex align-items-center gap-3">
                        <asp:HyperLink ID="HyperLink1" runat="server" Text="View All Chat &rarr;" NavigateUrl="~/Pages/Shared/Chat.aspx" CssClass="ec-link-primary m-0 small fw-bold" />
                        <button class="btn rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseMessages">
                            <i class="bi bi-dash-lg"></i>
                        </button>
                    </div>
                </div>
    
                <div class="collapse show" id="collapseMessages">
                    <asp:UpdatePanel ID="upRecentMessages" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="ec-glass-card-body p-0">
                                <asp:Repeater ID="rptUnreadMessages" runat="server">
                                    <ItemTemplate>
                                        <a href='<%# ResolveUrl("~/Pages/Shared/Chat.aspx") %>' class="d-flex align-items-center p-3 border-bottom text-decoration-none" style="transition: background-color 0.2s ease;">
                                
                                            <div class="me-3 ps-2">
                                                <i class="bi bi-person-circle text-primary" style="font-size: 2.2rem;"></i>
                                            </div>
                                
                                            <div class="flex-grow-1 overflow-hidden pe-2">
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

                                <asp:Label ID="lblNoUnreadMessages" runat="server" Visible="false" CssClass="text-muted d-block text-center mt-4 mb-4 pb-2 small">
                                    <i class="bi bi-check2-all d-block mb-2" style="font-size: 2rem; color: #198754;"></i>
                                    All caught up!<br />No unread messages.
                                </asp:Label>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>

        <div class="layout-sidebar-30">
            
            <div class="ec-glass-card p-0">
                <div class="ec-glass-card-header p-4 border-0 pb-0 mb-2">
                    <h5 class="fw-bold m-0">Calendar</h5>
                </div>
                <div class="p-3 pt-0">
                    <uc:Calendar runat="server" ID="ucCalendar" />
                </div>
            </div>
            
            <div class="ec-glass-card p-0 overflow-hidden mt-4">
                <div class="ec-glass-card-header px-4 pt-4 border-0 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Notifications</h5>
                    <div class="d-flex align-items-center gap-3">
                        <asp:HyperLink ID="hlViewNotifications" runat="server" NavigateUrl="~/Pages/Staff/Notifications.aspx" CssClass="ec-link-primary m-0 small fw-bold">View All &rarr;</asp:HyperLink>
                        <button class="btn rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseNotifications">
                            <i class="bi bi-dash-lg"></i>
                        </button>
                    </div>
                </div>
                <div class="collapse show" id="collapseNotifications">
                    <div class="px-4 pb-4 pt-2">
                        <asp:UpdatePanel ID="upRecentNotifications" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <ul class="list-unstyled m-0">
                                    <asp:Repeater ID="rptRecentNotifications" runat="server">
                                        <ItemTemplate>
                                            <li class="ec-feed-item py-3 border-bottom">
                                                <div class="d-flex align-items-start">
                                                    <i class="bi bi-bell-fill text-warning me-3 mt-1 fs-5"></i>
                                                    <div class="w-100">
                                                        <div class="text-dark fw-bold mb-1" style="font-size: 0.95rem; word-break: break-word;">
                                                            <%# Eval("content") %>
                                                        </div>
                                                        
                                                        <div class="text-secondary small mb-2 ec-desc-truncate" style="max-width: 100%;">
                                                            <%# Eval("announcement_message") %>
                                                        </div>
                                                        
                                                        <div class="text-muted" style="font-size: 0.75rem;">
                                                            <%# Convert.ToDateTime(Eval("created_at")).ToString("dd MMM yyyy, h:mm tt") %>
                                                        </div>
                                                    </div>
                                                </div>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                                <asp:Label ID="lblNoRecentNotifications" runat="server" Visible="false" CssClass="text-muted d-block text-center mt-3 mb-2 small">
                                    No new notifications.
                                </asp:Label>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card p-0 mt-4">
                <div class="ec-glass-card-header p-4 border-0 pb-0 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">System Status</h5>
                    <button class="btn rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseSystemStatus">
                        <i class="bi bi-dash-lg"></i>
                    </button>
                </div>
                <div class="collapse show" id="collapseSystemStatus">
                    <div class="p-4 pt-3">
                        <ul class="list-unstyled m-0">
                            <li class="ec-feed-item pb-3 border-bottom">
                                <span class="d-block mb-1">
                                    <strong>Total Students:</strong> 
                                    <asp:Label ID="lblTotalStudents" runat="server" CssClass="ec-highlight-num text-primary fs-5 ms-1">0</asp:Label>
                                </span>
                                <asp:HyperLink ID="hlGoToStudents" runat="server" NavigateUrl="~/Pages/Staff/UserManagement.aspx?role=4" CssClass="btn-sub small fw-bold text-decoration-none">View Students &rarr;</asp:HyperLink>
                            </li>
                            <li class="ec-feed-item py-3 border-bottom">
                                <span class="d-block mb-1">
                                    <strong>Total Tutors:</strong> 
                                    <asp:Label ID="lblTotalTutors" runat="server" CssClass="ec-highlight-num text-primary fs-5 ms-1">0</asp:Label>
                                </span>
                                <asp:HyperLink ID="hlGoToTutors" runat="server" NavigateUrl="~/Pages/Staff/UserManagement.aspx?role=3" CssClass="btn-sub small fw-bold text-decoration-none">View Tutors &rarr;</asp:HyperLink>
                            </li>
                            <li class="ec-feed-item py-3 border-bottom">
                                <span class="d-block mb-1">
                                    <strong>Active Courses:</strong> 
                                    <asp:Label ID="lblActiveCourses" runat="server" CssClass="ec-highlight-num text-primary fs-5 ms-1">0</asp:Label>
                                </span>
                                <asp:HyperLink ID="hlGoToCoursesMain" runat="server" NavigateUrl="~/Pages/Staff/CourseManagement.aspx?status=PUBLISHED" CssClass="btn-sub small fw-bold text-decoration-none">View Courses &rarr;</asp:HyperLink>
                            </li>
                            <li class="d-flex justify-content-between align-items-center pt-3">
                                <strong>Server Load:</strong> <span class="text-success fw-bold"><i class="bi bi-check-circle-fill me-1"></i>Normal</span>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>

        </div>
    </div>
    
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            var collapseElements = document.querySelectorAll('.collapse');

            collapseElements.forEach(function (collapseEl) {
                var id = collapseEl.id;
                var btnIcon = document.querySelector('[data-bs-target="#' + id + '"] i');
                
                // Prefix with staff_ to avoid conflict with tutor dashboards
                var isMinimized = localStorage.getItem("staff_" + id + "_state") === "minimized";

                if (isMinimized) {
                    collapseEl.classList.remove('show');
                    
                    // Add collapsed class to button so Bootstrap recognizes state
                    var btn = document.querySelector('[data-bs-target="#' + id + '"]');
                    if (btn) btn.classList.add('collapsed');

                    if (btnIcon) {
                        btnIcon.classList.remove('bi-dash-lg');
                        btnIcon.classList.add('bi-plus-lg');
                    }
                } else {
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-plus-lg');
                        btnIcon.classList.add('bi-dash-lg');
                    }
                }

                collapseEl.addEventListener('shown.bs.collapse', function () {
                    localStorage.setItem("staff_" + id + "_state", "maximized");
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-plus-lg');
                        btnIcon.classList.add('bi-dash-lg');
                    }
                });

                collapseEl.addEventListener('hidden.bs.collapse', function () {
                    localStorage.setItem("staff_" + id + "_state", "minimized");
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-dash-lg');
                        btnIcon.classList.add('bi-plus-lg');
                    }
                });
            });
        });
    </script>
</asp:Content>