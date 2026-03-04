<%@ Page Title="Preview Course" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="ViewCourse.aspx.cs" Inherits="WAPP.Pages.Tutor.ViewCourse" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <style>
        /* Highlight the active lesson in the sidebar */
        .lesson-btn.active-lesson {
            background-color: var(--ec-primary-subtle, #e0f2fe);
            border-left: 4px solid var(--ec-primary, #0284c7);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>

        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="ec-page-title m-0">
                <i class="bi bi-eye-fill me-2 text-primary"></i> 
                Preview: <asp:Literal ID="litCourseTitle" runat="server"></asp:Literal>
            </h2>
            <a href="Teaching.aspx" class="btn btn-secondary rounded-pill px-4 fw-bold shadow-sm text-white">Exit Preview</a>
        </div>

        <div class="page-layout-split">
            
            <div class="layout-main-70">
                
                <asp:PlaceHolder ID="phHasContent" runat="server">
                    <div class="ec-glass-card mb-4">
                        <div class="ec-glass-card-header">
                            <h5 class="fw-bold m-0 text-main"><i class="bi bi-journal-text me-2 text-primary"></i> Tutor's Corner</h5>
                            <span class="text-muted small"><asp:Label ID="lblCurrentLessonName" runat="server"></asp:Label></span>
                        </div>
                        <div class="ec-glass-card-body text-main" style="font-size: 1.1rem; line-height: 1.8;">
                            <asp:Literal ID="litLessonNote" runat="server"></asp:Literal>
                        </div>
                    </div>

                    <div class="ec-video-container shadow-lg mb-4 bg-light rounded-4 overflow-hidden d-flex align-items-center justify-content-center" style="min-height: 450px;">
                        <asp:Literal ID="litVideoPlayer" runat="server"></asp:Literal>
                    </div>
                </asp:PlaceHolder>

                <asp:PlaceHolder ID="phNoContent" runat="server" Visible="false">
                    <div class="text-center py-5 ec-glass-card">
                        <i class="bi bi-folder-x text-muted" style="font-size: 3rem;"></i>
                        <h5 class="text-muted mt-3">No learning resources uploaded yet.</h5>
                        <p class="text-muted small">Go to the Edit page to add lessons to this course.</p>
                    </div>
                </asp:PlaceHolder>

            </div>

            <div class="layout-sidebar-30">
                <div class="ec-glass-card p-0 overflow-hidden sticky-top" style="top: 20px;">
                    <div class="ec-glass-card-header border-bottom p-3 mb-0" style="background: var(--ec-bg-alt);">
                        <span class="fw-bold small text-muted text-uppercase">COURSE CURRICULUM</span>
                    </div>
                    <div style="max-height: 70vh; overflow-y: auto;">
                        <asp:Repeater ID="rptLessons" runat="server">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkLesson" runat="server"
                                    CssClass='<%# GetLessonCSS(Eval("Id")) %>'
                                    OnClick="lnkLesson_Click" CommandArgument='<%# Eval("Id") %>'>
                                    <div class="d-flex align-items-center justify-content-between">
                                        <div class="d-flex align-items-center">
                                            <i class='<%# GetIcon(Eval("resource_type")) %> me-3 fs-5'></i>
                                            <div>
                                                <p class="mb-0 fw-bold small text-main">Lesson <%# Container.ItemIndex + 1 %></p>
                                                <small class="text-muted"><%# Eval("title") %></small>
                                            </div>
                                        </div>
                                        <asp:PlaceHolder runat="server" Visible='<%# Eval("Id").ToString() == ActiveResourceId.ToString() %>'>
                                            <i class='bi bi-play-fill text-primary fs-5'></i>
                                        </asp:PlaceHolder>
                                    </div>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>

        </div>
    </《div>

</asp:Content>