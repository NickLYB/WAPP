<%@ Page Title="Course Overview" Language="C#" MasterPageFile="~/Masters/Student.Master" AutoEventWireup="true" CodeBehind="CourseDetail.aspx.cs" Inherits="WAPP.Pages.Student.CourseDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
        <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="ec-content-wrapper">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StudentMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <div class="ec-section-gap">
            <h1 class="ec-page-title">
                <asp:Literal ID="litCourseTitle" runat="server"></asp:Literal>
            </h1>
            <p class="ec-page-subtitle">
                By <strong>
                    <asp:HyperLink ID="hlTutorNameLink" runat="server" CssClass="text-primary text-decoration-none">
                        <asp:Literal ID="litTutorName" runat="server"></asp:Literal>
                    </asp:HyperLink>
                </strong>
            </p>
        </div>

        <div class="page-layout-split">

            <div class="layout-main-70">

                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="ec-section-title">Course Content</h5>
                    </div>

                    <div class="ec-glass-card-body p-0">
                        <asp:Repeater ID="rptResources" runat="server">
                            <ItemTemplate>
                                <div class="ec-item-row px-3">
                                    <div>
                                        <strong>Lesson <%# Container.ItemIndex + 1 %>:</strong>
                                        <%# Eval("TypeName") %>
                                    </div>
                                    <i class='<%# GetIcon(Eval("resource_type").ToString()) %> text-muted'></i>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlLockedMessage" runat="server" Visible="false" CssClass="mt-3 text-center">
                            <div class="ec-empty-state" style="padding: 20px; background: #f8fafc; border-radius: 12px; border: 1px dashed #cbd5e1;">
                                <span style="font-size: 1rem;">🔒 Enroll in this course to unlock full content.</span>
                            </div>
                        </asp:Panel>

                    </div>
                </div>

                <div class="ec-glass-card mt-4">
                    
                    <div class="ec-glass-card-body">

                        <ul class="nav nav-tabs border-bottom mb-4" id="courseTab" role="tablist">
                            <li class="nav-item" role="presentation">
                                <button class="nav-link active" 
                                        id="about-tab" 
                                        data-bs-toggle="tab" 
                                        data-bs-target="#about" 
                                        type="button" role="tab">About Course</button>
                            </li>
                            <li class="nav-item" role="presentation">
                                <button class="nav-link" 
                                        id="reviews-tab" 
                                        data-bs-toggle="tab" 
                                        data-bs-target="#reviews" 
                                        type="button" role="tab">Reviews</button>
                            </li>
                        </ul>

                        <div class="tab-content pt-2">

                            <div class="tab-pane fade show active"
                                 id="about"
                                 role="tabpanel">
                                
                                <p class="text-dark" style="line-height: 1.8;">
                                    <asp:Literal ID="litDescription" runat="server"></asp:Literal>
                                </p>

                            </div>

                            <div class="tab-pane fade"
                                 id="reviews"
                                 role="tabpanel">

                                <asp:Repeater ID="rptFeedback" runat="server">
                                    <ItemTemplate>
                                        <div class="ec-item-row py-3" style="flex-direction: column; align-items: flex-start;">
                                            <div style="display: flex; justify-content: space-between; width: 100%;">
                                                <strong class="text-dark"><%# Eval("StudentName") %></strong>
                                                <span class="text-warning small">
                                                    <%# new String('★', Convert.ToInt32(Eval("rating"))) %>
                                                </span>
                                            </div>
                                            <small class="text-muted mb-2"><%# Eval("created_at", "{0:MMM dd, yyyy}") %></small>
                                            <p class="mb-0 text-secondary" style="font-size: 0.95rem;"><%# Eval("comment") %></p>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </div>

                        </div>

                    </div>

                </div>

            </div>
            
            <div class="layout-sidebar-30">

                <div class="ec-glass-card mb-4">
                    <div class="ec-glass-card-header">
                        <h5 class="ec-section-title">Course Overview</h5>
                    </div>

                    <div class="ec-glass-card-body">
                        <p class="text-muted small mb-4">
                            <asp:Literal ID="litSidebarDescription" runat="server"></asp:Literal>
                        </p>
                        
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Course Type</span>
                            <span class="text-dark"><asp:Literal ID="litCategory" runat="server"></asp:Literal></span>
                        </div>
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Duration</span>
                            <span class="text-dark"><asp:Literal ID="litDuration" runat="server"></asp:Literal></span>
                        </div>
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Skill Level</span>
                            <span class="ec-status-pill ec-status-locked" style="background:#f1f5f9;"><asp:Literal ID="litSkillLevel" runat="server"></asp:Literal></span>
                        </div>
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Enrolled</span>
                            <span class="text-dark"><asp:Literal ID="litEnrolledCount" runat="server"></asp:Literal> Students</span>
                        </div>
                        <div class="ec-item-row py-2 border-0 mb-4">
                            <span class="text-muted small fw-bold">Rating</span>
                            <span class="text-warning"><asp:Literal ID="litRatingStars" runat="server"></asp:Literal></span>
                        </div>

                        <asp:Button ID="btnStart" runat="server"
                            Text="Start Learning Now"
                            CssClass="btn btn-primary w-100 btn-start shadow-sm" 
                            OnClick="btnStart_Click" />

                    </div>
                </div>

                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="ec-section-title">Instructor</h5>
                    </div>
                    <div class="ec-glass-card-body text-center pb-4">
                        <div class="mb-3 mt-2">
                            <i class="bi bi-person-circle text-primary opacity-50" style="font-size: 3.5rem;"></i>
                        </div>
                        <h6 class="fw-bold text-main mb-1"><asp:Literal ID="litSidebarTutorName" runat="server"></asp:Literal></h6>
                        <span class="text-muted small d-block mb-3">EduConnect Educator</span>
                        
                        <asp:HyperLink ID="hlTutorProfileBtn" runat="server" CssClass="btn btn-sub rounded-pill w-100 fw-bold shadow-sm">
                            View Full Profile
                        </asp:HyperLink>
                    </div>
                </div>

            </div>

        </div>

    </div>

</asp:Content>