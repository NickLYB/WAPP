<%@ Page Title="Tutor Application Management" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="TutorApplication.aspx.cs" Inherits="WAPP.Pages.Staff.TutorApplications" EnableEventValidation="false" %>

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
                    cell.className = "ec-no-records"; 
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
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upTutorApp">
        <ProgressTemplate><div class="loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper">
        <h2 class="ec-staff-title text-uppercase mb-4">Tutor Application Management</h2>
            
        <asp:UpdatePanel ID="upTutorApp" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card mb-4">
                    <div class="card-body p-4">
                        <div class="row mb-4 align-items-end">
                            <div class="col-lg-4">
                                <label class="form-label fw-bold text-muted mb-2">Search / Filter:</label>
                                <div class="input-group">
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" Placeholder="Search ID, name..." onkeydown="return handleEnter(event)"></asp:TextBox>
                                    <span class="input-group-text ec-search-btn"><i class="bi bi-search"></i></span>
                                </div>
                            </div>
                            <div class="col-lg-8 text-end">
                                <asp:LinkButton ID="btnTriggerRemove" runat="server" CssClass="btn btn-danger rounded-pill fw-bold px-4 shadow-sm" OnClick="btnTriggerRemove_Click">- Remove Application</asp:LinkButton>
                            </div>
                        </div>
                        <div class="row g-2">
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Status</label>
                                <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="All">All</asp:ListItem>
                                    <asp:ListItem Value="PENDING">Pending</asp:ListItem>
                                    <asp:ListItem Value="APPROVED">Verified</asp:ListItem>
                                    <asp:ListItem Value="REJECTED">Rejected</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <%-- NEW: Sort DropDownList --%>
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Sort by</label>
                                <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed">
                                    <asp:ListItem Value="DESC">Newest First</asp:ListItem>
                                    <asp:ListItem Value="ASC">Oldest First</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="ec-staff-card">
                    <div class="card-body p-0"> 
                        
                        <asp:GridView ID="gvApplications" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0" 
                            AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id" 
                            ShowHeaderWhenEmpty="true" OnRowDataBound="gvApplications_RowDataBound"
                            AllowPaging="True" PageSize="10" PagerSettings-Visible="false">
                            
                            <Columns>
                                <asp:TemplateField><ItemTemplate><asp:CheckBox ID="chkSelect" runat="server" /></ItemTemplate></asp:TemplateField>
                                <asp:BoundField DataField="Id" HeaderText="App ID" />
                                <asp:BoundField DataField="tutor_id" HeaderText="Tutor ID" />
                                <asp:BoundField DataField="FullName" HeaderText="Name" />
                                <asp:BoundField DataField="submitted_at" HeaderText="Submission Date" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                
                                <asp:TemplateField HeaderText="Resume/CV">
                                    <ItemTemplate>
                                        <a href='<%# ResolveUrl("~/Uploads/Verification/" + Eval("verification_document").ToString()) %>' target="_blank" class="text-danger fs-4" title="View Document">
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
                                        <div class="d-flex align-items-center">
                                            <asp:RadioButton ID="rbVerify" runat="server" GroupName="Action" Text="Verify" CssClass="me-3 radio-verify" AutoPostBack="true" OnCheckedChanged="Action_Changed" />
                                            <asp:RadioButton ID="rbReject" runat="server" GroupName="Action" Text="Reject" CssClass="radio-reject" AutoPostBack="true" OnCheckedChanged="Action_Changed" />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="ReviewerName" HeaderText="Reviewed By" NullDisplayText="None" />
                                <asp:BoundField DataField="verified_at" HeaderText="Verified Date" DataFormatString="{0:dd/MM/yyyy HH:mm}" NullDisplayText="Pending" />
                            </Columns>
                            
                            <EmptyDataTemplate><tr><td colspan="100%" class="ec-no-records">No applications found matching your filters.</td></tr></EmptyDataTemplate>
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
                        <div class="modal-content ec-modal-content">
                            <div class="ec-modal-header bg-danger text-white border-0">
                                <h5 class="modal-title fw-bold text-uppercase">REMOVE APPLICATION CONFIRMATION</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="closeRemoveModal()"></button>
                            </div>
                            <div class="ec-modal-body text-center p-4">
                                <h4 class="mb-3 fw-normal">Are you sure to remove the selected application(s)?</h4>
                                <p class="text-danger fw-bold fs-5 mb-4"><asp:Literal ID="litSelectedIds" runat="server"></asp:Literal></p>
                                <div class="d-flex justify-content-center gap-3">
                                    <asp:Button ID="btnConfirmRemove" runat="server" Text="Remove" CssClass="btn btn-danger px-4 rounded-pill fw-bold" OnClick="btnConfirmRemove_Click" />
                                    <button type="button" class="btn btn-secondary px-4 rounded-pill fw-bold" onclick="closeRemoveModal()">Cancel</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>