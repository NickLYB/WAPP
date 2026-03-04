<%@ Page Title="Staff Dashboard" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="StaffDashboard.aspx.cs" Inherits="WAPP.Pages.Staff.StaffDashboard" %>

<%@ Register Src="~/Pages/Tutor/Calendar.ascx" TagPrefix="uc" TagName="Calendar" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-layout-split">
        
        <div class="layout-main-70">
            <div class="ec-hero">
                <asp:Image ID="imgStaff" runat="server" ImageUrl="~/Images/profile_f.png" CssClass="ec-hero-avatar" />
                <h2 class="ec-hero-text">Welcome back, <asp:Label ID="lblStaffName" runat="server"></asp:Label>!</h2>
            </div>

            <div class="layout-grid-half">
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5>Manage Courses</h5>
                    </div>
                    <div class="ec-glass-card-body">
                        <p class="text-muted">Create, manage, and review system courses.</p>
                        <div class="ec-card-actions">
                            <asp:Button ID="btnAddCourse" runat="server" Text="+ Add New Course" CssClass="btn-main btn-pill w-100" OnClick="btnAddCourse_Click" />
                            <asp:HyperLink ID="hlViewCourses" runat="server" NavigateUrl="~/Pages/Staff/CourseManagement.aspx" CssClass="ec-link-primary">View All Courses &rarr;</asp:HyperLink>
                        </div>
                    </div>
                </div>

                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5>Learning Resources</h5>
                    </div>
                    <div class="ec-glass-card-body">
                        <p class="text-muted">Upload and manage learning materials.</p>
                        <div class="ec-card-actions">
                            <asp:Button ID="btnAddResource" runat="server" Text="+ Add New Resource" CssClass="btn-main btn-pill w-100" OnClick="btnAddResource_Click" />
                            <asp:HyperLink ID="hlViewResources" runat="server" NavigateUrl="~/Pages/Staff/LearningResourceManagement.aspx" CssClass="ec-link-primary">View All Resources &rarr;</asp:HyperLink>
                        </div>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card p-0 overflow-hidden">
                <div class="ec-glass-card-header px-4 pt-4 border-0">
                    <h5>Recent Feedback Tickets</h5>
                    <asp:HyperLink ID="hlViewAllFeedback" runat="server" NavigateUrl="~/Pages/Staff/FeedbackManagement.aspx" CssClass="ec-link-primary m-0">View All &rarr;</asp:HyperLink>
                </div>
                <div class="px-4 pb-4">
                    <asp:GridView ID="gvFeedback" runat="server" CssClass="table table-striped align-middle mb-0" AutoGenerateColumns="False" GridLines="None">
                        </asp:GridView>
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
            
            <div class="ec-glass-card">
                <div class="ec-glass-card-header border-0 pb-0">
                    <h5>Pending Tutors</h5>
                </div>
                <p class="text-muted small mt-2 mb-3">New applications waiting for review.</p>
                <div class="d-flex justify-content-between align-items-center">
                    <asp:Label ID="lblPendingTutors" runat="server" CssClass="ec-stat-giant">0</asp:Label>
                    <asp:HyperLink ID="hlReviewNow" runat="server" NavigateUrl="~/Pages/Staff/TutorApplication.aspx?status=PENDING" CssClass="btn-sub">Review Now</asp:HyperLink>
                </div>
            </div>

            <div class="ec-glass-card">
                <div class="ec-glass-card-header">
                    <h5>System Status</h5>
                </div>
                <ul class="list-unstyled m-0 mt-3">
                    <li class="ec-feed-item">
                        <span>
                            <strong>Total Students:</strong> 
                            <asp:Label ID="lblTotalStudents" runat="server" CssClass="ec-highlight-num text-primary">0</asp:Label>
                        </span>
                        <asp:HyperLink ID="hlGoToStudents" runat="server" NavigateUrl="~/Pages/Staff/UserManagement.aspx?role=4" CssClass="btn-sub">View</asp:HyperLink>
                    </li>
                    <li class="ec-feed-item">
                        <span>
                            <strong>Total Tutors:</strong> 
                            <asp:Label ID="lblTotalTutors" runat="server" CssClass="ec-highlight-num text-primary">0</asp:Label>
                        </span>
                        <asp:HyperLink ID="hlGoToTutors" runat="server" NavigateUrl="~/Pages/Staff/UserManagement.aspx?role=3" CssClass="btn-sub">View</asp:HyperLink>
                    </li>
                    <li class="ec-feed-item">
                        <span>
                            <strong>Active Courses:</strong> 
                            <asp:Label ID="lblActiveCourses" runat="server" CssClass="ec-highlight-num text-primary">0</asp:Label>
                        </span>
                        <asp:HyperLink ID="hlGoToCoursesMain" runat="server" NavigateUrl="~/Pages/Staff/CourseManagement.aspx" CssClass="btn-sub">View</asp:HyperLink>
                    </li>
                    <li class="d-flex justify-content-between align-items-center pt-2">
                        <strong>Server Load:</strong> <span class="text-success fw-bold">Normal</span>
                    </li>
                </ul>
            </div>
        </div>
    </div>
</asp:Content>