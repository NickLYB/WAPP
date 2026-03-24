<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="TutorProfile.aspx.cs" Inherits="WAPP.Pages.Student.TutorProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>

        .tutor-header-card {
            background: rgba(255, 255, 255, 0.85);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            border: 1px solid var(--ec-border-light);
            border-radius: var(--ec-radius-lg);
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
            padding: 40px;
            margin-bottom: 24px;
        }

        .tutor-avatar-circle {
            width: 130px;
            height: 130px;
            background: linear-gradient(135deg, var(--ec-primary) 0%, #0a58ca 100%);
            color: white;
            border-radius: var(--ec-radius-pill);
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 3.5rem;
            font-weight: 700;
            box-shadow: var(--ec-shadow-md);
            border: 4px solid white;
            flex-shrink: 0;
        }

        /* Dark Mode overrides for the new card */
        html.dark-mode .tutor-header-card {
            background: rgba(30, 30, 30, 0.85);
            border: 1px solid rgba(255, 255, 255, 0.1);
        }
        html.dark-mode .tutor-avatar-circle {
            border-color: #2a2a2a;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="ec-content-wrapper py-4">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StudentMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>

        <div class="tutor-header-card d-flex flex-column flex-md-row align-items-center align-items-md-start gap-4 text-center text-md-start">
            
            <div class="tutor-avatar-circle" id="divAvatarCircle" runat="server" style="overflow: hidden;">
                <asp:Image ID="imgProfilePic" runat="server" Visible="false" style="width: 100%; height: 100%; object-fit: cover;" />
                <asp:Literal ID="litInitials" runat="server"></asp:Literal>
            </div>

            <div class="d-flex flex-column justify-content-between flex-grow-1 w-100">
                
                <div class="d-flex flex-column flex-md-row justify-content-between align-items-center align-items-md-start w-100 gap-3">
                    <div>
                        <div class="d-flex align-items-center justify-content-center justify-content-md-start gap-2 mb-2">
                            <h2 class="ec-page-title mb-0">
                                <asp:Literal ID="litTutorName" runat="server"></asp:Literal>
                            </h2>
                            <asp:PlaceHolder ID="phVerified" runat="server" Visible="false">
                                <i class="bi bi-patch-check-fill text-success fs-4" title="Verified Educator" data-bs-toggle="tooltip"></i>
                            </asp:PlaceHolder>
                        </div>
                        <p class="text-muted mb-0 fw-medium">
                            <i class="bi bi-envelope-fill text-primary opacity-50 me-2"></i><asp:Literal ID="litEmail" runat="server"></asp:Literal>
                            <span class="mx-3 opacity-25 d-none d-md-inline">|</span>
                            <span class="d-block d-md-inline mt-2 mt-md-0">
                                <i class="bi bi-calendar-check-fill text-primary opacity-50 me-2"></i>Joined <asp:Literal ID="litJoinedDate" runat="server"></asp:Literal>
                            </span>
                        </p>
                    </div>

                    <div class="text-center text-md-end bg-light rounded-4 p-3 border shadow-sm" style="min-width: 160px;">
                        <div class="d-flex align-items-baseline justify-content-center mb-1">
                            <h3 class="fw-bold text-main mb-0 me-1"><asp:Literal ID="litAvgRating" runat="server">0.0</asp:Literal></h3>
                            <span class="text-muted small">/ 5.0</span>
                        </div>
                        <div class="text-warning mb-1" style="font-size: 1.1rem;">
                            <asp:Literal ID="litStarRating" runat="server"></asp:Literal>
                        </div>
                        <span class="text-muted small fw-bold text-uppercase" style="letter-spacing: 1px;">Rating</span>
                    </div>
                </div>

                <div class="mt-4 pt-4 border-top w-100 d-flex justify-content-center justify-content-md-start">
                    <button type="button" class="btn-main rounded-pill px-4 shadow-sm d-flex align-items-center" data-bs-toggle="modal" data-bs-target="#bookAppointmentModal">
                        <i class="bi bi-calendar-plus me-2"></i> Book Appointment
                    </button>
                </div>

            </div>
        </div>

        <div class="ec-glass-card p-0 shadow-sm border-0">
            
            <div class="px-4 pt-3 border-bottom border-light" style="background-color: var(--ec-bg-alt); border-top-left-radius: var(--ec-radius-lg); border-top-right-radius: var(--ec-radius-lg);">
                <ul class="nav nav-tabs border-0" id="tutorTabs" role="tablist">
                    <li class="nav-item me-2" role="presentation">
                        <button class="nav-link active pb-3 px-4 fw-bold" id="courses-tab" data-bs-toggle="tab" data-bs-target="#courses" type="button" role="tab">Courses Offered</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link pb-3 px-4 fw-bold" id="reviews-tab" data-bs-toggle="tab" data-bs-target="#reviews" type="button" role="tab">Student Reviews</button>
                    </li>
                </ul>
            </div>

            <div class="tab-content p-4 p-md-5" id="tutorTabsContent">
                
                <div class="tab-pane fade show active" id="courses" role="tabpanel">
                    <div class="ec-course-grid">
                        <asp:Repeater ID="rptCourses" runat="server">
                            <ItemTemplate>
                                <div class="ec-course-box">
                                    
                                    <div class="ec-course-img-wrapper">
                                        <asp:Image ID="imgCourse" runat="server" CssClass="ec-course-img" ImageUrl='<%# GetCourseImage(Eval("image_path")) %>' />
                                    </div>

                                    <div class="ec-course-body">
                                        <div class="ec-badge-row">
                                            <span class="ec-status-pill ec-status-active">
                                                <%# Eval("category") %>
                                            </span>
                                        </div>
                                        <h5 class="fw-bold text-main mb-3"><%# Eval("title") %></h5>
                                        
                                        <div class="ec-course-footer mt-auto pt-3 border-top">
                                            <asp:HyperLink ID="hlViewCourse" runat="server" 
                                                NavigateUrl='<%# "~/Pages/Student/CourseDetail.aspx?id=" + Eval("Id") + "&source=tutor" %>' 
                                                CssClass="ec-link-primary m-0 d-flex align-items-center justify-content-between w-100 text-decoration-none">
                                                <span>View Course</span> <i class="bi bi-arrow-right"></i>
                                            </asp:HyperLink>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

                <div class="tab-pane fade" id="reviews" role="tabpanel">
                    
                    <asp:PlaceHolder ID="phWriteReview" runat="server" Visible="false">
                        <div class="ec-glass-card mb-5 border-0" style="background-color: var(--ec-bg-alt);">
                            <h5 class="fw-bold mb-3 text-main"><i class="bi bi-pencil-square me-2 text-primary"></i>Rate this Tutor</h5>
                            <div class="row g-3">
                                <div class="col-md-4">
                                    <asp:DropDownList ID="ddlRating" runat="server" CssClass="ec-filter-ddl w-100">
                                        <asp:ListItem Value="5">5 - Excellent</asp:ListItem>
                                        <asp:ListItem Value="4">4 - Very Good</asp:ListItem>
                                        <asp:ListItem Value="3">3 - Average</asp:ListItem>
                                        <asp:ListItem Value="2">2 - Poor</asp:ListItem>
                                        <asp:ListItem Value="1">1 - Terrible</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-8">
                                    <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="2" CssClass="login-input m-0 mb-3" placeholder="Share your experience learning with this tutor..."></asp:TextBox>
                                </div>
                            </div>
                            <div class="text-end">
                                <asp:Button ID="btnSubmitReview" runat="server" Text="Submit Review" CssClass="ec-btn-primary rounded-pill shadow-sm" OnClick="btnSubmitReview_Click" />
                            </div>
                        </div>
                    </asp:PlaceHolder>

                    <h5 class="fw-bold mb-4 text-main">What Students Say</h5>
                    
                    <div class="d-flex flex-column">
                        <asp:Repeater ID="rptReviews" runat="server">
                            <ItemTemplate>
                                <div class="ec-feed-item flex-column align-items-start py-3">
                                    <div class="d-flex justify-content-between align-items-center w-100 mb-2">
                                        <div class="d-flex align-items-center">
                                            <div class="bg-primary bg-opacity-10 text-primary rounded-circle d-flex align-items-center justify-content-center me-3" style="width: 40px; height: 40px;">
                                                <i class="bi bi-person-fill fs-5"></i>
                                            </div>
                                            <div>
                                                <span class="fw-bold text-main d-block"><%# Eval("fname") %> <%# Eval("lname") %></span>
                                                <small class="text-muted"><%# Eval("created_at", "{0:MMMM dd, yyyy}") %></small>
                                            </div>
                                        </div>
                                        <div class="text-warning fw-bold bg-warning bg-opacity-10 px-3 py-1 rounded-pill border border-warning border-opacity-25">
                                            <i class="bi bi-star-fill me-1"></i><%# Eval("rating") %>.0
                                        </div>
                                    </div>
                                    <div class="text-secondary ms-5 ps-2" style="line-height: 1.6;">
                                        <%# Eval("comment") %>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:PlaceHolder ID="phNoReviews" runat="server" Visible="false">
                        <div class="ec-empty-state">
                            <i class="bi bi-chat-square-text"></i>
                            <h5 class="fw-bold text-dark">No reviews yet</h5>
                            <p>Students haven't written any reviews for this tutor yet.</p>
                        </div>
                    </asp:PlaceHolder>

                </div>
            </div>

        </div>
    </div>

    <div class="modal fade" id="bookAppointmentModal" tabindex="-1" aria-labelledby="bookAppointmentModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content ec-modal-content w-100 bg-white">
                <div class="ec-modal-header d-flex justify-content-between align-items-center">
                    <h4 class="modal-title m-0" id="bookAppointmentModalLabel">
                        <i class="bi bi-calendar2-week text-primary me-2"></i>Book a Session
                    </h4>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                
                <asp:UpdatePanel ID="upAppointment" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="ec-modal-body">
                            <div class="alert bg-primary bg-opacity-10 text-primary border-0 mb-4 rounded-3 small">
                                <i class="bi bi-info-circle-fill me-2"></i>
                                Select a date to view available time slots. Sessions require a 30-minute buffer between appointments.
                            </div>

                            <div class="mb-3">
                                <label class="form-label fw-bold text-muted small">Topic / Subject</label>
                                <asp:DropDownList ID="ddlSubject" runat="server" CssClass="ec-filter-ddl w-100">
                                    <asp:ListItem Value="Consultation">General Consultation</asp:ListItem>
                                    <asp:ListItem Value="Code Review">Code Review & Debugging</asp:ListItem>
                                    <asp:ListItem Value="Career Advice">Career & Study Advice</asp:ListItem>
                                    <asp:ListItem Value="Assignment Help">Assignment Guidance</asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <div class="mb-3">
                                <label class="form-label fw-bold text-muted small">Select Date</label>
                                <asp:TextBox ID="txtApptDate" runat="server" TextMode="Date" CssClass="login-input m-0 rounded-3" AutoPostBack="true" OnTextChanged="txtApptDate_TextChanged"></asp:TextBox>
                            </div>

                            <div class="mb-3">
                                <label class="form-label fw-bold text-muted small">Duration</label>
                                <asp:DropDownList ID="ddlDuration" runat="server" CssClass="ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="txtApptDate_TextChanged">
                                    <asp:ListItem Value="30">30 Minutes</asp:ListItem>
                                    <asp:ListItem Value="60">1 Hour</asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <div class="mb-4">
                                <label class="form-label fw-bold text-muted small">Available Time Slots</label>
                                <asp:DropDownList ID="ddlTimeSlots" runat="server" CssClass="ec-filter-ddl w-100">
                                    <asp:ListItem Value="" Text="-- Please select a date first --"></asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <asp:Label ID="lblApptMessage" runat="server" CssClass="d-block text-center fw-bold mt-3"></asp:Label>
                        </div>

                        <div class="ec-modal-footer d-flex justify-content-end gap-2">
                            <button type="button" class="btn-sub" data-bs-dismiss="modal">Cancel</button>
                            <asp:Button ID="btnConfirmBooking" runat="server" 
                                Text="Request Appointment" 
                                CssClass="ec-btn-primary" 
                                OnClick="btnConfirmBooking_Click"
                                OnClientClick="if(confirm('Are you sure you want to book this session?')) { this.value='Sending Request...'; this.style.pointerEvents='none'; this.style.opacity='0.7'; return true; } else { return false; }" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>