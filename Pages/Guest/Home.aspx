<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Guest.Home" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* HERO SECTION - White Glass */
        .guest-hero { 
            background: rgba(255, 255, 255, 0.85); 
            backdrop-filter: blur(12px); 
            -webkit-backdrop-filter: blur(12px); 
            border: 1px solid rgba(255, 255, 255, 0.6); 
            border-radius: var(--ec-radius-lg); 
            padding: 60px 40px; margin-top: 40px; margin-bottom: 40px; 
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05); 
            position: relative; overflow: hidden; 
        }
        .guest-hero-content { position: relative; z-index: 1; }
        .guest-hero h1 { font-size: 3rem; color: var(--ec-text-main); margin-bottom: 1rem; line-height: 1.2; }
        .guest-hero p { font-size: 1.25rem; color: var(--ec-text-muted); margin-bottom: 2rem; max-width: 700px; }
        .hero-buttons { display: flex; gap: 1rem; flex-wrap: wrap; }
        
        /* FEATURES GRID - White Glass */
        .guest-features-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 24px; margin-bottom: 40px; }
        .guest-feature-card { 
            background: rgba(255, 255, 255, 0.85); 
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.6); 
            border-radius: var(--ec-radius-lg); padding: 30px 24px; text-align: center; 
            transition: all 0.3s ease; box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
        }
        .guest-feature-card:hover { transform: translateY(-8px); box-shadow: 0 15px 30px rgba(0,0,0,0.08); border-color: var(--ec-primary); }
        .feature-icon { font-size: 3rem; margin-bottom: 1rem; display: block; }
        .guest-feature-card h3 { font-size: 1.5rem; color: var(--ec-text-main); margin-bottom: 0.75rem; }
        .guest-feature-card p { color: var(--ec-text-muted); line-height: 1.6; margin: 0; }
        
        /* STATS SECTION - White Glass */
        .guest-stats { 
            background: rgba(255, 255, 255, 0.85); 
            backdrop-filter: blur(12px);
            border: 1px solid rgba(255, 255, 255, 0.6); 
            border-radius: var(--ec-radius-lg); padding: 50px 40px; margin-bottom: 40px; 
            color: var(--ec-text-main); /* Changed text to dark so you can read it on white */
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
        }
        .guest-stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 30px; text-align: center; }
        .guest-stat-item h2 { font-size: 3rem; margin-bottom: 0.5rem; color: var(--ec-primary); } /* Make numbers blue */
        .guest-stat-item p { font-size: 1.1rem; color: var(--ec-text-muted); margin: 0; font-weight: 600; }
        
        /* COURSE & TESTIMONIAL CARDS - White Glass */
        .guest-course-card, .guest-testimonial-card { 
            background: rgba(255, 255, 255, 0.85); 
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.6); 
            border-radius: var(--ec-radius-md); overflow: hidden; transition: all 0.3s ease; height: 100%; display: flex; flex-direction: column; 
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
        }
        .guest-course-card:hover { transform: translateY(-6px); box-shadow: 0 15px 30px rgba(0,0,0,0.08); border-color: var(--ec-primary); }
        .guest-course-body, .guest-testimonial-card { padding: 20px; }
        .guest-course-body h3, .author-info h4 { color: var(--ec-text-main); margin-bottom: 0.75rem; }
        .guest-course-body p, .course-meta, .testimonial-content, .author-info p { color: var(--ec-text-muted); }
        
        /* --- CONSISTENT COURSE IMAGES --- */
        .guest-course-img, .ec-course-img-wrapper { 
            width: 100%; 
            height: 180px; /* Force a strict height */
            overflow: hidden; /* Prevent the image from spilling out */
            background: rgba(255,255,255,0.5); /* Placeholder background */
        }

        /* Make sure the actual <img> tag respects the container */
        .guest-course-img img, .ec-course-img-wrapper img, .ec-course-img {
            width: 100%;
            height: 100%;
            object-fit: cover !important; /* Forces the image to fill the box without squishing */
        }
        .course-meta { border-top: 1px solid rgba(0,0,0,0.05); padding-top: 1rem; display: flex; gap: 1rem; font-size: 0.9rem; }
        .guest-testimonial-card::before { content: '"'; position: absolute; top: 15px; left: 25px; font-size: 4rem; color: rgba(13, 110, 253, 0.1); font-family: Georgia, serif; }
        .testimonial-author { display: flex; align-items: center; gap: 1rem; margin-top: auto; }
        .author-avatar { width: 50px; height: 50px; border-radius: 50%; background: var(--ec-primary); display: flex; align-items: center; justify-content: center; color: white; font-weight: 700; }
        
        /* CTA SECTION - White Glass */
        .guest-cta { 
            background: rgba(255, 255, 255, 0.85); 
            backdrop-filter: blur(12px);
            border: 1px solid rgba(255, 255, 255, 0.6); 
            border-radius: var(--ec-radius-lg); padding: 60px 40px; text-align: center; 
            margin-bottom: 40px; box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
        }
        .guest-cta h2 { font-size: 2.5rem; color: var(--ec-text-main); margin-bottom: 1rem; }
        .guest-cta p { font-size: 1.25rem; color: var(--ec-text-muted); margin-bottom: 2rem; }

        .scroll-fade-in { opacity: 0; transform: translateY(30px); transition: opacity 0.6s ease, transform 0.6s ease; }
        .scroll-fade-in.visible { opacity: 1; transform: translateY(0); }

        @media (max-width: 768px) {
            .guest-hero h1 { font-size: 2rem; }
            .guest-hero p { font-size: 1rem; }
            .hero-buttons { flex-direction: column; }
            .hero-buttons button, .hero-buttons a { width: 100%; text-align: center; }
            .guest-stats-grid { grid-template-columns: repeat(2, 1fr); }
            .guest-cta h2 { font-size: 1.75rem; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        <section class="guest-hero">
            <div class="guest-hero-content">
                <h1>Bridging Knowledge and Future</h1>
                <p>Empower your learning journey with world-class courses, expert tutors, and a vibrant community of learners.</p>
                <div class="hero-buttons d-flex gap-3">
                    <button type="button" class="ec-btn-primary" data-bs-toggle="modal" data-bs-target="#loginModal" style="border-radius: var(--ec-radius-pill); padding: 12px 28px; display: inline-flex; align-items: center; justify-content: center; font-weight: 600; border: none; height: 50px;">
                        Get Started Free
                    </button>
                    <a href="#courses" class="btn-sub" style="border-radius: var(--ec-radius-pill); padding: 0 28px; display: inline-flex; align-items: center; justify-content: center; text-decoration: none; font-weight: 600; height: 50px; box-sizing: border-box;">
                        Explore Courses
                    </a>
                </div>
            </div>
        </section>

        <section id="features" class="scroll-fade-in">
            <div class="ec-section-header mb-4">
                <div>
                    <h2 class="ec-section-title">Why Choose EduConnect?</h2>
                    <p class="ec-page-subtitle">Everything you need to succeed in your learning journey</p>
                </div>
            </div>
            
            <div class="guest-features-grid">
                <div class="guest-feature-card">
                    <span class="feature-icon">📚</span>
                    <h3>Quality Courses</h3>
                    <p>Access hundreds of professionally crafted courses across various subjects taught by industry experts.</p>
                </div>
                <div class="guest-feature-card">
                    <span class="feature-icon">👨‍🏫</span>
                    <h3>Expert Tutors</h3>
                    <p>Learn from experienced educators and professionals who are passionate about sharing their knowledge.</p>
                </div>
                <div class="guest-feature-card">
                    <span class="feature-icon">🎓</span>
                    <h3>Certifications</h3>
                    <p>Earn recognized certificates upon course completion to boost your career and showcase your skills.</p>
                </div>
                <div class="guest-feature-card">
                    <span class="feature-icon">💡</span>
                    <h3>Interactive Learning</h3>
                    <p>Engage with interactive content, quizzes, and hands-on projects for better understanding.</p>
                </div>
                <div class="guest-feature-card">
                    <span class="feature-icon">🌐</span>
                    <h3>Learn Anywhere</h3>
                    <p>Study at your own pace, anytime and anywhere with our mobile-friendly platform.</p>
                </div>
                <div class="guest-feature-card">
                    <span class="feature-icon">🤝</span>
                    <h3>Community Support</h3>
                    <p>Join a vibrant community of learners, share ideas, and collaborate on projects.</p>
                </div>
            </div>
        </section>
        
        <section class="guest-stats scroll-fade-in">
            <div class="guest-stats-grid">
                <div class="guest-stat-item">
                    <h2><asp:Literal ID="litStudentCount" runat="server" Text="0"></asp:Literal></h2>
                    <p>Active Students</p>
                </div>
                <div class="guest-stat-item">
                    <h2><asp:Literal ID="litCourseCount" runat="server" Text="0"></asp:Literal></h2>
                    <p>Online Courses</p>
                </div>
                <div class="guest-stat-item">
                    <h2><asp:Literal ID="litTutorCount" runat="server" Text="0"></asp:Literal></h2>
                    <p>Expert Tutors</p>
                </div>
            </div>
        </section>

        <section id="courses" class="scroll-fade-in">
            <div class="ec-section-header mb-4">
                <div>
                    <h2 class="ec-section-title">Popular Courses</h2>
                    <p class="ec-page-subtitle">Explore our most sought-after courses</p>
                </div>
            </div>
            
            <div class="ec-course-grid">
                <asp:Repeater ID="rptPopularCourses" runat="server">
                    <ItemTemplate>
                        <div class="guest-course-card">
                            <div class="guest-course-img" style="background: none; padding: 0;">
                                <img src='<%# Eval("image_path") == DBNull.Value || string.IsNullOrWhiteSpace(Eval("image_path").ToString()) ? ResolveUrl("~/Images/default-course.png") : ResolveUrl(Eval("image_path").ToString()) %>' 
                                     alt="Course Image" 
                                     style="width: 100%; height: 100%; object-fit: cover;" />
                            </div>
                            <div class="guest-course-body">
                                <h3><%# Eval("title") %></h3>
                                <p><%# Eval("description").ToString().Length > 80 ? Eval("description").ToString().Substring(0, 80) + "..." : Eval("description") %></p>
                                <div class="course-meta">
                                    <span><i class="far fa-clock"></i> <%# Eval("duration") %> mins</span>
                                    <span><i class="far fa-user"></i> <%# Eval("EnrollmentCount") %> students</span>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                
                <asp:Label ID="lblNoCourses" runat="server" CssClass="text-muted text-center w-100" Visible="false" Text="Check back soon for exciting new courses!"></asp:Label>
            </div>
        </section>

        <section id="testimonials" class="scroll-fade-in mt-5">
            <div class="ec-section-header mb-4">
                <div>
                    <h2 class="ec-section-title">What Our Students Say</h2>
                    <p class="ec-page-subtitle">Real success stories from our learning community</p>
                </div>
            </div>
            
            <div class="row g-4">
                <asp:Repeater ID="rptTestimonials" runat="server">
                    <ItemTemplate>
                        <div class="col-md-4">
                            <div class="guest-testimonial-card">
                                <p class="testimonial-content">
                                    <%# Eval("comment") %>
                                </p>
                                <div class="testimonial-author">
                                    <div class="author-avatar"><%# Eval("initials") %></div>
                                    <div class="author-info">
                                        <h4><%# Eval("fname") %> <%# Eval("lname") %></h4>
                                        <p class="text-warning mb-0" style="font-size: 0.9rem;">
                                            <%# GetStars(Convert.ToInt32(Eval("rating"))) %>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            
            <asp:Label ID="lblNoTestimonials" runat="server" CssClass="text-muted text-center w-100 d-block mt-4" Visible="false" Text="No testimonials available yet."></asp:Label>
        </section>

        <section class="guest-cta scroll-fade-in mt-5 mb-5">
            <h2>Ready to Start Your Learning Journey?</h2>
            <p>Join thousands of students already learning on EduConnect</p>
            <button type="button" class="ec-btn-primary" data-bs-toggle="modal" data-bs-target="#loginModal" style="background: white !important; color: var(--ec-primary) !important; font-size: 1.1rem; padding: 14px 32px !important; border-radius: var(--ec-radius-pill); border: none; font-weight: 700;">Join Now - It's Free!</button>
        </section>
    </div>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const observerOptions = {
                threshold: 0.1,
                rootMargin: '0px 0px -50px 0px'
            };

            const observer = new IntersectionObserver(function (entries) {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('visible');
                    }
                });
            }, observerOptions);

            document.querySelectorAll('.scroll-fade-in').forEach(el => {
                observer.observe(el);
            });
        });
    </script>
</asp:Content>