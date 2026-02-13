<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WAPP.Pages.Guest.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server" CssClass="login-container">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- THIS is the flex parent -->
    <asp:Panel ID="pnlLoginContainer" runat="server" CssClass="login-container">

        <asp:Panel ID="pnlLeft" runat="server" CssClass="login-left">
            <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/logo.png" CssClass="logo-img" Height="200px"/>
            <asp:Label ID="lblTagLine" runat="server" Text="Welcome to EduConnect!" CssClass="tagline"></asp:Label>
            <asp:Label ID="Label1" runat="server" Text="Let's learn more knowledge together!!!" CssClass="tagline"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="pnlRight" runat="server" CssClass="login-right">
            <asp:Image ID="imgUser" runat="server" />
            <asp:TextBox ID="txtEmail" runat="server" TextMode="SingleLine" placeholder="Email" CssClass="login-input"></asp:TextBox>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Password" CssClass="login-input"></asp:TextBox>

            <br/ >
            <asp:HyperLink ID="hlForgot" runat="server">Forgot Password?</asp:HyperLink>
            <asp:Label ID="Label2" runat="server" Text=" | "></asp:Label>
            <asp:HyperLink ID="HyperLink1" runat="server">Sign Up</asp:HyperLink>
            <br/ >
            <asp:Button ID="Button1" runat="server" Text="Login" />
        </asp:Panel>

    </asp:Panel>

</asp:Content>
