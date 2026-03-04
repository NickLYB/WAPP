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
                    cell.className = "ec-no-records"; /* Updated to Master CSS class */
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
    </script>
    </asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upCourseMgmt">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper">
        <h2 class="ec-staff-title text-uppercase mb-4">Course Management</h2>
            
        <asp:UpdatePanel ID="upCourseMgmt" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card mb-4">
                    <div class="card-body p-4">
                        
                        <div class="row mb-4 align-items-end">
                            <div class="col-lg-4">
                                <label class="form-label fw-bold text-muted mb-2">Search / Filter:</label>
                                <div class="input-group">
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" Placeholder="Search by Title..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                    <span class="input-group-text ec-search-btn"><i class="bi bi-search"></i></span>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Sort By:</label>
                                <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="Latest">Latest</asp:ListItem>
                                    <asp:ListItem Value="Oldest">Oldest</asp:ListItem>
                                    <asp:ListItem Value="TutorID">Tutor ID</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-lg-5 text-end">
                                <asp:Button ID="btnAddCourseRedirect" runat="server" Text="+ Add New Course" CssClass="btn btn-primary rounded-pill fw-bold px-4 shadow-sm me-2" OnClick="btnAddCourseRedirect_Click" />
                                <asp:LinkButton ID="btnTriggerRemove" runat="server" CssClass="btn btn-danger rounded-pill fw-bold px-4 shadow-sm" OnClick="btnTriggerRemove_Click">- Remove Course</asp:LinkButton>
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
                                    <asp:ListItem Value="All">All</asp:ListItem>
                                    <asp:ListItem Value="ACTIVE">Approved</asp:ListItem>
                                    <asp:ListItem Value="DRAFT">Pending</asp:ListItem>
                                    <asp:ListItem Value="ARCHIVED">Rejected</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Rating</label>
                                <asp:DropDownList ID="ddlFilterRating" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="All">All</asp:ListItem>
                                    <asp:ListItem Value="5">5 Stars</asp:ListItem>
                                    <asp:ListItem Value="4">4 Stars</asp:ListItem>
                                    <asp:ListItem Value="3">3 Stars</asp:ListItem>
                                    <asp:ListItem Value="2">2 Stars</asp:ListItem>
                                    <asp:ListItem Value="1">1 Star</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Month</label>
                                <asp:DropDownList ID="ddlFilterMonth" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="All">All</asp:ListItem>
                                    <asp:ListItem Value="1">January</asp:ListItem>
                                    <asp:ListItem Value="2">February</asp:ListItem>
                                    <asp:ListItem Value="3">March</asp:ListItem>
                                    <asp:ListItem Value="4">April</asp:ListItem>
                                    <asp:ListItem Value="5">May</asp:ListItem>
                                    <asp:ListItem Value="6">June</asp:ListItem>
                                    <asp:ListItem Value="7">July</asp:ListItem>
                                    <asp:ListItem Value="8">August</asp:ListItem>
                                    <asp:ListItem Value="9">September</asp:ListItem>
                                    <asp:ListItem Value="10">October</asp:ListItem>
                                    <asp:ListItem Value="11">November</asp:ListItem>
                                    <asp:ListItem Value="12">December</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <label class="form-label small text-muted mb-1 fw-bold">Year</label>
                                <asp:DropDownList ID="ddlFilterYear" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                        </div>

                    </div>
                </div>

                <div class="ec-staff-card">
                    <div class="card-body p-0"> 
                        
                        <asp:GridView ID="gvCourses" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" 
                            AutoGenerateColumns="False" GridLines="None" 
                            DataKeyNames="Id" 
                            ShowHeaderWhenEmpty="true"
                            AllowPaging="True" 
                            PageSize="10" 
                            PagerSettings-Visible="false"
                            OnDataBound="gvCourses_DataBound">
                            
                            <Columns>
                                <asp:TemplateField><ItemTemplate><asp:CheckBox ID="chkSelect" runat="server" /></ItemTemplate></asp:TemplateField>
                                <asp:BoundField DataField="title" HeaderText="Title" />
                                <asp:TemplateField HeaderText="Description">
                                    <ItemTemplate>
                                        <span class="ec-desc-truncate" title='<%# Eval("description") %>'><%# Eval("description") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="category_name" HeaderText="Category" />
                                <asp:BoundField DataField="skill_level" HeaderText="Skill Level" />
                                <asp:BoundField DataField="tutor_name" HeaderText="Tutor ID & Name" />
                                <asp:BoundField DataField="created_at" HeaderText="Date Created" DataFormatString="{0:dd/MM/yyyy}" />
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='<%# GetStatusDotClass(Eval("status")) %>'></span>
                                        <%# GetStatusText(Eval("status")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Rating">
                                    <ItemTemplate>
                                        <span class="fw-bold"><%# Eval("average_rating") ?? "N/A" %> <i class="bi bi-star-fill text-warning"></i></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <EmptyDataTemplate><tr><td colspan="100%" class="ec-no-records">No courses found matching your filters.</td></tr></EmptyDataTemplate>
                        </asp:GridView>

                        <div class="ec-pager-wrapper">
                            <div class="ec-pager-info">
                                <asp:Literal ID="litPagerInfo" runat="server"></asp:Literal>
                            </div>
                            <div class="d-flex gap-4">
                                <asp:LinkButton ID="btnPrev" runat="server" CssClass="ec-pager-link" OnClick="btnPrev_Click"><i class="bi bi-chevron-left me-1"></i>Previous</asp:LinkButton>
                                <asp:LinkButton ID="btnNext" runat="server" CssClass="ec-pager-link" OnClick="btnNext_Click">Next<i class="bi bi-chevron-right ms-1"></i></asp:LinkButton>
                            </div>
                        </div>

                    </div>
                </div>

                <div class="modal fade" id="removeModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content border-danger" style="border-width: 4px !important;">
                            <div class="ec-modal-header bg-danger text-white border-0">
                                <h5 class="modal-title fw-bold text-uppercase">REMOVE COURSE CONFIRMATION</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="closeRemoveModal()"></button>
                            </div>
                            <div class="ec-modal-body text-center p-4">
                                <h4 class="mb-3 fw-normal">Are you sure to remove the selected course(s)?</h4>
                                <p class="text-danger fw-bold fs-5 mb-4"><asp:Literal ID="litSelectedTitles" runat="server"></asp:Literal></p>
                                <div class="d-flex justify-content-center gap-3">
                                    <asp:Button ID="btnConfirmRemove" runat="server" Text="Remove" CssClass="btn btn-danger px-4 fw-bold rounded-pill" OnClick="btnConfirmRemove_Click" />
                                    <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill" onclick="closeRemoveModal()">Cancel</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>