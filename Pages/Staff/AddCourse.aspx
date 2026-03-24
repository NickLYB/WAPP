<%@ Page Title="Add New Course" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="AddCourse.aspx.cs" Inherits="WAPP.Pages.Staff.AddCourse" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <style>
        .btn-outline-primary { border-color: var(--ec-primary); color: var(--ec-primary); }
        .btn-outline-primary:hover { background-color: var(--ec-primary); color: white; }
        
        .btn-outline-secondary { border-color: #6c757d; color: #6c757d; }
        .btn-outline-secondary:hover { background-color: #6c757d; color: white; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="ec-staff-wrapper">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StaffMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-1">Course Management</h2>
        <p class="ec-page-subtitle mb-4">Add New Course</p>

        <div class="ec-staff-card p-4 p-md-5 shadow-sm">
            <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold" Visible="false"></asp:Label>

            <div class="row mb-4 align-items-center">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Course Title:</label>
                <div class="col-md-10">
                    <asp:TextBox ID="txtAddTitle" runat="server" CssClass="form-control ec-form-control" Placeholder="Enter Course Title..." required="required"></asp:TextBox>
                </div>
            </div>

            <div class="row mb-4">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Description:</label>
                <div class="col-md-10">
                    <asp:TextBox ID="txtAddDesc" runat="server" CssClass="form-control ec-form-control" TextMode="MultiLine" Rows="5" Placeholder="Enter Course Description..." required="required"></asp:TextBox>
                </div>
            </div>

            <div class="row mb-4 align-items-center">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Category:</label>
                <div class="col-md-4 mb-3 mb-md-0">
                    <asp:DropDownList ID="ddlAddCategory" runat="server" CssClass="form-select ec-form-control"></asp:DropDownList>
                </div>
                
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Duration (Mins):</label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtAddDuration" runat="server" CssClass="form-control ec-form-control" TextMode="Number" Placeholder="e.g. 120" required="required" min="1"></asp:TextBox>
                </div>
            </div>

            <div class="row mb-4 align-items-center">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Skill Level:</label>
                <div class="col-md-2 mb-3 mb-md-0">
                    <asp:DropDownList ID="ddlAddSkill" runat="server" CssClass="form-select ec-form-control">
                        <asp:ListItem Value="BEGINNER">BEGINNER</asp:ListItem>
                        <asp:ListItem Value="INTERMEDIATE">INTERMEDIATE</asp:ListItem>
                        <asp:ListItem Value="ADVANCED">ADVANCED</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Assign Tutor:</label>
                <div class="col-md-2 mb-3 mb-md-0">
                    <asp:DropDownList ID="ddlAddTutor" runat="server" CssClass="form-select ec-form-control"></asp:DropDownList>
                </div>

                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Status:</label>
                <div class="col-md-2">
                    <asp:DropDownList ID="ddlAddStatus" runat="server" CssClass="form-select ec-form-control">
                        <asp:ListItem Value="PUBLISHED">Published / Active</asp:ListItem>
                        <asp:ListItem Value="APPROVED">Approved</asp:ListItem>
                        <asp:ListItem Value="PENDING">Pending / Draft</asp:ListItem>
                        <asp:ListItem Value="PRIVATE">Private</asp:ListItem>
                        <asp:ListItem Value="REJECT">Rejected</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div class="row mb-5 align-items-center">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Course Image:</label>
                <div class="col-md-10">
                    <asp:FileUpload ID="fuCourseImage" runat="server" CssClass="form-control ec-form-control" accept=".jpg,.jpeg,.png,.gif" />
                    <small class="text-muted mt-1 d-block">Optional. Allowed formats: JPG, PNG, GIF.</small>
                </div>
            </div>

            <div class="row">
                <div class="col-md-10 offset-md-2 d-flex gap-3">
                    <asp:Button ID="btnSaveCourse" runat="server" Text="Save Course" CssClass="btn btn-outline-primary rounded-pill px-4 fw-bold shadow-sm" OnClick="btnSaveCourse_Click" />
                    <asp:HyperLink ID="hlCancel" runat="server" NavigateUrl="~/Pages/Staff/CourseManagement.aspx" CssClass="btn btn-outline-secondary rounded-pill px-4 fw-bold shadow-sm">Cancel</asp:HyperLink>
                </div>
            </div>

        </div>
    </div>

</asp:Content>