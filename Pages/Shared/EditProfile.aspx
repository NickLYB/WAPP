<%@ Page Title="Edit Profile" Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="EditProfile.aspx.cs" Inherits="WAPP.Pages.Shared.EditProfile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <style>
        .profile-wrapper { max-width: 800px; margin: 0 auto; padding: 40px 20px; }
        .profile-card { background: #fff; border-radius: 15px; box-shadow: 0 10px 30px rgba(0,0,0,0.08); padding: 40px; border-top: 5px solid #0d6efd; }
        .avatar-preview { width: 120px; height: 120px; border-radius: 50%; object-fit: cover; border: 4px solid #fff; box-shadow: 0 4px 15px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .form-control:focus { border-color: #0d6efd; box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25); }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="profile-wrapper">
        <h2 class="fw-bold mb-4"><i class="bi bi-person-circle me-2 text-primary"></i>My Profile</h2>
        
        <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block" Visible="false"></asp:Label>

        <div class="profile-card">
            <div class="text-center">
                <asp:Image ID="imgAvatar" runat="server" ImageUrl="~/Images/profile_m.png" CssClass="avatar-preview" />
                <h4 class="fw-bold"><asp:Literal ID="litFullName" runat="server"></asp:Literal></h4>
                <p class="text-muted"><asp:Literal ID="litRole" runat="server"></asp:Literal></p>
            </div>

            <hr class="my-4" />

            <div class="row g-4">
                <div class="col-md-6">
                    <label class="form-label fw-bold">First Name</label>
                    <asp:TextBox ID="txtFname" runat="server" CssClass="form-control" ValidationGroup="UpdateProfile"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvFname" runat="server" ControlToValidate="txtFname" ValidationGroup="UpdateProfile" ErrorMessage="First name is required." CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">Last Name</label>
                    <asp:TextBox ID="txtLname" runat="server" CssClass="form-control" ValidationGroup="UpdateProfile"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvLname" runat="server" ControlToValidate="txtLname" ValidationGroup="UpdateProfile" ErrorMessage="Last name is required." CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" ValidationGroup="UpdateProfile"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="UpdateProfile" ErrorMessage="Email is required." CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" ValidationGroup="UpdateProfile"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone" ValidationGroup="UpdateProfile" ErrorMessage="Phone number is required." CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">Date of Birth</label>
                    <asp:TextBox ID="txtDob" runat="server" CssClass="form-control" TextMode="Date" ValidationGroup="UpdateProfile"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvDob" runat="server" ControlToValidate="txtDob" ValidationGroup="UpdateProfile" ErrorMessage="Date of Birth is required." CssClass="text-danger small" Display="Dynamic" />
                </div> 
            </div>
            
            <div class="text-end mt-5">
                <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary px-5 py-2 fw-bold rounded-pill shadow-sm" ValidationGroup="UpdateProfile" OnClick="btnSave_Click" />
            &nbsp;</div>
        </div>
    </div>
</asp:Content>