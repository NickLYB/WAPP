<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForgotPasswordModal.ascx.cs" Inherits="WAPP.Controls.ForgotPasswordModal" %>

<div class="modal fade" id="forgotPasswordModal" tabindex="-1" aria-labelledby="forgotPasswordModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered" style="max-width: 400px;">
        <div class="modal-content p-4">
            <asp:UpdatePanel ID="upForgotPassword" runat="server">
                <ContentTemplate>
                    <div class="text-center mb-4">
                        <button type="button" class="btn-close float-end" data-bs-dismiss="modal" aria-label="Close"></button>
                        <h4 class="mt-2">Reset Password</h4>
                        <p class="text-muted small">Enter your email and we'll send you an OTP to reset your password.</p>
                    </div>

                    <div class="modal-body p-0">
                        <div class="mb-3">
                            <asp:TextBox ID="txtResetEmail" runat="server" placeholder="Email Address" CssClass="login-input w-100"></asp:TextBox>
                        </div>
                        
                        <asp:Button ID="btnSendOtp" runat="server" Text="Send OTP" OnClick="btnSendOtp_Click" CssClass="btn btn-primary w-100 py-2 mb-3" />
                        
                        <div class="text-center">
                            <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Size="Small"></asp:Label>
                        </div>
                    </div>

                    <div class="text-center mt-3 border-top pt-3">
                        <span class="text-muted small">Remember your password?</span>
                        <a href="javascript:void(0);" 
                           class="small fw-bold text-decoration-none" 
                           data-bs-toggle="modal" 
                           data-bs-target="#loginModal" 
                           data-bs-dismiss="modal">Login here</a>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>