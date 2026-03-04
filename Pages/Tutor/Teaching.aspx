<%@ Page Title="My Courses" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="Teaching.aspx.cs" Inherits="WAPP.Pages.Tutor.Study" %>
<%@ Register Src="~/Pages/Tutor/CreateCourseModal.ascx" TagPrefix="uc1" TagName="CreateCourseModal" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <div class="ec-item-gap">
                <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                    PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
            </div>
            
            <div class="ec-section-header border-0">
                <h1 class="ec-page-title m-0">My Teaching Portfolio</h1>
                <button type="button" class="btn-main btn-pill" data-bs-toggle="modal" data-bs-target="#createCourseModal">
                    <i class="bi bi-plus-lg me-1"></i> Create New Course
                </button>
            </div>
            <p class="ec-page-subtitle">Manage your published content and course drafts.</p>
        </div>

        <div class="ec-glass-card">
            <div class="ec-glass-card-header">
                <h5 class="ec-section-title">Course Inventory</h5>
                <span class="text-muted small">Total Courses: <asp:Literal ID="litCount" runat="server" Text="0"></asp:Literal></span>
            </div>

            <div class="ec-glass-card-body p-0">
                <asp:Repeater ID="rptCourses" runat="server">
                    <ItemTemplate>
                        <div class="ec-item-row px-3">
                            
                            <div style="display: flex; align-items: center; gap: 20px;">
                                <div style="width: 80px; height: 50px; border-radius: 8px; overflow: hidden; background: #f1f5f9;">
                                    <asp:Image runat="server" style="width:100%; height:100%; object-fit:cover;"
                                        ImageUrl='<%# GetCourseImage(Eval("image_path")) %>' AlternateText="Thumb" />
                                </div>

                                <div>
                                    <div class="fw-bold text-dark" style="font-size: 1.05rem;"><%# Eval("title") %></div>
                                    <span class='ec-status-pill <%# GetStatusClass(Eval("status")) %>'>
                                        <%# Eval("status") %>
                                    </span>
                                </div>
                            </div>

                            <div style="display: flex; gap: 10px;">
                                <asp:HyperLink runat="server" CssClass="btn-sub" 
                                    NavigateUrl='<%# "ViewCourse.aspx?id=" + Eval("Id") %>'>
                                    <i class="bi bi-eye me-1"></i> View
                                </asp:HyperLink>
                                <asp:HyperLink runat="server" CssClass="btn-sub" 
                                    NavigateUrl='<%# "EditCourse.aspx?id=" + Eval("Id") %>'>
                                    <i class="bi bi-pencil me-1"></i> Edit
                                </asp:HyperLink>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
                    <div class="ec-empty-state">
                        <i class="bi bi-journal-plus"></i>
                        <h5 class="text-dark fw-bold">Start your journey</h5>
                        <p>You haven't created any courses yet. Use the "Create New Course" button to begin.</p>
                    </div>
                </asp:PlaceHolder>
            </div>
        </div>
    </div>

    <uc1:CreateCourseModal ID="CreateCourseModal1" runat="server" />

</asp:Content>