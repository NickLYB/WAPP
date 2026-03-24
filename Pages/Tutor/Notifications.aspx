<%@ Page Title="Notifications" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="WAPP.Pages.Tutor.Notifications" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <div class="ec-item-gap">
                <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                    PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
            </div>
            
            <div class="ec-section-header border-0 d-flex justify-content-between align-items-center">
                <div>
                    <h1 class="ec-page-title m-0">Your Notifications</h1>
                    <p class="ec-page-subtitle m-0">Stay updated with the latest messages and system alerts.</p>
                </div>
                <div>
                    <asp:Button ID="btnMarkAllRead" runat="server" Text="Mark All as Read" CssClass="btn-sub" OnClick="btnMarkAllRead_Click" />
                </div>
            </div>
        </div>

        <asp:UpdatePanel ID="upNotifications" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="ec-glass-card p-0 overflow-hidden">
                    <asp:Repeater ID="rptAllNotifications" runat="server" OnItemCommand="rptAllNotifications_ItemCommand">
                        <HeaderTemplate>
                            <div class="list-group list-group-flush">
                        </HeaderTemplate>
                        
                        <ItemTemplate>
                            <div class='list-group-item p-0 border-bottom <%# Eval("status").ToString() == "UNREAD" ? "notif-unread" : "notif-read" %>'>
                                <div class="d-flex w-100 justify-content-between align-items-center p-4">
                                    
                                    <asp:LinkButton ID="lnkViewNotification" runat="server" CommandName="View" 
                                        CommandArgument='<%# Eval("notification_id") + "|" + Eval("title") + "|" + Eval("message") + "|" + Eval("created_at") + "|" + (Eval("appointment_id") == DBNull.Value ? "" : Eval("appointment_id").ToString()) %>'
                                        CssClass="d-flex gap-3 text-decoration-none flex-grow-1" style="color: inherit;">
                                        
                                        <div class="mt-1">
                                            <i class='<%# Eval("status").ToString() == "UNREAD" ? "bi bi-bell-fill text-primary" : "bi bi-bell text-muted" %>' style="font-size: 1.2rem;"></i>
                                        </div>
                                        <div>
                                            <h6 class="mb-1 fw-bold text-dark">
                                                <%# Eval("title") %>
                                                <asp:Label runat="server" Visible='<%# Eval("status").ToString() == "UNREAD" %>' CssClass="badge bg-danger ms-2" style="font-size: 0.65rem;">New</asp:Label>
                                            </h6>
                                            <p class="mb-1 text-secondary text-truncate" style="font-size: 0.9rem; line-height: 1.5; max-width: 600px;">
                                                <%# StripHTML(Eval("message")) %>
                                            </p>
                                            <small class="text-muted"><%# FormatDate(Eval("created_at")) %></small>
                                        </div>
                                    </asp:LinkButton>

                                    <div class="notif-action-wrapper ms-3">
                                        <asp:LinkButton ID="lnkArchive" runat="server" CommandName="Archive" CommandArgument='<%# Eval("notification_id") %>' 
                                            CssClass="btn btn-sm btn-outline-danger rounded-pill px-3" ToolTip="Delete Notification">
                                            <i class="bi bi-trash"></i>
                                        </asp:LinkButton>
                                    </div>

                                </div>
                            </div>
                        </ItemTemplate>
                        
                        <FooterTemplate>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>

                    <div class="p-5 text-center" id="divNoNotifications" runat="server" visible="false">
                        <i class="bi bi-bell-slash text-muted" style="font-size: 3rem;"></i>
                        <h5 class="mt-3 text-dark fw-bold">No notifications yet</h5>
                        <p class="text-muted">You're all caught up! New alerts will appear here.</p>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

    </div>

    <asp:UpdatePanel ID="upModal" runat="server">
        <ContentTemplate>
            <div class="modal fade" id="notificationModal" tabindex="-1" aria-labelledby="notificationModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content border-0 shadow">
                        <div class="modal-header border-bottom-0 pb-0">
                            <h5 class="modal-title fw-bold text-dark" id="notificationModalLabel">
                                <asp:Label ID="lblModalTitle" runat="server"></asp:Label>
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body pt-2">
                            <p class="text-muted small mb-3 border-bottom pb-2">
                                Received on <asp:Label ID="lblModalDate" runat="server"></asp:Label>
                            </p>
                            <div class="text-dark" style="font-size: 0.95rem; line-height: 1.6;">
                                <asp:Literal ID="litModalMessage" runat="server"></asp:Literal>
                            </div>
                        </div>
                        <div class="modal-footer border-top-0 pt-0 d-flex justify-content-end gap-2">
                            <asp:HyperLink ID="hlViewAppointment" runat="server" 
                                CssClass="btn-main rounded-pill px-4 text-decoration-none" 
                                Text="Manage Appointment" 
                                NavigateUrl="TutorAppointments.aspx" 
                                Visible="false">
                            </asp:HyperLink>
                             <button type="button" class="btn btn-light border rounded-pill px-4" data-bs-dismiss="modal">Close</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>