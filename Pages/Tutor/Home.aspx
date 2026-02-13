<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Tutor.Home" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        <div class="dashboard-wrapper">
    <div class="main-content">
        <div class="welcome-banner">
             <asp:Image ID="imgTutor" runat="server" ImageUrl="~/Images/tutor_avatar.png" CssClass="avatar-small" />
             <h2>Welcome back, <asp:Label ID="lblTutorName" runat="server"></asp:Label>!</h2>
        </div>

        <div class="dashboard-grid">
             <div class="card teaching">...</div>
             <div class="card announcements">...</div>
        </div>

        <div class="card messages">
             ...
        </div>
    </div>

    <div class="sidebar">
        <div class="card calendar">
            <h5>Calendar</h5>
            </div>
        <div class="card notifications">
            <h5>Notifications</h5>
            </div>
    </div>
</div>
</asp:Content>
