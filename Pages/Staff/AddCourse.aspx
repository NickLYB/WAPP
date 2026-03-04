<%@ Page Title="Add New Course" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="AddCourse.aspx.cs" Inherits="WAPP.Pages.Staff.AddCourse" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    </asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="ec-staff-wrapper">
        
        <h2 class="ec-staff-title text-uppercase mb-1">Course Management</h2>
        <p class="ec-page-subtitle mb-4">Add New Course</p>

        <div class="ec-staff-card p-4 p-md-5">
            <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold" Visible="false"></asp:Label>

            <div class="row mb-4 align-items-center">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Course Title:</label>
                <div class="col-md-10">
                    <asp:TextBox ID="txtAddTitle" runat="server" CssClass="form-control ec-form-control" Placeholder="Enter Course Title..."></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtAddTitle" ErrorMessage="Title is required" ForeColor="#ef4444" Display="Dynamic" ValidationGroup="AddCourse" CssClass="fw-bold mt-2 d-block" />
                </div>
            </div>

            <div class="row mb-4">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Description:</label>
                <div class="col-md-10">
                    <asp:TextBox ID="txtAddDesc" runat="server" CssClass="form-control ec-form-control" TextMode="MultiLine" Rows="5" Placeholder="Enter Course Description..."></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvDesc" runat="server" ControlToValidate="txtAddDesc" ErrorMessage="Description is required" ForeColor="#ef4444" Display="Dynamic" ValidationGroup="AddCourse" CssClass="fw-bold mt-2 d-block" />
                </div>
            </div>

            <div class="row mb-4 align-items-center">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Category:</label>
                <div class="col-md-4 mb-3 mb-md-0">
                    <asp:DropDownList ID="ddlAddCategory" runat="server" CssClass="form-select ec-form-control"></asp:DropDownList>
                </div>
                
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Duration (Mins):</label>
                <div class="col-md-4">
                    <asp:TextBox ID="txtAddDuration" runat="server" CssClass="form-control ec-form-control" TextMode="Number" Placeholder="e.g. 120"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvDur" runat="server" ControlToValidate="txtAddDuration" ErrorMessage="Duration is required" ForeColor="#ef4444" Display="Dynamic" ValidationGroup="AddCourse" CssClass="fw-bold mt-2 d-block" />
                </div>
            </div>

            <div class="row mb-5 align-items-center">
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
                        <asp:ListItem Value="ACTIVE">Approved / Active</asp:ListItem>
                        <asp:ListItem Value="DRAFT">Pending / Draft</asp:ListItem>
                        <asp:ListItem Value="ARCHIVED">Rejected / Archived</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div class="row">
                <div class="col-md-10 offset-md-2 d-flex gap-3">
                    <asp:Button ID="btnSaveCourse" runat="server" Text="Save Course" CssClass="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" OnClick="btnSaveCourse_Click" ValidationGroup="AddCourse" />
                    <asp:HyperLink ID="hlCancel" runat="server" NavigateUrl="~/Pages/Staff/CourseManagement.aspx" CssClass="btn btn-secondary rounded-pill px-4 fw-bold shadow-sm text-white">Cancel</asp:HyperLink>
                </div>
            </div>

        </div>
    </div>

</asp:Content>