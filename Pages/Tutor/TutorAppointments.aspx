<%@ Page Title="Manage Appointments" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="TutorAppointments.aspx.cs" Inherits="WAPP.Pages.Tutor.TutorAppointments" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        <div class="ec-item-gap">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <div class="ec-section-gap">
            <h1 class="ec-page-title">Manage Appointments</h1>
            <p class="ec-page-subtitle">Review student requests and view your upcoming teaching schedule.</p>
        </div>

        <div class="row g-4">
            <div class="col-lg-7">
                <div class="ec-glass-card h-100">
                    <div class="ec-glass-card-header border-bottom">
                        <h5 class="fw-bold m-0 text-main"><i class="bi bi-inbox text-primary me-2"></i>Pending Requests</h5>
                    </div>
                    <div class="ec-glass-card-body p-4">
                        
                        <asp:Label ID="lblActionMessage" runat="server" CssClass="d-block mb-3 fw-bold"></asp:Label>

                        <div class="d-flex flex-column gap-3">
                            <asp:Repeater ID="rptPending" runat="server" OnItemCommand="rptPending_ItemCommand">
                                <ItemTemplate>
                                    <div class="p-3 border rounded shadow-sm bg-white border-start border-4 border-warning">
                                        <div class="d-flex justify-content-between align-items-center flex-wrap gap-2">
                                            <div>
                                                <h6 class="fw-bold text-main mb-1">
                                                    <i class="bi bi-person-circle text-muted me-2"></i><%# Eval("StudentName") %>
                                                </h6>
                                                <div class="mb-2">
                                                    <span class="badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25 px-2 py-1 small">
                                                        <i class="bi bi-tag-fill me-1"></i><%# Eval("subject") %>
                                                    </span>
                                                </div>
                                                <div class="text-muted small">
                                                    <span class="badge bg-light text-dark border me-2">
                                                        <i class="bi bi-calendar-event me-1 text-primary"></i><%# Eval("appointment_date", "{0:MMM dd, yyyy}") %>
                                                    </span>
                                                    <span class="badge bg-light text-dark border">
                                                        <i class="bi bi-clock me-1 text-primary"></i><%# Eval("TimeRange") %>
                                                    </span>
                                                </div>
                                            </div>
                                            <div class="d-flex gap-2">
                                                <asp:LinkButton ID="btnApprove" runat="server" CommandName="Approve" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-sm btn-success fw-bold rounded-pill px-3 shadow-sm">
                                                    <i class="bi bi-check-lg me-1"></i>Approve
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnReject" runat="server" CommandName="Reject" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-sm btn-outline-danger fw-bold rounded-pill px-3 shadow-sm">
                                                    <i class="bi bi-x-lg"></i>
                                                </asp:LinkButton>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:PlaceHolder ID="phNoPending" runat="server" Visible="false">
                                <div class="text-center text-muted p-5 bg-light rounded border border-dashed">
                                    <i class="bi bi-cup-hot fs-1 opacity-50 d-block mb-2"></i>
                                    You have no pending appointment requests.
                                </div>
                            </asp:PlaceHolder>
                        </div>

                    </div>
                </div>
            </div>

            <div class="col-lg-5">
                <div class="ec-glass-card h-100">
                    <div class="ec-glass-card-header border-bottom">
                        <h5 class="fw-bold m-0 text-main"><i class="bi bi-calendar-check text-success me-2"></i>Upcoming Sessions</h5>
                    </div>
                    <div class="ec-glass-card-body p-4">
                        
                        <div class="d-flex flex-column gap-3">
                           <asp:Repeater ID="rptUpcoming" runat="server" OnItemCommand="rptUpcoming_ItemCommand">
                                <ItemTemplate>
                                    <div class="p-3 border rounded bg-white border-start border-4 border-success mb-3 position-relative">
                                        <h6 class="fw-bold text-main mb-2"><%# Eval("StudentName") %></h6>
                                        
                                        <div class="mb-2">
                                            <span class="badge bg-success bg-opacity-10 text-success border border-success border-opacity-25 px-2 py-1 small">
                                                <i class="bi bi-tag-fill me-1"></i><%# Eval("subject") %>
                                            </span>
                                        </div>

                                        <div class="d-flex justify-content-between align-items-center text-secondary small fw-bold mt-3 border-top pt-2">
                                            <div>
                                                <span class="me-3"><i class="bi bi-calendar-event me-1 text-dark"></i><%# Eval("appointment_date", "{0:MMM dd, yyyy}") %></span>
                                                <span><i class="bi bi-clock me-1 text-dark"></i><%# Eval("TimeRange") %></span>
                                            </div>
                                            
                                            <asp:LinkButton ID="btnCancel" runat="server" CommandName="Cancel" CommandArgument='<%# Eval("Id") %>' 
                                                CssClass="btn btn-sm btn-outline-danger py-0 px-2" OnClientClick="return confirm('Are you sure you want to cancel this approved session? The student will be notified.');">
                                                <i class="bi bi-x-circle me-1"></i>Cancel
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:PlaceHolder ID="phNoUpcoming" runat="server" Visible="false">
                                <div class="text-center text-muted p-4">
                                    No upcoming sessions scheduled.
                                </div>
                            </asp:PlaceHolder>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>