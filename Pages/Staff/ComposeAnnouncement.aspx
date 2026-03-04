<%@ Page Title="Compose Announcement" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="ComposeAnnouncement.aspx.cs" Inherits="WAPP.Pages.Staff.ComposeAnnouncement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script type="text/javascript">
        function openScheduleModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('scheduleModal')).show();
            return false; // Prevent postback
        }
    </script>
    <style>
        /* FULL WINDOW FIT STYLES (No separate background box) */
        .compose-container { 
            background-color: transparent; /* Blends perfectly with your master page */
            min-height: 80vh; 
            width: 100%;       
            padding: 20px 40px; 
            box-sizing: border-box; 
        }
        
        .page-title { color: #a10d2d; font-weight: 900; font-size: 32px; letter-spacing: 1px; margin-bottom: 5px; }
        .sub-title { font-weight: 700; font-size: 22px; color: #333; margin-bottom: 40px; }
        .form-label-custom { font-weight: 800; font-size: 18px; color: #000; width: 160px; text-align: right; padding-right: 20px; }
        
        /* Custom Radio Buttons */
        .radio-group input[type="radio"] { display: none; }
        .radio-group label {
            background-color: #fff; border: 2px solid #a10d2d; color: #a10d2d;
            padding: 10px 40px; border-radius: 30px; font-weight: bold; cursor: pointer; margin-right: 15px;
            transition: all 0.3s ease-in-out;
            box-shadow: 0px 4px 6px rgba(0,0,0,0.05);
        }
        .radio-group input[type="radio"]:checked + label { background-color: #a10d2d; color: #fff; box-shadow: inset 0px 4px 6px rgba(0,0,0,0.2); }
        .radio-group label:hover { transform: translateY(-2px); box-shadow: 0px 6px 10px rgba(0,0,0,0.1); }

        /* Full Width Inputs */
        .input-wrapper { flex-grow: 1; max-width: 1200px; }
        .custom-input { border: 2px solid #a10d2d; border-radius: 12px; padding: 15px 20px; font-size: 16px; width: 100%; box-shadow: inset 0px 2px 4px rgba(0,0,0,0.05); }
        .custom-textarea { border: 2px solid #a10d2d; border-radius: 12px; padding: 20px; font-size: 16px; width: 100%; height: 400px; resize: vertical; box-shadow: inset 0px 2px 4px rgba(0,0,0,0.05); }
        
        /* Action Buttons */
        .btn-send { background-color: #a10d2d; color: white; border-radius: 12px; padding: 12px 40px; font-weight: bold; font-size: 18px; border: none; transition: 0.3s; box-shadow: 0px 4px 8px rgba(161,13,45,0.3); cursor: pointer; }
        .btn-send:hover { background-color: #800a23; color: white; transform: translateY(-2px); }
        
        .btn-schedule { background-color: #198754; color: white; border-radius: 12px; padding: 12px 40px; font-weight: bold; font-size: 18px; border: none; transition: 0.3s; box-shadow: 0px 4px 8px rgba(25,135,84,0.3); cursor: pointer; }
        .btn-schedule:hover { background-color: #146c43; color: white; transform: translateY(-2px); }
        
        .btn-cancel { border-radius: 12px; padding: 12px 40px; font-weight: bold; font-size: 18px; transition: 0.3s; text-decoration: none; display: inline-block; }
        .btn-cancel:hover { transform: translateY(-2px); }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="compose-container">
        
        <h1 class="page-title text-uppercase">Operational Announcement Management</h1>
        <h3 class="sub-title">Compose New Operational Announcement</h3>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold" Visible="false"></asp:Label>

        <div class="d-flex align-items-center mb-5">
            <div class="form-label-custom">Target Group:</div>
            <div class="radio-group input-wrapper">
                <asp:RadioButtonList ID="rblTarget" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="d-inline">
                    <asp:ListItem Value="0" Selected="True">All</asp:ListItem>
                    <asp:ListItem Value="4">Student</asp:ListItem>
                    <asp:ListItem Value="3">Tutor</asp:ListItem>
                </asp:RadioButtonList>
            </div>
        </div>

        <div class="d-flex align-items-start mb-4">
            <div class="form-label-custom pt-3">Title:</div>
            <div class="input-wrapper">
                <asp:TextBox ID="txtTitle" runat="server" CssClass="custom-input" Placeholder="Type your announcement title here..."></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ErrorMessage="Title is required" ForeColor="Red" Display="Dynamic" ValidationGroup="Compose" CssClass="fw-bold mt-2 d-block" />
            </div>
        </div>

        <div class="d-flex align-items-start mb-5">
            <div class="form-label-custom pt-3">Message:</div>
            <div class="input-wrapper">
                <asp:TextBox ID="txtMessage" runat="server" CssClass="custom-textarea" TextMode="MultiLine" Placeholder="Type your announcement message here..."></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvMessage" runat="server" ControlToValidate="txtMessage" ErrorMessage="Message is required" ForeColor="Red" Display="Dynamic" ValidationGroup="Compose" CssClass="fw-bold mt-2 d-block" />
            </div>
        </div>

        <div class="d-flex align-items-start" style="margin-left: 160px;">
            <asp:Button ID="btnSendNow" runat="server" Text="Send Now" CssClass="btn-send me-3" OnClick="btnSendNow_Click" ValidationGroup="Compose" />
            <button type="button" class="btn-schedule" onclick="return openScheduleModal();">Schedule Publish</button>
            <asp:HyperLink ID="hlCancel" runat="server" NavigateUrl="~/Pages/Staff/AnnouncementManagement.aspx" CssClass="btn btn-secondary btn-cancel ms-3">Cancel</asp:HyperLink>
        </div>

        <div class="modal fade" id="scheduleModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow-lg" style="border: 4px solid #a10d2d !important; border-radius: 15px;">
                    <div class="modal-header text-white" style="background-color: #a10d2d;">
                        <h5 class="modal-title fw-bold text-uppercase">Schedule Announcement</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-5 text-center">
                        <h5 class="text-dark fw-bold mb-4">Select a date and time to publish:</h5>
                        <asp:TextBox ID="txtScheduleDate" runat="server" CssClass="form-control text-center mx-auto mb-4 custom-input" TextMode="DateTimeLocal" style="max-width: 350px;"></asp:TextBox>
                        
                        <div class="d-flex justify-content-center gap-3 mt-4">
                            <asp:Button ID="btnConfirmSchedule" runat="server" Text="Confirm Schedule" CssClass="btn btn-success px-5 py-3 fw-bold rounded-pill" OnClick="btnConfirmSchedule_Click" ValidationGroup="Compose" />
                            <button type="button" class="btn btn-secondary px-5 py-3 fw-bold rounded-pill" data-bs-dismiss="modal">Cancel</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>

</asp:Content>