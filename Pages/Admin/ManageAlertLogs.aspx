<%@ Page Title="Alert Logs" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true" CodeBehind="ManageAlertLogs.aspx.cs" Inherits="WAPP.Pages.Admin.ManageAlertLogs" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    
    <style>
        /* Forces all status badges to be consistent */
        .ec-status-badge { display: inline-block; min-width: 100px; text-align: center; letter-spacing: 0.5px; }

        /* Prevents long descriptions from breaking the table layout */
        .log-desc-col { max-width: 350px; white-space: normal; word-break: break-word; }
        .log-time-col { min-width: 130px; white-space: nowrap; }
        .log-status-col { min-width: 130px; }
        
        .clickable-row { cursor: pointer; transition: background-color 0.2s; }
        .clickable-row:hover { background-color: rgba(13, 110, 253, 0.05) !important; }
    </style>

    <script type="text/javascript">
        // ---------------------------------------------------
        // Universal Auto-Search (Client-Side Filtering)
        // ---------------------------------------------------
        function pageLoad() {
            $("#<%= txtSearch.ClientID %>").off("keyup").on("keyup", SearchTable);
            SearchTable();
        }

        function SearchTable() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            if (!input) return;
            var filter = input.value.toUpperCase();
            var table = document.getElementById('<%= gvAlerts.ClientID %>');
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
                    cell.innerText = "No alerts found matching your search.";
                    noRecRow.appendChild(cell);
                    tbody.appendChild(noRecRow);
                }
                noRecRow.style.display = "";
            } else if (noRecRow) { noRecRow.style.display = "none"; }
        }

        // ---------------------------------------------------
        // Modal & Click Logic
        // ---------------------------------------------------
        function showConfirmModal(id, current) {
            document.getElementById('<%= hfAlertId.ClientID %>').value = id;
            var target = current === "OPEN" ? "RESOLVED" : "OPEN";
            document.getElementById('<%= hfNewStatus.ClientID %>').value = target;
            document.getElementById('confirmText').innerHTML = "Mark this alert as <strong class='text-dark'>" + target + "</strong>?";
            bootstrap.Modal.getOrCreateInstance(document.getElementById('confirmModal')).show();
        }

        function closeConfirmModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('confirmModal')).hide();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        function openViewModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).show(); }
        
        function closeViewModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('viewModal')).hide();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        function selectAllAlerts(headerCheckBox) {
            var gridView = document.getElementById('<%= gvAlerts.ClientID %>');
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
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelAlerts">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper container-fluid mt-4">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="AdminMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-4">Alert Log Management</h2>

        <asp:UpdatePanel ID="upPanelAlerts" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                
                <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold mb-4" Visible="false"></asp:Label>

                <div class="ec-staff-card p-4 mb-4 shadow-sm">
                    
                    <%-- Top Row: Search, Sort, Clear Filters, and Trash Button --%>
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
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Sort Date</label>
                            <asp:DropDownList ID="ddlSort" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                <asp:ListItem Text="Newest First" Value="DESC"></asp:ListItem>
                                <asp:ListItem Text="Oldest First" Value="ASC"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-6 text-end d-flex justify-content-end align-items-center gap-2">
                            <asp:LinkButton ID="btnClear" runat="server" CssClass="btn btn-outline-secondary rounded-pill fw-bold px-4 shadow-sm" style="height: 42px; display: inline-flex; align-items: center;" OnClick="btnClear_Click">
                                <i class="bi bi-x-circle me-1"></i> Clear Filters
                            </asp:LinkButton>

                            <asp:LinkButton ID="btnRemoveSelected" runat="server" CssClass="btn btn-outline-danger rounded-circle shadow-sm" style="width: 42px; height: 42px; display: inline-flex; align-items: center; justify-content: center;" OnClick="btnRemoveSelected_Click" OnClientClick="return confirm('Are you sure you want to remove the selected alerts? This action cannot be undone.');" ToolTip="Remove Selected Alerts">
                                <i class="bi bi-trash3 fs-5" style="line-height: 0;"></i>
                            </asp:LinkButton>
                        </div>
                    </div>

                    <%-- Bottom Row: Advanced Filters --%>
                    <div class="row g-3">
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Severity</label>
                            <asp:DropDownList ID="ddlSeverityFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"></asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Status</label>
                            <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                <asp:ListItem Text="All Statuses" Value="All"></asp:ListItem>
                                <asp:ListItem Text="OPEN" Value="OPEN"></asp:ListItem>
                                <asp:ListItem Text="RESOLVED" Value="RESOLVED"></asp:ListItem>
                                <asp:ListItem Text="IGNORED" Value="IGNORED"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Month</label>
                            <asp:DropDownList ID="ddlFilterMonth" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"></asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Year</label>
                            <asp:DropDownList ID="ddlFilterYear" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"></asp:DropDownList>
                        </div>
                    </div>

                </div>

                <div class="ec-staff-card p-0 shadow-sm">
                    <div class="table-responsive" style="overflow-x: auto;">
                        <asp:GridView ID="gvAlerts" runat="server" AutoGenerateColumns="False" 
                            CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" GridLines="None" Width="100%" 
                            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvAlerts_PageIndexChanging" 
                            OnRowDataBound="gvAlerts_RowDataBound" OnRowCommand="gvAlerts_RowCommand"
                            ShowHeaderWhenEmpty="true" DataKeyNames="Id">
                            <PagerSettings Visible="False" />
                            
                            <Columns>
                                <asp:TemplateField ItemStyle-Width="40px">
                                    <HeaderTemplate>
                                        <div class="form-check d-flex justify-content-center">
                                            <input type="checkbox" class="form-check-input select-all-checkbox" onclick="selectAllAlerts(this)" />
                                        </div>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="form-check d-flex justify-content-center">
                                            <asp:CheckBox ID="chkSelect" runat="server" onclick="stopPropagation(event);" />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="Id" HeaderText="Alert ID" ItemStyle-Width="80px" ItemStyle-CssClass="fw-bold" />
                                
                                <asp:TemplateField HeaderText="Severity" ItemStyle-Width="100px">
                                    <ItemTemplate>
                                        <span class='fw-bold <%# Eval("SeverityName").ToString() == "CRITICAL" || Eval("SeverityName").ToString() == "EMERGENCY" ? "text-danger" : "text-warning" %>'>
                                            <%# Eval("SeverityName") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="action_type" HeaderText="Alert Type" ItemStyle-Width="180px" />
                                <asp:BoundField DataField="description" HeaderText="Details" ItemStyle-CssClass="log-desc-col text-muted" />
                                <asp:BoundField DataField="created_at" HeaderText="Time" DataFormatString="{0:dd/MM/yyyy HH:mm}" ItemStyle-CssClass="log-time-col" />
                                
                                <asp:TemplateField HeaderText="Status" ItemStyle-CssClass="log-status-col">
                                    <ItemTemplate>
                                        <div onclick="stopPropagation(event);">
                                            <asp:LinkButton ID="btnToggle" runat="server" CssClass="text-decoration-none d-flex align-items-center" OnClientClick='<%# "showConfirmModal(" + Eval("Id") + ", \"" + Eval("status") + "\"); return false;" %>'>
                                                <span class='ec-toggle-btn <%# Eval("status").ToString() == "RESOLVED" ? "ec-status-active-toggle" : "ec-status-locked-toggle" %>'></span>
                                                <span class='ms-2 fw-bold small <%# Eval("status").ToString() == "RESOLVED" ? "text-success" : "text-danger" %>'>
                                                    <%# Eval("status") %>
                                                </span>
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            
                            <EmptyDataTemplate>
                                <tr><td colspan="100%" class="ec-no-records text-center py-4">No alerts found matching your criteria.</td></tr>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>

                    <div class="ec-pager-wrapper p-3 d-flex justify-content-between align-items-center border-top bg-white">
                        <div class="ec-pager-info">
                            <asp:Label ID="lblShowing" runat="server" CssClass="text-muted small fw-bold"></asp:Label>
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

                <div class="modal fade" id="confirmModal" data-bs-backdrop="static" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered" style="max-width: 400px;">
                        <div class="modal-content ec-modal-content text-center p-4 border-warning" style="border-width: 2px !important;">
                            
                            <div class="mb-3">
                                <i class="bi bi-bell-fill" style="font-size: 3.5rem; color: var(--ec-warning-text, #ffc107);"></i>
                            </div>
                            
                            <h4 class="fw-bold text-dark mb-2">Confirm Resolution</h4>
                            
                            <asp:HiddenField ID="hfAlertId" runat="server" />
                            <asp:HiddenField ID="hfNewStatus" runat="server" />
                            
                            <p id="confirmText" class="text-muted mb-4">Mark alert as RESOLVED?</p>
                            
                            <div class="d-flex justify-content-center gap-3">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeConfirmModal()">Cancel</button>
                                <asp:Button ID="btnConfirm" runat="server" Text="Confirm Update" CssClass="ec-btn-primary px-4 rounded-pill" OnClick="btnConfirm_Click" UseSubmitBehavior="false" />
                            </div>

                        </div>
                    </div>
                </div>

                <div class="modal fade" id="viewModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered modal-lg">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="modal-header ec-modal-header bg-light border-0 pb-0">
                                <h5 class="modal-title fw-bold text-primary text-uppercase">Alert Details</h5>
                                <button type="button" class="btn-close" onclick="closeViewModal()"></button>
                            </div>
                            <div class="modal-body ec-modal-body p-4 pt-3">
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="fw-bold text-muted small">Alert ID</label>
                                        <div class="fw-bold fs-5 text-dark"><asp:Literal ID="litViewId" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-6 text-end">
                                        <label class="fw-bold text-muted small">Date Occurred</label>
                                        <div class="fw-bold"><asp:Literal ID="litViewDate" runat="server"></asp:Literal></div>
                                    </div>
                                </div>
                                <div class="mb-3 border-top pt-3">
                                    <label class="fw-bold text-muted small">Alert Type (Action)</label>
                                    <div class="fw-bold text-dark fs-5"><asp:Literal ID="litViewAction" runat="server"></asp:Literal></div>
                                </div>
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="fw-bold text-muted small">Severity</label>
                                        <div class="text-dark fw-bold"><asp:Literal ID="litViewSeverity" runat="server"></asp:Literal></div>
                                    </div>
                                    <div class="col-md-6 text-end">
                                        <label class="fw-bold text-muted small">Status</label>
                                        <div class="fw-bold text-dark mt-1"><asp:Literal ID="litViewStatus" runat="server"></asp:Literal></div>
                                    </div>
                                </div>
                                <div class="mb-4 border-top pt-3">
                                    <label class="fw-bold text-muted small">Details / Description</label>
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