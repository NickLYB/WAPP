<%@ Page Title="Admin Home" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Admin.Home" %>
<%@ Register Src="~/Pages/Tutor/Calendar.ascx" TagPrefix="uc" TagName="Calendar" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <script type="text/javascript">
        // LocalStorage Memory Logic for Minimize/Expand
        // Using pageLoad() ensures this keeps working perfectly even after UpdatePanel partial reloads!
        function pageLoad() {
            var collapseElements = document.querySelectorAll('.collapse');

            collapseElements.forEach(function (collapseEl) {
                var id = collapseEl.id;
                if (!id) return; // Skip if no ID

                var storageKey = "admin_" + id + "_state";
                var btnIcon = document.querySelector('[data-bs-target="#' + id + '"] i');
                var isMinimized = localStorage.getItem(storageKey) === "minimized";

                // 1. Apply saved state immediately on load/reload
                if (isMinimized) {
                    collapseEl.classList.remove('show');
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-dash-lg');
                        btnIcon.classList.add('bi-plus-lg');
                    }
                } else {
                    collapseEl.classList.add('show');
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-plus-lg');
                        btnIcon.classList.add('bi-dash-lg');
                    }
                }

                // 2. Attach listeners using jQuery to easily prevent duplicates (.off().on())
                $(collapseEl).off('shown.bs.collapse').on('shown.bs.collapse', function () {
                    localStorage.setItem(storageKey, "maximized");
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-plus-lg');
                        btnIcon.classList.add('bi-dash-lg');
                    }
                });

                $(collapseEl).off('hidden.bs.collapse').on('hidden.bs.collapse', function () {
                    localStorage.setItem(storageKey, "minimized");
                    if (btnIcon) {
                        btnIcon.classList.remove('bi-dash-lg');
                        btnIcon.classList.add('bi-plus-lg');
                    }
                });
            });
        }
    </script>
    
    <style>
        /* Base Button Styles */
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
    
    <%-- SignalR Hidden Field for Current User ID --%>
    <asp:HiddenField ID="hfMyId" runat="server" ClientIDMode="Static" />

    <div class="page-layout-split">
        
        <div class="layout-main-70">
            
            <%-- WELCOME HERO (Not Collapsible) --%>
            <div class="ec-hero mb-4">
                <asp:Image ID="imgAdmin" runat="server" ImageUrl="~/Images/profile_f.png" CssClass="ec-hero-avatar" />
                <h2 class="ec-hero-text m-0 fw-bold">Welcome back, <asp:Label ID="lblAdminName" runat="server"></asp:Label>!</h2>
            </div>

            <%-- MERGED USER ACTIVITY BLOCK --%>
            <asp:UpdatePanel ID="upStats" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="ec-glass-card p-0 mb-4">
                        <div class="ec-glass-card-header p-4 border-0 pb-2 d-flex justify-content-between align-items-center">
                            <h5 class="fw-bold m-0"><i class="bi bi-activity text-primary me-2"></i>User Activity</h5>
                            <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseUserActivity">
                                <i class="bi bi-dash-lg"></i>
                            </button>
                        </div>
                        <div class="collapse show" id="collapseUserActivity">
                            <div class="ec-glass-card-body p-4 pt-0">
                                <div class="row g-4 border-top pt-3">
                                    <%-- Active Sessions --%>
                                    <div class="col-md-6 d-flex flex-column justify-content-center align-items-center border-end">
                                        <h6 class="fw-bold text-muted text-uppercase mb-3 text-center">Active Sessions</h6>
                                        <asp:Label ID="lblActiveSessions" runat="server" CssClass="ec-stat-giant text-success fw-bold" style="font-size: 4.5rem; line-height: 1;">0</asp:Label>
                                        <p class="text-muted small mt-2 mb-0 fw-bold text-uppercase">Online (Last 1 Hour)</p>
                                    </div>
                                    <%-- Recent Logins --%>
                                    <div class="col-md-6">
                                        <h6 class="fw-bold text-muted text-uppercase mb-2 px-2">Recent Logins</h6>
                                        <div style="height: 140px; overflow-y: auto;" class="px-2">
                                            <ul class="list-group list-group-flush m-0">
                                                <asp:Repeater ID="rptRecentLogins" runat="server">
                                                    <ItemTemplate>
                                                        <li class="list-group-item d-flex justify-content-between align-items-center border-0 px-0 py-2" style="border-bottom: 1px solid #f1f5f9 !important;">
                                                            <div class="d-flex align-items-center">
                                                                <i class="bi bi-person-circle text-primary me-3 fs-4"></i>
                                                                <div style="line-height: 1.2;">
                                                                    <span class="m-0 text-dark small fw-bold"><%# Eval("UserName") %></span><br />
                                                                    <span class="text-muted" style="font-size: 0.7rem;"><%# Eval("RoleName") %></span>
                                                                </div>
                                                            </div>
                                                            <span class="text-muted fw-bold" style="font-size: 0.75rem;"><%# FormatTimeAgo(Eval("LoginTime")) %></span>
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                            <asp:Label ID="lblNoRecentLogins" runat="server" Visible="false" CssClass="text-muted small d-block text-center py-4">
                                                No recent logins.
                                            </asp:Label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <%-- RECENT USER REGISTRATION --%>
            <div class="ec-glass-card p-0 mb-4">
                <div class="ec-glass-card-header p-4 border-0 pb-2 d-flex justify-content-between align-items-center">
                    <h5 class="m-0 fw-bold">Recent User Registration</h5>
                    <div class="d-flex align-items-center gap-3">
                        <asp:HyperLink ID="hlViewAllUsers" runat="server" NavigateUrl="~/Pages/Admin/ManageUsers.aspx" CssClass="text-primary text-decoration-none fw-bold small">View All &rarr;</asp:HyperLink>
                        <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseRecentUsers">
                            <i class="bi bi-dash-lg"></i>
                        </button>
                    </div>
                </div>
                <div class="collapse show" id="collapseRecentUsers">
                    <div class="ec-glass-card-body p-4 pt-0">
                        <asp:GridView ID="gvRecentUsers" runat="server" AutoGenerateColumns="False" 
                            CssClass="table table-hover ec-table-custom align-middle mb-0" GridLines="None" Width="100%">
                            <Columns>
                                <asp:BoundField DataField="ID" HeaderText="ID" ItemStyle-Width="80px" HeaderStyle-CssClass="text-muted small text-uppercase" ItemStyle-CssClass="text-muted fw-bold" />
                                <asp:BoundField DataField="Name" HeaderText="Name" HeaderStyle-CssClass="text-muted small text-uppercase" ItemStyle-CssClass="text-dark fw-bold" />
                                <asp:BoundField DataField="Role" HeaderText="Role" ItemStyle-Width="120px" HeaderStyle-CssClass="text-muted small text-uppercase" ItemStyle-CssClass="text-muted" />
                                <asp:BoundField DataField="Email" HeaderText="Email" HeaderStyle-CssClass="text-muted small text-uppercase" ItemStyle-CssClass="text-muted" />
                            </Columns>
                        </asp:GridView>
                        
                        <asp:Label ID="lblUserEmpty" runat="server" Visible="false" CssClass="ec-empty-state d-block mt-3">
                            No recent registrations found.
                        </asp:Label>
                    </div>
                </div>
            </div>

            <%-- RECENT MESSAGES --%>
            <div class="ec-glass-card p-0 mb-4">
                <div class="ec-glass-card-header p-4 border-0 pb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0"><i class="bi bi-chat-dots-fill text-info me-2"></i>Recent Messages</h5>
                    <div class="d-flex align-items-center gap-3">
                        <asp:HyperLink ID="HyperLink1" runat="server" Text="View All Chat &rarr;" NavigateUrl="~/Pages/Shared/Chat.aspx" CssClass="text-primary text-decoration-none fw-bold small" />
                        <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseRecentMessages">
                            <i class="bi bi-dash-lg"></i>
                        </button>
                    </div>
                </div>
    
                <div class="collapse show" id="collapseRecentMessages">
                    <asp:UpdatePanel ID="upRecentMessages" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="ec-glass-card-body p-4 pt-0">
                                <asp:Repeater ID="rptUnreadMessages" runat="server">
                                    <ItemTemplate>
                                        <a href='<%# ResolveUrl("~/Pages/Shared/Chat.aspx") %>' class="d-flex align-items-center py-3 border-bottom text-decoration-none" style="transition: background-color 0.2s ease;">
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

                                <asp:Label ID="lblNoUnreadMessages" runat="server" Visible="false" CssClass="text-muted d-block text-center mt-4 mb-3 py-3 small">
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
            
            <%-- CALENDAR (Not Collapsible) --%>
            <div class="ec-glass-card p-0 mb-4">
                <div class="ec-glass-card-header p-4 border-0 pb-0 mb-2">
                    <h5 class="fw-bold m-0">Calendar</h5>
                </div>
                <div class="p-3 pt-0">
                    <uc:Calendar runat="server" ID="ucCalendar" />
                </div>
            </div>

            <%-- QUICK ACTIONS --%>
            <div class="ec-glass-card p-0 mb-4">
                <div class="ec-glass-card-header p-4 border-0 pb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Quick Actions</h5>
                    <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseQuickActions">
                        <i class="bi bi-dash-lg"></i>
                    </button>
                </div>
                <div class="collapse show" id="collapseQuickActions">
                    <div class="ec-glass-card-body p-4 pt-0">
                        <p class="text-muted small mt-0 mb-3">Manage core system data.</p>
                        <div style="display: flex; flex-direction: column; gap: 10px;">
                            <asp:Button ID="btnAddUser" runat="server" Text="+ Add New User" 
                                CssClass="btn btn-dark btn-pill w-100 fw-bold py-2" PostBackUrl="~/Pages/Admin/ManageUsers.aspx?action=add" />
                            <asp:Button ID="btnAddAnnouncement" runat="server" Text="+ Add Announcement" 
                                CssClass="btn btn-light border btn-pill w-100 fw-bold py-2" PostBackUrl="~/Pages/Admin/ManageAnnouncements.aspx?action=compose" />
                        </div>
                    </div>
                </div>
            </div>

            <%-- ALERT LOGS --%>
            <div class="ec-glass-card p-0 mb-4">
                <div class="ec-glass-card-header p-4 border-0 pb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Alert Logs</h5>
                    <div class="d-flex align-items-center gap-3">
                        <asp:HyperLink ID="hlViewLogs" runat="server" NavigateUrl="~/Pages/Admin/ManageSystemLogs.aspx" CssClass="text-primary text-decoration-none fw-bold small">View All &rarr;</asp:HyperLink>
                        <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseAlertLogs">
                            <i class="bi bi-dash-lg"></i>
                        </button>
                    </div>
                </div>
                <div class="collapse show" id="collapseAlertLogs">
                    <div class="ec-glass-card-body p-4 pt-0">
                        <ul class="list-unstyled m-0 mt-3">
                            <asp:Repeater ID="rptSystemLogs" runat="server">
                                <ItemTemplate>
                                    <li class="ec-feed-item py-2 border-bottom border-light">
                                        <span class="text-dark small">
                                            <i class="status-dot dot-pending me-2"></i> <%# Eval("LogText") %>
                                        </span>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                        <asp:Label ID="lblLogsEmpty" runat="server" Visible="false" CssClass="ec-empty-state d-block py-3">
                            No recent alert logs.
                        </asp:Label>
                    </div>
                </div>
            </div>

            <%-- ANNOUNCEMENT QUEUE --%>
            <div class="ec-glass-card p-0 mb-4">
                <div class="ec-glass-card-header p-4 border-0 pb-2 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold m-0">Announcement Queue</h5>
                    <div class="d-flex align-items-center gap-3">
                        <asp:HyperLink ID="hlViewAnnouncements" runat="server" NavigateUrl="~/Pages/Admin/ManageAnnouncements.aspx" CssClass="text-primary text-decoration-none fw-bold small">View All &rarr;</asp:HyperLink>
                        <button class="btn btn-sm btn-light rounded-circle shadow-sm widget-toggle-btn" type="button" data-bs-toggle="collapse" data-bs-target="#collapseAnnouncementQueue">
                            <i class="bi bi-dash-lg"></i>
                        </button>
                    </div>
                </div>
                <div class="collapse show" id="collapseAnnouncementQueue">
                    <div class="ec-glass-card-body p-4 pt-0">
                        <ul class="list-unstyled m-0 mt-3">
                            <asp:Repeater ID="rptAnnouncementQueue" runat="server">
                                <ItemTemplate>
                                    <li class="ec-feed-item py-2 border-bottom border-light">
                                        <span class="text-dark small">
                                            <i class="status-dot dot-scheduled me-2"></i> <%# Eval("QueueText") %>
                                        </span>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                        <asp:Label ID="lblQueueEmpty" runat="server" Visible="false" CssClass="ec-empty-state d-block py-3">
                            Queue is empty.
                        </asp:Label>
                    </div>
                </div>
            </div>

        </div>
    </div>
    
</asp:Content>