<%@ Page Title="Explore Courses" Language="C#" MasterPageFile="~/Masters/Student.Master" AutoEventWireup="true" CodeBehind="Study.aspx.cs" Inherits="WAPP.Pages.Student.Study" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    
    <script type="text/javascript">
        // 1. The Timer: Waits 400ms after you STOP typing, then automatically searches!
        let typingTimer;
        const doneTypingInterval = 400;

        function startSearchTimer() {
            clearTimeout(typingTimer);
            typingTimer = setTimeout(performSearch, doneTypingInterval);
        }

        function performSearch() {
            // Automatically clicks the hidden C# button
            var btn = document.getElementById('btnHiddenSearch');
            if (btn) { btn.click(); }
        }

        // 2. Keeps your cursor inside the textbox so you can keep typing seamlessly
        var focusedElementId = "";
        var cursorPosition = 0;

        function pageLoad() {
            var prm = Sys.WebForms.PageRequestManager.getInstance();

            prm.add_beginRequest(function (source, args) {
                var activeElement = document.activeElement;
                if (activeElement && activeElement.tagName === "INPUT" && activeElement.type === "text") {
                    focusedElementId = activeElement.id;
                    cursorPosition = activeElement.selectionStart;
                }
            });

            prm.add_endRequest(function (source, args) {
                if (focusedElementId) {
                    var activeElement = document.getElementById(focusedElementId);
                    if (activeElement) {
                        activeElement.focus();
                        activeElement.setSelectionRange(cursorPosition, cursorPosition);
                    }
                }
            });
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StudentMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <div class="mb-4">
            <h1 class="ec-page-title text-uppercase">Explore Courses</h1>
            <p class="ec-page-subtitle">Browse available learning resources and enhance your skills.</p>
        </div>

        <asp:UpdatePanel ID="upSearch" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                
                <%-- Hidden button using ClientIDMode="Static" so JS can easily click it --%>
                <asp:Button ID="btnHiddenSearch" runat="server" ClientIDMode="Static" OnClick="Search_Changed" style="display:none;" />

                <div class="ec-glass-card mb-5 p-4">
                    <div class="row align-items-center justify-content-between g-3">
                        
                        <div class="col-lg-8">
                            <div class="input-group">
                                <%-- 'oninput' detects every single keystroke instantly --%>
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" 
                                    placeholder="Search courses, tutors, or keywords..." 
                                    oninput="startSearchTimer()">
                                </asp:TextBox>
                                
                                <span class="input-group-text ec-search-btn" id="searchIcon" style="border-radius: 0 50px 50px 0;">
                                    <i class="bi bi-search"></i>
                                </span>
                            </div>
                        </div>

                        <div class="col-lg-4">
                            <%-- AutoPostBack="true" makes the dropdown instantly filter when clicked --%>
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select ec-filter-ddl w-100"
                                AutoPostBack="true" OnSelectedIndexChanged="Search_Changed">
                            </asp:DropDownList>
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
                                    <img src='<%# ResolveUrl(Eval("image_path") == DBNull.Value || string.IsNullOrWhiteSpace(Eval("image_path").ToString()) ? "~/Images/default-course.png" : Eval("image_path").ToString()) %>' class="ec-course-img" alt="Course Image" />
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
                    
                    <asp:PlaceHolder ID="phNoCourses" runat="server" Visible="false">
                        <div class="col-12 text-center py-5">
                            <i class="bi bi-search" style="font-size: 3rem; color: var(--ec-border-light);"></i>
                            <h4 class="mt-3 text-muted">No courses found</h4>
                            <p class="text-muted">Try adjusting your search or category filter.</p>
                        </div>
                    </asp:PlaceHolder>
                </div>
                
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>