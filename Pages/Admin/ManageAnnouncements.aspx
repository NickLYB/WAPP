<%@ Page Title="Manage Announcements" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true" CodeBehind="ManageAnnouncements.aspx.cs" Inherits="WAPP.Pages.Admin.ManageAnnouncements" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    
    <script type="text/javascript">
        // Universal Auto-Search
        function pageLoad() {
            $("#<%= txtSearch.ClientID %>").off("keyup").on("keyup", SearchTable);
            SearchTable();
        }

        function SearchTable() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            if (!input) return;
            var filter = input.value.toUpperCase();
            var table = document.getElementById('<%= gvAnnouncements.ClientID %>');
            if (!table) return;

            var tr = table.getElementsByTagName("tr");
            var visibleRows = 0;

            for (var i = 1; i < tr.length; i++) {
                var row = tr[i];
                if (row.id === 'clientNoRecordRow' || row.getElementsByTagName('th').length > 0) continue;

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
                    cell.innerText = "No announcements found matching your search.";
                    noRecRow.appendChild(cell);
                    tbody.appendChild(noRecRow);
                }
                noRecRow.style.display = "";
            } else if (noRecRow) { noRecRow.style.display = "none"; }
        }

        // View Modal Logic
        function openViewModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).show(); }
        function closeViewModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).hide();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        // Remove Modal Logic
        function openRemoveModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('removeModal')).show(); }
        function closeRemoveModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('removeModal')).hide();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        function selectAll(headerCheckBox) {
            var gridView = document.getElementById('<%= gvAnnouncements.ClientID %>');
            var checkBoxes = gridView.getElementsByTagName("input");
            for (var i = 0; i < checkBoxes.length; i++) {
                if (checkBoxes[i].type == "checkbox") {
                    checkBoxes[i].checked = headerCheckBox.checked;
                }
            }
        }

        function handleEnter(e) {
            if (e.keyCode === 13 || e.key === 'Enter') {
                e.preventDefault();
                document.getElementById('<%= btnSearch.ClientID %>').click();
                return false;
            }
        }

        function stopPropagation(e) {
            if (!e) var e = window.event;
            e.cancelBubble = true;
            if (e.stopPropagation) e.stopPropagation();
        }
    </script>

    <style>
        .clickable-row { cursor: pointer; transition: background-color 0.2s; }
        .clickable-row:hover { background-color: rgba(13, 110, 253, 0.05) !important; }
        .btn-outline-primary { border-color: var(--ec-primary); color: var(--ec-primary); }
        .btn-outline-primary:hover { background-color: var(--ec-primary); color: white; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelAnnouncements">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper container-fluid mt-4">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="AdminMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-4">Operational Announcement Management</h2>

        <asp:UpdatePanel ID="upPanelAnnouncements" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                
                <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold mb-4" Visible="false"></asp:Label>
                
                <div class="ec-staff-card p-4 mb-4 shadow-sm">
                    
                    <div class="row align-items-end g-3 mb-4">
                        <div class="col-lg-4">
                            <label class="form-label fw-bold text-muted mb-2 small text-uppercase">Universal Search</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" placeholder="Search anything..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                <asp:LinkButton ID="btnSearch" runat="server" CssClass="input-group-text ec-search-btn text-decoration-none" OnClick="BtnSearch_Click">
                                    <i class="bi bi-search"></i>
                                </asp:LinkButton>
                            </div>
                        </div>
                        
                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Sort By Date</label>
                            <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                <asp:ListItem Value="Newest">Newest First</asp:ListItem>
                                <asp:ListItem Value="Oldest">Oldest First</asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-6 text-end d-flex justify-content-end align-items-center gap-2">
                            <asp:LinkButton ID="btnClear" runat="server" CssClass="btn btn-outline-secondary rounded-pill fw-bold px-4 shadow-sm" style="height: 42px; display: inline-flex; align-items: center;" OnClick="btnClear_Click">
                                <i class="bi bi-x-circle me-1"></i> Clear Filters
                            </asp:LinkButton>
                            
                            <asp:Button ID="btnComposeRedirect" runat="server" Text="+ Compose Announcement" CssClass="btn btn-outline-primary rounded-pill fw-bold px-4 shadow-sm" style="height: 42px;" OnClick="btnComposeRedirect_Click" />
                            
                            <asp:LinkButton ID="btnTriggerRemove" runat="server" CssClass="btn btn-outline-danger rounded-circle shadow-sm" style="width: 42px; height: 42px; display: inline-flex; align-items: center; justify-content: center;" OnClick="btnTriggerRemove_Click" ToolTip="Remove Selected">
                                <i class="bi bi-trash3 fs-5" style="line-height: 0;"></i>
                            </asp:LinkButton>
                        </div>
                    </div>

                    <div class="row g-3">
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Target Group</label>
                            <asp:DropDownList ID="ddlFilterRole" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Status</label>
                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                <asp:ListItem Text="All Statuses" Value="All"></asp:ListItem>
                                <asp:ListItem Text="ACTIVE" Value="ACTIVE"></asp:ListItem>
                                <asp:ListItem Text="ARCHIVED" Value="ARCHIVED"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Month</label>
                            <asp:DropDownList ID="ddlFilterMonth" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Year</label>
                            <asp:DropDownList ID="ddlFilterYear" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                        </div>
                    </div>
                </div>

                <div class="ec-staff-card p-0 shadow-sm">
                    <div class="table-responsive" style="overflow-x: auto;">
                        <asp:GridView ID="gvAnnouncements" runat="server" AutoGenerateColumns="False" 
                            CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" GridLines="None" Width="100%" 
                            AllowPaging="True" PageSize="10" PagerSettings-Visible="false" ShowHeaderWhenEmpty="true" DataKeyNames="Id"
                            OnRowDataBound="gvAnnouncements_RowDataBound" OnRowCommand="gvAnnouncements_RowCommand">
                            
                            <Columns>
                                <asp:TemplateField ItemStyle-Width="40px">
                                    <HeaderTemplate>
                                        <div class="form-check d-flex justify-content-center">
                                            <input type="checkbox" class="form-check-input select-all-checkbox" onclick="selectAll(this)" />
                                        </div>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="form-check d-flex justify-content-center">
                                            <asp:CheckBox ID="chkSelect" runat="server" onclick="stopPropagation(event);" />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="ID" ItemStyle-Width="60px">
                                    <ItemTemplate><span class="fw-bold"><%# Eval("Id", "A{0:D3}") %></span></ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="title" HeaderText="Title" ItemStyle-Font-Bold="true" />
                                
                                <asp:TemplateField HeaderText="Message">
                                    <ItemTemplate>
                                        <span class="ec-desc-truncate" style="max-width: 400px; display: inline-block;" title='<%# Eval("message") %>'>
                                            <%# Eval("message") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="role_name" HeaderText="Target Group" NullDisplayText="All Roles" />
                                <asp:BoundField DataField="created_at" HeaderText="Date Created" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='ec-status-pill <%# Eval("status").ToString() == "ACTIVE" ? "ec-status-active" : "ec-status-locked" %>'>
                                            <%# Eval("status") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            
                            <EmptyDataTemplate>
                                <tr><td colspan="100%" class="ec-no-records text-center py-4">No announcements found matching your criteria.</td></tr>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>

                    <div class="ec-pager-wrapper p-3 d-flex justify-content-between align-items-center border-top">
                        <div class="ec-pager-info">
                            <span class="text-muted small fw-bold">
                                <asp:Literal ID="litPagerInfo" runat="server"></asp:Literal>
                            </span>
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

                <div class="modal fade" id="removeModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content border-danger" style="border-width: 4px !important;">
                            <div class="modal-header bg-danger text-white border-0">
                                <h5 class="modal-title fw-bold text-uppercase">Confirm Removal</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="closeRemoveModal()"></button>
                            </div>
                            <div class="modal-body ec-modal-body text-center p-4">
                                <h4 class="mb-3 fw-normal">Are you sure you want to remove these announcements?</h4>
                                <p class="text-danger fw-bold fs-5 mb-4"><asp:Literal ID="litSelectedTitles" runat="server"></asp:Literal></p>
                                <div class="d-flex justify-content-center gap-3">
                                    <asp:Button ID="btnConfirmRemove" runat="server" Text="Remove" CssClass="btn btn-danger px-4 fw-bold rounded-pill" OnClick="btnConfirmRemove_Click" UseSubmitBehavior="false" />
                                    <button type="button" class="btn btn-light px-4 fw-bold rounded-pill" onclick="closeRemoveModal()">Cancel</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="viewModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered modal-lg">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="modal-header ec-modal-header bg-light border-0 pb-0">
                                <h5 class="modal-title fw-bold text-primary text-uppercase">Announcement Details</h5>
                                <button type="button" class="btn-close" onclick="closeViewModal()"></button>
                            </div>
                            <div class="modal-body ec-modal-body p-4 pt-3">
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="fw-bold text-muted small">Announcement ID</label>
                                        <div class="fw-bold fs-5 text-dark"><asp:Literal ID="litViewId" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-6 text-end">
                                        <label class="fw-bold text-muted small">Date Created</label>
                                        <div class="fw-bold"><asp:Literal ID="litViewDate" runat="server"></asp:Literal></div>
                                    </div>
                                </div>
                                <div class="mb-3 border-top pt-3">
                                    <label class="fw-bold text-muted small">Title</label>
                                    <div class="fw-bold text-dark fs-5"><asp:Literal ID="litViewTitle" runat="server"></asp:Literal></div>
                                </div>
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="fw-bold text-muted small">Target Group</label>
                                        <div class="text-dark"><asp:Literal ID="litViewTarget" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-6 text-end">
                                        <label class="fw-bold text-muted small">Status</label>
                                        <div class="fw-bold text-dark mt-1"><asp:Literal ID="litViewStatus" runat="server"></asp:Literal></div>
                                    </div>
                                </div>
                                <div class="mb-4 border-top pt-3">
                                    <label class="fw-bold text-muted small">Message</label>
                                    <div class="p-3 bg-light rounded text-dark border mt-1" style="min-height: 80px;">
                                        <asp:Literal ID="litViewMessage" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                            <div class="modal-footer ec-modal-footer text-end border-0 pt-0">
                                <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill" onclick="closeViewModal()">Close</button>
                            </div>
                        </div>
                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>