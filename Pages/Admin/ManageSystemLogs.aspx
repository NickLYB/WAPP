<%@ Page Title="System Logs" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true" CodeBehind="ManageSystemLogs.aspx.cs" Inherits="WAPP.Pages.Admin.ManageSystemLogs" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    
    <script type="text/javascript">
        // Standard Bootstrap Modal Logic
        function showLogConfirmModal(id, currentStatus) {
            document.getElementById('<%= hfLogId.ClientID %>').value = id;
            var nextStatus = currentStatus === "OPEN" ? "RESOLVED" : "OPEN";
            document.getElementById('<%= hfNewStatus.ClientID %>').value = nextStatus;
            document.getElementById('confirmText').innerHTML = "Mark this log as <strong class='text-dark'>" + nextStatus + "</strong>?";

            bootstrap.Modal.getOrCreateInstance(document.getElementById('logConfirmModal')).show();
        }

        function closeLogConfirmModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('logConfirmModal')).hide();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelSystemLogs">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper">
        <h2 class="ec-staff-title text-uppercase mb-4">System Log Management</h2>

        <asp:UpdatePanel ID="upPanelSystemLogs" runat="server">
            <ContentTemplate>
                
                <div class="ec-staff-card p-4 mb-4">
                    <div class="row align-items-end g-3">
                        
                        <div class="col-lg-5">
                            <label class="form-label fw-bold text-muted mb-2 small text-uppercase">Search Logs</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" placeholder="Search action or description..."></asp:TextBox>
                                <asp:LinkButton ID="btnSearch" runat="server" CssClass="input-group-text ec-search-btn text-decoration-none" OnClick="BtnSearch_Click">
                                    <i class="bi bi-search"></i>
                                </asp:LinkButton>
                            </div>
                        </div>

                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Sort By Date</label>
                            <asp:DropDownList ID="ddlSort" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                <asp:ListItem Text="Newest First" Value="DESC"></asp:ListItem>
                                <asp:ListItem Text="Oldest First" Value="ASC"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Severity</label>
                            <asp:DropDownList ID="ddlSeverityFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"></asp:DropDownList>
                        </div>

                        <div class="col-lg-3">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Status</label>
                            <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                <asp:ListItem Text="All Status" Value="All"></asp:ListItem>
                                <asp:ListItem Text="OPEN" Value="OPEN"></asp:ListItem>
                                <asp:ListItem Text="RESOLVED" Value="RESOLVED"></asp:ListItem>
                                <asp:ListItem Text="IGNORED" Value="IGNORED"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                    </div>
                </div>

                <div class="ec-staff-card p-0">
                    <asp:GridView ID="gvLogs" runat="server" AutoGenerateColumns="False" 
                        CssClass="table table-hover ec-table-custom align-middle mb-0" GridLines="None" Width="100%" 
                        AllowPaging="True" PageSize="10" OnPageIndexChanging="gvLogs_PageIndexChanging" ShowHeaderWhenEmpty="true">
                        <PagerSettings Visible="False" />
                        
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="Log ID" ItemStyle-Width="80px" />
                            <asp:BoundField DataField="UserName" HeaderText="Performed By" />
                            <asp:BoundField DataField="SeverityName" HeaderText="Severity" />
                            <asp:BoundField DataField="action_type" HeaderText="Action" />
                            <asp:BoundField DataField="description" HeaderText="Description" />
                            <asp:BoundField DataField="created_at" HeaderText="Timestamp" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                            
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnToggle" runat="server" CssClass="text-decoration-none d-flex align-items-center" OnClientClick='<%# "showLogConfirmModal(" + Eval("Id") + ", \"" + Eval("status") + "\"); return false;" %>'>
                                        <span class='ec-toggle-btn <%# Eval("status").ToString() == "RESOLVED" ? "ec-status-active-toggle" : "ec-status-locked-toggle" %>'></span>
                                        <span class='ms-2 fw-bold small <%# Eval("status").ToString() == "RESOLVED" ? "text-success" : "text-danger" %>'>
                                            <%# Eval("status") %>
                                        </span>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        
                        <EmptyDataTemplate>
                            <tr><td colspan="100%" class="ec-no-records">No system logs found matching your criteria.</td></tr>
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

                <div class="modal fade" id="logConfirmModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered" style="max-width: 400px;">
                        <div class="modal-content ec-modal-content text-center p-4">
                            
                            <div class="mb-3">
                                <i class="bi bi-info-circle-fill" style="font-size: 3.5rem; color: var(--ec-primary);"></i>
                            </div>
                            
                            <h4 class="fw-bold text-dark mb-2">Update Log Status</h4>
                            
                            <asp:HiddenField ID="hfLogId" runat="server" />
                            <asp:HiddenField ID="hfNewStatus" runat="server" />
                            
                            <p id="confirmText" class="text-muted mb-4">Change status to RESOLVED?</p>
                            
                            <div class="d-flex justify-content-center gap-3">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeLogConfirmModal()">Cancel</button>
                                <asp:Button ID="btnConfirmChange" runat="server" Text="Confirm Update" CssClass="ec-btn-primary px-4" OnClick="btnConfirmChange_Click" />
                            </div>

                        </div>
                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>