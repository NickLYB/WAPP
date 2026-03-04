<%@ Page Title="Upload Course Material" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="UploadCourseMaterial.aspx.cs" Inherits="WAPP.Pages.Tutor.UploadCourseMaterial" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.tiny.cloud/1/se4de6ff8koipe875tskvfyghjwer395r0j6o0g79t2k5ioq/tinymce/8/tinymce.min.js" referrerpolicy="origin" crossorigin="anonymous"></script>
    
    <script>
        tinymce.init({
            selector: '#description',
            plugins: 'lists link charmap preview code textcolor',
            toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline | forecolor backcolor | alignleft aligncenter alignright | bullist numlist | link code',
            menubar: false,
            height: 300,
            branding: false,
            setup: function (editor) {
                editor.on('change', function () {
                    editor.save();
                });
            }
        });

        // Client-side UI toggling for File vs YouTube link
        function toggleInputs() {
            var resType = document.getElementById('<%= resource_type.ClientID %>').value;
            
            // Find which radio button is selected for Video Source
            var rblSource = document.getElementsByName('<%= rblVideoSource.UniqueID %>');
            var selectedSource = "File";
            for (var i = 0; i < rblSource.length; i++) {
                if (rblSource[i].checked) {
                    selectedSource = rblSource[i].value;
                    break;
                }
            }

            var videoSourceGroup = document.getElementById('videoSourceGroup');
            var fileUploadGroup = document.getElementById('fileUploadGroup');
            var youtubeLinkGroup = document.getElementById('youtubeLinkGroup');

            if (resType === 'Video') {
                videoSourceGroup.style.display = 'block';
                if (selectedSource === 'Link') {
                    fileUploadGroup.style.display = 'none';
                    youtubeLinkGroup.style.display = 'block';
                } else {
                    fileUploadGroup.style.display = 'block';
                    youtubeLinkGroup.style.display = 'none';
                }
            } else if (resType === 'PDF') {
                videoSourceGroup.style.display = 'none';
                youtubeLinkGroup.style.display = 'none';
                fileUploadGroup.style.display = 'block';
            } else {
                // Default / empty state
                videoSourceGroup.style.display = 'none';
                youtubeLinkGroup.style.display = 'none';
                fileUploadGroup.style.display = 'block';
            }
        }

        // Run on page load to set correct initial state
        window.onload = function () { toggleInputs(); };
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <div class="ec-item-gap">
                <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                    PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
            </div>
            
            <div class="ec-section-header border-0">
                <h1 class="ec-page-title m-0">Upload Course Material</h1>
                <asp:Button ID="Button1" runat="server" Text="Upload Material" CssClass="btn-main btn-pill" OnClick="Button1_Click"/>
            </div>
            <p class="ec-page-subtitle">Add videos, PDF notes, or presentations to your course curriculum.</p>
        </div>

        <div class="ec-glass-card">
            <div class="ec-glass-card-header">
                <h5 class="ec-section-title"><i class="bi bi-cloud-upload me-2 text-primary"></i>Resource Details</h5>
                <asp:Label ID="lblMsg" runat="server" CssClass="text-danger small fw-bold" Text=""></asp:Label>
            </div>

            <div class="ec-glass-card-body">
                <div class="ec-section-gap">
                    <asp:Label ID="Label1" runat="server" Text="Material Title" CssClass="fw-bold text-dark small mb-2 d-block" />
                    <asp:TextBox ID="title" runat="server" CssClass="login-input m-0" placeholder="e.g., Introduction to Advanced Calculus"></asp:TextBox>
                </div>

                <div class="ec-section-gap">
                    <asp:Label ID="Label2" runat="server" Text="Description (Optional)" CssClass="fw-bold text-dark small mb-2 d-block"/>
                    <asp:TextBox ID="description" runat="server" ClientIDMode="Static" CssClass="login-input m-0" 
                        placeholder="Provide a brief overview of this material..." 
                        TextMode="MultiLine" Rows="5"></asp:TextBox>
                </div>

                <div class="layout-grid-half">
                    <div class="ec-item-gap">
                        <asp:Label ID="Label4" runat="server" Text="Resource Type" CssClass="fw-bold text-dark small mb-2 d-block" />
                        <asp:DropDownList ID="resource_type" runat="server" CssClass="login-input m-0" style="padding: 10px 15px;" onchange="toggleInputs()">
                            <asp:ListItem Value="">-- Select Type --</asp:ListItem>
                            <asp:ListItem Value="Video">Video</asp:ListItem>
                            <asp:ListItem Value="PDF">PDF</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="ec-item-gap" id="videoSourceGroup" style="display:none;">
                        <asp:Label runat="server" Text="Video Source" CssClass="fw-bold text-dark small mb-2 d-block" />
                        <div class="p-2 border rounded-pill bg-light d-flex align-items-center" style="height: 48px;">
                            <asp:RadioButtonList ID="rblVideoSource" runat="server" RepeatDirection="Horizontal" CssClass="m-0 ms-2" onclick="toggleInputs()">
                                <asp:ListItem Value="File" Selected="True">Local File (.mp4)&nbsp;&nbsp;</asp:ListItem>
                                <asp:ListItem Value="Link">YouTube Link</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>
                    </div>
                </div>

                <div class="ec-section-gap mt-4" id="fileUploadGroup">
                    <asp:Label ID="Label3" runat="server" Text="Select File" CssClass="fw-bold text-dark small mb-2 d-block" />
                    <div class="p-2 border rounded-pill bg-light">
                        <asp:FileUpload ID="FileUpload1" runat="server" CssClass="form-control border-0 bg-transparent shadow-none" accept=".pdf,.mp4" />
                    </div>
                </div>

                <div class="ec-section-gap mt-4" id="youtubeLinkGroup" style="display:none;">
                    <asp:Label runat="server" Text="YouTube URL" CssClass="fw-bold text-dark small mb-2 d-block" />
                    <asp:TextBox ID="txtYoutubeLink" runat="server" CssClass="login-input m-0" placeholder="https://www.youtube.com/watch?v=..."></asp:TextBox>
                </div>

                <div class="mt-4 p-3 rounded-3" style="background: rgba(13, 110, 253, 0.05); border: 1px dashed rgba(13, 110, 253, 0.2);">
                    <small class="text-muted">
                        <i class="bi bi-info-circle me-1"></i> 
                        <strong>Guidelines:</strong> Upload <b>.pdf</b> for documents, or <b>.mp4</b> for local videos. You can also paste a YouTube video link. Max file size: 50MB.
                    </small>
                </div>
            </div>
        </div>

    </div>
</asp:Content>