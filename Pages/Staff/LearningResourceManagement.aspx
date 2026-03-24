<%@ Page Title="Learning Resources Management" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="LearningResourceManagement.aspx.cs" Inherits="WAPP.Pages.Staff.LearningResourcesManagement" EnableEventValidation="false" %>

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
            var table = document.getElementById('<%= gvResources.ClientID %>');
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
                    cell.innerText = "No learning resources found matching your filters.";
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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upResourceMgmt">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper container-fluid mt-4">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StaffMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-4">Learning Resources Management</h2>
            
        <asp:UpdatePanel ID="upResourceMgmt" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card mb-4 card shadow-sm">
                    <div class="card-body p-4">
                        
                        <div class="row mb-4 align-items-end">
                            <div class="col-lg-4">
                                <div class="d-flex justify-content-between align-items-end mb-2">
                                    <label class="form-label fw-bold text-muted m-0">Search / Filter:</label>
                                </div>
                                <div class="input-group">
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" Placeholder="Search Title, Type..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                    <span class="input-group-text ec-search-btn"><i class="bi bi-search"></i></span>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Sort By:</label>
                                <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="Latest">Latest First</asp:ListItem>
                                    <asp:ListItem Value="Oldest">Oldest First</asp:ListItem>
                                    <asp:ListItem Value="TutorID">Tutor ID</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-lg-5 text-end d-flex justify-content-end align-items-center gap-2">
                                <asp:LinkButton ID="LinkButton1" runat="server" CssClass="btn btn-outline-secondary rounded-pill fw-bold px-4 shadow-sm d-inline-flex align-items-center" OnClick="btnClearFilters_Click" ToolTip="Clear Filters">
                                    <i class="bi bi-x-circle fs-5 me-2" style="line-height: 0;"></i> Clear Filters
                                </asp:LinkButton>
                                
                                <asp:Button ID="btnAddResourceRedirect" runat="server" Text="+ Add New Resource" CssClass="btn btn-outline-primary rounded-pill fw-bold px-4 shadow-sm" OnClick="btnAddResourceRedirect_Click" />
                                
                                <asp:LinkButton ID="btnTriggerRemove" runat="server" CssClass="btn btn-outline-danger rounded-circle shadow-sm" style="width: 42px; height: 42px; display: inline-flex; align-items: center; justify-content: center;" OnClick="btnTriggerRemove_Click" ToolTip="Remove Selected">
                                    <i class="bi bi-trash3 fs-5" style="line-height: 0;"></i>
                                </asp:LinkButton>
                            </div>
                        </div>
                        
                        <div class="row g-2">
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Type</label>
                                <asp:DropDownList ID="ddlFilterType" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Tutor</label>
                                <asp:DropDownList ID="ddlFilterTutor" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Month</label>
                                <asp:DropDownList ID="ddlFilterMonth" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Year</label>
                                <asp:DropDownList ID="ddlFilterYear" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                        </div>

                    </div>
                </div>

                <div class="ec-staff-card card shadow-sm">
                    <div class="card-body p-0"> 
                        
                        <div class="table-responsive" style="overflow-x: auto;">
                            <asp:GridView ID="gvResources" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" 
                                AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id" ShowHeaderWhenEmpty="true"
                                AllowPaging="True" PageSize="10" PagerSettings-Visible="false"
                                OnRowDataBound="gvResources_RowDataBound" OnRowCommand="gvResources_RowCommand">
                                
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkSelect" runat="server" onclick="event.stopPropagation();" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
    
                                    <asp:TemplateField HeaderText="Resource ID">
                                        <ItemTemplate>
                                            <span class="fw-bold"><%# Eval("Id", "R{0:D3}") %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="CourseAndResource" HeaderText="Course & Resource Title" />
                                    <asp:BoundField DataField="created_at" HeaderText="Date Uploaded" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                    <asp:BoundField DataField="ResourceTypeName" HeaderText="Type" />
                                    <asp:BoundField DataField="TutorName" HeaderText="Tutor" />
                                    <asp:TemplateField HeaderText="Resource Link">
                                        <ItemTemplate>
                                            <a href='<%# ResolveUrl(Eval("resource_link").ToString()) %>' target="_blank" class='<%# Eval("ResourceTypeName").ToString().ToLower() == "pdf" || Eval("ResourceTypeName").ToString().ToLower() == "document" ? "text-danger fw-bold fs-4" : "text-primary fw-bold" %>' title='<%# Eval("resource_link") %>' onclick="event.stopPropagation();">
                                                <i class='<%# Eval("ResourceTypeName").ToString().ToLower() == "pdf" || Eval("ResourceTypeName").ToString().ToLower() == "document" ? "bi bi-file-earmark-pdf-fill" : "bi bi-box-arrow-up-right me-1" %>'></i> 
                                                <%# Eval("ResourceTypeName").ToString().ToLower() == "pdf" || Eval("ResourceTypeName").ToString().ToLower() == "document" ? "" : "View" %>
                                            </a>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>

                                <EmptyDataTemplate><tr><td colspan="100%" class="text-center py-4 text-muted">No learning resources found matching your filters.</td></tr></EmptyDataTemplate>
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
                            <div class="modal-header bg-danger text-white border-0 position-relative">
                                <h5 class="modal-title fw-bold text-uppercase">Confirm Removal</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="closeRemoveModal()" style="position:absolute; top:20px; right:20px;"></button>
                            </div>
                            <div class="modal-body text-center p-4">
                                <h4 class="mb-3 fw-normal">Are you sure you want to remove these resources?</h4>
                                
                                <div class="mb-4">
                                    <asp:Literal ID="litSelectedTitles" runat="server"></asp:Literal>
                                </div>
                                
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
                                <h5 class="modal-title fw-bold text-primary text-uppercase">Resource Details</h5>
                                <button type="button" class="btn-close" onclick="closeViewModal()" style="position:absolute; top:20px; right:20px;"></button>
                            </div>
                            <div class="ec-modal-body p-4 pt-3">
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="fw-bold text-muted small">Resource ID</label>
                                        <div class="fw-bold fs-5 text-dark"><asp:Literal ID="litViewId" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-6 text-end">
                                        <label class="fw-bold text-muted small">Date Uploaded</label>
                                        <div class="fw-bold"><asp:Literal ID="litViewDate" runat="server"></asp:Literal></div>
                                    </div>
                                </div>
                                
                                <div class="mb-3 border-top pt-3">
                                    <label class="fw-bold text-muted small">Course & Resource Title</label>
                                    <div class="fw-bold text-dark fs-5"><asp:Literal ID="litViewTitle" runat="server"></asp:Literal></div>
                                </div>

                                <div class="row mb-4">
                                    <div class="col-md-6">
                                        <label class="fw-bold text-muted small">Type</label>
                                        <div class="text-dark"><asp:Literal ID="litViewType" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="fw-bold text-muted small">Tutor</label>
                                        <div class="text-dark fw-bold"><asp:Literal ID="litViewTutor" runat="server"></asp:Literal></div>
                                    </div>
                                </div>

                                <div class="text-end border-top pt-3">
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