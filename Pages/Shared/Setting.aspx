<%@ Page Title="Settings" Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="Settings.aspx.cs" Inherits="WAPP.Pages.Shared.Settings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <style>
        .settings-wrapper { max-width: 900px; margin: 0 auto; padding: 40px 20px; }
        .settings-card { background: #fff; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); padding: 30px; margin-bottom: 30px; }
        .card-title { font-weight: 800; border-bottom: 2px solid #f1f1f1; padding-bottom: 15px; margin-bottom: 25px; color: #333; }
        
        /* Dark Mode Toggle Switch CSS */
        .theme-switch { display: flex; align-items: center; justify-content: space-between; padding: 15px; background: #f8f9fa; border-radius: 10px; }
        .switch { position: relative; display: inline-block; width: 60px; height: 34px; }
        .switch input { opacity: 0; width: 0; height: 0; }
        .slider { position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; background-color: #ccc; transition: .4s; border-radius: 34px; }
        .slider:before { position: absolute; content: ""; height: 26px; width: 26px; left: 4px; bottom: 4px; background-color: white; transition: .4s; border-radius: 50%; }
        input:checked + .slider { background-color: #0d6efd; }
        input:checked + .slider:before { transform: translateX(26px); }

        /* Generic Dark Mode Body Class (Applies to the whole site when toggled) */
        body.dark-mode { background-color: #121212 !important; color: #ffffff !important; }
        body.dark-mode .settings-card, body.dark-mode .card, body.dark-mode .profile-card { background-color: #1e1e1e !important; color: #ffffff !important; border-color: #333 !important; }
        body.dark-mode .card-title { border-color: #444 !important; color: #fff !important; }
        body.dark-mode .theme-switch { background-color: #2d2d2d !important; }
        body.dark-mode .form-control { background-color: #2d2d2d !important; color: #fff !important; border-color: #444 !important; }
    </style>

    <script>
        // Check local storage for theme on page load
        document.addEventListener("DOMContentLoaded", function () {
            const toggle = document.getElementById("themeToggle");
            if (localStorage.getItem("theme") === "dark") {
                document.body.classList.add("dark-mode");
                toggle.checked = true;
            }

            // Listen for toggle switch
            toggle.addEventListener("change", function () {
                if (this.checked) {
                    document.body.classList.add("dark-mode");
                    localStorage.setItem("theme", "dark");
                } else {
                    document.body.classList.remove("dark-mode");
                    localStorage.setItem("theme", "light");
                }
            });
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="settings-wrapper">
        <h2 class="fw-bold mb-4"><i class="bi bi-gear-fill me-2 text-secondary"></i>Account Settings</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block" Visible="false"></asp:Label>

        <div class="settings-card">
            <h4 class="card-title"><i class="bi bi-palette-fill text-primary me-2"></i>Appearance</h4>
            <div class="theme-switch">
                <div>
                    <h6 class="fw-bold mb-1">Dark Mode</h6>
                    <small class="text-muted">Switch the website theme to dark mode to reduce eye strain.</small>
                </div>
                <label class="switch">
                    <input type="checkbox" id="themeToggle">
                    <span class="slider"></span>
                </label>
            </div>
        </div>

        <div class="settings-card">
            <h4 class="card-title"><i class="bi bi-shield-lock-fill text-warning me-2"></i>Security</h4>
            <div class="row g-3">
                <div class="col-md-12">
                    <label class="form-label fw-bold">Current Password</label>
                    <asp:TextBox ID="txtCurrentPass" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtCurrentPass" ErrorMessage="Required" ValidationGroup="PasswordGroup" CssClass="text-danger small"></asp:RequiredFieldValidator>
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">New Password</label>
                    <asp:TextBox ID="txtNewPass" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfv2" runat="server" ControlToValidate="txtNewPass" ErrorMessage="Required" ValidationGroup="PasswordGroup" CssClass="text-danger small"></asp:RequiredFieldValidator>
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">Confirm New Password</label>
                    <asp:TextBox ID="txtConfirmPass" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    <asp:CompareValidator ID="cvPass" runat="server" ControlToValidate="txtConfirmPass" ControlToCompare="txtNewPass" ErrorMessage="Passwords do not match." ValidationGroup="PasswordGroup" CssClass="text-danger small"></asp:CompareValidator>
                </div>
                <div class="col-12 text-end mt-3">
                    <asp:Button ID="btnChangePassword" runat="server" Text="Update Password" CssClass="btn btn-warning fw-bold px-4 rounded-pill" ValidationGroup="PasswordGroup" OnClick="btnChangePassword_Click" />
                </div>
            </div>
        </div>

        <div class="settings-card" style="border: 2px solid #f8d7da; background-color: #fff5f5;">
            <h4 class="card-title text-danger" style="border-bottom-color: #f5c2c7;"><i class="bi bi-exclamation-triangle-fill me-2"></i>Danger Zone</h4>
            <p class="text-dark">Once you delete your account, there is no going back. Please be certain.</p>
            
            <button type="button" class="btn btn-danger fw-bold px-4 rounded-pill" data-bs-toggle="modal" data-bs-target="#deleteModal">
                Delete Account
            </button>
        </div>

        <div class="modal fade" id="deleteModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow-lg" style="border: 4px solid #dc3545 !important; border-radius: 15px;">
                    <div class="modal-header bg-danger text-white">
                        <h5 class="modal-title fw-bold">Delete Account</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-4 text-center">
                        <h4 class="fw-bold mb-3">Are you absolutely sure?</h4>
                        <p class="text-muted">This action will permanently remove your account from EduConnect. All your data will be lost.</p>
                    </div>
                    <div class="modal-footer justify-content-center border-0 pb-4">
                        <button type="button" class="btn btn-secondary px-4 rounded-pill fw-bold" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnDeleteAccount" runat="server" Text="Yes, Delete My Account" CssClass="btn btn-outline-danger px-4 rounded-pill fw-bold" OnClick="btnDeleteAccount_Click" />
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>