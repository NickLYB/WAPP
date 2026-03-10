<%@ Page Title="Course Announcement" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="Announcement.aspx.cs" Inherits="WAPP.Pages.Tutor.Announcement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <div class="ec-item-gap">
                <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                    PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
            </div>
            
            <div class="ec-section-header border-0">
                <h1 class="ec-page-title m-0">Create Announcement</h1>
                <div class="d-flex gap-2">
                    <asp:Button ID="Button2" runat="server" Text="Schedule" CssClass="btn-sub" 
                            data-bs-toggle="modal" data-bs-target="#scheduleModal" 
                            OnClientClick="return false;" />
                     <asp:Button ID="Button1" runat="server" Text="Send Now" CssClass="btn-main btn-pill" OnClick="Button1_Click"/>
                </div>
            </div>
            <p class="ec-page-subtitle">Notify your students about updates, deadlines, or new materials.</p>
        </div>

        <div class="ec-glass-card">
            <div class="ec-glass-card-header">
                <h5 class="ec-section-title"><i class="bi bi-megaphone me-2 text-primary"></i>Compose Message</h5>
                <span class="ec-status-pill ec-status-active">Recipient: All Students</span>
            </div>

            <div class="ec-glass-card-body">
                <div class="ec-section-gap">
                    <asp:Label ID="Label3" runat="server" Text="Announcement Subject" CssClass="fw-bold text-dark small mb-2 d-block" />
                    <asp:TextBox ID="title" runat="server" CssClass="login-input m-0" 
                        placeholder="e.g., New Quiz Published for Week 4"></asp:TextBox>
                </div>

                <div class="ec-section-gap">
                    <asp:Label ID="Label4" runat="server" Text="Message Content" CssClass="fw-bold text-dark small mb-2 d-block"/>
                    <asp:TextBox ID="description" runat="server" CssClass="login-input m-0" 
                        style="border-radius: 20px; padding: 20px;"
                        placeholder="Type your announcement message here..." 
                        TextMode="MultiLine" Rows="10"></asp:TextBox>
                </div>

                <div class="d-flex justify-content-between align-items-center">
                    <asp:Label ID="lblMsg" runat="server" CssClass="text-danger small fw-bold" Text=""></asp:Label>
                    <small class="text-muted italic">Drafts are saved automatically.</small>
                </div>
            </div>
        </div>

        <div class="modal fade" id="scheduleModal" tabindex="-1" aria-labelledby="scheduleModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header border-0 pb-0">
                        <h5 class="modal-title fw-bold" id="scheduleModalLabel">Schedule Announcement</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted small">Select the exact date and time you want this announcement to be published to your students.</p>
                        <div class="mb-3">
                            <label for="txtScheduleDate" class="form-label fw-bold small text-dark">Publish Date & Time</label>
                            <asp:TextBox ID="txtScheduleDate" runat="server" CssClass="login-input form-control m-0" TextMode="DateTimeLocal"></asp:TextBox>
                        </div>
                    </div>
                    <div class="modal-footer border-0 pt-0">
                        <button type="button" class="btn text-muted" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnConfirmSchedule" runat="server" Text="Confirm Schedule" CssClass="btn-main rounded-pill" OnClick="btnConfirmSchedule_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>