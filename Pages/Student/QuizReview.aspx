<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="QuizReview.aspx.cs" Inherits="WAPP.Pages.Student.QuizReview" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    </asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container py-5">
        <div class="ec-section-gap mb-3">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="StudentMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <div style="max-width: 850px; margin: 0 auto;">
            
            <div class="d-flex justify-content-between align-items-center mb-5">
                <h2 class="ec-page-title m-0">Review Your Answers</h2>
                <asp:LinkButton ID="btnBack" runat="server" CssClass="btn btn-secondary rounded-pill px-4 fw-bold text-white shadow-sm" OnClick="btnBack_Click">
                    <i class="bi bi-arrow-left me-2"></i>Back to Lesson
                </asp:LinkButton>
            </div>

            <asp:Repeater ID="rptReview" runat="server" OnItemDataBound="rptReview_ItemDataBound">
                <ItemTemplate>
                    <div class="ec-glass-card mb-4 p-4 p-md-5">
                        
                        <h5 class="fw-bold mb-4 text-main" style="line-height: 1.6;">
                            <span class="badge me-2 px-3 py-2 rounded-pill" style="background: var(--ec-bg-alt); color: var(--ec-primary); font-size: 0.9rem;">
                                #<%# Container.ItemIndex + 1 %>
                            </span> 
                            <%# Eval("question_text") %>
                        </h5>
                        
                        <asp:HiddenField ID="hfQuestionId" runat="server" Value='<%# Eval("Id") %>' />
                        
                        <div class="options-list">
                            <asp:Repeater ID="rptOptions" runat="server">
                                <ItemTemplate>
                                    <div class='<%# GetOptionClass(Eval("is_correct"), Eval("is_selected")) %>'>
                                        <div class="icon-box">
                                            <i class='<%# GetOptionIcon(Eval("is_correct"), Eval("is_selected")) %>'></i>
                                        </div>
                                        <span><%# Eval("text") %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        
                    </div>
                </ItemTemplate>
            </asp:Repeater>

        </div>
    </div>
</asp:Content>