<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileDropdown.ascx.cs" Inherits="WAPP.Pages.Shared.ProfileDropdown" %>

<div class="dropdown">
    <button class="profile-icon-btn dropdown-toggle" type="button" id="profileDropdown" data-bs-toggle="dropdown" aria-expanded="false" style="background: none; border: none; padding: 0;">
        <asp:Image ID="imgProfileNavbar" runat="server" ImageUrl="~/Images/profile_m.png" style="width: 40px; height: 40px; border-radius: 50%; object-fit: cover;" />
    </button>
    <ul class="dropdown-menu dropdown-menu-end p-3 shadow-sm border-0" aria-labelledby="profileDropdown" style="min-width: 220px; border-radius: 12px; margin-top: 10px;">
        <li class="text-center mb-2">
            <asp:Image ID="imgDropdownThumb" runat="server" ImageUrl="~/Images/profile_m.png" CssClass="avatar-small mb-2" style="width:60px; height:60px; border-radius: 50%; object-fit: cover;" />
            <p class="mb-0 fw-bold" style="color: #333; font-size: 1.1rem;"><asp:Label ID="lblUserFullName" runat="server"></asp:Label></p>
            
            <small class="text-muted fw-bold" style="color: #0d6efd !important;"><asp:Literal ID="litUserRole" runat="server"></asp:Literal></small>
        </li>
        <li><hr class="dropdown-divider" style="margin: 15px 0;"></li>
        
        <li>
            <asp:HyperLink ID="hlProfile" runat="server" CssClass="dropdown-item py-2 fw-bold text-secondary" NavigateUrl="~/Pages/Shared/EditProfile.aspx">
                <i class="bi bi-person-circle me-2 text-primary"></i> My Profile
            </asp:HyperLink>
        </li>
        <li>
            <asp:HyperLink ID="hlSettings" runat="server" CssClass="dropdown-item py-2 fw-bold text-secondary" NavigateUrl="~/Pages/Shared/Setting.aspx">
                <i class="bi bi-gear-fill me-2 text-secondary"></i> Settings
            </asp:HyperLink>
        </li>
        
        <li><hr class="dropdown-divider" style="margin: 10px 0;"></li>
        <li>
            <asp:LinkButton ID="btnSignOut" runat="server" CssClass="dropdown-item text-danger py-2 fw-bold" OnClick="btnSignOut_Click">
                <i class="bi bi-box-arrow-right me-2"></i> Sign Out
            </asp:LinkButton>
        </li>
    </ul>
</div>