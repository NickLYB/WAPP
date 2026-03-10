<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerifyOtp.aspx.cs" Inherits="WAPP.Pages.Guest.VerifyOtp" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Verify Your Account - EduConnect</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <style>
        body { background-color: #f8fafc; }
        .otp-card {
            max-width: 450px;
            margin: 100px auto;
            background: white;
            padding: 40px;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.05);
            text-align: center;
        }
        .otp-icon {
            font-size: 3rem;
            color: #0d6efd;
            margin-bottom: 20px;
        }
        .otp-input {
            font-size: 1.5rem;
            letter-spacing: 5px;
            text-align: center;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="otp-card border border-primary border-opacity-25">
                
                <i class="bi bi-envelope-check-fill otp-icon"></i>
                <h3 class="fw-bold mb-3">Verify Your Email</h3>
                <p class="text-muted mb-4">We've sent a 6-digit verification code to your email. Please enter it below to activate your account.</p>

                <div class="mb-4">
                    <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control form-control-lg otp-input" MaxLength="6" placeholder="------" AutoCompleteType="Disabled"></asp:TextBox>
                </div>

                <asp:Button ID="btnVerify" runat="server" Text="Verify Account" CssClass="btn btn-primary w-100 py-2 fw-bold rounded-pill mb-3 shadow-sm" OnClick="btnVerify_Click" />
                
                <asp:Label ID="lblMessage" runat="server" Font-Bold="true" CssClass="d-block mb-3"></asp:Label>

                <div class="text-muted small">
                    Didn't receive the code? <br />
                    <a href="Home.aspx" class="text-decoration-none">Return to Home</a>
                </div>

            </div>
        </div>
    </form>
</body>
</html>