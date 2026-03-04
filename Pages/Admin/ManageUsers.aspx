<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="WAPP.Pages.Admin.ManageUsers" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    
    <script type="text/javascript">
        // Standard Bootstrap Modal Logic
        function cleanupModals() {
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }

        // Add Modal
        function openAddModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('addUserModal')).show(); }
        function closeAddModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('addUserModal')).hide(); cleanupModals(); }

        // Edit Modal (Can be called from Code-Behind)
        function openEditModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('editUserModal')).show(); }
        function closeEditModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('editUserModal')).hide(); cleanupModals(); }

        // Status Confirm Modal
        function showStatusConfirmModal(userId, currentIsActive) {
            document.getElementById('<%= hfConfirmUserId.ClientID %>').value = userId;
            var newLockValue = currentIsActive === 1 ? 1 : 0; 
            document.getElementById('<%= hfConfirmNewStatus.ClientID %>').value = newLockValue;
            document.getElementById('confirmText').innerHTML = "Are you sure you want to <strong class='text-dark'>" + (currentIsActive === 1 ? "LOCK" : "ACTIVATE") + "</strong> this user's account?";

            bootstrap.Modal.getOrCreateInstance(document.getElementById('statusConfirmModal')).show();
        }
        function closeStatusConfirmModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('statusConfirmModal')).hide(); cleanupModals(); }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelUsers">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper">
        <h2 class="ec-staff-title text-uppercase mb-4">User Management</h2>

        <asp:UpdatePanel ID="upPanelUsers" runat="server">
            <ContentTemplate>
                
                <div class="ec-staff-card p-4 mb-4">
                    <div class="row align-items-end g-3">
                        
                        <div class="col-lg-4">
                            <label class="form-label fw-bold text-muted mb-2 small text-uppercase">Search</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" placeholder="Search by Name or Email..."></asp:TextBox>
                                <asp:LinkButton ID="btnSearch" runat="server" CssClass="input-group-text ec-search-btn text-decoration-none" OnClick="BtnSearch_Click">
                                    <i class="bi bi-search"></i>
                                </asp:LinkButton>
                            </div>
                        </div>

                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Sort By</label>
                            <asp:DropDownList ID="ddlSort" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                <asp:ListItem Text="Latest" Value="DESC"></asp:ListItem>
                                <asp:ListItem Text="Oldest" Value="ASC"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Role</label>
                            <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"></asp:DropDownList>
                        </div>

                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Status</label>
                            <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                <asp:ListItem Text="All" Value="All"></asp:ListItem>
                                <asp:ListItem Text="Active" Value="0"></asp:ListItem>
                                <asp:ListItem Text="Locked" Value="1"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-2 text-end">
                            <asp:Button ID="btnAdd" runat="server" Text="+ Add New User" CssClass="btn btn-primary rounded-pill fw-bold w-100 shadow-sm" OnClientClick="openAddModal(); return false;" style="height: 42px;" />
                        </div>

                    </div>
                </div>

                <div class="ec-staff-card p-0">
                    <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" 
                        CssClass="table table-hover ec-table-custom align-middle mb-0" GridLines="None" Width="100%" 
                        AllowPaging="True" PageSize="10" OnPageIndexChanging="gvUsers_PageIndexChanging" OnRowCommand="gvUsers_RowCommand" ShowHeaderWhenEmpty="true">
                        <PagerSettings Visible="False" />
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="ID" ItemStyle-Width="60px" />
                            <asp:BoundField DataField="fname" HeaderText="First Name" />
                            <asp:BoundField DataField="lname" HeaderText="Last Name" />
                            <asp:BoundField DataField="dob" HeaderText="DOB" DataFormatString="{0:yyyy-MM-dd}" />
                            <asp:BoundField DataField="contact" HeaderText="Phone No." />
                            <asp:BoundField DataField="email" HeaderText="Email" />
                            <asp:BoundField DataField="RoleName" HeaderText="Role" />
                            
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnToggle" runat="server" CssClass="text-decoration-none d-flex align-items-center" OnClientClick='<%# "showStatusConfirmModal(" + Eval("Id") + ", " + (Convert.ToBoolean(Eval("IsActive")) ? "1" : "0") + "); return false;" %>'>
                                        <span class='ec-toggle-btn <%# Convert.ToBoolean(Eval("IsActive")) ? "ec-status-active-toggle" : "ec-status-locked-toggle" %>'></span>
                                        <span class='ms-2 fw-bold small <%# Convert.ToBoolean(Eval("IsActive")) ? "text-success" : "text-danger" %>'>
                                            <%# Eval("StatusText") %>
                                        </span>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditUser" CommandArgument='<%# Eval("Id") %>' CssClass="ec-btn-edit">
                                        <i class="bi bi-pencil"></i> Edit
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        
                        <EmptyDataTemplate>
                            <tr><td colspan="100%" class="ec-no-records">No users found matching your criteria.</td></tr>
                        </EmptyDataTemplate>
                    </asp:GridView>

                    <div class="ec-pager-wrapper">
                        <div class="ec-pager-info">
                            <asp:Label ID="lblShowing" runat="server"></asp:Label>
                        </div>
                        <div class="d-flex gap-3">
                            <asp:LinkButton ID="btnPrev" runat="server" CssClass="ec-pager-link" OnClick="btnPrev_Click"><i class="bi bi-chevron-left me-1"></i> Prev</asp:LinkButton>
                            <asp:LinkButton ID="btnNext" runat="server" CssClass="ec-pager-link" OnClick="btnNext_Click">Next <i class="bi bi-chevron-right ms-1"></i></asp:LinkButton>
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
                            <div class="modal-body ec-modal-body p-4">
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Role</label></div><div class="col-8"><asp:DropDownList ID="ddlAddRole" runat="server" CssClass="form-select"></asp:DropDownList></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">First Name</label></div><div class="col-8"><asp:TextBox ID="txtAddFname" runat="server" CssClass="form-control"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Last Name</label></div><div class="col-8"><asp:TextBox ID="txtAddLname" runat="server" CssClass="form-control"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Email</label></div><div class="col-8"><asp:TextBox ID="txtAddEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Phone No.</label></div><div class="col-8"><asp:TextBox ID="txtAddPhone" runat="server" CssClass="form-control"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">DOB</label></div><div class="col-8"><asp:TextBox ID="txtAddDob" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Password</label></div><div class="col-8"><asp:TextBox ID="txtAddPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox></div></div>
                            </div>
                            <div class="modal-footer ec-modal-footer">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeAddModal()">Cancel</button>
                                <asp:Button ID="btnSaveUser" runat="server" Text="Save User" CssClass="ec-btn-primary px-4" OnClick="btnSaveUser_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="editUserModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content ec-modal-content">
                            <div class="modal-header ec-modal-header">
                                <h5 class="modal-title fw-bold text-dark">Edit User Details</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body ec-modal-body p-4">
                                <asp:HiddenField ID="hfEditUserId" runat="server" />
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">First Name</label></div><div class="col-8"><asp:TextBox ID="txtEditFname" runat="server" CssClass="form-control"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Last Name</label></div><div class="col-8"><asp:TextBox ID="txtEditLname" runat="server" CssClass="form-control"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Email</label></div><div class="col-8"><asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Phone No.</label></div><div class="col-8"><asp:TextBox ID="txtEditPhone" runat="server" CssClass="form-control"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">DOB</label></div><div class="col-8"><asp:TextBox ID="txtEditDob" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div></div>
                                <div class="row mb-3"><div class="col-4"><label class="fw-bold text-muted small mt-2">Role</label></div><div class="col-8"><asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select"></asp:DropDownList></div></div>
                            </div>
                            <div class="modal-footer ec-modal-footer">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeEditModal()">Cancel</button>
                                <asp:Button ID="btnUpdateUser" runat="server" Text="Update Details" CssClass="ec-btn-primary px-4" OnClick="btnUpdateUser_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="modal fade" id="statusConfirmModal" tabindex="-1" aria-hidden="true">
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
                                <asp:Button ID="btnConfirmStatusChange" runat="server" Text="Confirm Change" CssClass="ec-btn-primary px-4" OnClick="btnConfirmStatusChange_Click" />
                            </div>
                        </div>
                    </div>
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSaveUser" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnUpdateUser" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>