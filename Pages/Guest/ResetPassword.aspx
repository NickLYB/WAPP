<%@ Page Title="Reset Password" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="WAPP.Pages.Guest.ResetPassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .reset-wrapper {
            min-height: 70vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 40px 20px;
        }
        .reset-card {
            max-width: 450px;
            width: 100%;
            padding: 40px;
            border-radius: var(--ec-radius-lg);
            background: #fff;
            box-shadow: 0 10px 30px rgba(0,0,0,0.08);
            border: 1px solid var(--ec-border-light);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="reset-wrapper">
        <div class="reset-card text-center">
            
            <div class="mb-4">
                <i class="bi bi-shield-lock-fill text-primary" style="font-size: 3rem;"></i>
                <h3 class="fw-bold mt-2">Create New Password</h3>
                <p class="text-muted small">Your identity has been verified. Please enter a strong new password below.</p>
            </div>
            <asp:Panel ID="pnlResetPassword" runat="server" DefaultButton="btnResetPassword">
                
                <div class="text-start mb-3">
                    <label class="form-label fw-bold small text-muted">New Password</label>
                    <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" placeholder="Enter new password" CssClass="form-control py-2"></asp:TextBox>
                </div>

                <div class="text-start mb-2">
                    <label class="form-label fw-bold small text-muted">Confirm Password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" placeholder="Confirm new password" CssClass="form-control py-2"></asp:TextBox>
                </div>

                <div class="text-start mb-3">
                    <small class="text-muted" style="font-size: 12px; line-height: 1.2; display: block;">
                        Password must be at least 8 characters and include a number, uppercase, lowercase, and special character.
                    </small>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="d-block mb-3 fw-bold small"></asp:Label>

                <asp:Button ID="btnResetPassword" runat="server" Text="Update Password" OnClick="btnResetPassword_Click" CssClass="btn btn-primary w-100 py-2 fw-bold rounded-pill shadow-sm" />
            
            </asp:Panel>
            
            <div class="mt-4 border-top pt-3">
                <asp:HyperLink ID="hlBackToHome" runat="server" NavigateUrl="~/Pages/Guest/Home.aspx" CssClass="text-muted small text-decoration-none">
                    <i class="bi bi-arrow-left me-1"></i> Return to Home
                </asp:HyperLink>
            </div>

        </div>
    </div>
</asp:Content>