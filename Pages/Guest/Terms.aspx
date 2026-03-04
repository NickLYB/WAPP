<%@ Page Title="Terms of Service" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Terms.aspx.cs" Inherits="WAPP.Pages.Guest.Terms" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-content">
        <div class="ec-content-wrapper" style="max-width: 900px;">
            
            <h2 class="ec-page-title mb-4">Terms of Service</h2>

            <div class="ec-glass-card">
                <div class="ec-glass-card-body p-5">
                    <p class="text-muted mb-4"><strong>Effective Date:</strong> <%= DateTime.Now.ToString("MMMM dd, yyyy") %></p>

                    <h5 class="fw-bold mt-4 mb-3">1. Acceptance of Terms</h5>
                    <p>By accessing and using the EduConnect platform, you accept and agree to be bound by the terms and provision of this agreement. In addition, when using these particular services, you shall be subject to any posted guidelines or rules applicable to such services.</p>

                    <h5 class="fw-bold mt-4 mb-3">2. User Accounts and Responsibilities</h5>
                    <p>To access certain features of the platform, you must register for an account. You agree to:</p>
                    <ul>
                        <li>Provide accurate, current, and complete information during the registration process.</li>
                        <li>Maintain the security of your password and identification.</li>
                        <li>Accept all responsibility for any and all activities that occur under your account.</li>
                    </ul>

                    <h5 class="fw-bold mt-4 mb-3">3. User Conduct</h5>
                    <p>You agree not to use the platform to:</p>
                    <ul>
                        <li>Upload, post, or transmit any content that is unlawful, harmful, threatening, abusive, or harassing.</li>
                        <li>Impersonate any person or entity, or falsely state or otherwise misrepresent your affiliation with a person or entity.</li>
                        <li>Interfere with or disrupt the platform or servers or networks connected to the platform.</li>
                    </ul>

                    <h5 class="fw-bold mt-4 mb-3">4. Intellectual Property Rights</h5>
                    <p>All content included on this site, such as text, graphics, logos, images, course materials, and software, is the property of EduConnect or its content suppliers and protected by international copyright laws. You may not reproduce, distribute, or create derivative works without explicit permission.</p>

                    <h5 class="fw-bold mt-4 mb-3">5. Termination</h5>
                    <p>We may terminate or suspend access to our platform immediately, without prior notice or liability, for any reason whatsoever, including without limitation if you breach the Terms. All provisions of the Terms which by their nature should survive termination shall survive.</p>

                    <h5 class="fw-bold mt-4 mb-3">6. Limitation of Liability</h5>
                    <p>In no event shall EduConnect, nor its directors, employees, partners, agents, suppliers, or affiliates, be liable for any indirect, incidental, special, consequential or punitive damages, including without limitation, loss of profits, data, use, goodwill, or other intangible losses, resulting from your access to or use of or inability to access or use the platform.</p>

                    <h5 class="fw-bold mt-4 mb-3">7. Changes to Terms</h5>
                    <p>We reserve the right, at our sole discretion, to modify or replace these Terms at any time. What constitutes a material change will be determined at our sole discretion. By continuing to access or use our platform after those revisions become effective, you agree to be bound by the revised terms.</p>
                </div>
            </div>

        </div>
    </div>
</asp:Content>