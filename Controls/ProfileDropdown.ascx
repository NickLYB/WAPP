<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileDropdown.ascx.cs" Inherits="WAPP.Controls.ProfileDropdown" %>

<div class="dropdown">
    <button class="profile-icon-btn dropdown-toggle" type="button" id="profileDropdown" data-bs-toggle="dropdown" aria-expanded="false" style="border: none; background: transparent;">
        <asp:Image ID="imgProfileNavbar" runat="server" ImageUrl="~/Images/profile_m.png" style="width: 45px; height: 45px; border-radius: 50%; object-fit: cover; border: 2px solid #fff; box-shadow: 0 2px 5px rgba(0,0,0,0.1);" />
    </button>
    <ul class="dropdown-menu dropdown-menu-end p-3 shadow" aria-labelledby="profileDropdown" style="min-width: 220px; border-radius: var(--ec-radius-md); border: 1px solid #eaeaea;">
        <li class="text-center mb-2">
            <asp:Image ID="imgDropdownThumb" runat="server" ImageUrl="~/Images/profile_m.png" CssClass="mb-2" style="width:60px; height:60px; border-radius: 50%; object-fit: cover;" />
            <p class="mb-0 fw-bold" style="color: var(--ec-text-main);">
                <asp:Label ID="lblUserFullName" runat="server"></asp:Label>
            </p>
            <asp:Label ID="lblUserRole" runat="server" CssClass="text-muted small" style="text-transform: uppercase; font-weight: 600; letter-spacing: 0.5px;"></asp:Label>
        </li>
        <li><hr class="dropdown-divider"></li>
        
        <li><asp:HyperLink ID="hlProfile" runat="server" CssClass="dropdown-item fw-medium">My Profile</asp:HyperLink></li>
        <li><asp:HyperLink ID="hlSettings" runat="server" CssClass="dropdown-item fw-medium">Settings</asp:HyperLink></li>
        
        <li><hr class="dropdown-divider"></li>
        <li>
            <asp:LinkButton ID="btnSignOut" runat="server" CssClass="dropdown-item text-danger fw-bold" OnClick="btnSignOut_Click">
                <i class="bi bi-box-arrow-right me-2"></i> Sign Out
            </asp:LinkButton>
        </li>
    </ul>
</div>