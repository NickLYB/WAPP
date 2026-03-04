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
                    cell.colSpan = "100"; cell.className = "ec-no-records";
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
            bootstrap.Modal.getOrCreateInstance(document.getElementById('editUserModal')).hide();
            $('.modal-backdrop').remove(); $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelUser">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper">
        <h2 class="ec-staff-title text-uppercase mb-4">User Management</h2>
        
        <asp:UpdatePanel ID="upPanelUser" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" Text="" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card p-4 mb-4">
                    <div class="row align-items-end mb-4">
                        <div class="col-lg-6">
                            <label class="form-label fw-bold text-muted mb-2">Search / Filter:</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" Placeholder="Search users, emails, or keywords..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                <span class="input-group-text ec-search-btn"><i class="bi bi-search"></i></span>
                            </div>
                        </div>
                        <div class="col-lg-6 text-end">
                            <button type="button" class="btn btn-primary rounded-pill fw-bold px-4 shadow-sm" data-bs-toggle="modal" data-bs-target="#addUserModal">+ Add New User</button>
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
                    <asp:GridView ID="gvUsers" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0" 
                        AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id" 
                        ShowHeaderWhenEmpty="true" OnRowCommand="gvUsers_RowCommand"
                        AllowPaging="True" PageSize="10" PagerSettings-Visible="false">
                        
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="ID" />
                            <asp:BoundField DataField="fname" HeaderText="First Name" />
                            <asp:BoundField DataField="lname" HeaderText="Last Name" />
                            <asp:BoundField DataField="dob" HeaderText="Date of Birth" DataFormatString="{0:yyyy-MM-dd}" />
                            <asp:BoundField DataField="contact" HeaderText="Phone" />
                            <asp:BoundField DataField="email" HeaderText="Email" />
                            <asp:TemplateField HeaderText="Role"><ItemTemplate><%# GetRoleName(Eval("role_id")) %></ItemTemplate></asp:TemplateField>

                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <div class="d-flex align-items-center">
                                        <asp:LinkButton ID="btnToggleStatus" runat="server" 
                                            CssClass='<%# Convert.ToBoolean(Eval("is_locked")) ? "ec-toggle-btn ec-status-locked-toggle" : "ec-toggle-btn ec-status-active-toggle" %>'
                                            CommandName="ToggleStatus" CommandArgument='<%# Eval("Id") %>'>
                                        </asp:LinkButton>
                                        <span class='<%# Convert.ToBoolean(Eval("is_locked")) ? "ms-2 fw-bold text-danger small" : "ms-2 fw-bold text-success small" %>'>
                                            <%# Convert.ToBoolean(Eval("is_locked")) ? "Locked" : "Active" %>
                                        </span>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEdit" runat="server" CssClass="ec-btn-edit" CommandName="EditUser" CommandArgument='<%# Eval("Id") %>'>Edit</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><tr><td colspan="100%" class="ec-no-records">No users found matching your filters.</td></tr></EmptyDataTemplate>
                    </asp:GridView>

                    <div class="ec-pager-wrapper">
                        <div class="ec-pager-info">
                            <asp:Literal ID="litPagerInfo" runat="server"></asp:Literal>
                        </div>
                        <div class="d-flex gap-3">
                            <asp:LinkButton ID="btnPrev" runat="server" CssClass="ec-pager-link" OnClick="btnPrev_Click"><i class="bi bi-chevron-left me-1"></i> Prev</asp:LinkButton>
                            <asp:LinkButton ID="btnNext" runat="server" CssClass="ec-pager-link" OnClick="btnNext_Click">Next <i class="bi bi-chevron-right ms-1"></i></asp:LinkButton>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="editUserModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content">
                            <div class="modal-header ec-modal-header">
                                <h5 class="modal-title fw-bold text-dark">Edit User</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body ec-modal-body">
                                <asp:ValidationSummary ID="valSummaryEdit" runat="server" ValidationGroup="EditUser" CssClass="alert alert-danger" />
                                <asp:HiddenField ID="hfEditUserId" runat="server" />
                                
                                <div class="mb-3"><label class="fw-bold small text-muted">User ID</label><asp:TextBox ID="txtEditID" runat="server" CssClass="form-control bg-light" ReadOnly="true"></asp:TextBox></div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">First Name</label><asp:TextBox ID="txtEditFirstName" runat="server" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="rfvEditFN" runat="server" ControlToValidate="txtEditFirstName" ErrorMessage="First Name required" ValidationGroup="EditUser" Display="None" /></div>
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">Last Name</label><asp:TextBox ID="txtEditLastName" runat="server" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="rfvEditLN" runat="server" ControlToValidate="txtEditLastName" ErrorMessage="Last Name required" ValidationGroup="EditUser" Display="None" /></div>
                                </div>

                                <div class="mb-3"><label class="fw-bold small text-muted">Date of Birth</label><asp:TextBox ID="txtEditDOB" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox><asp:RequiredFieldValidator ID="rfvEditDOB" runat="server" ControlToValidate="txtEditDOB" ErrorMessage="DOB required" ValidationGroup="EditUser" Display="None" /></div>
                                
                                <div class="mb-3"><label class="fw-bold small text-muted">Phone Number</label><asp:TextBox ID="txtEditPhone" runat="server" CssClass="form-control" Placeholder="01x-xxxxxxx"></asp:TextBox><asp:RequiredFieldValidator ID="rfvEditPhone" runat="server" ControlToValidate="txtEditPhone" ErrorMessage="Phone required" ValidationGroup="EditUser" Display="None" /><asp:RegularExpressionValidator ID="revEditPhone" runat="server" ControlToValidate="txtEditPhone" ValidationExpression="^01[0-9]-[0-9]{7,8}$" ErrorMessage="Invalid Format" CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditUser" /></div>
                                
                                <div class="mb-3"><label class="fw-bold small text-muted">Email</label><asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox><asp:RequiredFieldValidator ID="rfvEditEmail" runat="server" ControlToValidate="txtEditEmail" ErrorMessage="Email required" ValidationGroup="EditUser" Display="None" /></div>
                                
                                <div class="mb-3"><label class="fw-bold small text-muted">Password</label><asp:TextBox ID="txtEditPass" runat="server" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="rfvEditPass" runat="server" ControlToValidate="txtEditPass" ErrorMessage="Password required" ValidationGroup="EditUser" Display="None" /></div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">Role</label><asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select bg-light" Enabled="false"><asp:ListItem Value="4">Student</asp:ListItem><asp:ListItem Value="3">Tutor</asp:ListItem></asp:DropDownList></div>
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">Status</label><asp:DropDownList ID="ddlEditStatus" runat="server" CssClass="form-select"><asp:ListItem Value="Active">Active</asp:ListItem><asp:ListItem Value="Locked">Locked</asp:ListItem></asp:DropDownList></div>
                                </div>
                            </div>
                            <div class="modal-footer ec-modal-footer">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeEditModal()">Cancel</button>
                                <asp:Button ID="btnUpdateUser" runat="server" Text="Save Changes" CssClass="ec-btn-primary" OnClick="btnUpdateUser_Click" ValidationGroup="EditUser" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="addUserModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content">
                            <div class="modal-header ec-modal-header">
                                <h5 class="modal-title fw-bold text-dark">Add New User</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body ec-modal-body">
                                <asp:ValidationSummary ID="valSummary" runat="server" ValidationGroup="AddUser" CssClass="alert alert-danger" />
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">First Name</label><asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtFirstName" ErrorMessage="Required" ValidationGroup="AddUser" Display="None" /></div>
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">Last Name</label><asp:TextBox ID="txtLastName" runat="server" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="rfv2" runat="server" ControlToValidate="txtLastName" ErrorMessage="Required" ValidationGroup="AddUser" Display="None" /></div>
                                </div>

                                <div class="mb-3"><label class="fw-bold small text-muted">Date of Birth</label><asp:TextBox ID="txtDOB" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox><asp:RequiredFieldValidator ID="rfv3" runat="server" ControlToValidate="txtDOB" ErrorMessage="Required" ValidationGroup="AddUser" Display="None" /></div>
                                
                                <div class="mb-3"><label class="fw-bold small text-muted">Phone Number</label><asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" Placeholder="01x-xxxxxxx"></asp:TextBox><asp:RequiredFieldValidator ID="rfv4" runat="server" ControlToValidate="txtPhone" ErrorMessage="Required" ValidationGroup="AddUser" Display="None" /><asp:RegularExpressionValidator ID="revPhone" runat="server" ControlToValidate="txtPhone" ValidationExpression="^01[0-9]-[0-9]{7,8}$" ErrorMessage="Invalid Format" CssClass="text-danger small" Display="Dynamic" ValidationGroup="AddUser" /></div>
                                
                                <div class="mb-3"><label class="fw-bold small text-muted">Email</label><asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox><asp:RequiredFieldValidator ID="rfv5" runat="server" ControlToValidate="txtEmail" ErrorMessage="Required" ValidationGroup="AddUser" Display="None" /></div>
                                
                                <div class="mb-3"><label class="fw-bold small text-muted">Password</label><asp:TextBox ID="txtPass" runat="server" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="rfv6" runat="server" ControlToValidate="txtPass" ErrorMessage="Required" ValidationGroup="AddUser" Display="None" /></div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">Role</label><asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select"><asp:ListItem Value="4" Selected="True">Student</asp:ListItem><asp:ListItem Value="3">Tutor</asp:ListItem></asp:DropDownList></div>
                                    <div class="col-md-6 mb-3"><label class="fw-bold small text-muted">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"><asp:ListItem Value="Active" Selected="True">Active</asp:ListItem><asp:ListItem Value="Locked">Locked</asp:ListItem></asp:DropDownList></div>
                                </div>
                            </div>
                            <div class="modal-footer ec-modal-footer">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" data-bs-dismiss="modal">Cancel</button>
                                <asp:Button ID="btnSaveUser" runat="server" Text="Save User" CssClass="ec-btn-primary" OnClick="btnSaveUser_Click" ValidationGroup="AddUser" />
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers><asp:AsyncPostBackTrigger ControlID="btnUpdateUser" EventName="Click" /></Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>