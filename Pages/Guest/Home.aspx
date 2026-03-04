<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP.Pages.Guest.Home" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .guest-hero { background: linear-gradient(135deg, var(--ec-bg-nav) 0%, #fdf396 100%); border-radius: var(--ec-radius-lg); padding: 60px 40px; margin-top: 40px; margin-bottom: 40px; box-shadow: var(--ec-shadow-sm); position: relative; overflow: hidden; }
        .guest-hero::before { content: ''; position: absolute; top: -50%; right: -10%; width: 600px; height: 600px; background: radial-gradient(circle, rgba(13, 110, 253, 0.08) 0%, transparent 70%); border-radius: 50%; z-index: 0; }
        .guest-hero-content { position: relative; z-index: 1; }
        .guest-hero h1 { font-size: 3rem; font-weight: 900; color: var(--ec-dark); margin-bottom: 1rem; line-height: 1.2; }
        .guest-hero p { font-size: 1.25rem; color: var(--ec-text-muted); margin-bottom: 2rem; max-width: 700px; }
        .hero-buttons { display: flex; gap: 1rem; flex-wrap: wrap; }
        
        .guest-features-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 24px; margin-bottom: 40px; }
        .guest-feature-card { background: var(--ec-bg-surface); border: 1px solid var(--ec-border-light); border-radius: var(--ec-radius-lg); padding: 30px 24px; text-align: center; transition: all 0.3s ease; position: relative; overflow: hidden; }
        .guest-feature-card::before { content: ''; position: absolute; top: 0; left: 0; width: 100%; height: 100%; background: linear-gradient(135deg, rgba(13, 110, 253, 0.03) 0%, transparent 100%); opacity: 0; transition: opacity 0.3s ease; }
        .guest-feature-card:hover { transform: translateY(-8px); box-shadow: var(--ec-shadow-hover); border-color: var(--ec-primary); }
        .guest-feature-card:hover::before { opacity: 1; }
        .feature-icon { font-size: 3rem; margin-bottom: 1rem; display: block; }
        .guest-feature-card h3 { font-size: 1.5rem; font-weight: 700; color: var(--ec-text-main); margin-bottom: 0.75rem; }
        .guest-feature-card p { color: var(--ec-text-muted); line-height: 1.6; margin: 0; }
        
        .guest-stats { background: linear-gradient(135deg, var(--ec-dark) 0%, var(--ec-dark-hover) 100%); border-radius: var(--ec-radius-lg); padding: 50px 40px; margin-bottom: 40px; color: white; }
        .guest-stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 30px; text-align: center; }
        .guest-stat-item h2 { font-size: 3rem; font-weight: 900; margin-bottom: 0.5rem; }
        .guest-stat-item p { font-size: 1.1rem; opacity: 0.9; margin: 0; }
        
        .guest-course-card { background: var(--ec-bg-surface); border: 1px solid var(--ec-border-light); border-radius: var(--ec-radius-md); overflow: hidden; transition: all 0.3s ease; height: 100%; display: flex; flex-direction: column; }
        .guest-course-card:hover { transform: translateY(-6px); box-shadow: var(--ec-shadow-hover); border-color: var(--ec-primary); }
        .guest-course-img { width: 100%; height: 180px; background: linear-gradient(135deg, var(--ec-bg-nav) 0%, #fdf396 100%); display: flex; align-items: center; justify-content: center; font-size: 4rem; }
        .guest-course-body { padding: 20px; flex-grow: 1; display: flex; flex-direction: column; }
        .guest-course-body h3 { font-size: 1.25rem; font-weight: 700; color: var(--ec-text-main); margin-bottom: 0.75rem; }
        .guest-course-body p { color: var(--ec-text-muted); margin-bottom: 1rem; flex-grow: 1; }
        
        .course-meta { display: flex; gap: 1rem; font-size: 0.9rem; color: var(--ec-text-muted); padding-top: 1rem; border-top: 1px solid var(--ec-border-subtle); }
        .course-meta span { display: flex; align-items: center; gap: 0.4rem; }
        
        .guest-testimonial-card { background: var(--ec-bg-surface); border: 1px solid var(--ec-border-light); border-radius: var(--ec-radius-lg); padding: 30px; position: relative; height: 100%; display: flex; flex-direction: column; }
        .guest-testimonial-card::before { content: '"'; position: absolute; top: 15px; left: 25px; font-size: 4rem; color: rgba(13, 110, 253, 0.1); font-family: Georgia, serif; line-height: 1; }
        .testimonial-content { margin-bottom: 1.5rem; color: var(--ec-text-muted); line-height: 1.8; position: relative; z-index: 1; flex-grow: 1; }
        .testimonial-author { display: flex; align-items: center; gap: 1rem; }
        .author-avatar { width: 50px; height: 50px; border-radius: 50%; background: var(--ec-primary); display: flex; align-items: center; justify-content: center; color: white; font-weight: 700; font-size: 1.1rem; }
        .author-info h4 { color: var(--ec-text-main); margin-bottom: 0.2rem; font-size: 1rem; font-weight: 600; }
        .author-info p { color: var(--ec-text-muted); font-size: 0.9rem; margin: 0; }
        
        .guest-cta { background: linear-gradient(135deg, var(--ec-primary) 0%, var(--ec-primary-hover) 100%); border-radius: var(--ec-radius-lg); padding: 60px 40px; text-align: center; color: white; margin-bottom: 40px; }
        .guest-cta h2 { font-size: 2.5rem; font-weight: 900; margin-bottom: 1rem; }
        .guest-cta p { font-size: 1.25rem; margin-bottom: 2rem; opacity: 0.95; }
        
        .scroll-fade-in { opacity: 0; transform: translateY(30px); transition: opacity 0.6s ease, transform 0.6s ease; }
        .scroll-fade-in.visible { opacity: 1; transform: translateY(0); }

        @media (max-width: 768px) {
            .guest-hero h1 { font-size: 2rem; }
            .guest-hero p { font-size: 1rem; }
            .hero-buttons { flex-direction: column; }
            .hero-buttons .btn, .hero-buttons a, .hero-buttons button { width: 100%; text-align: center; }
            .guest-stats-grid { grid-template-columns: repeat(2, 1fr); }
            .guest-cta h2 { font-size: 1.75rem; }
            .guest-cta p { font-size: 1rem; }
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
                <div class="col-md-4">
                    <div class="guest-testimonial-card">
                        <p class="testimonial-content">
                            EduConnect transformed my career! The courses are well-structured and the instructors are incredibly knowledgeable. I landed my dream job as a web developer within 3 months!
                        </p>
                        <div class="testimonial-author">
                            <div class="author-avatar">SM</div>
                            <div class="author-info">
                                <h4>Sarah Martinez</h4>
                                <p>Web Developer</p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="guest-testimonial-card">
                        <p class="testimonial-content">
                            The flexibility of learning at my own pace while working full-time was a game-changer. The community support and resources are outstanding!
                        </p>
                        <div class="testimonial-author">
                            <div class="author-avatar">JL</div>
                            <div class="author-info">
                                <h4>James Lee</h4>
                                <p>Data Analyst</p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="guest-testimonial-card">
                        <p class="testimonial-content">
                            As a complete beginner, I was nervous about learning to code. EduConnect made it so easy and fun! The projects were engaging and practical.
                        </p>
                        <div class="testimonial-author">
                            <div class="author-avatar">EP</div>
                            <div class="author-info">
                                <h4>Emily Parker</h4>
                                <p>Software Engineer</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
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