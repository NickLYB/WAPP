<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="LessonView.aspx.cs" Inherits="WAPP.Pages.Student.LessonView" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    </asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="ec-content-wrapper">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StudentMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="ec-page-title m-0">
                <i class="bi bi-book-half me-2 text-primary"></i> 
                <asp:Literal ID="litCourseTitle" runat="server"></asp:Literal>
            </h2>
            <a href="MyCourses.aspx" class="btn btn-secondary rounded-pill px-4 fw-bold shadow-sm text-white">Exit Class</a>
        </div>

        <div class="page-layout-split">
            
            <div class="layout-main-70">
                
                <div class="ec-glass-card p-4">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <span class="text-muted small fw-bold text-uppercase" style="letter-spacing:1px;">Your Progress</span>
                        <div>
                            <button type="button" id="btnRateCourse" runat="server" visible="false" class="btn btn-warning btn-sm rounded-pill fw-bold me-2 shadow-sm text-dark" data-bs-toggle="modal" data-bs-target="#courseFeedbackModal">
                                <i class="bi bi-star-fill me-1"></i> Rate Course
                            </button>
                            <span class="ec-status-pill ec-status-active"><asp:Label ID="lblProgressPercent" runat="server" Text="0%"></asp:Label></span>
                        </div>
                    </div>
                    <div class="ec-progress-container">
                        <div class="ec-progress-fill" id="progressBar" runat="server"></div>
                    </div>
                    <asp:Label ID="lblCompletionMessage" runat="server" CssClass="text-success small fw-bold d-block mt-2"></asp:Label>
                </div>

                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="fw-bold m-0 text-main"><i class="bi bi-pencil-square me-2 text-primary"></i> Tutor's Corner</h5>
                        <span class="text-muted small"><asp:Label ID="lblCurrentLessonName" runat="server"></asp:Label></span>
                    </div>
                    <div class="ec-glass-card-body text-main" style="font-size: 1.1rem; line-height: 1.8;">
                        <asp:Literal ID="litLessonNote" runat="server"></asp:Literal>
                    </div>
                </div>

                <div class="ec-video-container shadow-lg mb-4">
                    <asp:Literal ID="litVideoPlayer" runat="server"></asp:Literal>
                </div>

                <asp:PlaceHolder ID="phQuizTrigger" runat="server" Visible="false">
                    <div class="ec-glass-card">
                        <div class="row align-items-center text-center text-md-start">
                            <div class="col-md-2 text-center">
                                <i class="bi bi-trophy-fill text-primary opacity-50" style="font-size: 2.5rem;"></i>
                            </div>
                            <div class="col-md-6">
                                <asp:Panel ID="pnlQuizResult" runat="server" Visible="false">
                                    <span id="spanStatus" runat="server" class="ec-status-pill mb-2"></span>
                                    <div class="fw-bold fs-4 text-main mb-1"><asp:Literal ID="litScoreText" runat="server"></asp:Literal></div>
                                    <p class="text-muted small mb-0"><asp:Literal ID="litResultMessage" runat="server"></asp:Literal></p>
                                </asp:Panel>
                
                                <asp:Panel ID="pnlQuizPrompt" runat="server">
                                    <h6 class="fw-bold mb-1 text-main">Lesson Assessment</h6>
                                    <p class="text-muted small mb-0">Complete the quiz to unlock the next chapter.</p>
                                </asp:Panel>
                            </div>
                            <div class="col-md-4 text-md-end mt-3 mt-md-0 d-flex flex-column gap-2">
                                <asp:Button ID="btnProceedToQuiz" runat="server" Text="Start Quiz" CssClass="btn btn-primary rounded-pill px-4 fw-bold btn-sm" OnClick="btnProceedToQuiz_Click" />
                                <asp:Button ID="btnRetakeQuiz" runat="server" Text="Retake Quiz" CssClass="btn btn-outline-danger rounded-pill px-4 fw-bold btn-sm" OnClick="btnProceedToQuiz_Click" Visible="false" />
                                <asp:Button ID="btnReviewQuiz" runat="server" Text="Review Answers" CssClass="btn btn-secondary rounded-pill px-4 fw-bold btn-sm text-white" OnClick="btnReviewQuiz_Click" Visible="false" />
                            </div>
                        </div>
                    </div>
                </asp:PlaceHolder>

                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="fw-bold m-0 text-main"><i class="bi bi-people-fill me-2 text-primary"></i> Community Insights</h5>
                    </div>
                    <div class="ec-glass-card-body" style="max-height: 350px; overflow-y: auto;">
                        <asp:Repeater ID="rptPublicFeedback" runat="server">
                            <ItemTemplate>
                                <div class="ec-item-row align-items-start flex-column mb-3 p-3 rounded" style="background: var(--ec-bg-alt); border: none;">
                                    <div class="d-flex justify-content-between align-items-center w-100 mb-2">
                                        <div class="d-flex align-items-center">
                                            <div class="rounded-circle d-flex align-items-center justify-content-center me-3 shadow-sm" style="width: 35px; height: 35px; background: var(--ec-primary); color: white; font-weight: bold;">
                                                <%# Eval("fname").ToString().Substring(0,1) %>
                                            </div>
                                            <div>
                                                <h6 class="mb-0 fw-bold small text-main"><%# Eval("fname") %> <%# Eval("lname") %></h6>
                                                <small class="text-muted" style="font-size: 0.7rem;"><%# Eval("created_at", "{0:MMM dd, yyyy}") %></small>
                                            </div>
                                        </div>
                                        <div class="text-warning small"><%# new String('★', Convert.ToInt32(Eval("rating"))) %></div>
                                    </div>
                                    <p class="mb-0 text-muted small w-100" style="font-style: italic;">"<%# Eval("comment") %>"</p>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        
                        <asp:PlaceHolder ID="phNoReviews" runat="server" Visible="false">
                            <div class="text-center py-5 text-muted">
                                <p class="mt-2 small">No student reviews yet. Be the first to share your thoughts!</p>
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </div>
            </div>

            <div class="layout-sidebar-30">
                <div class="ec-glass-card p-0 overflow-hidden sticky-top" style="top: 20px;">
                    <div class="ec-glass-card-header border-bottom p-3 mb-0" style="background: var(--ec-bg-alt);">
                        <span class="fw-bold small text-muted text-uppercase">COURSE CURRICULUM</span>
                    </div>
                    <div style="max-height: 70vh; overflow-y: auto;">
                        <asp:Repeater ID="rptLessons" runat="server">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkLesson" runat="server"
                                    CssClass='<%# Convert.ToBoolean(Eval("IsAccessible")) ? "lesson-btn" : "lesson-btn lesson-locked" %>'
                                    OnClick="lnkLesson_Click" CommandArgument='<%# Eval("Id") %>'
                                    Enabled='<%# Convert.ToBoolean(Eval("IsAccessible")) %>'>
                                    <div class="d-flex align-items-center justify-content-between">
                                        <div class="d-flex align-items-center">
                                            <i class='<%# GetIcon(Eval("IsAccessible"), Eval("IsCompleted")) %> me-3 fs-5'></i>
                                            <div>
                                                <p class="mb-0 fw-bold small text-main">Lesson <%# Container.ItemIndex + 1 %></p>
                                                <small class="text-muted"><%# Eval("TypeName") %></small>
                                            </div>
                                        </div>
                                        <%# Convert.ToBoolean(Eval("IsCompleted")) ? "<i class='bi bi-check-circle-fill text-success fs-5'></i>" : "" %>
                                    </div>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>

        </div>
    </div>

    <button type="button" class="ec-fab" data-bs-toggle="modal" data-bs-target="#feedbackModal">
        <i class="bi bi-chat-square-heart-fill fs-5"></i><span>Feedback</span>
    </button>

    <div class="modal fade" id="feedbackModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content ec-modal-content">
                <div class="modal-header ec-modal-header border-0 pb-0">
                    <h5 class="modal-title fw-bold mb-0 text-main"><asp:Literal ID="litModalTitle" runat="server" Text="Lesson Feedback"></asp:Literal></h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body ec-modal-body p-4">
                    
                    <div class="mb-3">
                        <label class="form-label fw-bold small text-muted">Your Rating</label>
                        <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select ec-form-control rounded-pill">
                            <asp:ListItem Text="★ ★ ★ ★ ★ (Excellent)" Value="5"></asp:ListItem>
                            <asp:ListItem Text="★ ★ ★ ★ ☆ (Very Good)" Value="4"></asp:ListItem>
                            <asp:ListItem Text="★ ★ ★ ☆ ☆ (Average)" Value="3"></asp:ListItem>
                            <asp:ListItem Text="★ ★ ☆ ☆ ☆ (Poor)" Value="2"></asp:ListItem>
                            <asp:ListItem Text="★ ☆ ☆ ☆ ☆ (Terrible)" Value="1"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    
                    <div class="mb-4">
                        <label class="form-label fw-bold small text-muted">Your Thoughts</label>
                        <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="4" 
                            CssClass="form-control ec-form-control rounded-4" placeholder="How can we make this lesson better?"></asp:TextBox>
                    </div>
                    
                    <asp:Button ID="btnSubmitFeedback" runat="server" Text="Submit" 
                        CssClass="btn btn-primary w-100 py-2 rounded-pill fw-bold shadow-sm" OnClick="btnSubmitFeedback_Click" />
                    
                    <asp:Panel ID="pnlRemoveFeedback" runat="server" Visible="false" CssClass="mt-4 pt-3 border-top text-center" style="border-color: var(--ec-border-light) !important;">
                        <p class="text-muted small mb-2">Want to change your mind?</p>
                        <asp:Button ID="btnDeleteFeedback" runat="server" 
                            CssClass="btn btn-link text-danger small text-decoration-none fw-bold shadow-none p-0" 
                            Text="Delete My Feedback" OnClick="btnDeleteFeedback_Click" 
                            OnClientClick="return confirm('Are you sure you want to remove your review?');" />
                    </asp:Panel>

                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="courseFeedbackModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content ec-modal-content" style="border: 2px solid var(--ec-warning, #ffc107);">
                <div class="modal-header ec-modal-header border-0 pb-0 text-center d-block">
                    <h4 class="modal-title fw-bold mb-0 text-warning"><i class="bi bi-trophy-fill me-2"></i>Course Completed!</h4>
                    <p class="text-muted small mt-1 mb-0">How was your overall experience?</p>
                    <button type="button" class="btn-close btn-close-white position-absolute top-0 end-0 mt-3 me-3" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body ec-modal-body p-4 text-start">
                    
                    <div class="mb-3 text-center">
                        <label class="form-label fw-bold small text-muted text-uppercase">Overall Rating</label>
                        <asp:DropDownList ID="ddlCourseRating" runat="server" CssClass="form-select ec-form-control rounded-pill text-center mx-auto" style="max-width: 250px;">
                            <asp:ListItem Text="★ ★ ★ ★ ★ (Excellent)" Value="5"></asp:ListItem>
                            <asp:ListItem Text="★ ★ ★ ★ ☆ (Very Good)" Value="4"></asp:ListItem>
                            <asp:ListItem Text="★ ★ ★ ☆ ☆ (Average)" Value="3"></asp:ListItem>
                            <asp:ListItem Text="★ ★ ☆ ☆ ☆ (Poor)" Value="2"></asp:ListItem>
                            <asp:ListItem Text="★ ☆ ☆ ☆ ☆ (Terrible)" Value="1"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    
                    <div class="mb-4">
                        <label class="form-label fw-bold small text-muted">Write a Public Review</label>
                        <asp:TextBox ID="txtCourseComment" runat="server" TextMode="MultiLine" Rows="4" 
                            CssClass="form-control ec-form-control rounded-4" placeholder="What did you think of the whole course?"></asp:TextBox>
                    </div>
                    
                    <asp:Button ID="btnSubmitCourseFeedback" runat="server" Text="Submit Course Review" 
                        CssClass="btn btn-warning w-100 py-2 rounded-pill fw-bold text-dark shadow-sm" OnClick="btnSubmitCourseFeedback_Click" />

                    <asp:Panel ID="pnlRemoveCourseFeedback" runat="server" Visible="false" CssClass="mt-4 pt-3 border-top text-center" style="border-color: var(--ec-border-light) !important;">
                        <p class="text-muted small mb-2">Want to remove your course rating?</p>
                        <asp:Button ID="btnDeleteCourseFeedback" runat="server" 
                            CssClass="btn btn-link text-danger small text-decoration-none fw-bold shadow-none p-0" 
                            Text="Delete My Course Review" OnClick="btnDeleteCourseFeedback_Click" 
                            OnClientClick="return confirm('Are you sure you want to remove your course review? Your overall progress will remain.');" />
                    </asp:Panel>

                </div>
            </div>
        </div>
    </div>

</asp:Content>