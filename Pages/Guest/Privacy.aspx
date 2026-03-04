<%@ Page Title="Privacy Policy" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Privacy.aspx.cs" Inherits="WAPP.Pages.Guest.Privacy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-content">
        <div class="ec-content-wrapper" style="max-width: 900px;">
            
            <h2 class="ec-page-title mb-4">Privacy Policy</h2>

            <div class="ec-glass-card">
                <div class="ec-glass-card-body p-5">
                    <p class="text-muted mb-4"><strong>Last Updated:</strong> <%= DateTime.Now.ToString("MMMM dd, yyyy") %></p>

                    <h5 class="fw-bold mt-4 mb-3">1. Introduction</h5>
                    <p>Welcome to EduConnect. We respect your privacy and are committed to protecting your personal data. This privacy policy will inform you as to how we look after your personal data when you visit our website and tell you about your privacy rights.</p>

                    <h5 class="fw-bold mt-4 mb-3">2. Information We Collect</h5>
                    <p>We may collect, use, store, and transfer different kinds of personal data about you, including:</p>
                    <ul>
                        <li><strong>Identity Data:</strong> First name, last name, username, and role (Student, Tutor, Admin).</li>
                        <li><strong>Contact Data:</strong> Email address and telephone numbers.</li>
                        <li><strong>Technical Data:</strong> Internet protocol (IP) address, your login data, browser type and version.</li>
                        <li><strong>Usage Data:</strong> Information about how you use our website, courses, and educational materials.</li>
                    </ul>

                    <h5 class="fw-bold mt-4 mb-3">3. How We Use Your Information</h5>
                    <p>We will only use your personal data when the law allows us to. Most commonly, we will use your personal data in the following circumstances:</p>
                    <ul>
                        <li>To register you as a new user and manage your account.</li>
                        <li>To process and deliver your educational content and track your progress.</li>
                        <li>To manage our relationship with you, including notifying you about changes to our terms or privacy policy.</li>
                        <li>To administer and protect our platform (including troubleshooting, data analysis, and system maintenance).</li>
                    </ul>

                    <h5 class="fw-bold mt-4 mb-3">4. Data Security</h5>
                    <p>We have put in place appropriate security measures to prevent your personal data from being accidentally lost, used, or accessed in an unauthorized way, altered, or disclosed. We limit access to your personal data to those employees, agents, and contractors who have a business need to know.</p>

                    <h5 class="fw-bold mt-4 mb-3">5. Your Legal Rights</h5>
                    <p>Under certain circumstances, you have rights under data protection laws in relation to your personal data, including the right to request access, correction, erasure, or restriction of your personal data.</p>

                    <h5 class="fw-bold mt-4 mb-3">6. Contact Us</h5>
                    <p>If you have any questions about this privacy policy or our privacy practices, please contact us at <strong>info@educonnect.com</strong>.</p>
                </div>
            </div>

        </div>
    </div>
</asp:Content>