<%@ Page Title="My Enrolled Courses" Language="C#" MasterPageFile="~/Masters/Student.Master" AutoEventWireup="true" CodeBehind="MyCourses.aspx.cs" Inherits="WAPP.Pages.Student.MyCourses" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <div class="ec-section-header border-0">
                <div>
                    <h1 class="ec-page-title m-0">My Learning Journey</h1>
                    <p class="ec-page-subtitle">Pick up right where you left off.</p>
                </div>
                <asp:HyperLink runat="server" NavigateUrl="~/Pages/Student/Study.aspx" CssClass="btn-sub">
                    <i class="bi bi-search me-1"></i> Browse More
                </asp:HyperLink>
            </div>
        </div>

        <div class="ec-course-grid">
            <asp:Repeater ID="rptMyCourses" runat="server">
                <ItemTemplate>
                    <div class="ec-course-box">
                        <div class="ec-course-img-wrapper">
                            <asp:Image ID="imgCourse" runat="server" 
                                ImageUrl='<%# string.IsNullOrEmpty(Eval("image_path").ToString()) ? "~/Images/default-course.png" : Eval("image_path") %>' 
                                CssClass="ec-course-img" />
                        </div>
                        
                        <div class="ec-course-body">
                            <div class="ec-badge-row">
                                <span class="ec-status-pill ec-status-active">
                                    <%# Eval("status") %>
                                </span>
                                <span class="small text-muted ms-auto">
                                    <i class="bi bi-star-fill text-warning me-1"></i> 4.8
                                </span>
                            </div>
                            
                            <h5 class="fw-bold text-dark mb-3" style="min-height: 48px; line-height: 1.4;">
                                <%# Eval("title") %>
                            </h5>
                            
                            <div class="mb-4">
                                <div class="d-flex justify-content-between align-items-center mb-1">
                                    <span class="text-muted small fw-bold text-uppercase" style="letter-spacing: 0.5px; font-size: 0.7rem;">Your Progress</span>
                                    <span class="text-primary small fw-bold"><%# Eval("Progress") %>%</span>
                                </div>
                                <div class="ec-progress-container m-0">
                                    <div class="ec-progress-fill" style='width: <%# Eval("Progress") %>%'></div>
                                </div>
                            </div>

                            <div class="ec-course-footer border-0 p-0 mt-auto">
                                <a href='<%# "LessonView.aspx?resourceId=" + GetFirstResourceId(Eval("course_id")) %>' class="btn-main btn-pill w-100">
                                    <i class="bi bi-play-circle-fill me-2"></i>Continue Lesson
                                </a>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
            <div class="ec-empty-state">
                <i class="bi bi-journal-bookmark-fill"></i>
                <h5 class="fw-bold text-dark">No active courses</h5>
                <p>Start your journey by enrolling in a new course today.</p>
                <asp:HyperLink runat="server" NavigateUrl="~/Pages/Student/Study.aspx" CssClass="btn-main btn-pill mt-3">Explore Now</asp:HyperLink>
            </div>
        </asp:PlaceHolder>

    </div>
</asp:Content>