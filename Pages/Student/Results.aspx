<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="Results.aspx.cs" Inherits="WAPP.Pages.Student.Results" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container py-5" style="max-width: 1000px;">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StudentMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <div class="d-flex justify-content-between align-items-end mb-4">
            <div>
                <h2 class="ec-page-title m-0">My Quiz Results</h2>
                <p class="text-muted mb-0">Track your performance and review your past assessments.</p>
            </div>
            <div>
                </div>
        </div>

        <asp:Repeater ID="rptCourses" runat="server" OnItemDataBound="rptCourses_ItemDataBound">
            <ItemTemplate>
                
                <div class="ec-glass-card mb-5 p-0 overflow-hidden shadow-sm">
                    <div class="bg-primary bg-opacity-10 px-4 py-3 border-bottom border-primary border-opacity-25 d-flex justify-content-between align-items-center">
                        <h5 class="fw-bold text-main m-0">
                            <i class="bi bi-book-half me-2 text-primary"></i><%# Eval("CourseName") %>
                        </h5>
                        <span class="badge bg-white text-primary border shadow-sm">Enrolled</span>
                    </div>

                    <asp:HiddenField ID="hfEnrollmentId" runat="server" Value='<%# Eval("EnrollmentId") %>' />

                    <div class="table-responsive">
                        <table class="table ec-table-custom table-hover mb-0">
                            <thead>
                                <tr>
                                    <th class="ps-4">Lesson / Quiz</th>
                                    <th>Date Completed</th>
                                    <th>Score</th>
                                    <th>Status</th>
                                    <th class="pe-4 text-end">Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptQuizzes" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="ps-4">
                                                <span class="fw-bold d-block text-main"><%# Eval("QuizTitle") %></span>
                                                <small class="text-muted"><i class="bi bi-file-earmark-text me-1"></i>Lesson Reference: <%# Eval("ResourceId") %></small>
                                            </td>
                                            <td>
                                                <span class="text-muted small fw-bold"><%# Eval("finished_at", "{0:MMM dd, yyyy}") %></span>
                                            </td>
                                            <td>
                                                <span class="fw-bold fs-5 text-main"><%# Eval("score") %>%</span>
                                            </td>
                                            <td>
                                                <span class='<%# GetStatusClass(Eval("score")) %>'>
                                                    <%# GetStatusText(Eval("score")) %>
                                                </span>
                                            </td>
                                            <td class="pe-4 text-end">
                                                <a href='QuizReview.aspx?quizId=<%# Eval("quiz_id") %>&enrollmentId=<%# Eval("enrollment_id") %>&source=results' 
                                                   class="btn btn-sm btn-sub rounded-pill fw-bold shadow-sm">
                                                   <i class="bi bi-search me-1"></i>Review
                                                </a>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                        
                        <asp:PlaceHolder ID="phNoQuizzes" runat="server" Visible="false">
                            <div class="text-center py-5 text-muted">
                                <i class="bi bi-inbox fs-3 d-block mb-2 opacity-50"></i>
                                <span class="small fw-bold">No quizzes completed in this course yet.</span>
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </div>

            </ItemTemplate>
        </asp:Repeater>

        <asp:PlaceHolder ID="phNoEnrollments" runat="server" Visible="false">
            <div class="ec-empty-state ec-glass-card shadow-sm">
                <i class="bi bi-inbox fs-1 text-muted opacity-50"></i>
                <h4 class="fw-bold mt-3 text-main">No quizzes completed yet.</h4>
                <p class="text-muted mb-4">Enroll in a course and take a quiz to see your results here.</p>
                <a href="Study.aspx" class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm">Browse Courses</a>
            </div>
        </asp:PlaceHolder>

    </div>
</asp:Content>