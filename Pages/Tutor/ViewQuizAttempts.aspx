<%@ Page Title="View Quiz Attempts" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="ViewQuizAttempts.aspx.cs" Inherits="WAPP.Pages.Tutor.ViewQuizAttempts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <div class="ec-item-gap">
                <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                    PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
            </div>
            
            <div class="ec-section-header border-0 pb-0">
                <h1 class="ec-page-title m-0">
                    <asp:Label ID="lblQuizTitle" runat="server" Text="Quiz Attempts"></asp:Label>
                </h1>
            </div>
            <p class="ec-page-subtitle">Review student progress, scores, and completion statuses.</p>
        </div>

        <div class="ec-glass-card">
            <div class="ec-glass-card-body p-4">
                
                <div class="row g-3 mb-4 align-items-center">
                    <div class="col-md-5">
                        <div class="d-flex align-items-center" style="height: 42px;">
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="ec-search-input w-100" placeholder="Search by student name..." style="height: 100%; border-right: none !important; border-top-right-radius: 0; border-bottom-right-radius: 0;"></asp:TextBox>
                            <asp:LinkButton ID="btnSearch" runat="server" CssClass="ec-search-btn d-flex align-items-center justify-content-center" OnClick="BtnSearch_Click" style="height: 100%; border-top-left-radius: 0; border-bottom-left-radius: 0; text-decoration: none;">
                                <i class="bi bi-search"></i>
                            </asp:LinkButton>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" style="height: 42px; border-radius: var(--ec-radius-pill);">
                            <asp:ListItem Value="All">All Statuses</asp:ListItem>
                            <asp:ListItem Value="IN_PROGRESS">In Progress</asp:ListItem>
                            <asp:ListItem Value="SUBMITTED">Submitted (Pending Grade)</asp:ListItem>
                            <asp:ListItem Value="GRADED">Graded</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <asp:Label ID="lblEmpty" runat="server" CssClass="text-danger fw-bold d-block mb-3" Visible="false"></asp:Label>

                <div class="table-responsive">
                    <asp:GridView ID="gvAttempts" runat="server" AutoGenerateColumns="False" 
                        CssClass="table ec-table" GridLines="None"
                        AllowPaging="True" PageSize="10" OnPageIndexChanging="gvAttempts_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="StudentName" HeaderText="Student Name" ItemStyle-CssClass="fw-bold" />
                            <asp:BoundField DataField="StudentEmail" HeaderText="Email" ItemStyle-CssClass="text-muted" />
                            <asp:BoundField DataField="started_at" HeaderText="Started At" DataFormatString="{0:dd MMM yyyy, hh:mm tt}" />
                            <asp:BoundField DataField="finished_at" HeaderText="Finished At" DataFormatString="{0:dd MMM yyyy, hh:mm tt}" NullDisplayText="-" />
                            
                            <asp:TemplateField HeaderText="Score">
                                <ItemTemplate>
                                    <span class="badge bg-light text-dark border">
                                        <%# Eval("score") %> Points
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <%# GetStatusBadge(Eval("status").ToString()) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="d-none" />
                    </asp:GridView>
                </div>

                <div class="ec-pager-wrapper mt-4 rounded" id="pagerWrapper" runat="server">
                    <asp:Label ID="lblShowing" runat="server" CssClass="ec-pager-info"></asp:Label>
                    <div class="d-flex gap-2">
                        <asp:LinkButton ID="btnPrev" runat="server" CssClass="btn-sub" OnClick="btnPrev_Click">&laquo; Prev</asp:LinkButton>
                        <asp:LinkButton ID="btnNext" runat="server" CssClass="btn-sub" OnClick="btnNext_Click">Next &raquo;</asp:LinkButton>
                    </div>
                </div>

            </div>
        </div>
    </div>
</asp:Content>