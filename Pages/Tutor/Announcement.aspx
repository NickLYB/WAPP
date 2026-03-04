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
                     <asp:Button ID="Button2" runat="server" Text="Schedule" CssClass="btn-sub" />
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

    </div>
</asp:Content>