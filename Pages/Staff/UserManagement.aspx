<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="WAPP.Pages.Staff.UserManagement" EnableEventValidation="false" %>

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
            var table = document.getElementById('<%= gvUsers.ClientID %>');
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
                    cell.innerText = "No users found matching your search.";
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

        function openEditModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('editUserModal')).show(); }
        function closeEditModal() {
            var modal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            if (modal) { modal.hide(); }
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        function openAddModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('addUserModal')).show(); }
        function closeAddModal() {
            var modal = bootstrap.Modal.getInstance(document.getElementById('addUserModal'));
            if (modal) { modal.hide(); }
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        // NEW: Modal functions for locking/unlocking
        function openLockModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('lockUserModal')).show(); }
        function closeLockModal() {
            var modal = bootstrap.Modal.getInstance(document.getElementById('lockUserModal'));
            if (modal) { modal.hide(); }
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
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

        .btn-outline-secondary { border-color: #6c757d; color: #6c757d; }
        .btn-outline-secondary:hover { background-color: #6c757d; color: white; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelUser">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper container-fluid mt-4">
        <h2 class="ec-staff-title text-uppercase mb-4">User Management</h2>
        
        <asp:UpdatePanel ID="upPanelUser" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" Text="" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card p-4 mb-4">
                    <div class="row align-items-end mb-4">
                        <div class="col-lg-4">
                            <label class="form-label fw-bold text-muted mb-2">Search / Filter:</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" Placeholder="Search users, emails, or keywords..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                <span class="input-group-text ec-search-btn"><i class="bi bi-search"></i></span>
                            </div>
                        </div>
                        <div class="col-lg-3">
                            <label class="form-label small text-muted mb-1 fw-bold">Sort By</label>
                            <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                <asp:ListItem Value="ID_ASC">User ID (Ascending)</asp:ListItem>
                                <asp:ListItem Value="ID_DESC">User ID (Descending)</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="col-lg-5 text-end d-flex justify-content-end align-items-center gap-2">
                            <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="btn btn-outline-secondary rounded-pill fw-bold px-4 shadow-sm d-inline-flex align-items-center" OnClick="btnClearFilters_Click" ToolTip="Clear Filters">
                                <i class="bi bi-x-circle fs-5 me-2" style="line-height: 0;"></i> Clear Filters
                            </asp:LinkButton>
                            <button type="button" class="btn btn-outline-primary rounded-pill fw-bold px-4 shadow-sm" onclick="openAddModal()">
                                + Add New User
                            </button>
                        </div>
                    </div>

                    <div class="row g-3">
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold">Filter by Role</label>
                            <asp:DropDownList ID="ddlFilterRole" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                <asp:ListItem Value="All">All Categories</asp:ListItem>
                                <asp:ListItem Value="4">Students</asp:ListItem>
                                <asp:ListItem Value="3">Tutors</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small text-muted mb-1 fw-bold">Filter by Status</label>
                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                <asp:ListItem Value="All">All Statuses</asp:ListItem>
                                <asp:ListItem Value="0">Active</asp:ListItem>
                                <asp:ListItem Value="1">Locked</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>

                <div class="ec-staff-card p-0">
                    <div class="table-responsive" style="overflow-x: auto;">
                        <asp:GridView ID="gvUsers" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" 
                            AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id" 
                            ShowHeaderWhenEmpty="true" OnRowCommand="gvUsers_RowCommand" OnRowDataBound="gvUsers_RowDataBound"
                            AllowPaging="True" PageSize="10" PagerSettings-Visible="false">
                            
                            <Columns>
                                <asp:TemplateField HeaderText="User ID">
                                    <ItemTemplate>
                                        <span class="fw-bold"><%# Eval("Id", "U{0:D3}") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="fname" HeaderText="First Name" />
                                <asp:BoundField DataField="lname" HeaderText="Last Name" />
                                <asp:BoundField DataField="dob" HeaderText="Date of Birth" DataFormatString="{0:yyyy-MM-dd}" />
                                <asp:BoundField DataField="contact" HeaderText="Phone" />
                                <asp:BoundField DataField="email" HeaderText="Email" />
                                <asp:TemplateField HeaderText="Role"><ItemTemplate><%# GetRoleName(Eval("role_id")) %></ItemTemplate></asp:TemplateField>

                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <div class="d-flex align-items-center" onclick="stopPropagation(event);">
                                            <asp:LinkButton ID="btnToggleStatus" runat="server" 
                                                CssClass='<%# Convert.ToBoolean(Eval("is_locked")) ? "ec-toggle-btn ec-status-locked-toggle" : "ec-toggle-btn ec-status-active-toggle" %>'
                                                CommandName="ConfirmToggleStatus" CommandArgument='<%# Eval("Id") %>'>
                                            </asp:LinkButton>
                                            <span class='<%# Convert.ToBoolean(Eval("is_locked")) ? "ms-2 fw-bold text-danger small" : "ms-2 fw-bold text-success small" %>'>
                                                <%# Convert.ToBoolean(Eval("is_locked")) ? "Locked" : "Active" %>
                                            </span>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Action">
                                    <ItemTemplate>
                                        <div onclick="stopPropagation(event);">
                                            <asp:LinkButton ID="btnEdit" runat="server" CssClass="ec-btn-edit rounded-pill px-3 shadow-sm" CommandName="EditUser" CommandArgument='<%# Eval("Id") %>'>Edit</asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate><tr><td colspan="100%" class="text-center py-4 text-muted">No users found matching your filters.</td></tr></EmptyDataTemplate>
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

                <div class="modal fade" id="editUserModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="modal-header ec-modal-header bg-light border-0 pb-0 position-relative">
                                <h5 class="modal-title fw-bold text-dark">Edit User</h5>
                                <button type="button" class="btn-close" onclick="closeEditModal()" style="position:absolute; top:20px; right:20px;"></button>
                            </div>
                            <div class="modal-body ec-modal-body p-4 pt-3">
                                <asp:HiddenField ID="hfEditUserId" runat="server" />
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">User ID</label>
                                    <asp:TextBox ID="txtEditID" runat="server" CssClass="form-control bg-light" ReadOnly="true"></asp:TextBox>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">First Name</label>
                                        <asp:TextBox ID="txtEditFirstName" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditFirstName" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Last Name</label>
                                        <asp:TextBox ID="txtEditLastName" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditLastName" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Date of Birth</label>
                                    <asp:TextBox ID="txtEditDOB" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditDOB" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Phone Number</label>
                                    <asp:TextBox ID="txtEditPhone" runat="server" CssClass="form-control" Placeholder="01x xxxxxxx"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditPhone" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEditPhone" ValidationGroup="EditUserGroup" ValidationExpression="^01[0-9]{8,9}$" ErrorMessage="Invalid format (e.g. 0123456789)" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RegularExpressionValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Email</label>
                                    <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditEmail" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>

                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Role</label>
                                        <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select bg-light" Enabled="false">
                                            <asp:ListItem Value="4">Student</asp:ListItem>
                                            <asp:ListItem Value="3">Tutor</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Status</label>
                                        <asp:DropDownList ID="ddlEditStatus" runat="server" CssClass="form-select">
                                            <asp:ListItem Value="Active">Active</asp:ListItem>
                                            <asp:ListItem Value="Locked">Locked</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="text-end border-top pt-3 mt-4">
                                    <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill me-2" onclick="closeEditModal()">Cancel</button>
                                    <asp:Button ID="btnUpdateUser" runat="server" Text="Save Changes" CssClass="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" ValidationGroup="EditUserGroup" OnClick="btnUpdateUser_Click" UseSubmitBehavior="false" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="addUserModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="modal-header ec-modal-header bg-light border-0 pb-0 position-relative">
                                <h5 class="modal-title fw-bold text-dark">Add New User</h5>
                                <button type="button" class="btn-close" onclick="closeAddModal()" style="position:absolute; top:20px; right:20px;"></button>
                            </div>
                            <div class="modal-body ec-modal-body p-4 pt-3">
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">First Name</label>
                                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirstName" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Last Name</label>
                                        <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLastName" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Date of Birth</label>
                                    <asp:TextBox ID="txtDOB" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDOB" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Phone Number</label>
                                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" Placeholder="01xxxxxxxx"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPhone" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPhone" ValidationGroup="AddUserGroup" ValidationExpression="^01[0-9]{8,9}$" ErrorMessage="Invalid format (e.g. 0123456789)" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RegularExpressionValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Email</label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Password</label>
                                    <asp:TextBox ID="txtPass" runat="server" CssClass="form-control"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPass" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Role</label>
                                        <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
                                            <asp:ListItem Value="4" Selected="True">Student</asp:ListItem>
                                            <asp:ListItem Value="3">Tutor</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Status</label>
                                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                                            <asp:ListItem Value="Active" Selected="True">Active</asp:ListItem>
                                            <asp:ListItem Value="Locked">Locked</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="text-end border-top pt-3 mt-4">
                                    <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill me-2" onclick="closeAddModal()">Cancel</button>
                                    <asp:Button ID="btnSaveUser" runat="server" Text="Save User" CssClass="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" ValidationGroup="AddUserGroup" OnClick="btnSaveUser_Click" UseSubmitBehavior="false" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="lockUserModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content border-danger" style="border-width: 4px !important;">
                            <div class="modal-header bg-danger text-white border-0 position-relative">
                                <h5 class="modal-title fw-bold text-uppercase">Confirm Action</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="closeLockModal()" style="position:absolute; top:20px; right:20px;"></button>
                            </div>
                            <div class="modal-body text-center p-4">
                                <asp:HiddenField ID="hfToggleUserId" runat="server" />
                                <h4 class="mb-3 fw-normal"><asp:Literal ID="litLockModalTitle" runat="server"></asp:Literal></h4>
                                <p class="text-danger fs-6 mb-4 bg-light p-3 rounded border">
                                    <asp:Literal ID="litLockUserDetails" runat="server"></asp:Literal>
                                </p>
                                <div class="d-flex justify-content-center gap-3">
                                    <asp:Button ID="btnConfirmToggleStatus" runat="server" Text="Confirm" CssClass="btn btn-danger px-4 fw-bold rounded-pill" OnClick="btnConfirmToggleStatus_Click" />
                                    <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill" onclick="closeLockModal()">Cancel</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnUpdateUser" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSaveUser" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnConfirmToggleStatus" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>