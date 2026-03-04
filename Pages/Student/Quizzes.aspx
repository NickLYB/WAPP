<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="Quizzes.aspx.cs" Inherits="WAPP.Pages.Student.Quizzes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    
    <script type="text/javascript">
        function startTimer(duration, display) {
            var timer = duration, minutes, seconds;
            var countdown = setInterval(function () {
                minutes = parseInt(timer / 60, 10);
                seconds = parseInt(timer % 60, 10);

                minutes = minutes < 10 ? "0" + minutes : minutes;
                seconds = seconds < 10 ? "0" + seconds : seconds;

                display.textContent = minutes + ":" + seconds;

                // Color change logic for last 2 minutes
                if (timer < 120) {
                    display.parentElement.classList.remove('text-primary');
                    display.parentElement.classList.add('text-danger');
                }

                if (--timer < 0) {
                    clearInterval(countdown);
                    alert("Time is up! Your quiz will be submitted automatically.");
                    // C# ID selector for the submit button
                    document.getElementById('<%= btnSubmitQuiz.ClientID %>').click();
                }
            }, 1000);
        }

        window.onload = function () {
            // QuizDuration is passed from Code-Behind
            var duration = 60 * <%= QuizDuration %>;
            var display = document.querySelector('#time-left');
            if (display) startTimer(duration, display);
        };
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="ec-quiz-timer shadow-sm">
        <div class="container d-flex justify-content-between align-items-center">
            <div>
                <h5 class="mb-0 fw-bold text-main"><asp:Literal ID="litQuizTitle" runat="server"></asp:Literal></h5>
                <span class="text-muted small">Target Course: <asp:Literal ID="litCourseName" runat="server"></asp:Literal></span>
            </div>
            <div class="d-flex align-items-center text-primary fw-bold fs-4">
                <i class="bi bi-hourglass-split me-2"></i>
                <span id="time-left">00:00</span>
            </div>
        </div>
    </div>

    <div class="container pb-5">
        <div class="ec-quiz-container">
            
            <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound">
                <ItemTemplate>
                    <div class="ec-glass-card mb-4 p-4 p-md-5">
                        
                        <h5 class="fw-bold text-main mb-4" style="line-height: 1.6;">
                            <span class="badge me-2 px-3 py-2 rounded-pill" style="background: var(--ec-bg-alt); color: var(--ec-primary); font-size: 0.9rem;">
                                Q<%# Container.ItemIndex + 1 %>
                            </span>
                            <%# Eval("question_text") %>
                        </h5>
                        
                        <asp:HiddenField ID="hfQuestionId" runat="server" Value='<%# Eval("Id") %>' />
                        
                        <div class="ec-option-list">
                            <asp:RadioButtonList ID="rblOptions" runat="server" 
                                DataTextField="option_text" DataValueField="Id" 
                                RepeatLayout="UnorderedList" CssClass="list-unstyled p-0 m-0">
                            </asp:RadioButtonList>
                        </div>

                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <div class="text-center mt-5">
                
                <div class="p-4 rounded-4 border mb-4" style="background-color: var(--ec-bg-alt); border-color: var(--ec-border-light) !important;">
                    <p class="text-muted mb-0 fw-bold"><i class="bi bi-exclamation-circle-fill text-warning me-2"></i>Once you submit, you cannot change your answers. Please review your work carefully.</p>
                </div>
                
                <asp:Button ID="btnSubmitQuiz" runat="server" Text="Confirm and Submit Assessment" 
                    CssClass="btn btn-success rounded-pill px-5 py-3 fw-bold shadow-lg fs-5" 
                    OnClick="btnSubmitQuiz_Click" 
                    OnClientClick="return confirm('Ready to finalize your answers?');" />
            </div>

        </div>
    </div>
</asp:Content>