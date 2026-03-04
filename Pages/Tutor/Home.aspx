<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Tutor.Home" %>
<%@ Register Src="~/Pages/Tutor/Calendar.ascx" TagPrefix="uc" TagName="Calendar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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
        <div class="layout-grid-half">
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
        <div class="ec-glass-card">
            <div class="ec-glass-card-header border-0 m-0 p-0 d-flex justify-content-between align-items-center">
                     <h5 class="fw-bold m-0">Recent Messages</h5>
                     <asp:HyperLink ID="HyperLink1" runat="server" Text="View All Chat &rarr;" NavigateUrl="~/Pages/Tutor/Chat.aspx" CssClass="ec-link-primary m-0" />
                 </div>
        </div>
    </div>

    <div class="layout-sidebar-30">
        <div class="ec-glass-card">
            <div class="ec-glass-card-header border-0 pb-0 mb-2">
                <h5 class="fw-bold m-0">Calendar</h5>
            </div>
            <uc:Calendar runat="server" ID="ucCalendar" />
        </div>
        <div class="ec-glass-card notifications">
            <h5>Notifications</h5>
            </div>
    </div>
</div>
</asp:Content>
