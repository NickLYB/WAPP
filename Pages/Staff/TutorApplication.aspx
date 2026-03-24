<%@ Page Title="Tutor Application Management" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="TutorApplication.aspx.cs" Inherits="WAPP.Pages.Staff.TutorApplication" EnableEventValidation="false" %>

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
            var table = document.getElementById('<%= gvApplications.ClientID %>');
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
                    cell.innerText = "No applications found matching your filters.";
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
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '');
        }

        function openViewModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).show(); }
        function closeViewModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).hide();
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        function stopPropagation(e) {
            if (!e) var e = window.event;
            e.cancelBubble = true;
            if (e.stopPropagation) e.stopPropagation();
        }
    </script>
    <style>
        .radio-verify label { color: var(--ec-success-text, #198754); font-weight: 700; cursor: pointer; margin-left: 5px; }
        .radio-reject label { color: var(--ec-danger-brand, #dc3545); font-weight: 700; cursor: pointer; margin-left: 5px; }
        .clickable-row { cursor: pointer; transition: background-color 0.2s; }
        .clickable-row:hover { background-color: rgba(13, 110, 253, 0.05) !important; }

        .btn-outline-danger { border-color: #dc3545; color: #dc3545; }
        .btn-outline-danger:hover { background-color: #dc3545; color: white; }

        .btn-outline-secondary { border-color: #6c757d; color: #6c757d; }
        .btn-outline-secondary:hover { background-color: #6c757d; color: white; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upTutorApp">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper container-fluid mt-4">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StaffMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-4">Tutor Application Management</h2>
            
        <asp:UpdatePanel ID="upTutorApp" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card mb-4 card shadow-sm">
                    <div class="card-body p-4">
                        
                        <div class="row mb-4 align-items-end">
                            <div class="col-lg-4">
                                <label class="form-label fw-bold text-muted mb-2">Search / Filter:</label>
                                <div class="input-group">
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" Placeholder="Search ID, name..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                    <span class="input-group-text ec-search-btn"><i class="bi bi-search"></i></span>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Sort By Date:</label>
                                <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="DESC">Newest First</asp:ListItem>
                                    <asp:ListItem Value="ASC">Oldest First</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-lg-5 text-end d-flex justify-content-end align-items-center gap-2">
                                <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="btn btn-outline-secondary rounded-pill fw-bold px-4 shadow-sm d-inline-flex align-items-center" OnClick="btnClearFilters_Click" ToolTip="Clear Filters">
                                    <i class="bi bi-x-circle fs-5 me-2" style="line-height: 0;"></i> Clear Filters
                                </asp:LinkButton>

                                <asp:LinkButton ID="btnTriggerRemove" runat="server" CssClass="btn btn-outline-danger rounded-circle shadow-sm" style="width: 42px; height: 42px; display: inline-flex; align-items: center; justify-content: center;" OnClick="btnTriggerRemove_Click" ToolTip="Remove Selected">
                                    <i class="bi bi-trash3 fs-5" style="line-height: 0;"></i>
                                </asp:LinkButton>
                            </div>
                        </div>

                        <div class="row g-2">
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Status Filter:</label>
                                <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="All">All</asp:ListItem>
                                    <asp:ListItem Value="PENDING">Pending</asp:ListItem>
                                    <asp:ListItem Value="APPROVED">Verified</asp:ListItem>
                                    <asp:ListItem Value="REJECTED">Rejected</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                    </div>
                </div>

                <div class="ec-staff-card card shadow-sm">
                    <div class="card-body p-0"> 
                        
                        <div class="table-responsive" style="overflow-x: auto;">
                            <asp:GridView ID="gvApplications" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" 
                                AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id" 
                                ShowHeaderWhenEmpty="true" OnRowDataBound="gvApplications_RowDataBound" OnRowCommand="gvApplications_RowCommand"
                                AllowPaging="True" PageSize="10" PagerSettings-Visible="false">
                                
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <div onclick="stopPropagation(event);">
                                                <asp:CheckBox ID="chkSelect" runat="server" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Id" HeaderText="App ID" />
                                    <asp:BoundField DataField="tutor_id" HeaderText="Tutor ID" />
                                    <asp:BoundField DataField="FullName" HeaderText="Name" />
                                    <asp:BoundField DataField="submitted_at" HeaderText="Submission Date" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                    
                                    <asp:TemplateField HeaderText="Resume/CV">
                                        <ItemTemplate>
                                            <a href='<%# ResolveUrl("~/Uploads/Verification/" + Eval("verification_document").ToString()) %>' target="_blank" class="text-danger fs-4" title="View Document" onclick="stopPropagation(event);">
                                                <i class="bi bi-file-earmark-pdf-fill"></i>
                                            </a>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <span class='<%# GetStatusDotClass(Eval("status")) %>'></span>
                                            <%# GetStatusText(Eval("status")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Verify / Rejected">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" onclick="stopPropagation(event);">
                                                <asp:RadioButton ID="rbVerify" runat="server" GroupName="Action" Text="Verify" CssClass="me-3 radio-verify" AutoPostBack="true" OnCheckedChanged="Action_Changed" />
                                                <asp:RadioButton ID="rbReject" runat="server" GroupName="Action" Text="Reject" CssClass="radio-reject" AutoPostBack="true" OnCheckedChanged="Action_Changed" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:BoundField DataField="ReviewerName" HeaderText="Reviewed By" NullDisplayText="None" />
                                    <asp:BoundField DataField="verified_at" HeaderText="Verified Date" DataFormatString="{0:dd/MM/yyyy HH:mm}" NullDisplayText="Pending" />
                                </Columns>
                                
                                <EmptyDataTemplate><tr><td colspan="100%" class="ec-no-records text-center py-4">No applications found matching your filters.</td></tr></EmptyDataTemplate>
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
                                <h4 class="mb-3 fw-normal">Are you sure to remove the selected application(s)?</h4>
                                <p class="text-danger fw-bold fs-5 mb-4">
                                    <asp:Literal ID="litSelectedIds" runat="server"></asp:Literal>
                                </p>
                                <div class="d-flex justify-content-center gap-3">
                                    <asp:Button ID="btnConfirmRemove" runat="server" Text="Delete Permanently" CssClass="btn btn-danger px-4 rounded-pill fw-bold" OnClick="btnConfirmRemove_Click" />
                                    <button type="button" class="btn btn-secondary px-4 rounded-pill fw-bold" onclick="closeRemoveModal()">Cancel</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="viewModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="ec-modal-header bg-light border-0 pb-0 position-relative">
                                <h5 class="modal-title fw-bold text-primary text-uppercase">Application Details</h5>
                                <button type="button" class="btn-close" onclick="closeViewModal()" style="position:absolute; top:20px; right:20px;"></button>
                            </div>
                            <div class="ec-modal-body p-4 pt-3">
                                <div class="row mb-3">
                                    <div class="col-6">
                                        <label class="fw-bold text-muted small">Application ID</label>
                                        <div class="fw-bold fs-5 text-dark"><asp:Literal ID="litViewId" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-6 text-end">
                                        <label class="fw-bold text-muted small">Submission Date</label>
                                        <div class="fw-bold text-dark"><asp:Literal ID="litViewDate" runat="server"></asp:Literal></div>
                                    </div>
                                </div>
                                
                                <div class="mb-3 border-top pt-3">
                                    <label class="fw-bold text-muted small">Applicant Name</label>
                                    <div class="fw-bold text-dark fs-5"><asp:Literal ID="litViewName" runat="server"></asp:Literal></div>
                                </div>

                                <div class="row mb-3">
                                    <div class="col-6">
                                        <label class="fw-bold text-muted small">Status</label>
                                        <div class="text-dark"><asp:Literal ID="litViewStatus" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-6 text-end">
                                        <label class="fw-bold text-muted small">Document</label>
                                        <div><asp:Literal ID="litViewDoc" runat="server"></asp:Literal></div>
                                    </div>
                                </div>

                                <div class="mb-4 border-top pt-3">
                                    <label class="fw-bold text-muted small">Reviewed By</label>
                                    <div class="text-dark"><asp:Literal ID="litViewReviewer" runat="server"></asp:Literal></div>
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