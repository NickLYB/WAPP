<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PendingTutorsWidget.ascx.cs" Inherits="WAPP.Pages.Staff.PendingTutorsWidget" %>

<div class="card teaching">
    <h5>Pending Tutor Verifications</h5>
    <div style="padding: 15px; text-align: center;">
        <h1 style="color: #d9534f; font-weight: bold;">
            <asp:Label ID="lblPendingCount" runat="server" Text="0"></asp:Label>
        </h1>
        <p>New applications waiting for review.</p>
        <asp:Button ID="btnReview" runat="server" Text="Review Now" CssClass="btn btn-outline-danger btn-sm" OnClick="btnReview_Click" />
    </div>
</div>