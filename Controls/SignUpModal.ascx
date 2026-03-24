<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SignUpModal.ascx.cs" Inherits="WAPP.Controls.SignUpModal" %>

<div class="modal fade" id="signUpModal" tabindex="-1" aria-labelledby="signUpModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered" style="max-width: 500px;">
        <div class="modal-content p-4">
            <asp:UpdatePanel ID="upSignUp" runat="server" UpdateMode="Conditional">
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnSignUp" />
                </Triggers>
                <ContentTemplate>
                    <div class="text-center mb-4">
                        <button type="button" class="btn-close float-end" data-bs-dismiss="modal" aria-label="Close"></button>
                        <h4 class="mt-2">Join EduConnect</h4>
                        <p class="text-muted">Create your account to get started</p>
                    </div>

                    <div class="modal-body p-0">
                        <div class="row g-2 mb-3">
                            <div class="col-md-6">
                                <asp:TextBox ID="txtFirstName" runat="server" placeholder="First Name" CssClass="login-input w-100"></asp:TextBox>
                            </div>
                            <div class="col-md-6">
                                <asp:TextBox ID="txtLastName" runat="server" placeholder="Last Name" CssClass="login-input w-100"></asp:TextBox>
                            </div>
                        </div>

                        <div class="mb-3">
                            <label class="form-label small text-muted">Date of Birth</label>
                            <asp:TextBox ID="txtDob" runat="server" TextMode="Date" CssClass="login-input w-100"></asp:TextBox>
                        </div>

                        <div class="mb-3">
                            <asp:TextBox ID="txtContact" runat="server" placeholder="Contact Number (e.g. 0123456789)" CssClass="login-input w-100"></asp:TextBox>
                        </div>

                        <div class="mb-3">
                            <asp:TextBox ID="txtEmail" runat="server" placeholder="Email Address" CssClass="login-input w-100"></asp:TextBox>
                        </div>
                        <div class="row g-2 mb-1">
                            <div class="col-md-6">
                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Password" CssClass="login-input w-100"></asp:TextBox>
                            </div>
                            <div class="col-md-6">
                                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" placeholder="Confirm Password" CssClass="login-input w-100"></asp:TextBox>
                            </div>
                        </div>
                        <div class="mb-3">
                            <small class="text-muted" style="font-size: 11px;">Password must be at least 8 characters and include a number, uppercase, lowercase, and special character.</small>
                        </div>

                        <div class="mb-3">
                            <label class="form-label small text-muted d-block">Register as:</label>
                            <asp:RadioButtonList ID="rblRole" runat="server" RepeatDirection="Horizontal" CssClass="w-100" AutoPostBack="true" OnSelectedIndexChanged="rblRole_SelectedIndexChanged">
                                <asp:ListItem Value="4" Selected="True">&nbsp;Student&nbsp;&nbsp;</asp:ListItem>
                                <asp:ListItem Value="3">&nbsp;Tutor</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>

                        <%-- Tutor Only Section --%>
                        <asp:Panel ID="pnlTutorDocs" runat="server" Visible="false" CssClass="mb-3 p-3 border rounded bg-light">
                            <label class="form-label small fw-bold">Verification Document (PDF/Image)</label>
                            <asp:FileUpload ID="fileVerification" runat="server" CssClass="form-control form-control-sm" />
                            <small class="text-muted" style="font-size: 10px;">Tutors must provide certification for approval.</small>
                        </asp:Panel>

                        <asp:Button ID="btnSignUp" runat="server" Text="Create Account" OnClick="btnSignUp_Click" CssClass="btn btn-success w-100 py-2 mb-2" />
                        
                        <div class="text-center">
                            <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Size="Small"></asp:Label>
                        </div>
                    </div>

                    <div class="text-center mt-3 border-top pt-3">
                        <span class="text-muted small">Already have an account?</span>
                        <a href="javascript:void(0);" data-bs-toggle="modal" data-bs-target="#loginModal" data-bs-dismiss="modal" class="small fw-bold text-decoration-none">Login</a>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>