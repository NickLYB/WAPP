<%@ Page Title="Student Dashboard" Language="C#" MasterPageFile="~/Masters/Student.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Student.Home" %>
<%@ Register Src="~/Pages/Student/StudentCalendar.ascx" TagPrefix="uc" TagName="StudentCalendar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-layout-split">

        <div class="layout-main-70">
            
            <div class="ec-hero ec-hero-student">
                <span class="ec-status-pill ec-status-active mb-3">DASHBOARD</span>
                <h2 class="fw-bold mb-2">Welcome back, <asp:Label ID="lblStudentName" runat="server">Student</asp:Label>!</h2>
                <p class="mb-4">You've completed 85% of your goals this week. Your next milestone is just a few lessons away!</p>
                <a href="Study.aspx" class="btn btn-primary rounded-pill px-4 py-2 fw-bold shadow-sm">
                    <i class="bi bi-rocket-takeoff me-2"></i>Continue Learning
                </a>
            </div>

            <div class="ec-glass-card">
                <div class="ec-stat-box d-flex align-items-center p-3 text-start">
                    <div class="bg-primary bg-opacity-10 text-primary rounded-circle d-flex justify-content-center align-items-center me-3" style="width: 50px; height: 50px; font-size: 1.5rem;">
                        <i class="bi bi-trophy-fill"></i>
                    </div>
                    <div>
                        <p class="mb-0 fw-bold fs-5 text-main">Intro to Python Quiz 1</p>
                        <small class="text-success fw-bold">Score: 90% • Outstanding Performance!</small>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card">
                <div class="ec-glass-card-header border-0 pb-0">
                    <h5 class="fw-bold m-0"><i class="bi bi-bookmark-star-fill text-primary me-2"></i>Pick up where you left off</h5>
                </div>
                <div class="ec-glass-card-body pt-3">
                    <div class="layout-grid-half">
                        <div class="ec-course-box p-4 text-center">
                            <i class="bi bi-file-earmark-pdf-fill text-danger display-5 mb-3"></i>
                            <h6 class="fw-bold text-main">Advanced Calculus</h6>
                            <p class="text-muted small mb-4">Material: Lecture Notes.pdf</p>
                            <button class="btn btn-outline-danger btn-pill w-100 mt-auto fw-bold btn-sm">View Material</button>
                        </div>
                        
                        <div class="ec-course-box p-4 text-center">
                            <i class="bi bi-play-circle-fill text-primary display-5 mb-3"></i>
                            <h6 class="fw-bold text-main">Database Management</h6>
                            <p class="text-muted small mb-4">Next: SQL Joins & Unions</p>
                            <button class="btn btn-primary btn-pill w-100 mt-auto fw-bold btn-sm">Resume Lesson</button>
                        </div>
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

            <div class="ec-glass-card">
                <div class="ec-glass-card-header">
                    <h5 class="fw-bold m-0"><i class="bi bi-bell-fill text-danger me-2"></i>Notifications</h5>
                    <span class="ec-status-pill ec-status-danger">2 NEW</span>
                </div>
                <div class="ec-glass-card-body p-0 pt-2">
                    <div class="ec-feed-item flex-column align-items-start border-start border-4 border-danger ps-3 mb-3">
                        <p class="mb-0 fw-bold text-main">System Maintenance</p>
                        <small class="text-muted">System will be offline for 2 hours tomorrow.</small>
                    </div>

                    <div class="ec-feed-item flex-column align-items-start border-start border-4 border-primary ps-3 mb-0">
                        <p class="mb-0 fw-bold text-main">New Quiz Available</p>
                        <small class="text-muted">Check out the new Python Quiz 2.</small>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>