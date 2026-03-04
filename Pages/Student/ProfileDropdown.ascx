<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileDropdown.ascx.cs" Inherits="WAPP.Pages.Student.ProfileDropdown" %>

<div class="dropdown">
    <button class="profile-icon-btn dropdown-toggle" type="button" id="profileDropdown" data-bs-toggle="dropdown" aria-expanded="false">
        <asp:Image ID="imgProfileNavbar" runat="server" ImageUrl="~/Images/profile_m.png" />
    </button>
    <ul class="dropdown-menu dropdown-menu-end p-3 shadow" aria-labelledby="profileDropdown" style="min-width: 220px;">
        <li class="text-center mb-2">
            <asp:Image ID="imgDropdownThumb" runat="server" ImageUrl="~/Images/profile_m.png" CssClass="avatar-small mb-2" style="width:50px; height:50px;" />
            <p class="mb-0 fw-bold"><asp:Label ID="lblUserFullName" runat="server"></asp:Label></p>
            <small class="text-muted">Student</small>
        </li>
        <li><hr class="dropdown-divider"></li>
        <li><asp:HyperLink ID="hlProfile" runat="server" CssClass="dropdown-item" NavigateUrl="~/Pages/Student/Profile.aspx">My Profile</asp:HyperLink></li>
        <li><asp:HyperLink ID="hlSettings" runat="server" CssClass="dropdown-item" NavigateUrl="~/Pages/Student/Setting.aspx">Settings</asp:HyperLink></li>
        <li><hr class="dropdown-divider"></li>
        <li>
            <asp:LinkButton ID="btnSignOut" runat="server" CssClass="dropdown-item text-danger" OnClick="btnSignOut_Click">
                <i class="bi bi-box-arrow-right"></i> Sign Out
            </asp:LinkButton>
        </li>
    </ul>
</div>