<%@ Page Title="Compose Announcement" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="ComposeAnnouncement.aspx.cs" Inherits="WAPP.Pages.Staff.ComposeAnnouncement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    
    <script type="text/javascript">
        function openScheduleModal() {
            // Check ASP.NET validation before opening the modal!
            if (typeof Page_ClientValidate === "function") {
                var isValid = Page_ClientValidate("Compose");
                if (!isValid) {
                    return false;
                }
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('scheduleModal')).show();
            return false;
        }
    </script>
    <style>
        .btn-outline-primary { border-color: var(--ec-primary); color: var(--ec-primary); }
        .btn-outline-primary:hover { background-color: var(--ec-primary); color: white; }
        
        .btn-outline-success { border-color: #198754; color: #198754; }
        .btn-outline-success:hover { background-color: #198754; color: white; }
        
        .btn-outline-secondary { border-color: #6c757d; color: #6c757d; }
        .btn-outline-secondary:hover { background-color: #6c757d; color: white; }

        .radio-group > span {
            display: flex !important;
            flex-direction: row !important;
            flex-wrap: nowrap !important;
            gap: 15px;
            align-items: center;
        }
        
        .radio-group input[type="radio"] { 
            display: none; 
        }
        
        .radio-group label {
            background-color: #fff; 
            border: 1px solid #ced4da; 
            color: #495057;
            padding: 8px 25px; 
            border-radius: 8px; 
            cursor: pointer; 
            margin: 0 !important;
            transition: all 0.2s ease-in-out;
            font-size: 0.95rem;
            font-weight: 600;
            white-space: nowrap; 
        }
        
        .radio-group input[type="radio"]:checked + label { 
            background-color: var(--ec-primary); 
            color: #fff; 
            border-color: var(--ec-primary);
        }
        
        .radio-group label:hover { 
            background-color: #f8f9fa; 
            border-color: #b3b3b3; 
            color: #333; 
        }
        
        .radio-group input[type="radio"]:checked + label:hover { 
            background-color: #0b5ed7; 
            color: white; 
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="ec-staff-wrapper">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StaffMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-1">Compose New Operational Announcement</h2>

        <div class="ec-staff-card p-4 p-md-5 shadow-sm mt-4">
            <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold" Visible="false"></asp:Label>

            <div class="row mb-4 align-items-center">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Target Group:</label>
                <div class="col-md-10">
                    <div class="radio-group">
                        <asp:RadioButtonList ID="rblTarget" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                            <asp:ListItem Value="0" Selected="True">All Roles</asp:ListItem>
                            <asp:ListItem Value="4">Student Only</asp:ListItem>
                            <asp:ListItem Value="3">Tutor Only</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                </div>
            </div>

            <div class="row mb-4 align-items-start">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end">Title:</label>
                <div class="col-md-10">
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control ec-form-control" Placeholder="Type your announcement title here..."></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ErrorMessage="Title is required" ForeColor="#dc3545" Display="Dynamic" ValidationGroup="Compose" CssClass="small fw-bold mt-1" />
                </div>
            </div>

            <div class="row mb-5 align-items-start">
                <label class="col-md-2 col-form-label fw-bold text-muted text-md-end pt-2">Message:</label>
                <div class="col-md-10">
                    <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control ec-form-control" TextMode="MultiLine" Rows="8" Placeholder="Type your announcement message here..."></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvMessage" runat="server" ControlToValidate="txtMessage" ErrorMessage="Message is required" ForeColor="#dc3545" Display="Dynamic" ValidationGroup="Compose" CssClass="small fw-bold mt-1" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-10 offset-md-2 d-flex gap-3">
                    <asp:Button ID="btnSendNow" runat="server" Text="Send Now" CssClass="btn btn-outline-primary rounded-pill px-4 fw-bold shadow-sm" OnClick="btnSendNow_Click" ValidationGroup="Compose" />
                    
                    <button type="button" class="btn btn-outline-success rounded-pill px-4 fw-bold shadow-sm" onclick="return openScheduleModal();">Schedule Publish</button>
                    
                    <asp:HyperLink ID="hlCancel" runat="server" NavigateUrl="~/Pages/Staff/AnnouncementManagement.aspx" CssClass="btn btn-outline-secondary rounded-pill px-4 fw-bold shadow-sm">Cancel</asp:HyperLink>
                </div>
            </div>

        </div>

        <div class="modal fade" id="scheduleModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                    <div class="modal-header ec-modal-header bg-light border-0 pb-0 position-relative">
                        <h5 class="modal-title fw-bold text-dark text-uppercase">Schedule Announcement</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" style="position:absolute; top:20px; right:20px;"></button>
                    </div>
                    <div class="modal-body ec-modal-body p-4 pt-3 text-center">
                        <h6 class="text-muted fw-bold mb-4">Select a date and time to automatically publish:</h6>
                        
                        <asp:TextBox ID="txtScheduleDate" runat="server" CssClass="form-control text-center mx-auto mb-4 ec-form-control" TextMode="DateTimeLocal" style="max-width: 300px;"></asp:TextBox>
                        
                        <div class="d-flex justify-content-center gap-3 mt-2">
                            <asp:Button ID="btnConfirmSchedule" runat="server" Text="Confirm Schedule" CssClass="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" OnClick="btnConfirmSchedule_Click" ValidationGroup="Compose" />
                            <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill" data-bs-dismiss="modal">Cancel</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>

</asp:Content>