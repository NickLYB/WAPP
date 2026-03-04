<%@ Page Title="Explore Courses" Language="C#" MasterPageFile="~/Masters/Student.Master" AutoEventWireup="true" CodeBehind="Study.aspx.cs" Inherits="WAPP.Pages.Student.Study" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="ec-content-wrapper">

        <div class="mb-4">
            <h1 class="ec-page-title text-uppercase">Explore Courses</h1>
            <p class="ec-page-subtitle">Browse available learning resources and enhance your skills.</p>
        </div>

        <div class="ec-glass-card mb-5 p-4">
            <div class="row align-items-center g-3">
                
                <div class="col-lg-6">
                    <div class="input-group">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" 
                            placeholder="Search courses, tutors, or keywords..." oninput="toggleClearButton()">
                        </asp:TextBox>
                        
                        <span class="input-group-text ec-search-btn" id="searchIcon" style="border-radius: 0 50px 50px 0;">
                            <i class="bi bi-search"></i>
                        </span>
                    </div>
                </div>

                <div class="col-lg-4">
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select ec-filter-ddl w-100">
                    </asp:DropDownList>
                </div>

                <div class="col-lg-2 text-end">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary rounded-pill fw-bold w-100 shadow-sm" OnClick="btnSearch_Click" />
                </div>

            </div>
        </div>

        <div class="ec-section-header d-flex justify-content-between align-items-center mb-4">
            <h5 class="ec-section-title m-0">Available Courses</h5>
            <span class="badge bg-secondary bg-opacity-10 text-secondary fs-6 rounded-pill px-3 py-2">
                <asp:Label ID="lblCount" runat="server" Text="0"></asp:Label> results found
            </span>
        </div>

        <div class="ec-course-grid">
            <asp:Repeater ID="rptCourses" runat="server">
                <ItemTemplate>
                    <div class="ec-course-box shadow-sm">
                        
                        <div class="ec-course-img-wrapper">
                            <img src='<%# ResolveUrl(Eval("image_path").ToString()) %>' class="ec-course-img" alt="Course Image" />
                        </div>
                        
                        <div class="ec-course-body">

                            <div class="ec-badge-row align-items-center">
                                <span style="color: var(--ec-primary); font-weight: 800; font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.5px;">
                                    <%# Eval("TypeName") %>
                                </span>
                                <span style="color: var(--ec-text-muted); font-weight: 700; font-size: 0.75rem; text-transform: uppercase;">
                                    <%# Eval("skill_level") %>
                                </span>
                            </div>

                            <h5 class="fw-bold mb-1" style="color: var(--ec-text-main); font-size: 1.15rem; line-height: 1.4;">
                                <%# Eval("title") %>
                            </h5>

                            <small class="mb-0" style="color: var(--ec-text-muted); font-weight: 500;">
                                <%# Eval("TutorFullName") %>
                            </small>

                            <div class="ec-course-footer mt-auto">
                                <span style="color: var(--ec-text-muted); font-weight: 500; font-size: 0.85rem;">
                                    <i class="bi bi-clock text-secondary me-1"></i>
                                    <%# Eval("duration_minutes") %> mins
                                </span>

                                <asp:HyperLink ID="hlDetails" runat="server"
                                    NavigateUrl='<%# ResolveUrl("~/Pages/Student/CourseDetail.aspx?id=" + Eval("Id")) %>'
                                    CssClass="btn btn-outline-primary rounded-pill btn-sm px-4 fw-bold">
                                    View
                                </asp:HyperLink>
                            </div>

                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

    </div>

    <script>
        // Simplified script for handling clearing
        function toggleClearButton() {
            // Optional: If you want to dynamically show/hide a clear icon 
            // inside the textbox, handle it here.
        }

        function clearSearch() {
            const input = document.getElementById('<%= txtSearch.ClientID %>');
            input.value = '';
            input.focus();
        }
    </script>

</asp:Content>