<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="WAPP.Pages.Admin.ManageUsers" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    
    <script type="text/javascript">
        // Universal Auto-Search
        function pageLoad() {
            cleanupModals();

            $("#<%= txtSearch.ClientID %>").off("keyup").on("keyup", SearchTable);
            SearchTable();

            const urlParams = new URLSearchParams(window.location.search);
            if (urlParams.get('action') === 'add') {
                setTimeout(function () {
                    openAddModal();
                    const cleanUrl = window.location.protocol + "//" + window.location.host + window.location.pathname;
                    window.history.replaceState({ path: cleanUrl }, '', cleanUrl);
                }, 300);
            }
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

        // Standard Bootstrap Modal Logic with Hard Cleanup
        function cleanupModals() {
            document.querySelectorAll('.modal-backdrop').forEach(function (el) {
                el.parentNode.removeChild(el);
            });

            document.querySelectorAll('.modal').forEach(function (modal) {
                modal.classList.remove('show');
                modal.style.display = 'none';
                modal.setAttribute('aria-hidden', 'true');
                modal.removeAttribute('aria-modal');
                modal.removeAttribute('role');
            });

            document.body.classList.remove('modal-open');
            document.body.style.removeProperty('overflow');
            document.body.style.removeProperty('padding-right');
        }

        function forceCloseModal(modalId) {
            var modalEl = document.getElementById(modalId);
            if (!modalEl) {
                cleanupModals();
                return;
            }

            try {
                if (typeof bootstrap !== 'undefined') {
                    var instance = bootstrap.Modal.getInstance(modalEl);
                    if (!instance) {
                        instance = bootstrap.Modal.getOrCreateInstance(modalEl);
                    }
                    instance.hide();
                }
            } catch (ex) {
                // ignore JS bootstrap instance errors and continue hard cleanup
            }

            modalEl.classList.remove('show');
            modalEl.style.display = 'none';
            modalEl.setAttribute('aria-hidden', 'true');
            modalEl.removeAttribute('aria-modal');
            modalEl.removeAttribute('role');

            cleanupModals();
        }

        // Add Modal
        function openAddModal() {
            cleanupModals();
            var modalEl = document.getElementById('addUserModal');
            if (modalEl && typeof bootstrap !== 'undefined') {
                bootstrap.Modal.getOrCreateInstance(modalEl).show();
            }
        }
        function closeAddModal() { forceCloseModal('addUserModal'); }

        // Edit Modal (Acts as View Details + Edit)
        function openEditModal() {
            cleanupModals();
            var modalEl = document.getElementById('editUserModal');
            if (modalEl && typeof bootstrap !== 'undefined') {
                bootstrap.Modal.getOrCreateInstance(modalEl).show();
            }
        }
        function closeEditModal() { forceCloseModal('editUserModal'); }

        // Status Confirm Modal (WITH USER ID & NAME)
        function showStatusConfirmModal(userId, currentIsActive, userName) {
            document.getElementById('<%= hfConfirmUserId.ClientID %>').value = userId;
            var newLockValue = currentIsActive === 1 ? 1 : 0;
            document.getElementById('<%= hfConfirmNewStatus.ClientID %>').value = newLockValue;

            var action = currentIsActive === 1 ? "LOCK" : "ACTIVATE";
            var formattedId = "U" + String(userId).padStart(3, '0');

            document.getElementById('confirmText').innerHTML = "Are you sure you want to <strong class='text-dark'>" + action + "</strong> the account for <strong>" + userName + " (ID: " + formattedId + ")</strong>?";

            bootstrap.Modal.getOrCreateInstance(document.getElementById('statusConfirmModal')).show();
        }
        function closeStatusConfirmModal() { forceCloseModal('statusConfirmModal'); }

        // Delete Confirm Modal
        function showDeleteConfirmModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('deleteConfirmModal')).show();
        }
        function closeDeleteConfirmModal() { forceCloseModal('deleteConfirmModal'); }

        // Key Press Logic
        function handleEnter(e) {
            if (e.keyCode === 13 || e.key === 'Enter') {
                e.preventDefault();
                document.getElementById('<%= btnSearch.ClientID %>').click();
                return false;
            }
        }

        function stopPropagation(e) {
            if (!e) e = window.event;
            e.cancelBubble = true;
            if (e.stopPropagation) e.stopPropagation();
        }

        if (typeof Sys !== 'undefined') {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                cleanupModals();
            });
        }
    </script>

    <style>
        .clickable-row { cursor: pointer; transition: background-color 0.2s; }
        .clickable-row:hover { background-color: rgba(13, 110, 253, 0.05) !important; }
        
        a.btn-outline-danger, button.btn-outline-danger { color: #dc3545 !important; text-decoration: none !important; border-color: #dc3545; }
        a.btn-outline-danger:hover, button.btn-outline-danger:hover { color: #ffffff !important; background-color: #dc3545; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelUsers">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper container-fluid mt-4">    
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="AdminMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <h2 class="ec-staff-title text-uppercase mb-4">User Management</h2>

        <asp:UpdatePanel ID="upPanelUsers" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                
                <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block fw-bold mb-4" Visible="false"></asp:Label>

                <div class="ec-staff-card mb-4 card shadow-sm">
                    <div class="card-body p-4">
                        <div class="row mb-4 align-items-end g-3">
                            
                            <div class="col-lg-4">
                                <label class="form-label fw-bold text-muted mb-2 small text-uppercase">Universal Search</label>
                                <div class="input-group">
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" placeholder="Search anything..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                    <asp:LinkButton ID="btnSearch" runat="server" CssClass="input-group-text ec-search-btn text-decoration-none" OnClick="BtnSearch_Click">
                                        <i class="bi bi-search"></i>
                                    </asp:LinkButton>
                                </div>
                            </div>

                            <div class="col-lg-3">
                                <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Sort By</label>
                                <asp:DropDownList ID="ddlSort" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="Latest ID" Value="DESC"></asp:ListItem>
                                    <asp:ListItem Text="Oldest ID" Value="ASC"></asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <div class="col-lg-5 text-end d-flex justify-content-end align-items-center gap-2">
                                <asp:LinkButton ID="btnClear" runat="server" CssClass="btn btn-outline-secondary rounded-pill fw-bold px-4 shadow-sm" style="height: 42px; display: inline-flex; align-items: center;" OnClick="btnClear_Click">
                                    <i class="bi bi-x-circle me-1"></i> Clear Filters
                                </asp:LinkButton>
                                <asp:Button ID="btnAdd" runat="server" Text="+ Add New User" CssClass="btn btn-outline-primary rounded-pill fw-bold px-4 shadow-sm" OnClientClick="openAddModal(); return false;" style="height: 42px;" />
                            </div>

                        </div>

                        <div class="row g-2">
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Target Role</label>
                                <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"></asp:DropDownList>
                            </div>

                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold text-uppercase">Status</label>
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All Statuses" Value="All"></asp:ListItem>
                                    <asp:ListItem Text="Active" Value="0"></asp:ListItem>
                                    <asp:ListItem Text="Locked" Value="1"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="ec-staff-card card shadow-sm">
                    <div class="card-body p-0">
                        <div class="table-responsive" style="overflow-x: auto;">
                            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" 
                                CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" GridLines="None" Width="100%" 
                                AllowPaging="True" PageSize="10" OnPageIndexChanging="gvUsers_PageIndexChanging" OnRowCommand="gvUsers_RowCommand" OnRowDataBound="gvUsers_RowDataBound"
                                ShowHeaderWhenEmpty="true" DataKeyNames="Id">
                                <PagerSettings Visible="False" />
                                <Columns>
                                    <asp:BoundField DataField="Id" HeaderText="ID" ItemStyle-Width="60px" ItemStyle-CssClass="fw-bold" />
                                    <asp:BoundField DataField="fname" HeaderText="First Name" />
                                    <asp:BoundField DataField="lname" HeaderText="Last Name" />
                                    <asp:BoundField DataField="dob" HeaderText="DOB" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:BoundField DataField="contact" HeaderText="Phone No." />
                                    <asp:BoundField DataField="email" HeaderText="Email" />
                                    <asp:BoundField DataField="RoleName" HeaderText="Role" />
                                    
                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" onclick="stopPropagation(event);">
                                                <asp:LinkButton ID="btnToggle" runat="server"
                                                    CssClass="text-decoration-none d-flex align-items-center"
                                                    OnClientClick='<%# "showStatusConfirmModal(" + Eval("Id") + ", " + (Convert.ToBoolean(Eval("IsActive")) ? "1" : "0") + ", `" + Eval("fname").ToString().Replace("`", "") + " " + Eval("lname").ToString().Replace("`", "") + "`); return false;" %>'>
                                                    <span class='ec-toggle-btn <%# Convert.ToBoolean(Eval("IsActive")) ? "ec-status-active-toggle" : "ec-status-locked-toggle" %>'></span>
                                                    <span class='ms-2 fw-bold small <%# Convert.ToBoolean(Eval("IsActive")) ? "text-success" : "text-danger" %>'>
                                                        <%# Eval("StatusText") %>
                                                    </span>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Action">
                                        <ItemTemplate>
                                            <div onclick="stopPropagation(event);">
                                                <asp:LinkButton ID="btnEdit" runat="server"
                                                    CommandName="EditUser"
                                                    CommandArgument='<%# Eval("Id") %>'
                                                    CssClass="ec-btn-edit rounded-pill px-3 shadow-sm">
                                                    <i class="bi bi-pencil me-1"></i> Edit
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>

                                <EmptyDataTemplate>
                                    <tr>
                                        <td colspan="100%" class="text-center py-4 text-muted">No users found matching your criteria.</td>
                                    </tr>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>

                        <div class="ec-pager-wrapper d-flex justify-content-between align-items-center p-3 border-top bg-white">
                            <div class="text-muted small fw-bold">
                                <asp:Label ID="lblShowing" runat="server"></asp:Label>
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <asp:LinkButton ID="btnPrev" runat="server" CssClass="ec-pager-link" OnClick="btnPrev_Click">
                                    <i class="bi bi-chevron-left me-1"></i> Prev
                                </asp:LinkButton>

                                <div class="d-flex align-items-center bg-light border rounded px-2 py-1">
                                    <span class="small text-muted fw-bold me-2">Page</span>
                                    <asp:TextBox ID="txtPageJump" runat="server" CssClass="form-control form-control-sm text-center ec-pager-input" Width="50px" AutoPostBack="true" OnTextChanged="txtPageJump_TextChanged"></asp:TextBox>
                                </div>

                                <asp:LinkButton ID="btnNext" runat="server" CssClass="ec-pager-link" OnClick="btnNext_Click">
                                    Next <i class="bi bi-chevron-right ms-1"></i>
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="addUserModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="modal-header ec-modal-header bg-light border-0 pb-0 position-relative">
                                <h5 class="modal-title fw-bold text-dark">Add New User</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" onclick="closeAddModal()"></button>
                            </div>
                            <div class="modal-body ec-modal-body p-4 pt-3">
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">First Name</label>
                                        <asp:TextBox ID="txtAddFname" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAddFname" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Last Name</label>
                                        <asp:TextBox ID="txtAddLname" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAddLname" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Date of Birth</label>
                                    <asp:TextBox ID="txtAddDob" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAddDob" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Phone Number</label>
                                    <asp:TextBox ID="txtAddPhone" runat="server" CssClass="form-control" Placeholder="0123456789"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAddPhone" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtAddPhone" ValidationGroup="AddUserGroup" ValidationExpression="^01[0-9]{8,9}$" ErrorMessage="Invalid format (e.g. 0123456789)" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RegularExpressionValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Email</label>
                                    <asp:TextBox ID="txtAddEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAddEmail" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Password</label>
                                    <asp:TextBox ID="txtAddPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAddPassword" ValidationGroup="AddUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Role</label>
                                        <asp:DropDownList ID="ddlAddRole" runat="server" CssClass="form-select"></asp:DropDownList>
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

                <div class="modal fade" id="editUserModal" data-bs-backdrop="static" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content" style="border: 1px solid var(--ec-primary);">
                            <div class="modal-header ec-modal-header bg-light border-0 pb-0 position-relative">
                                <h5 class="modal-title fw-bold text-dark">User Details & Edit</h5>
                                <button type="button" class="btn-close" aria-label="Close" onclick="closeEditModal()"></button>
                            </div>
                            <div class="modal-body ec-modal-body p-4 pt-3">
                                <asp:HiddenField ID="hfEditUserId" runat="server" />
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">First Name</label>
                                        <asp:TextBox ID="txtEditFname" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditFname" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Last Name</label>
                                        <asp:TextBox ID="txtEditLname" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditLname" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Date of Birth</label>
                                    <asp:TextBox ID="txtEditDob" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditDob" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Phone Number</label>
                                    <asp:TextBox ID="txtEditPhone" runat="server" CssClass="form-control" Placeholder="0123456789"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditPhone" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEditPhone" ValidationGroup="EditUserGroup" ValidationExpression="^01[0-9]{8,9}$" ErrorMessage="Invalid format (e.g. 0123456789)" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RegularExpressionValidator>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Email</label>
                                    <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditEmail" ValidationGroup="EditUserGroup" ErrorMessage="Required" CssClass="text-danger small fw-bold" Display="Dynamic"></asp:RequiredFieldValidator>
                                </div>

                                <div class="mb-3">
                                    <label class="fw-bold small text-muted">Password</label>
                                    <asp:TextBox ID="txtEditPass" runat="server" CssClass="form-control" placeholder="Leave blank to keep current"></asp:TextBox>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Role</label>
                                        <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select"></asp:DropDownList>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-bold small text-muted">Status</label>
                                        <asp:DropDownList ID="ddlEditStatus" runat="server" CssClass="form-select">
                                            <asp:ListItem Value="Active">Active</asp:ListItem>
                                            <asp:ListItem Value="Locked">Locked</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>

                                <div class="d-flex justify-content-between align-items-center border-top pt-3 mt-4">
                                    <div>
                                        <%-- Trigger Delete Confirm Modal purely via Javascript --%>
                                        <button type="button" class="btn btn-outline-danger rounded-pill px-3 fw-bold" onclick="showDeleteConfirmModal()">
                                            <i class="bi bi-trash3-fill me-1"></i> Delete
                                        </button>
                                    </div>
                                    <div>
                                        <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill me-2" onclick="closeEditModal()">Cancel</button>
                                        <asp:Button ID="btnUpdateUser" runat="server" Text="Save Changes" CssClass="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" ValidationGroup="EditUserGroup" OnClick="btnUpdateUser_Click" UseSubmitBehavior="false" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="deleteConfirmModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
                    <div class="modal-dialog modal-dialog-centered" style="max-width: 400px;">
                        <div class="modal-content ec-modal-content text-center p-4 border-danger" style="border-width: 2px !important;">
                            <div class="mb-3">
                                <i class="bi bi-exclamation-triangle-fill text-danger" style="font-size: 3.5rem;"></i>
                            </div>
                            <h4 class="fw-bold text-dark mb-2">Confirm Deletion</h4>
                            <p class="text-muted mb-4">Are you sure you want to PERMANENTLY delete this user? This action cannot be undone.</p>
                            
                            <div class="d-flex justify-content-center gap-3">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeDeleteConfirmModal()">Cancel</button>
                                <asp:Button ID="btnDeleteUser" runat="server" Text="Delete User" CssClass="btn btn-danger rounded-pill px-4 shadow-sm fw-bold" OnClick="btnDeleteUser_Click" UseSubmitBehavior="false" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="statusConfirmModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
                    <div class="modal-dialog modal-dialog-centered" style="max-width: 400px;">
                        <div class="modal-content ec-modal-content text-center p-4">
                            <div class="mb-3">
                                <i class="bi bi-exclamation-circle-fill" style="font-size: 3.5rem; color: var(--ec-warning-text, #ffc107);"></i>
                            </div>
                            <h4 class="fw-bold text-dark mb-2">Confirm Status Change</h4>
                            
                            <asp:HiddenField ID="hfConfirmUserId" runat="server" />
                            <asp:HiddenField ID="hfConfirmNewStatus" runat="server" />
                            
                            <p id="confirmText" class="text-muted mb-4">Are you sure you want to change this user's status?</p>
                            
                            <div class="d-flex justify-content-center gap-3">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeStatusConfirmModal()">Cancel</button>
                                <asp:Button ID="btnConfirmStatusChange" runat="server" Text="Confirm Change" CssClass="btn btn-primary rounded-pill px-4 shadow-sm fw-bold" OnClick="btnConfirmStatusChange_Click" UseSubmitBehavior="false" />
                            </div>
                        </div>
                    </div>
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSaveUser" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnUpdateUser" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnDeleteUser" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>