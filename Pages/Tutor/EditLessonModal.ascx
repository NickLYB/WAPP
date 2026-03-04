<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditLessonModal.ascx.cs" Inherits="WAPP.Pages.Tutor.EditLessonModal" %>

<script src="https://cdn.tiny.cloud/1/se4de6ff8koipe875tskvfyghjwer395r0j6o0g79t2k5ioq/tinymce/8/tinymce.min.js" referrerpolicy="origin" crossorigin="anonymous"></script>

<script>
    // Must initialize TinyMCE every time the UpdatePanel finishes rendering
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_endRequest(function () {
        initTinyMceEdit();
        toggleEditInputs(); // Re-bind the toggle logic on postback
    });

    // Initial load
    document.addEventListener("DOMContentLoaded", function () {
        initTinyMceEdit();
    });

    function initTinyMceEdit() {
        tinymce.remove('#<%= txtDescription.ClientID %>'); // Clear existing instance if it exists
        tinymce.init({
            selector: '#<%= txtDescription.ClientID %>',
            plugins: 'lists link charmap preview code textcolor',
            toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline | forecolor backcolor | alignleft aligncenter alignright | bullist numlist | link code',
            menubar: false,
            height: 250,
            branding: false,
            setup: function (editor) {
                editor.on('change', function () {
                    editor.save(); // Sync to asp:TextBox
                });
            }
        });
    }

    function toggleEditInputs() {
        var resType = document.getElementById('<%= ddlResourceType.ClientID %>').value;
        
        var rblSource = document.getElementsByName('<%= rblEditVideoSource.UniqueID %>');
        var selectedSource = "File";
        for (var i = 0; i < rblSource.length; i++) {
            if (rblSource[i].checked) {
                selectedSource = rblSource[i].value;
                break;
            }
        }

        var videoSourceGroup = document.getElementById('editVideoSourceGroup');
        var fileUploadGroup = document.getElementById('editFileUploadGroup');
        var youtubeLinkGroup = document.getElementById('editYoutubeLinkGroup');

        if (resType === 'Video') {
            videoSourceGroup.style.display = 'flex';
            if (selectedSource === 'Link') {
                fileUploadGroup.style.display = 'none';
                youtubeLinkGroup.style.display = 'flex';
            } else {
                fileUploadGroup.style.display = 'flex';
                youtubeLinkGroup.style.display = 'none';
            }
        } else if (resType === 'PDF') {
            videoSourceGroup.style.display = 'none';
            youtubeLinkGroup.style.display = 'none';
            fileUploadGroup.style.display = 'flex';
        }
    }
</script>

<div class="modal fade" id="editLessonModal" tabindex="-1" aria-labelledby="editLessonModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
            
            <div class="modal-header bg-light">
                <h5 class="modal-title" id="editLessonModalLabel" style="font-weight:bold; color:#a31d3d;">EDIT LESSON MATERIAL</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>

            <asp:UpdatePanel ID="upEditLesson" runat="server">
                <ContentTemplate>
                    <div class="modal-body p-4">
                        
                        <div class="text-center mb-3">
                            <asp:Label ID="lblMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
                        </div>

                        <asp:HiddenField ID="hfLessonId" runat="server" />

                        <div class="form-row mb-3 d-flex align-items-center">
                            <asp:Label ID="lblTitle" runat="server" Text="Material Title:" CssClass="form-label" style="min-width: 150px;" />
                            <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control flex-grow-1"></asp:TextBox>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-start">
                            <asp:Label ID="lblDesc" runat="server" Text="Note/Description:" CssClass="form-label mt-1" style="min-width: 150px;" />
                            <div class="flex-grow-1">
                                <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                            </div>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-center">
                            <asp:Label ID="lblType" runat="server" Text="Resource Type:" CssClass="form-label" style="min-width: 150px;" />
                            <asp:DropDownList ID="ddlResourceType" runat="server" CssClass="form-select flex-grow-1" onchange="toggleEditInputs()">
                                <asp:ListItem>Video</asp:ListItem>
                                <asp:ListItem>PDF</asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="form-row mb-3 align-items-center" id="editVideoSourceGroup" style="display:none;">
                            <asp:Label runat="server" Text="Video Source:" CssClass="form-label" style="min-width: 150px;" />
                            <div class="p-2 border rounded bg-light flex-grow-1">
                                <asp:RadioButtonList ID="rblEditVideoSource" runat="server" RepeatDirection="Horizontal" CssClass="m-0" onclick="toggleEditInputs()">
                                    <asp:ListItem Value="File" Selected="True">Local File (.mp4)&nbsp;&nbsp;</asp:ListItem>
                                    <asp:ListItem Value="Link">YouTube Link</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>
                        </div>

                        <div class="form-row mb-2 align-items-center" id="editFileUploadGroup">
                            <asp:Label ID="lblFile" runat="server" Text="Replace File:" CssClass="form-label" style="min-width: 150px;" />
                            <asp:FileUpload ID="fileUploadEdit" runat="server" CssClass="form-control flex-grow-1" accept=".pdf,.mp4" />
                        </div>

                        <div class="form-row mb-2 align-items-center" id="editYoutubeLinkGroup" style="display:none;">
                            <asp:Label runat="server" Text="YouTube URL:" CssClass="form-label" style="min-width: 150px;" />
                            <asp:TextBox ID="txtEditYoutubeLink" runat="server" CssClass="form-control flex-grow-1" placeholder="https://www.youtube.com/watch?v=..."></asp:TextBox>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-center">
                            <span style="min-width: 150px;"></span>
                            <small class="text-muted">Leave file/link blank if you want to keep the current resource.</small>
                        </div>

                    </div>

                    <div class="modal-footer bg-light" style="display: flex; justify-content: space-between;">
                        <asp:Button ID="btnDelete" runat="server" Text="Delete Material" CssClass="btn btn-outline-danger" OnClick="btnDelete_Click" OnClientClick="return confirm('Are you sure you want to delete this material?');" />
                        
                        <div>
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" data-bs-dismiss="modal" OnClientClick="return false;" />
                            <asp:Button ID="btnUpdate" runat="server" Text="Save Changes" OnClick="btnUpdate_Click" CssClass="btn text-white" style="background-color: #a31d3d;" />
                        </div>
                    </div>
                    
                    <script type="text/javascript">
                        Sys.Application.add_load(toggleEditInputs);
                    </script>
                </ContentTemplate>
                
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnUpdate" />
                </Triggers>

            </asp:UpdatePanel>
        </div>
    </div>
</div>