<%@ Page Title="EduConnect Help Center" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Help.aspx.cs" Inherits="WAPP.Pages.Guest.Help" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Help Center Specific Styles */
        .help-hero {
            background: linear-gradient(135deg, var(--ec-primary) 0%, var(--ec-primary-hover) 100%);
            border-radius: var(--ec-radius-lg);
            padding: 80px 40px;
            text-align: center;
            color: white;
            margin-bottom: 40px;
        }

        .help-search-wrapper {
            max-width: 600px;
            margin: 0 auto;
            position: relative;
        }

        .help-search-input {
            border-radius: var(--ec-radius-pill) !important;
            padding: 15px 30px !important;
            font-size: 1.1rem;
            border: none !important;
            box-shadow: 0 10px 25px rgba(0,0,0,0.1) !important;
        }

        .help-search-btn {
            position: absolute;
            right: 8px;
            top: 8px;
            bottom: 8px;
            background: var(--ec-dark);
            color: white;
            border: none;
            border-radius: var(--ec-radius-pill);
            padding: 0 25px;
            font-weight: 600;
            transition: background 0.2s;
        }

        .help-search-btn:hover {
            background: var(--ec-dark-hover);
        }

        .help-category-card {
            text-align: center;
            padding: 30px 20px;
            cursor: pointer;
            transition: all 0.3s ease;
            height: 100%;
        }

        .help-category-card:hover {
            transform: translateY(-8px);
            border-color: var(--ec-primary);
        }

        .help-icon {
            font-size: 2.5rem;
            color: var(--ec-primary);
            margin-bottom: 15px;
        }

        /* Customizing Bootstrap Accordion to match EduConnect */
        .accordion-item {
            border: 1px solid var(--ec-border-light);
            border-radius: var(--ec-radius-md) !important;
            margin-bottom: 10px;
            overflow: hidden;
        }
        
        .accordion-button {
            font-weight: 600;
            color: var(--ec-dark);
            background-color: var(--ec-bg-surface);
            padding: 20px;
        }

        .accordion-button:not(.collapsed) {
            color: var(--ec-primary);
            background-color: rgba(13, 110, 253, 0.05);
            box-shadow: none;
        }

        .accordion-button:focus {
            box-shadow: none;
            border-color: rgba(13, 110, 253, 0.1);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-content">
        <div class="ec-content-wrapper" style="max-width: 1000px;">
            
            <div class="help-hero">
                <h1 style="font-weight: 900; margin-bottom: 15px;">How can we help you?</h1>
                <p style="font-size: 1.1rem; opacity: 0.9; margin-bottom: 30px;">Search our knowledge base or browse categories below.</p>
                
                <div class="help-search-wrapper">
                    <asp:TextBox ID="txtSearchHelp" runat="server" CssClass="form-control help-search-input" placeholder="Type your question here (e.g., 'Reset password')"></asp:TextBox>
                    <asp:LinkButton ID="btnSearchHelp" runat="server" CssClass="help-search-btn d-flex align-items-center justify-content-center" OnClick="btnSearchHelp_Click">
                        Search
                    </asp:LinkButton>
                </div>
            </div>

            <h4 class="ec-section-title text-center mb-4">Browse Topics</h4>
            <div class="row g-4 mb-5">
                <div class="col-md-4">
                    <div class="ec-glass-card help-category-card">
                        <i class="bi bi-person-badge help-icon"></i>
                        <h5 class="fw-bold">Account & Profile</h5>
                        <p class="text-muted small mb-0">Manage your details, reset passwords, and notification settings.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="ec-glass-card help-category-card">
                        <i class="bi bi-laptop help-icon"></i>
                        <h5 class="fw-bold">Taking Courses</h5>
                        <p class="text-muted small mb-0">Learn how to enroll, track progress, and download materials.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="ec-glass-card help-category-card">
                        <i class="bi bi-easel help-icon"></i>
                        <h5 class="fw-bold">For Tutors</h5>
                        <p class="text-muted small mb-0">Guide to uploading courses, grading quizzes, and managing students.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="ec-glass-card help-category-card">
                        <i class="bi bi-credit-card help-icon"></i>
                        <h5 class="fw-bold">Billing & Payments</h5>
                        <p class="text-muted small mb-0">Understand subscriptions, payment methods, and refunds.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="ec-glass-card help-category-card">
                        <i class="bi bi-shield-check help-icon"></i>
                        <h5 class="fw-bold">Trust & Safety</h5>
                        <p class="text-muted small mb-0">Community guidelines, reporting issues, and privacy details.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="ec-glass-card help-category-card">
                        <i class="bi bi-bug help-icon"></i>
                        <h5 class="fw-bold">Technical Issues</h5>
                        <p class="text-muted small mb-0">Troubleshoot video playback, login errors, and browser support.</p>
                    </div>
                </div>
            </div>

            <h4 class="ec-section-title mb-4">Frequently Asked Questions</h4>
            <div class="accordion" id="faqAccordion">
                
                <div class="accordion-item">
                    <h2 class="accordion-header" id="headingOne">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseOne" aria-expanded="false" aria-controls="collapseOne">
                            How do I reset my password?
                        </button>
                    </h2>
                    <div id="collapseOne" class="accordion-collapse collapse" aria-labelledby="headingOne" data-bs-parent="#faqAccordion">
                        <div class="accordion-body text-muted">
                            To reset your password, click on the "Login" button at the top of the page, then select "Forgot Password". Enter the email address associated with your account, and we will send you a secure link to create a new password.
                        </div>
                    </div>
                </div>

                <div class="accordion-item">
                    <h2 class="accordion-header" id="headingTwo">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseTwo" aria-expanded="false" aria-controls="collapseTwo">
                            How do I apply to become a Tutor?
                        </button>
                    </h2>
                    <div id="collapseTwo" class="accordion-collapse collapse" aria-labelledby="headingTwo" data-bs-parent="#faqAccordion">
                        <div class="accordion-body text-muted">
                            If you have expertise you'd like to share, you can apply to be a tutor by creating an account, navigating to your profile settings, and selecting "Apply as Tutor". Our administrative team will review your application within 3-5 business days.
                        </div>
                    </div>
                </div>

                <div class="accordion-item">
                    <h2 class="accordion-header" id="headingThree">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseThree" aria-expanded="false" aria-controls="collapseThree">
                            Are the course certificates officially recognized?
                        </button>
                    </h2>
                    <div id="collapseThree" class="accordion-collapse collapse" aria-labelledby="headingThree" data-bs-parent="#faqAccordion">
                        <div class="accordion-body text-muted">
                            Yes! Upon successfully passing all quizzes and completing a course, you will receive an EduConnect Certificate of Completion. While we are not a traditional university, our certificates are widely recognized by employers to demonstrate continued learning and skill acquisition.
                        </div>
                    </div>
                </div>

                <div class="accordion-item">
                    <h2 class="accordion-header" id="headingFour">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseFour" aria-expanded="false" aria-controls="collapseFour">
                            I am experiencing video playback issues. What should I do?
                        </button>
                    </h2>
                    <div id="collapseFour" class="accordion-collapse collapse" aria-labelledby="headingFour" data-bs-parent="#faqAccordion">
                        <div class="accordion-body text-muted">
                            First, ensure you have a stable internet connection. Try clearing your browser cache or switching to an incognito/private window. If the issue persists, ensure your browser is updated to the latest version. If you still need help, contact our support team.
                        </div>
                    </div>
                </div>

            </div>

            <div class="ec-glass-card mt-5 text-center p-5" style="background-color: var(--ec-bg-alt);">
                <i class="bi bi-headset mb-3" style="font-size: 3rem; color: var(--ec-text-muted);"></i>
                <h4 class="fw-bold">Still can't find what you're looking for?</h4>
                <p class="text-muted mb-4">Our support team is ready to assist you directly.</p>
                <asp:Button ID="btnContactSupport" runat="server" Text="Contact Support" CssClass="ec-btn-primary" style="border-radius: var(--ec-radius-pill);" PostBackUrl="~/Contact.aspx" />
            </div>

        </div>
    </div>
</asp:Content>