<%@ Page Title="Add New Learning Resource" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="AddResource.aspx.cs" Inherits="WAPP.Pages.Staff.AddResource" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
        <h2 class="ec-staff-title text-uppercase mb-1">Learning Resources Management</h2>
        <p class="ec-page-subtitle mb-4">Upload New Learning Resource</p>

        <div class="ec-staff-card p-4 p-md-5">
            <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold" Visible="false"></asp:Label>

            <asp:UpdatePanel ID="upResourceForm" runat="server">
                <ContentTemplate>
                    
                    <div class="row mb-4 align-items-center">
                        <label class="col-md-3 col-form-label fw-bold text-muted text-md-end">Select Course:</label>
                        <div class="col-md-9">
                            <asp:DropDownList ID="ddlAddCourse" runat="server" CssClass="form-select ec-form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlAddCourse_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="row mb-4 align-items-center">
                        <label class="col-md-3 col-form-label fw-bold text-muted text-md-end">Assign Tutor:</label>
                        <div class="col-md-9">
                            <asp:DropDownList ID="ddlAddTutor" runat="server" CssClass="form-select ec-form-control" Enabled="false"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="row mb-4 align-items-center">
                        <label class="col-md-3 col-form-label fw-bold text-muted text-md-end">Resource Type:</label>
                        <div class="col-md-9">
                            <asp:DropDownList ID="ddlAddType" runat="server" CssClass="form-select ec-form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlAddType_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>

                    <div id="rowVideoLink" runat="server" class="row mb-4 align-items-start">
                        <label class="col-md-3 col-form-label fw-bold text-muted text-md-end pt-2">Video Link (URL):</label>
                        <div class="col-md-9">
                            <asp:TextBox ID="txtAddLink" runat="server" CssClass="form-control ec-form-control" Placeholder="e.g. https://youtube.com/watch..." type="url"></asp:TextBox>
                            <small class="text-muted d-block mt-1">Provide the direct link to the video.</small>
                        </div>
                    </div>

                    <div id="rowPdfUpload" runat="server" class="row mb-4 align-items-start" visible="false">
                        <label class="col-md-3 col-form-label fw-bold text-muted text-md-end pt-2">Upload PDF File:</label>
                        <div class="col-md-9">
                            <asp:FileUpload ID="fuPdfResource" runat="server" CssClass="form-control ec-form-control" accept=".pdf" />
                            <small class="text-muted d-block mt-1">Allowed formats: PDF only.</small>
                        </div>
                    </div>

                </ContentTemplate>
                
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnSaveResource" />
                </Triggers>
            </asp:UpdatePanel>

            <div class="row mb-5 align-items-center">
                <label class="col-md-3 col-form-label fw-bold text-muted text-md-end">Resource Title:</label>
                <div class="col-md-9">
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control ec-form-control" Placeholder="Enter the title of this resource..." required="required"></asp:TextBox>
                </div>
            </div>

            <div class="row">
                <div class="col-md-9 offset-md-3 d-flex gap-3">
                    <asp:Button ID="btnSaveResource" runat="server" Text="Upload Resource" CssClass="btn btn-outline-primary rounded-pill px-4 fw-bold shadow-sm" OnClick="btnSaveResource_Click" />
                    <asp:HyperLink ID="hlCancel" runat="server" NavigateUrl="~/Pages/Staff/LearningResourceManagement.aspx" CssClass="btn btn-outline-secondary rounded-pill px-4 fw-bold shadow-sm">Cancel</asp:HyperLink>
                </div>
            </div>

        </div>
    </div>

</asp:Content>