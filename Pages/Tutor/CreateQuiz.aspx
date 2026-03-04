<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="CreateQuiz.aspx.cs" Inherits="WAPP.Pages.Tutor.CreateNewQuiz" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>

        <div class="ec-section-header border-0">
            <h1 id="pageTitle" runat="server" class="ec-page-title m-0">CREATE NEW QUIZ</h1>
            <div class="d-flex gap-2">
                <asp:Button ID="Button2" runat="server" Text="Cancel" CssClass="btn-sub" OnClick="Button2_Click"/>
                <asp:Button ID="Button1" runat="server" Text="Save Quiz" CssClass="btn-main btn-pill" OnClick="Button1_Click" />
            </div>
        </div>

        <div class="page-layout-split">
            
            <div class="layout-main-70">
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header">
                        <h5 class="ec-section-title"><i class="bi bi-list-check me-2 text-primary"></i>Assessment Questions</h5>
                        <asp:Button ID="btnAddQuestion" runat="server" Text="+ Add Question" CssClass="btn-sub" OnClick="btnAddQuestion_Click" />
                    </div>

                    <div class="ec-glass-card-body">
                        <asp:Repeater ID="rptQuestions" runat="server" OnItemCommand="rptQuestions_ItemCommand">
                            <ItemTemplate>
                                <div class="ec-glass-card mb-4" style="background: #f8fafc; border: 1px solid #e2e8f0;">
                                    
                                    <div class="d-flex justify-content-between align-items-center mb-3">
                                        <span class="badge bg-white text-primary border rounded-pill px-3 py-2 fw-bold">Question <%# Container.ItemIndex + 1 %></span>
                                        <asp:LinkButton ID="btnRemove" runat="server" 
                                            CommandName="Remove" CommandArgument='<%# Container.ItemIndex %>' 
                                            CssClass="text-danger text-decoration-none small fw-bold">
                                            <i class="bi bi-trash-fill"></i> Remove
                                        </asp:LinkButton>
                                    </div>

                                    <div class="ec-item-gap">
                                        <asp:TextBox ID="txtQuestionText" runat="server" CssClass="login-input m-0" 
                                            style="border-radius: 12px;" TextMode="MultiLine" Rows="2" 
                                            placeholder="Type question here..." Text='<%# Eval("QuestionText") %>'></asp:TextBox>
                                    </div>

                                    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 15px;">
                                        <div class="d-flex align-items-center gap-2">
                                            <span class="fw-bold text-muted small">A</span>
                                            <asp:TextBox ID="txtOptionA" runat="server" CssClass="login-input m-0" placeholder="Option A" Text='<%# Eval("OptionA") %>'></asp:TextBox>
                                        </div>
                                        <div class="d-flex align-items-center gap-2">
                                            <span class="fw-bold text-muted small">B</span>
                                            <asp:TextBox ID="txtOptionB" runat="server" CssClass="login-input m-0" placeholder="Option B" Text='<%# Eval("OptionB") %>'></asp:TextBox>
                                        </div>
                                        <div class="d-flex align-items-center gap-2">
                                            <span class="fw-bold text-muted small">C</span>
                                            <asp:TextBox ID="txtOptionC" runat="server" CssClass="login-input m-0" placeholder="Option C" Text='<%# Eval("OptionC") %>'></asp:TextBox>
                                        </div>
                                        <div class="d-flex align-items-center gap-2">
                                            <span class="fw-bold text-muted small">D</span>
                                            <asp:TextBox ID="txtOptionD" runat="server" CssClass="login-input m-0" placeholder="Option D" Text='<%# Eval("OptionD") %>'></asp:TextBox>
                                        </div>
                                    </div>

                                    <div class="d-flex align-items-center gap-2">
                                        <span class="small fw-bold text-dark">Correct Answer:</span>
                                        <asp:DropDownList ID="ddlCorrectAnswer" runat="server" CssClass="login-input m-0" style="max-width: 120px; padding: 5px 15px;" SelectedValue='<%# Eval("CorrectAnswer") %>'>
                                            <asp:ListItem Text="Option A" Value="A"></asp:ListItem>
                                            <asp:ListItem Text="Option B" Value="B"></asp:ListItem>
                                            <asp:ListItem Text="Option C" Value="C"></asp:ListItem>
                                            <asp:ListItem Text="Option D" Value="D"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <div class="text-center py-3">
                            <asp:Label ID="lblMsg" runat="server" CssClass="text-danger fw-bold" Text=""></asp:Label>
                        </div>
                    </div>
                </div>
            </div>

            <div class="layout-sidebar-30">
                <div class="ec-glass-card">
                    <div class="ec-glass-card-header border-0 pb-0">
                        <h5 class="ec-section-title">Quiz Settings</h5>
                    </div>
                    
                    <div class="ec-glass-card-body mt-3">
                        <div class="ec-item-gap">
                            <asp:Label runat="server" Text="Target Lesson" CssClass="small fw-bold text-muted mb-1 d-block" />
                            <asp:DropDownList ID="ddlTargetLesson" runat="server" CssClass="login-input m-0" style="padding: 10px 15px;">
                            </asp:DropDownList>
                            <asp:Label ID="lblLessonLockHint" runat="server" CssClass="text-primary x-small d-block mt-1" Visible="false">
                                <i class="bi bi-lock-fill"></i> Locked to specific lesson
                            </asp:Label>
                        </div>
                        <div class="ec-item-gap">
                            <asp:Label runat="server" Text="Quiz Title" CssClass="small fw-bold text-muted mb-1 d-block" />
                            <asp:TextBox ID="txtQuizTitle" runat="server" CssClass="login-input m-0" placeholder="Final Exam..." />
                        </div>

                        <div class="ec-item-gap">
                            <asp:Label runat="server" Text="Instructions" CssClass="small fw-bold text-muted mb-1 d-block" />
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="login-input m-0" style="border-radius: 12px;" TextMode="MultiLine" Rows="4" placeholder="Brief guide for students..." />
                        </div>

                        <hr class="my-3" style="opacity: 0.1;" />

                        <div class="ec-item-gap">
                            <asp:Label runat="server" Text="Time Limit" CssClass="small fw-bold text-muted mb-1 d-block" />
                            <asp:DropDownList ID="ddlTimeLimit" runat="server" CssClass="login-input m-0" style="padding: 10px 15px;">
                                <asp:ListItem Text="No Limit" Value="0" />
                                <asp:ListItem Text="30 Minutes" Value="30" />
                                <asp:ListItem Text="1 Hour" Value="60" />
                            </asp:DropDownList>
                        </div>

                        <div class="ec-item-gap">
                            <asp:Label runat="server" Text="Required Score" CssClass="small fw-bold text-muted mb-1 d-block" />
                            <asp:DropDownList ID="ddlPassingScore" runat="server" CssClass="login-input m-0" style="padding: 10px 15px;">
                                <asp:ListItem Text="60% to Pass" Value="60" />
                                <asp:ListItem Text="70% to Pass" Value="70" />
                                <asp:ListItem Text="80% to Pass" Value="80" />
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>
                
                <div class="ec-glass-card" style="background: rgba(13, 110, 253, 0.05); border: 1px dashed rgba(13, 110, 253, 0.2);">
                    <small class="text-muted">
                        <i class="bi bi-info-circle me-1 text-primary"></i> 
                        Ensure every question has a correct answer selected before saving.
                    </small>
                </div>
            </div>

        </div>
    </div>
</asp:Content>