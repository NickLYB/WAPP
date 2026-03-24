<%@ Page Title="EduConnect Support" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Help.aspx.cs" Inherits="WAPP.Pages.Guest.Help" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .help-hero {
            background: linear-gradient(135deg, var(--ec-primary) 0%, var(--ec-primary-hover) 100%);
            border-radius: var(--ec-radius-lg);
            padding: 50px 20px;
            text-align: center;
            color: white;
            margin-bottom: 40px;
        }

        .help-category-card {
            text-align: center;
            padding: 30px 20px;
            transition: all 0.3s ease;
            height: 100%;
            border: 1px solid var(--ec-border-light);
            background: white;
        }

        .help-category-card:hover {
            transform: translateY(-5px);
            border-color: var(--ec-primary);
            box-shadow: var(--ec-shadow-sm);
        }

        .help-icon {
            font-size: 2.2rem;
            color: var(--ec-primary);
            margin-bottom: 15px;
            display: block;
        }

        .accordion-item {
            border: 1px solid var(--ec-border-light);
            border-radius: var(--ec-radius-md) !important;
            margin-bottom: 12px;
            overflow: hidden;
        }
        
        .accordion-button {
            font-weight: 600;
            padding: 18px;
        }

        .accordion-button:not(.collapsed) {
            background-color: rgba(13, 110, 253, 0.05);
            color: var(--ec-primary);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-content">
        <div class="ec-content-wrapper" style="max-width: 1000px;">
            
            <div class="help-hero shadow-sm">
                <h1 class="fw-bold mb-2">Support Center</h1>
                <p class="opacity-75 mb-0">Browse common topics below or reach out to our team for direct assistance.</p>
            </div>

            <h4 class="ec-section-title text-center mb-4">How can we help?</h4>
            <div class="row g-4 mb-5">
                <div class="col-md-4">
                    <div class="help-category-card rounded-4">
                        <i class="bi bi-person-circle help-icon"></i>
                        <h5 class="fw-bold">My Account</h5>
                        <p class="text-muted small mb-0">Login issues, profile updates, and account security.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="help-category-card rounded-4">
                        <i class="bi bi-mortarboard help-icon"></i>
                        <h5 class="fw-bold">Learning Hub</h5>
                        <p class="text-muted small mb-0">Enrolling in courses, viewing content, and quiz help.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="help-category-card rounded-4">
                        <i class="bi bi-megaphone help-icon"></i>
                        <h5 class="fw-bold">Teaching</h5>
                        <p class="text-muted small mb-0">Information for tutors on managing courses and students.</p>
                    </div>
                </div>
            </div>

            <div class="row justify-content-center">
                <div class="col-lg-9">
                    <h4 class="ec-section-title mb-4">Frequently Asked Questions</h4>
                    <div class="accordion" id="faqAccordion">
                        
                        <div class="accordion-item">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#q1">
                                    How do I reset my password?
                                </button>
                            </h2>
                            <div id="q1" class="accordion-collapse collapse" data-bs-parent="#faqAccordion">
                                <div class="accordion-body text-muted">
                                    Visit the Login page and click "Forgot Password." We will send a verification OTP to your email address to help you create a new password.
                                </div>
                            </div>
                        </div>

                        <div class="accordion-item">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#q2">
                                    How do I apply to become a Tutor?
                                </button>
                            </h2>
                            <div id="q2" class="accordion-collapse collapse" data-bs-parent="#faqAccordion">
                                <div class="accordion-body text-muted">
                                    After creating a standard account, go to your profile settings and click "Apply as Tutor." Fill in your credentials, and our staff will review your application.
                                </div>
                            </div>
                        </div>

                        <div class="accordion-item">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#q3">
                                    Can I cancel my enrollment?
                                </button>
                            </h2>
                            <div id="q3" class="accordion-collapse collapse" data-bs-parent="#faqAccordion">
                                <div class="accordion-body text-muted">
                                    Yes, you can leave a course at any time from your Student Dashboard. However, please note that progress data for that course may be reset if you choose to re-enroll later.
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
            </div>

            <div class="ec-glass-card mt-5 text-center p-5 border-0 shadow-sm" style="background-color: var(--ec-bg-alt);">
                <i class="bi bi-chat-dots mb-3" style="font-size: 2.5rem; color: var(--ec-primary);"></i>
                <h3 class="fw-bold">Still have questions?</h3>
                <p class="text-muted mb-4">Our dedicated support team is available Monday through Friday to assist you.</p>
                
                <a href="mailto:3bolow@gmail.com?subject=EduConnect%20Support%20Request" class="ec-btn-primary px-5 py-3 d-inline-block text-decoration-none" style="border-radius: var(--ec-radius-pill);">
                    <i class="bi bi-envelope-fill me-2"></i> Contact Support Team
                </a>
            </div>

        </div>
    </div>
</asp:Content>