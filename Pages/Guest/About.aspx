<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WAPP.Pages.Guest.About" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .about-hero {
            background: linear-gradient(135deg, rgba(13, 110, 253, 0.05) 0%, rgba(13, 110, 253, 0.15) 100%);
            border: 1px solid rgba(13, 110, 253, 0.1);
            position: relative;
            overflow: hidden;
        }
        .about-hero::before {
            content: '';
            position: absolute;
            top: -50px;
            right: -50px;
            width: 200px;
            height: 200px;
            background: radial-gradient(circle, rgba(13,110,253,0.2) 0%, rgba(255,255,255,0) 70%);
            border-radius: 50%;
        }
        .icon-box-large {
            width: 70px;
            height: 70px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 20px;
            background: rgba(13, 110, 253, 0.1);
            color: var(--ec-primary);
            font-size: 2rem;
            margin-bottom: 1.5rem;
            transition: transform 0.3s ease;
        }
        .ec-glass-card:hover .icon-box-large {
            transform: translateY(-5px);
            background: var(--ec-primary);
            color: white;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-content">
        <div class="ec-content-wrapper py-4" style="max-width: 1200px; margin: 0 auto;">
            
            <div class="about-hero rounded-4 p-5 text-center mb-5 shadow-sm">
                <div class="mx-auto" style="max-width: 800px;">
                    <span class="ec-status-pill ec-status-active mb-3 mx-auto">ABOUT EDUCONNECT</span>
                    <h1 class="display-4 fw-bold text-dark mb-4" style="letter-spacing: -1px;">Empowering the Future of Learning</h1>
                    <p class="lead text-secondary mb-0" style="line-height: 1.8;">
                        EduConnect bridges the gap between passionate educators and eager students, creating a seamless, interactive environment for growth, knowledge sharing, and academic success.
                    </p>
                </div>
            </div>

            <div class="row g-4 mb-5">
                <div class="col-md-6">
                    <div class="ec-glass-card h-100 p-4 p-md-5">
                        <div class="icon-box-large">
                            <i class="bi bi-bullseye"></i>
                        </div>
                        <h3 class="fw-bold text-dark mb-3">Our Mission</h3>
                        <p class="text-secondary" style="line-height: 1.7;">
                            To democratize education by providing a robust, accessible, and intuitive platform where anyone can teach and anyone can learn, regardless of geographical boundaries or traditional institutional limits.
                        </p>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="ec-glass-card h-100 p-4 p-md-5">
                        <div class="icon-box-large text-success" style="background: rgba(25, 135, 84, 0.1);">
                            <i class="bi bi-eye-fill"></i>
                        </div>
                        <h3 class="fw-bold text-dark mb-3">Our Vision</h3>
                        <p class="text-secondary" style="line-height: 1.7;">
                            To become the world's leading digital campus, cultivating a global community of lifelong learners and educators dedicated to achieving excellence through collaborative technology.
                        </p>
                    </div>
                </div>
            </div>

            <div class="ec-section-header border-0 text-center mb-4 pt-3">
                <h2 class="fw-bold m-0">What Drives Us</h2>
                <p class="text-muted mt-2">The core values that make EduConnect exceptional.</p>
            </div>

            <div class="row g-4 mb-5">
                <div class="col-md-4">
                    <div class="ec-glass-card h-100 text-center p-4">
                        <i class="bi bi-mortarboard-fill text-primary mb-3 d-block" style="font-size: 2.5rem;"></i>
                        <h5 class="fw-bold text-dark">Excellence in Education</h5>
                        <p class="text-secondary small mt-2 mb-0">We heavily vet our tutors to ensure our students receive the highest standard of educational support available.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="ec-glass-card h-100 text-center p-4">
                        <i class="bi bi-laptop text-info mb-3 d-block" style="font-size: 2.5rem;"></i>
                        <h5 class="fw-bold text-dark">Seamless Technology</h5>
                        <p class="text-secondary small mt-2 mb-0">From real-time chat to automated scheduling and file sharing, our tech gets out of the way so learning can happen.</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="ec-glass-card h-100 text-center p-4">
                        <i class="bi bi-people-fill text-warning mb-3 d-block" style="font-size: 2.5rem;"></i>
                        <h5 class="fw-bold text-dark">Community First</h5>
                        <p class="text-secondary small mt-2 mb-0">We foster a supportive, inclusive, and respectful environment where students and staff thrive together.</p>
                    </div>
                </div>
            </div>

            <div class="ec-glass-card p-0 overflow-hidden mb-5">
                <div class="row g-0">
                    <div class="col-lg-6 bg-light d-flex align-items-center justify-content-center p-5">
                        <i class="bi bi-globe-americas text-primary opacity-25" style="font-size: 15rem;"></i>
                    </div>
                    <div class="col-lg-6 p-4 p-md-5 d-flex flex-column justify-content-center">
                        <span class="badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25 rounded-pill px-3 py-2 w-fit-content mb-3">HOW IT WORKS</span>
                        <h2 class="fw-bold text-dark mb-4">A Unified Learning Ecosystem</h2>
                        <ul class="list-unstyled mb-0">
                            <li class="d-flex mb-4">
                                <i class="bi bi-check-circle-fill text-success fs-5 me-3 mt-1"></i>
                                <div>
                                    <h6 class="fw-bold text-dark mb-1">For Students</h6>
                                    <p class="text-secondary small mb-0">Browse rich course catalogs, track your progress, take interactive quizzes, and request 1-on-1 sessions with expert tutors.</p>
                                </div>
                            </li>
                            <li class="d-flex mb-4">
                                <i class="bi bi-check-circle-fill text-success fs-5 me-3 mt-1"></i>
                                <div>
                                    <h6 class="fw-bold text-dark mb-1">For Tutors</h6>
                                    <p class="text-secondary small mb-0">Create engaging learning resources, manage student schedules, broadcast announcements, and grade assessments with ease.</p>
                                </div>
                            </li>
                            <li class="d-flex">
                                <i class="bi bi-check-circle-fill text-success fs-5 me-3 mt-1"></i>
                                <div>
                                    <h6 class="fw-bold text-dark mb-1">For Staff</h6>
                                    <p class="text-secondary small mb-0">Maintain platform integrity, manage applications, handle system-wide announcements, and ensure a safe learning space.</p>
                                </div>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>

        </div>
    </div>
</asp:Content>