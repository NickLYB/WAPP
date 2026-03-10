<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MigratePasswords.aspx.cs" Inherits="WAPP.MigratePasswords" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Secret Developer Tool - Password Migration</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8fafc; display: flex; justify-content: center; padding-top: 100px; }
        .card { background: white; padding: 40px; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); text-align: center; max-width: 500px; }
        .btn-migrate { background-color: #dc3545; color: white; border: none; padding: 15px 25px; border-radius: 8px; font-weight: bold; cursor: pointer; margin-top: 20px; transition: 0.2s; }
        .btn-migrate:hover { background-color: #bb2d3b; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="card">
            <h2 style="color: #333; margin-top: 0;">Database Security Migration</h2>
            <p style="color: #666;">Clicking the button below will scan the database for plain-text passwords and convert them into secure BCrypt hashes. </p>
            <p style="color: #dc3545; font-weight: bold; font-size: 0.85rem;">⚠️ Remember to delete this file after you are done!</p>
            
            <asp:Button ID="btnMigrate" runat="server" Text="Convert Passwords Now" CssClass="btn-migrate" OnClick="btnMigrate_Click" />
            <br /><br />
            <asp:Label ID="lblMessage" runat="server" Font-Bold="true"></asp:Label>
        </div>
    </form>
</body>
</html>