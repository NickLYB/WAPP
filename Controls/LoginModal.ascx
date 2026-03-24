<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginModal.ascx.cs" Inherits="WAPP.Controls.LoginModal" %>

<div class="modal fade" id="loginModal" tabindex="-1" aria-labelledby="loginModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered" style="max-width: 400px;">
        <div class="modal-content p-4">
            <asp:UpdatePanel ID="upLogin" runat="server">
                <ContentTemplate>
                    <div class="text-center mb-4">
                        <button type="button" class="btn-close float-end" data-bs-dismiss="modal" aria-label="Close"></button>
                        <h4 class="mt-2">EduConnect</h4>
                        <p class="text-muted">Enter your details to login</p>
                    </div>

                    <div class="modal-body p-0">
                        <div class="mb-3">
                            <asp:TextBox ID="txtEmail" runat="server" placeholder="Email" CssClass="login-input w-100"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Password" CssClass="login-input w-100"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click" CssClass="btn btn-primary w-100 py-2 mb-3" />
                        <div class="text-center">
                            <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Size="Small"></asp:Label>
                        </div>
                    </div>

                    <div class="text-center mt-3 border-top pt-3">
                        <a href="javascript:void(0);" 
                           class="small text-decoration-none" 
                           data-bs-toggle="modal" 
                           data-bs-target="#forgotPasswordModal" 
                           data-bs-dismiss="modal">Forgot Password?</a>

                        <span class="mx-2">|</span>
    
                        <a href="javascript:void(0);" 
                           class="small text-decoration-none" 
                           data-bs-toggle="modal" 
                           data-bs-target="#signUpModal" 
                           data-bs-dismiss="modal">Sign Up</a>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>