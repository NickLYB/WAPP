<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="EditCourse.aspx.cs" Inherits="WAPP.Pages.Tutor.EditCourse" ValidateRequest="false" %>
<%@ Register Src="~/Pages/Tutor/EditCourseOverviewModal.ascx" TagPrefix="ucCourse" TagName="EditCourseOverviewModal" %>
<%@ Register Src="~/Pages/Tutor/EditLessonModal.ascx" TagPrefix="ucLesson" TagName="EditLessonModal" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sortablejs@latest/Sortable.min.js"></script>
    <style>
        /* Ghost class for SortableJS dragging */
        .sortable-ghost {
            opacity: 0.4;
            background-color: #e2e8f0 !important;
            border: 2px dashed #3b82f6 !important;
        }
        .drag-handle:active { cursor: grabbing; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>

        <div class="page-layout-split">
            
            <div class="layout-main-70">
                
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h4 class="ec-section-title" id="courseTitle" runat="server">[Course Content]</h4>
                        <asp:Button ID="btnSaveSequence" runat="server" Text="Save Order" CssClass="btn-main btn-pill" OnClick="btnSaveSequence_Click" />
                    </div>

                    <div class="ec-glass-card-body p-0">
                        <asp:HiddenField ID="hfSequenceData" runat="server" ClientIDMode="Static" />
    
                        <div id="sortable-list">
                            <asp:Repeater ID="rptCourseContent" runat="server" OnItemCommand="rptCourseContent_ItemCommand">
                                <ItemTemplate>
                                    <div class="ec-lesson-container mb-3" data-id='Lesson_<%# Eval("LessonId") %>'>
                    
                                        <div class="ec-item-row lesson-main-row px-3">
                                            <div class="d-flex align-items-center">
                                                <div class="drag-handle me-3 text-muted">
                                                    <i class="bi bi-grip-vertical"></i>
                                                </div>
                                                <div>
                                                    <span class="badge bg-primary-subtle text-primary border-primary-subtle me-2">LESSON</span>
                                                    <span class="fw-bold"><%# Eval("LessonTitle") %></span>
                                                </div>
                                            </div>

                                            <div class="d-flex gap-2">
                                                <asp:HyperLink ID="lnkView" runat="server" Text="View" CssClass="btn-sub"
                                                    NavigateUrl='<%# "~/Pages/Tutor/ViewCourse.aspx?id=" + Request.QueryString["id"] + "&resourceId=" + Eval("LessonId") %>' />
                                                <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn-sub"
                                                    CommandName="EditLesson" CommandArgument='<%# Eval("LessonId") %>' />
                                            </div>
                                        </div>

                                        <asp:PlaceHolder ID="phHasQuiz" runat="server" Visible='<%# Eval("quiz_id") != DBNull.Value %>'>
                                            <div class="ec-quiz-sub-row ms-5 me-3 mb-2 p-2 rounded-3 bg-light border-start border-4 border-info">
                                                <div class="d-flex justify-content-between align-items-center">
                                                    <div class="ps-2">
                                                        <i class="bi bi-arrow-return-right text-muted me-2"></i>
                                                        <span class="badge bg-info text-white me-2">QUIZ</span>
                                                        <span class="small fw-bold"><%# Eval("QuizTitle") %></span>
                                                    </div>
                                                    <div class="d-flex gap-2">
                                                        <asp:HyperLink ID="lnkQuizScores" runat="server" Text="Scores" CssClass="btn-sub btn-sm"
                                                            NavigateUrl='<%# "~/Pages/Tutor/ViewQuizAttempts.aspx?quiz_id=" + Eval("ActualQuizId") %>' />
                                                        <asp:HyperLink ID="lnkEditQuiz" runat="server" Text="Edit" CssClass="btn-sub btn-sm"
                                                            NavigateUrl='<%# "~/Pages/Tutor/CreateQuiz.aspx?id=" + Request.QueryString["id"] + "&quizId=" + Eval("ActualQuizId") %>' />
                                                    </div>
                                                </div>
                                            </div>
                                        </asp:PlaceHolder>

                                        <asp:PlaceHolder ID="phNoQuiz" runat="server" Visible='<%# Eval("quiz_id") == DBNull.Value %>'>
                                            <div class="ms-5 ps-3 py-1">
                                                <asp:HyperLink ID="lnkAddQuiz" runat="server" Text="+ Attach Quiz" 
                                                    CssClass="text-muted small text-decoration-none"
                                                    NavigateUrl='<%# "~/Pages/Tutor/CreateQuiz.aspx?id=" + Request.QueryString["id"] + "&lessonId=" + Eval("LessonId") %>' />
                                            </div>
                                        </asp:PlaceHolder>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div> </div>
                </div>

                <div class="ec-glass-card">
                    <div class="ec-glass-card-body">
                        <div class="d-flex gap-2 mb-4">
                            <asp:LinkButton ID="btnTabAbout" runat="server" CssClass="btn-sub active" OnClick="btnTabAbout_Click" CausesValidation="false">About</asp:LinkButton>
                            <asp:LinkButton ID="btnTabReviews" runat="server" CssClass="btn-sub" OnClick="btnTabReviews_Click" CausesValidation="false">Reviews</asp:LinkButton>
                        </div>

                        <asp:MultiView ID="mvAbout" runat="server" ActiveViewIndex="0">
                            <asp:View ID="viewAbout" runat="server">
                                <div class="p-3 bg-light rounded-3 text-muted">
                                    <asp:Label ID="lblCourseDesc" runat="server" />
                                </div>
                            </asp:View>
                            <asp:View ID="viewReviews" runat="server">
                                <asp:Label ID="lblReviewSummary" runat="server" CssClass="d-block mb-3 fw-bold" />
                                <asp:Repeater ID="rptReviews" runat="server">
                                    <ItemTemplate>
                                        <div class="ec-item-row py-3">
                                            <div>
                                                <div class="fw-bold"><%# Eval("reviewer_name") %></div>
                                                <small class="text-muted"><%# Eval("created_at", "{0:dd MMM yyyy}") %></small>
                                            </div>
                                            <div class="text-end">
                                                <span class="text-warning"><i class="bi bi-star-fill me-1"></i><%# Eval("rating") %></span>
                                                <p class="mb-0 small text-muted mt-1"><%# Eval("comment") %></p>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Label ID="lblNoReviews" runat="server" Visible="false" CssClass="ec-empty-state" Text="No reviews yet." />
                            </asp:View>
                        </asp:MultiView>
                    </div>
                </div>
            </div>

            <div class="layout-sidebar-30">
                
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="ec-section-title">Overview</h5>
                        <asp:Button ID="btnEditOverview" runat="server" Text="Edit" CssClass="btn-sub btn-sm" OnClick="btnEditOverview_Click" />
                    </div>
                    <div class="ec-glass-card-body">
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Type</span>
                            <asp:Label ID="lblCourseType" runat="server" CssClass="text-dark fw-bold" />
                        </div>
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Duration</span>
                            <asp:Label ID="lblDuration" runat="server" CssClass="text-dark fw-bold" />
                        </div>
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Lectures</span>
                            <asp:Label ID="lblLectures" runat="server" CssClass="text-dark fw-bold" />
                        </div>
                        <div class="ec-item-row py-2 border-0">
                            <span class="text-muted small fw-bold">Level</span>
                            <asp:Label ID="lblSkillLevel" runat="server" CssClass="text-dark fw-bold" />
                        </div>
                    </div>
                </div>

                <div class="ec-glass-card">
                    <div class="ec-glass-card-header border-0 pb-0">
                        <h5 class="ec-section-title">Quick Actions</h5>
                    </div>
                    <div class="ec-glass-card-body gap-2 mt-3">
                        <asp:HyperLink ID="lnkAnnouncements" runat="server" Text="Announcements" CssClass="btn-sub w-100 text-center py-2" NavigateUrl='<%# "~/Pages/Tutor/Announcement.aspx?id=" + Request.QueryString["id"] %>' />
                        <asp:HyperLink ID="lnkStudentProgress" runat="server" Text="Student Progress" CssClass="btn-sub w-100 text-center py-2" NavigateUrl='<%# "~/Pages/Tutor/StudentProgress.aspx?id=" + Request.QueryString["id"] %>' />
                        <asp:HyperLink ID="lnkUpload" runat="server" Text="Upload Materials" CssClass="btn-sub w-100 text-center py-2" NavigateUrl='<%# "~/Pages/Tutor/UploadCourseMaterial.aspx?id=" + Request.QueryString["id"] %>' />
                        <asp:HyperLink ID="lnkCreateQuiz" runat="server" Text="Add New Quiz" CssClass="btn-sub w-100 text-center py-2" NavigateUrl='<%# "~/Pages/Tutor/CreateQuiz.aspx?id=" + Request.QueryString["id"] %>' />
                    </div>
                </div>

            </div>
        </div>
    </div>

    <ucLesson:EditLessonModal ID="EditLessonModal1" runat="server" />
    <ucCourse:EditCourseOverviewModal ID="EditCourseOverviewModal1" runat="server" />

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var el = document.getElementById('sortable-list');
            var hiddenField = document.getElementById('hfSequenceData');

            if (el) {
                var sortable = Sortable.create(el, {
                    handle: '.drag-handle',
                    animation: 150,
                    ghostClass: 'sortable-ghost',
                    onEnd: function () {
                        var newOrder = sortable.toArray();
                        hiddenField.value = newOrder.join(',');
                    }
                });
            }
        });
    </script>
</asp:Content>