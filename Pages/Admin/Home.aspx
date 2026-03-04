<%@ Page Title="Admin Home" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true"
    CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Admin.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-layout-split">
        
        <div class="layout-main-70">
            
            <div class="ec-hero">
                <asp:Image ID="imgAdmin" runat="server" ImageUrl="~/Images/profile_f.png" CssClass="ec-hero-avatar" />
                <h2 class="ec-hero-text">Welcome back, <asp:Label ID="lblAdminName" runat="server"></asp:Label>!</h2>
            </div>

            <div class="layout-grid-half">
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5>Recent Logins</h5>
                    </div>
                    <div class="ec-glass-card-body text-center py-4">
                        <span class="ec-stat-giant text-muted">-</span>
                        <p class="text-muted small mt-2 mb-0">Data Pending</p>
                    </div>
                </div>

                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5>Active Sessions</h5>
                    </div>
                    <div class="ec-glass-card-body text-center py-4">
                        <span class="ec-stat-giant text-muted">-</span>
                        <p class="text-muted small mt-2 mb-0">Data Pending</p>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card p-0 overflow-hidden">
                <div class="ec-glass-card-header px-4 pt-4 border-0">
                    <h5>Recent User Registration</h5>
                    <asp:HyperLink ID="hlViewAllUsers" runat="server" NavigateUrl="~/Pages/Admin/ManageUsers.aspx" CssClass="ec-link-primary m-0">View All &rarr;</asp:HyperLink>
                </div>
                <div class="px-4 pb-4">
                    <asp:GridView ID="gvRecentUsers" runat="server" AutoGenerateColumns="False" 
                        CssClass="table table-striped align-middle mb-0" GridLines="None" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="ID" HeaderText="ID" ItemStyle-Width="80px" />
                            <asp:BoundField DataField="Name" HeaderText="Name" />
                            <asp:BoundField DataField="Role" HeaderText="Role" ItemStyle-Width="120px" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                        </Columns>
                    </asp:GridView>
                    
                    <asp:Label ID="lblUserEmpty" runat="server" Visible="false" CssClass="ec-empty-state d-block mt-3">
                        No recent registrations found.
                    </asp:Label>
                </div>
            </div>

        </div>

        <div class="layout-sidebar-30">
            
            <div class="ec-glass-card">
                <div class="ec-glass-card-header border-0 pb-0">
                    <h5>Quick Actions</h5>
                </div>
                <p class="text-muted small mt-2 mb-3">Manage core system data.</p>
                <div style="display: flex; flex-direction: column; gap: 10px;">
                    <asp:Button ID="btnAddUser" runat="server" Text="+ Add New User" 
                        CssClass="btn-main btn-pill w-100" PostBackUrl="~/Pages/Admin/ManageUsers.aspx" />
                    <asp:Button ID="btnAddAnnouncement" runat="server" Text="+ Add Announcement" 
                        CssClass="btn-sub btn-pill w-100" PostBackUrl="~/Pages/Admin/ManageAnnouncements.aspx" />
                </div>
            </div>

            <div class="ec-glass-card">
                <div class="ec-glass-card-header border-0 pb-0">
                    <h5>System Logs</h5>
                </div>
                <ul class="list-unstyled m-0 mt-3">
                    <asp:Repeater ID="rptSystemLogs" runat="server">
                        <ItemTemplate>
                            <li class="ec-feed-item">
                                <span style="font-size: 0.9em; color: var(--ec-text-main);">
                                    <i class="status-dot dot-pending"></i> <%# Eval("LogText") %>
                                </span>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
                <asp:Label ID="lblLogsEmpty" runat="server" Visible="false" CssClass="ec-empty-state d-block py-3">
                    No recent logs.
                </asp:Label>
            </div>

            <div class="ec-glass-card">
                <div class="ec-glass-card-header border-0 pb-0">
                    <h5>Announcement Queue</h5>
                </div>
                <ul class="list-unstyled m-0 mt-3">
                    <asp:Repeater ID="rptAnnouncementQueue" runat="server">
                        <ItemTemplate>
                            <li class="ec-feed-item">
                                <span style="font-size: 0.9em; color: var(--ec-text-main);">
                                    <i class="status-dot dot-scheduled"></i> <%# Eval("QueueText") %>
                                </span>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
                <asp:Label ID="lblQueueEmpty" runat="server" Visible="false" CssClass="ec-empty-state d-block py-3">
                    Queue is empty.
                </asp:Label>
            </div>

        </div>
    </div>
</asp:Content>