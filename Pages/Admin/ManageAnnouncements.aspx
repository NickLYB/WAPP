<%@ Page Title="Manage Announcements" Language="C#" MasterPageFile="~/Masters/Admin.Master" AutoEventWireup="true" CodeBehind="ManageAnnouncements.aspx.cs" Inherits="WAPP.Pages.Admin.ManageAnnouncements" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    
    <script type="text/javascript">
        // Standard Bootstrap Modal Logic
        function openComposeModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('composeModal')).show();
        }
        function closeComposeModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('composeModal')).hide();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upPanelAnnouncements">
        <ProgressTemplate><div class="ec-loading-overlay"><div class="spinner-border text-primary"></div></div></ProgressTemplate>
    </asp:UpdateProgress>

    <div class="ec-staff-wrapper">
        <h2 class="ec-staff-title text-uppercase mb-4">Operational Announcement Management</h2>

        <asp:UpdatePanel ID="upPanelAnnouncements" runat="server">
            <ContentTemplate>
                
                <div class="ec-staff-card p-4 mb-4">
                    <div class="row align-items-end g-3">
                        
                        <div class="col-lg-5">
                            <label class="form-label fw-bold text-muted mb-2 small text-uppercase">Search Announcements</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control ec-search-input" placeholder="Search title or message..."></asp:TextBox>
                                <asp:LinkButton ID="btnSearch" runat="server" CssClass="input-group-text ec-search-btn text-decoration-none" OnClick="BtnSearch_Click">
                                    <i class="bi bi-search"></i>
                                </asp:LinkButton>
                            </div>
                        </div>

                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Target Role</label>
                            <asp:DropDownList ID="ddlTargetFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-2">
                            <label class="form-label small text-muted mb-2 fw-bold text-uppercase">Status</label>
                            <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select ec-filter-ddl w-100" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                <asp:ListItem Text="All" Value="All"></asp:ListItem>
                                <asp:ListItem Text="ACTIVE" Value="ACTIVE"></asp:ListItem>
                                <asp:ListItem Text="ARCHIVED" Value="ARCHIVED"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-3 text-end">
                            <button type="button" class="btn btn-primary rounded-pill fw-bold px-4 shadow-sm w-100" style="height: 42px;" onclick="openComposeModal()">+ Add Announcement</button>
                        </div>

                    </div>
                </div>

                <div class="ec-staff-card p-0">
                    <asp:GridView ID="gvAnnouncements" runat="server" AutoGenerateColumns="False" 
                        CssClass="table table-hover ec-table-custom align-middle mb-0" GridLines="None" Width="100%" 
                        AllowPaging="True" PageSize="10" OnPageIndexChanging="gvAnnouncements_PageIndexChanging" ShowHeaderWhenEmpty="true">
                        <PagerSettings Visible="False" />
                        
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="ID" ItemStyle-Width="60px" />
                            <asp:BoundField DataField="title" HeaderText="Title" ItemStyle-Font-Bold="true" />
                            
                            <asp:TemplateField HeaderText="Message">
                                <ItemTemplate>
                                    <span class="ec-desc-truncate" style="max-width: 400px; display: inline-block;" title='<%# Eval("message") %>'>
                                        <%# Eval("message") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="RoleName" HeaderText="Target Role" />
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
                            <tr><td colspan="100%" class="ec-no-records">No announcements found matching your criteria.</td></tr>
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

                <div class="modal fade" id="composeModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered modal-lg">
                        <div class="modal-content ec-modal-content">
                            
                            <div class="modal-header ec-modal-header">
                                <div>
                                    <h5 class="modal-title fw-bold text-dark">Compose New Announcement</h5>
                                    <p class="mb-0 small text-muted">Broadcast operational messages to specific user roles.</p>
                                </div>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            
                            <div class="modal-body ec-modal-body p-4">
                                
                                <div class="row mb-4">
                                    <div class="col-md-3">
                                        <label class="fw-bold text-muted small mt-2">Target Group</label>
                                    </div>
                                    <div class="col-md-9">
                                        <div class="p-2 border rounded bg-light">
                                            <asp:RadioButtonList ID="rblTarget" runat="server" RepeatDirection="Horizontal" CssClass="w-100" CellSpacing="10" CellPadding="5">
                                            </asp:RadioButtonList>
                                        </div>
                                    </div>
                                </div>

                                <div class="row mb-4">
                                    <div class="col-md-3">
                                        <label class="fw-bold text-muted small mt-2">Title</label>
                                    </div>
                                    <div class="col-md-9">
                                        <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control fw-bold" placeholder="Type title here..."></asp:TextBox>
                                    </div>
                                </div>

                                <div class="row mb-2">
                                    <div class="col-md-3">
                                        <label class="fw-bold text-muted small mt-2">Message</label>
                                    </div>
                                    <div class="col-md-9">
                                        <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6" placeholder="Type message here..." style="resize: vertical;"></asp:TextBox>
                                    </div>
                                </div>

                            </div>
                            
                            <div class="modal-footer ec-modal-footer">
                                <button type="button" class="btn btn-light fw-bold rounded-pill px-4" onclick="closeComposeModal()">Cancel</button>
                                <asp:Button ID="btnSendNow" runat="server" Text="Send Announcement" CssClass="ec-btn-primary px-4" OnClick="btnSendNow_Click" />
                            </div>

                        </div>
                    </div>
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSendNow" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>