<%@ Page Title="Learning Resources Management" Language="C#" MasterPageFile="~/Masters/Staff.Master" AutoEventWireup="true" CodeBehind="LearningResource.aspx.cs" Inherits="WAPP.Pages.Staff.LearningResourcesManagement" EnableEventValidation="false" %>

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
                    cell.className = "ec-no-records"; /* Updated to Master CSS */
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
    </script>
    </asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upResourceMgmt">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper">
        <h2 class="ec-staff-title text-uppercase mb-4">Learning Resources Management</h2>
            
        <asp:UpdatePanel ID="upResourceMgmt" runat="server">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>

                <div class="ec-staff-card mb-4">
                    <div class="card-body p-4">
                        
                        <div class="row mb-4 align-items-end">
                            <div class="col-lg-4">
                                <label class="form-label fw-bold text-muted mb-2">Search / Filter:</label>
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
                            <div class="col-lg-5 text-end">
                                <asp:Button ID="btnAddResourceRedirect" runat="server" Text="+ Add New Resource" CssClass="btn btn-primary rounded-pill fw-bold px-4 shadow-sm me-2" OnClick="btnAddResourceRedirect_Click" />
                                <asp:LinkButton ID="btnTriggerRemove" runat="server" CssClass="btn btn-danger rounded-pill fw-bold px-4 shadow-sm" OnClick="btnTriggerRemove_Click">- Remove Resource</asp:LinkButton>
                            </div>
                        </div>
                        
                        <div class="row g-2">
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Type (Category)</label>
                                <asp:DropDownList ID="ddlFilterType" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Tutor</label>
                                <asp:DropDownList ID="ddlFilterTutor" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-3">
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
                            <div class="col-md-3">
                                <label class="form-label small text-muted mb-1 fw-bold">Year</label>
                                <asp:DropDownList ID="ddlFilterYear" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="FilterGrid_Changed"></asp:DropDownList>
                            </div>
                        </div>

                    </div>
                </div>

                <div class="ec-staff-card">
                    <div class="card-body p-0"> 
                        
                        <asp:GridView ID="gvResources" runat="server" CssClass="table table-hover ec-table-custom align-middle mb-0 border-top" 
                            AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id" ShowHeaderWhenEmpty="true"
                            AllowPaging="True" PageSize="10" PagerSettings-Visible="false">
                            
                            <Columns>
                                <asp:TemplateField><ItemTemplate><asp:CheckBox ID="chkSelect" runat="server" /></ItemTemplate></asp:TemplateField>
                                <asp:BoundField DataField="CourseTitle" HeaderText="Title (Course)" />
                                <asp:BoundField DataField="created_at" HeaderText="Date Uploaded" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                <asp:BoundField DataField="ResourceTypeName" HeaderText="Type" />
                                <asp:BoundField DataField="TutorName" HeaderText="Tutor" />
                                <asp:TemplateField HeaderText="Resource Link">
                                    <ItemTemplate>
                                        <a href='<%# ResolveUrl(Eval("resource_link").ToString()) %>' target="_blank" class="text-primary fw-bold ec-desc-truncate" title='<%# Eval("resource_link") %>'>
                                            <i class="bi bi-box-arrow-up-right me-1"></i> View
                                        </a>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <EmptyDataTemplate><tr><td colspan="100%" class="ec-no-records">No learning resources found matching your filters.</td></tr></EmptyDataTemplate>
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
                                <h5 class="modal-title fw-bold text-uppercase">Remove Confirmation</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="closeRemoveModal()"></button>
                            </div>
                            <div class="ec-modal-body text-center p-4">
                                <h4 class="mb-3 fw-normal">Are you sure to remove the selected resource(s)?</h4>
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