<%@ Page Title="Course Management" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="CourseManagement.aspx.cs" Inherits="WAPP.Pages.Staff.CourseManagement" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    
    <script type="text/javascript">
        function pageLoad() {
            $("#<%= txtSearch.ClientID %>").off("keyup").on("keyup", SearchTable);
            SearchTable();
        }

        function SearchTable() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            if (!input) return;
            var filter = input.value.toUpperCase();
            var table = document.getElementById('<%= gvCourses.ClientID %>');
            if (!table) return;

            var tr = table.getElementsByTagName("tr");
            var visibleRows = 0;

            for (var i = 1; i < tr.length; i++) {
                var row = tr[i];
                if (row.id === 'clientNoRecordRow') continue;
                if (row.getElementsByTagName('th').length > 0) continue;

                var rowText = row.innerText || row.textContent;
                if (rowText.toUpperCase().indexOf(filter) > -1) {
                    row.style.display = ""; visibleRows++;
                } else {
                    row.style.display = "none";
                }
            }

            var noRecRow = document.getElementById('clientNoRecordRow');
            if (visibleRows === 0) {
                if (!noRecRow) {
                    var tbody = table.getElementsByTagName('tbody')[0] || table;
                    noRecRow = document.createElement('tr');
                    noRecRow.id = 'clientNoRecordRow';
                    var cell = document.createElement('td');
                    cell.colSpan = "100";
                    cell.className = "ec-no-records text-center py-4";
                    cell.innerText = "No courses found matching your filters.";
                    noRecRow.appendChild(cell);
                    tbody.appendChild(noRecRow);
                }
                noRecRow.style.display = "";
            } else if (noRecRow) { noRecRow.style.display = "none"; }
        }

        function handleEnter(e) {
            if (e.keyCode === 13 || e.key === 'Enter') {
                e.preventDefault();
                var input = document.getElementById('<%= txtSearch.ClientID %>');
                input.value = ""; SearchTable();
                return false;
            }
        }

        function openRemoveModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('removeModal')).show(); }
        function closeRemoveModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('removeModal')).hide();
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        function openViewModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).show(); }
        function closeViewModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).hide();
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }
    </script>
    <style>
        .clickable-row { cursor: pointer; transition: background-color 0.2s; }
        .clickable-row:hover { background-color: rgba(13, 110, 253, 0.05) !important; }
        
        .btn-outline-danger { border-color: #dc3545; color: #dc3545; }
        .btn-outline-danger:hover { background-color: #dc3545; color: white; }
        
        .btn-outline-primary { border-color: var(--ec-primary); color: var(--ec-primary); }
        .btn-outline-primary:hover { background-color: var(--ec-primary); color: white; }

        .btn-outline-secondary { border-color: #6c757d; color: #6c757d; }
        .btn-outline-secondary:hover { background-color: #6c757d; color: white; }
        
        .radio-verify label { color: var(--ec-success-text, #198754); font-weight: 700; cursor: pointer; margin-left: 5px; font-size: 0.95rem; }
        .radio-reject label { color: var(--ec-danger-brand, #dc3545); font-weight: 700; cursor: pointer; margin-left: 5px; font-size: 0.95rem; }
        .radio-verify input[type="radio"], .radio-reject input[type="radio"] { transform: scale(1.1); margin-top: 3px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upCourseMgmt">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper container-fluid mt-4">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StaffMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-4">Course Management</h2>
            
        <asp:UpdatePanel ID="upCourseMgmt" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card mb-4 card shadow-sm">
                    <div class="card-body p-4">
                        
                        <div class="row mb-4 align-items-end">
                            <div class="col-lg-3">
                                <label class="form-label fw-bold text-muted mb-2">Search / Filter:</label>
                                <div class="input-group">
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" Placeholder="Search by Title..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                    <span class="input-group-text ec-search-btn"><i class="bi bi-search"></i></span>
                                </div>
                            </div>
                            <div class="col-lg-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Sort By:</label>
                                <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="Latest">Latest</asp:ListItem>
                                    <asp:ListItem Value="Oldest">Oldest</asp:ListItem>
                                    <asp:ListItem Value="TutorID">Tutor ID</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            
                            <div class="col-lg-7 text-end d-flex justify-content-end align-items-center gap-2">
                                <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="btn btn-outline-secondary rounded-pill fw-bold px-4 shadow-sm d-inline-flex align-items-center" OnClick="btnClearFilters_Click" ToolTip="Clear Filters">
                                    <i class="bi bi-x-circle fs-5 me-2" style="line-height: 0;"></i> Clear Filters
                                </asp:LinkButton>
                                
                                <asp:LinkButton ID="btnReviewApplications" runat="server" CssClass="btn btn-outline-success rounded-pill fw-bold px-4 shadow-sm" OnClick="btnReviewApplications_Click">
                                    <i class="bi bi-card-checklist me-1"></i> Review Applications
                                </asp:LinkButton>
                                
                                <asp:Button ID="btnAddCourseRedirect" runat="server" Text="+ Add New Course" CssClass="btn btn-outline-primary rounded-pill fw-bold px-4 shadow-sm" OnClick="btnAddCourseRedirect_Click" />
                                
                                <asp:LinkButton ID="btnTriggerRemove" runat="server" CssClass="btn btn-outline-danger rounded-circle shadow-sm" style="width: 42px; height: 42px; display: inline-flex; align-items: center; justify-content: center;" OnClick="btnTriggerRemove_Click" ToolTip="Remove Selected">
                                    <i class="bi bi-trash3 fs-5" style="line-height: 0;"></i>
                                </asp:LinkButton>
                            </div>
                        </div>
                        
                        <div class="row g-2">
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Category</label>
                                <asp:DropDownList ID="ddlFilterCategory" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Tutor</label>
                                <asp:DropDownList ID="ddlFilterTutor" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Status</label>
                                <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="All">All Statuses</asp:ListItem>
                                    <asp:ListItem Value="PENDING">Pending</asp:ListItem>
                                    <asp:ListItem Value="APPROVED">Approved</asp:ListItem>
                                    <asp:ListItem Value="REJECT">Rejected</asp:ListItem>
                                    <asp:ListItem Value="PUBLISHED">Published</asp:ListItem>
                                    <asp:ListItem Value="PRIVATE">Private</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Rating</label>
                                <asp:DropDownList ID="ddlFilterRating" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="All">All Ratings</asp:ListItem>
                                    <asp:ListItem Value="5">5 Stars</asp:ListItem>
                                    <asp:ListItem Value="4">4 Stars</asp:ListItem>
                                    <asp:ListItem Value="3">3 Stars</asp:ListItem>
                                    <asp:ListItem Value="2">2 Stars</asp:ListItem>
                                    <asp:ListItem Value="1">1 Star</asp:ListItem>
                                    <asp:ListItem Value="NoRating">No Rating</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Month</label>
                                <asp:DropDownList ID="ddlFilterMonth" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Year</label>
                                <asp:DropDownList ID="ddlFilterYear" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="ec-staff-card card shadow-sm">
                    <div class="card-body p-0"> 
                        
                        <div class="table-responsive" style="overflow-x: auto;">
                            <asp:GridView ID="gvCourses" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" 
                                AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id" 
                                ShowHeaderWhenEmpty="true" AllowPaging="True" PageSize="10" PagerSettings-Visible="false"
                                OnRowDataBound="gvCourses_RowDataBound" OnRowCommand="gvCourses_RowCommand">
                                
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkSelect" runat="server" onclick="event.stopPropagation();" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
    
                                    <asp:TemplateField HeaderText="Course ID">
                                        <ItemTemplate>
                                            <span class="fw-bold"><%# Eval("Id", "C{0:D3}") %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
    
                                    <asp:BoundField DataField="title" HeaderText="Title" />
                                    <asp:TemplateField HeaderText="Description">
                                        <ItemTemplate>
                                            <div class="text-truncate text-muted" style="max-width: 250px;" title='<%# StripHTML(Eval("description")) %>'>
                                                <%# StripHTML(Eval("description")) %>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="category_name" HeaderText="Category" />
                                    <asp:BoundField DataField="skill_level" HeaderText="Skill Level" />
                                    <asp:BoundField DataField="tutor_name" HeaderText="Tutor" />
                                    <asp:BoundField DataField="created_at" HeaderText="Date" DataFormatString="{0:dd/MM/yyyy}" />
                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <span class='<%# GetStatusDotClass(Eval("status")) %>'></span>
                                            <%# GetStatusText(Eval("status")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Rating">
                                        <ItemTemplate>
                                            <span class="fw-bold"><%# Eval("average_rating") != DBNull.Value ? Eval("average_rating") : "N/A" %> <i class="bi bi-star-fill text-warning"></i></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>

                                <EmptyDataTemplate><tr><td colspan="100%" class="text-center py-4 text-muted">No courses found matching your filters.</td></tr></EmptyDataTemplate>
                            </asp:GridView>
                        </div>

                        <div class="d-flex justify-content-between align-items-center p-3 border-top bg-white">
                            <div class="text-muted small fw-bold">
                                <asp:Literal ID="litPagerInfo" runat="server"></asp:Literal>
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <asp:LinkButton ID="btnPrev" runat="server" CssClass="btn btn-outline-primary btn-sm fw-bold px-3" OnClick="btnPrev_Click"><i class="bi bi-chevron-left me-1"></i> Prev</asp:LinkButton>
                                <div class="d-flex align-items-center bg-light border rounded px-2 py-1">
                                    <span class="small text-muted fw-bold me-2">Page</span>
                                    <asp:TextBox ID="txtPageJump" runat="server" CssClass="form-control form-control-sm text-center ec-pager-input" Width="50px" AutoPostBack="true" OnTextChanged="txtPageJump_TextChanged"></asp:TextBox>
                                </div>
                                <asp:LinkButton ID="btnNext" runat="server" CssClass="btn btn-outline-primary btn-sm fw-bold px-3" OnClick="btnNext_Click">Next <i class="bi bi-chevron-right ms-1"></i></asp:LinkButton>
                            </div>
                        </div>

                    </div>
                </div>

                <div class="modal fade" id="removeModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content border-danger" style="border-width: 4px !important;">
                            <div class="modal-header bg-danger text-white border-0">
                                <h5 class="modal-title fw-bold text-uppercase">Confirm Removal</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="closeRemoveModal()"></button>
                            </div>
                            <div class="modal-body text-center p-4">
                                <h4 class="mb-3 fw-normal">Are you sure you want to remove these courses?</h4>
                                <p class="text-danger fs-6 mb-4"><asp:Literal ID="litSelectedTitles" runat="server"></asp:Literal></p>
                                
                                <div class="d-flex justify-content-center gap-3">
                                    <asp:Button ID="btnConfirmRemove" runat="server" Text="Remove" CssClass="btn btn-danger px-4 fw-bold rounded-pill" OnClick="btnConfirmRemove_Click" />
                                    <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill" onclick="closeRemoveModal()">Cancel</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="viewModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered modal-lg">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="ec-modal-header bg-light border-0 pb-0 position-relative">
                                <h5 class="modal-title fw-bold text-primary text-uppercase">Course / Application Details</h5>
                                <button type="button" class="btn-close" onclick="closeViewModal()" style="position:absolute; top:20px; right:20px;"></button>
                            </div>
                            <div class="ec-modal-body p-4 pt-3">
                                
                                <div class="row mb-4">
                                    <div class="col-md-4 text-center">
                                        <asp:Image ID="imgViewCourse" runat="server" CssClass="img-fluid rounded border shadow-sm" AlternateText="Course Image" style="max-height: 150px; object-fit: cover; width: 100%;" />
                                    </div>
                                    <div class="col-md-8">
                                        <div class="d-flex justify-content-between align-items-start mb-2">
                                            <div>
                                                <label class="fw-bold text-muted small d-block">Course ID</label>
                                                <div class="fw-bold fs-5 text-dark"><asp:Literal ID="litViewId" runat="server"></asp:Literal></div>
                                            </div>
                                            <div class="text-end">
                                                <label class="fw-bold text-muted small d-block">Status</label>
                                                <div class="fw-bold"><asp:Literal ID="litViewStatus" runat="server"></asp:Literal></div>
                                            </div>
                                        </div>
                                        <div>
                                            <label class="fw-bold text-muted small">Title</label>
                                            <div class="fw-bold text-primary fs-4"><asp:Literal ID="litViewTitle" runat="server"></asp:Literal></div>
                                        </div>
                                    </div>
                                </div>

                                <div class="row mb-3 border-top pt-3">
                                    <div class="col-md-4">
                                        <label class="fw-bold text-muted small">Category</label>
                                        <div class="text-dark"><asp:Literal ID="litViewCategory" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-4">
                                        <label class="fw-bold text-muted small">Skill Level</label>
                                        <div class="text-dark"><asp:Literal ID="litViewSkill" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-4">
                                        <label class="fw-bold text-muted small">Duration</label>
                                        <div class="text-dark"><asp:Literal ID="litViewDuration" runat="server"></asp:Literal></div>
                                    </div>
                                </div>

                                <div class="row mb-3">
                                    <div class="col-md-4">
                                        <label class="fw-bold text-muted small">Tutor</label>
                                        <div class="text-dark fw-bold"><asp:Literal ID="litViewTutor" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-4">
                                        <label class="fw-bold text-muted small">Submission Date</label>
                                        <div class="text-dark"><asp:Literal ID="litViewDate" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-4">
                                    </div>
                                </div>

                                <div class="mb-4 border-top pt-3">
                                    <label class="fw-bold text-muted small">Description</label>
                                    <div class="p-3 bg-light rounded text-dark border mt-1" style="min-height: 80px;">
                                        <asp:Literal ID="litViewDescription" runat="server"></asp:Literal>
                                    </div>
                                </div>

                                <asp:Panel ID="pnlApprovalActions" runat="server" CssClass="p-3 bg-light border border-danger rounded mb-4" Visible="false">
                                    <h6 class="text-danger fw-bold mb-3 small"><i class="bi bi-exclamation-circle me-1"></i> Application Requires Review</h6>
                                    <asp:HiddenField ID="hfActionCourseId" runat="server" />
                                    <div class="d-flex align-items-center gap-3">
                                        <asp:RadioButton ID="rbVerify" runat="server" GroupName="Action" Text="Approve" CssClass="radio-verify" />
                                        <asp:RadioButton ID="rbReject" runat="server" GroupName="Action" Text="Reject" CssClass="radio-reject" />
                                        <asp:Button ID="btnSubmitAction" runat="server" Text="Submit Decision" CssClass="btn btn-dark btn-sm ms-auto fw-bold px-4 rounded-pill" OnClick="btnSubmitAction_Click" />
                                    </div>
                                </asp:Panel>

                                <div class="text-end">
                                    <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill" onclick="closeViewModal()">Close</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>