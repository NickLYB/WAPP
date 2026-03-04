<%@ Page Title="Student Progress" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="StudentProgress.aspx.cs" Inherits="WAPP.Pages.Tutor.StudentProgress" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <style>
        /* Segmented Progress Bar Styling */
        .progress-segmented-container {
            display: flex;
            gap: 4px; /* Gap between each lesson block */
            height: 14px;
            width: 100%;
            border-radius: 4px;
            overflow: hidden;
            background-color: transparent;
        }
        
        .progress-segment {
            flex: 1; /* Makes all blocks equal width */
            background-color: #e2e8f0; /* Default Gray (Incomplete) */
            border-radius: 3px;
            transition: background-color 0.3s ease;
            cursor: pointer;
        }
        
        .progress-segment.completed {
            background-color: var(--ec-primary, #0d6efd); /* Blue (Completed) */
        }

        .progress-segment:hover {
            opacity: 0.8;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        <div class="ec-item-gap">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>

        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="ec-page-title m-0">
                <i class="bi bi-bar-chart-line-fill me-2 text-primary"></i> 
                Student Progress: <asp:Literal ID="litCourseTitle" runat="server"></asp:Literal>
            </h2>
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-secondary rounded-pill px-4 fw-bold shadow-sm text-white">
                Back to Course
            </asp:HyperLink>
        </div>

        <div class="ec-glass-card">
            <div class="ec-glass-card-header border-bottom">
                <div class="d-flex justify-content-between align-items-center w-100">
                    <h5 class="fw-bold m-0 text-main">Enrolled Students (<asp:Label ID="lblTotalStudents" runat="server" Text="0"></asp:Label>)</h5>
                    <span class="small text-muted"><i class="bi bi-info-circle me-1"></i>Hover over blocks to see lesson names</span>
                </div>
            </div>

            <div class="ec-glass-card-body">
                
                <asp:PlaceHolder ID="phNoStudents" runat="server" Visible="false">
                    <div class="text-center py-5 text-muted">
                        <i class="bi bi-people text-muted opacity-50" style="font-size: 3rem;"></i>
                        <h5 class="mt-3 fw-bold">No Students Yet</h5>
                        <p class="small">Students who enroll in this course will appear here.</p>
                    </div>
                </asp:PlaceHolder>

                <asp:Repeater ID="rptStudents" runat="server" OnItemDataBound="rptStudents_ItemDataBound">
                    <ItemTemplate>
                        <div class="ec-item-row flex-column align-items-stretch mb-3 p-3 border rounded-3 bg-white shadow-sm hover-shadow transition">
                            
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <div class="d-flex align-items-center">
                                    <div class="rounded-circle d-flex align-items-center justify-content-center me-3" style="width: 35px; height: 35px; background: var(--ec-primary-subtle); color: var(--ec-primary); font-weight: bold;">
                                        <%# Eval("Initials") %>
                                    </div>
                                    <span class="fw-bold text-dark"><%# Eval("StudentName") %></span>
                                </div>
                                <span class="badge bg-primary-subtle text-primary border-primary-subtle px-3 py-2 rounded-pill fw-bold">
                                    <%# Eval("ProgressPercentage") %>%
                                </span>
                            </div>

                            <div class="progress-segmented-container mt-2">
                                <asp:Repeater ID="rptSegments" runat="server">
                                    <ItemTemplate>
                                        <div class='<%# Convert.ToBoolean(Eval("IsCompleted")) ? "progress-segment completed" : "progress-segment" %>' 
                                             title='<%# Eval("Tooltip") %>'></div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>

                        </div>
                    </ItemTemplate>
                </asp:Repeater>

            </div>
        </div>
    </div>
</asp:Content>