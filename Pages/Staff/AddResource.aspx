<%@ Page Title="Add New Learning Resource" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="AddResource.aspx.cs" Inherits="WAPP.Pages.Staff.AddResource" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="ec-staff-wrapper">
        
        <h2 class="ec-staff-title text-uppercase mb-1">Learning Resources Management</h2>
        <p class="ec-page-subtitle mb-4">Upload New Learning Resource</p>

        <div class="ec-staff-card p-4 p-md-5">
            <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold" Visible="false"></asp:Label>

            <div class="row mb-4 align-items-center">
                <label class="col-md-3 col-form-label fw-bold text-muted text-md-end">Select Course:</label>
                <div class="col-md-9">
                    <asp:DropDownList ID="ddlAddCourse" runat="server" CssClass="form-select ec-form-control"></asp:DropDownList>
                </div>
            </div>

            <div class="row mb-4 align-items-center">
                <label class="col-md-3 col-form-label fw-bold text-muted text-md-end">Resource Type:</label>
                <div class="col-md-9">
                    <asp:DropDownList ID="ddlAddType" runat="server" CssClass="form-select ec-form-control"></asp:DropDownList>
                </div>
            </div>

            <div class="row mb-4 align-items-center">
                <label class="col-md-3 col-form-label fw-bold text-muted text-md-end">Assign Tutor:</label>
                <div class="col-md-9">
                    <asp:DropDownList ID="ddlAddTutor" runat="server" CssClass="form-select ec-form-control"></asp:DropDownList>
                </div>
            </div>

            <div class="row mb-5 align-items-start">
                <label class="col-md-3 col-form-label fw-bold text-muted text-md-end pt-2">Resource Link (URL):</label>
                <div class="col-md-9">
                    <asp:TextBox ID="txtAddLink" runat="server" CssClass="form-control ec-form-control" Placeholder="e.g. https://youtube.com/watch..." required="required" type="url"></asp:TextBox>
                </div>
            </div>

            <div class="row">
                <div class="col-md-9 offset-md-3 d-flex gap-3">
                    <asp:Button ID="btnSaveResource" runat="server" Text="Upload Resource" CssClass="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" OnClick="btnSaveResource_Click" />
                    <asp:HyperLink ID="hlCancel" runat="server" NavigateUrl="~/Pages/Staff/LearningResourceManagement.aspx" CssClass="btn btn-secondary rounded-pill px-4 fw-bold shadow-sm text-white">Cancel</asp:HyperLink>
                </div>
            </div>

        </div>
    </div>

</asp:Content>