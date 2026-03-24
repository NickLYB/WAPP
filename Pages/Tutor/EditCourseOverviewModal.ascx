<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditCourseOverviewModal.ascx.cs" Inherits="WAPP.Pages.Tutor.EditCourseOverviewModal" %>

<script src="https://cdn.tiny.cloud/1/se4de6ff8koipe875tskvfyghjwer395r0j6o0g79t2k5ioq/tinymce/8/tinymce.min.js" referrerpolicy="origin" crossorigin="anonymous"></script>
<script>
    function initEditModalEditor() {
        if (typeof tinymce !== 'undefined') {
            // Remove old instance to prevent duplicate editors when UpdatePanel reloads
            tinymce.remove('#<%= txtDesc.ClientID %>');
            
            // Check if dark mode is active on the page
            var isDark = document.documentElement.classList.contains('dark-mode');
            
            tinymce.init({
                selector: '#<%= txtDesc.ClientID %>',
                width: '100%',

                // Enable TinyMCE native dark mode and match your background color
                skin: isDark ? 'oxide-dark' : 'oxide',
                content_css: isDark ? 'dark' : 'default',
                content_style: isDark ? "body { background-color: #2d2d2d; color: #ffffff; }" : "",

                plugins: 'lists link charmap preview code textcolor',
                toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline | forecolor backcolor | alignleft aligncenter alignright | bullist numlist | link code',
                menubar: false,
                height: 250,
                branding: false,
                setup: function (editor) {
                    editor.on('change', function () {
                        editor.save(); // Force sync so ASP.NET can read the text on submit
                    });
                }
            });
        }
    }

    // Hook into ASP.NET AJAX lifecycle
    if (typeof Sys !== 'undefined') {
        Sys.Application.add_load(initEditModalEditor);
    } else {
        window.addEventListener('DOMContentLoaded', initEditModalEditor);
    }

    document.addEventListener('focusin', function (e) {
        if (e.target.closest(".tox-tinymce, .tox-tinymce-aux, .moxman-window, .tam-assetmanager-root") !== null) {
            e.stopImmediatePropagation();
        }
    });
</script>

<div class="modal fade" id="editCourseModal" tabindex="-1" aria-labelledby="editCourseModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">

            <div class="modal-header bg-light">
                <h5 class="modal-title" id="editCourseModalLabel">EDIT COURSE</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>

            <asp:UpdatePanel ID="upEditCourse" runat="server">
                <ContentTemplate>

                    <asp:HiddenField ID="hfCourseId" runat="server" />
                    
                    <div class="modal-body p-4">

                        <div class="text-center mb-4">
                            <asp:Label ID="lblMsg" runat="server"></asp:Label>
                        </div>

                        <!-- Replaced d-flex with Bootstrap Grid (row/col) -->
                        <div class="row mb-3 align-items-center">
                            <asp:Label runat="server" Text="Title:" CssClass="col-sm-3 col-form-label fw-bold" />
                            <div class="col-sm-9">
                                <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <!-- Removed align-items-center so the label stays at the top of the TinyMCE editor -->
                        <div class="row mb-3">
                            <asp:Label runat="server" Text="Description:" CssClass="col-sm-3 col-form-label fw-bold" />
                            <div class="col-sm-9">
                                <asp:TextBox ID="txtDesc" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                            </div>
                        </div>

                        <div class="row mb-3 align-items-center">
                            <asp:Label runat="server" Text="Course Type:" CssClass="col-sm-3 col-form-label fw-bold" />
                            <div class="col-sm-9">
                                <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="row mb-3 align-items-center">
                            <asp:Label runat="server" Text="Duration (Mins):" CssClass="col-sm-3 col-form-label fw-bold" />
                            <div class="col-sm-9">
                                <asp:TextBox ID="txtDuration" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                            </div>
                        </div>

                        <div class="row mb-3 align-items-center">
                            <asp:Label runat="server" Text="Skill Level:" CssClass="col-sm-3 col-form-label fw-bold" />
                            <div class="col-sm-9">
                                <asp:DropDownList ID="ddlSkill" runat="server" CssClass="form-select">
                                    <asp:ListItem Text="-- Select Skill --" Value="" />
                                    <asp:ListItem Text="Beginner" Value="BEGINNER" />
                                    <asp:ListItem Text="Intermediate" Value="INTERMEDIATE" />
                                    <asp:ListItem Text="Advanced" Value="ADVANCED" />
                                </asp:DropDownList>
                            </div>
                        </div>
                        
                        <!-- Brought the image upload into the exact same grid alignment -->
                        <div class="row mb-3 align-items-center">
                            <asp:Label runat="server" Text="Cover Image:" CssClass="col-sm-3 col-form-label fw-bold" />
                            <div class="col-sm-9">
                                <asp:FileUpload ID="fuCourseImage" runat="server" CssClass="form-control mb-1" accept=".png,.jpg,.jpeg" />
                                <small class="text-muted">Leave blank to keep the current image.</small>
                            </div>
                        </div>
                    </div>

                    <div class="modal-footer bg-light">
                        <asp:Button ID="btnDelete" runat="server"
                            Text="Delete"
                            CssClass="btn btn-danger me-auto"
                            OnClick="btnDelete_Click"
                            OnClientClick="return confirm('Are you sure you want to delete this course? This cannot be undone.');" />

                        <asp:Button ID="btnCancel" runat="server"
                            Text="Cancel"
                            CssClass="btn btn-secondary"
                            data-bs-dismiss="modal"
                            OnClientClick="return false;" />

                        <asp:Button ID="btnSave" runat="server"
                            Text="Save"
                            CssClass="btn text-white"
                            style="background-color:#333;"
                            OnClick="btnSave_Click" 
                            OnClientClick="if(typeof tinymce !== 'undefined') { tinymce.triggerSave(); }" />
                    </div>

                </ContentTemplate>
                
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnSave" />
                </Triggers>

            </asp:UpdatePanel>

        </div>
    </div>
</div>