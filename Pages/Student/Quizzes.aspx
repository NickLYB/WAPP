<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/Student.Master" CodeBehind="Quizzes.aspx.cs" Inherits="WAPP.Pages.Student.Quizzes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    
    <script type="text/javascript">
        // Timer Logic
        function startTimer(duration, display) {
            var timer = duration, minutes, seconds;
            var countdown = setInterval(function () {
                minutes = parseInt(timer / 60, 10);
                seconds = parseInt(timer % 60, 10);

                minutes = minutes < 10 ? "0" + minutes : minutes;
                seconds = seconds < 10 ? "0" + seconds : seconds;

                display.textContent = minutes + ":" + seconds;

                if (timer < 120) {
                    display.parentElement.classList.remove('text-primary');
                    display.parentElement.classList.add('text-danger');
                }

                if (--timer < 0) {
                    clearInterval(countdown);
                    alert("Time is up! Your quiz will be submitted automatically.");
                    document.getElementById('<%= btnSubmitQuiz.ClientID %>').click();
                }
            }, 1000);
        }

        window.onload = function () {
            var duration = 60 * <%= QuizDuration %>;
            var display = document.querySelector('#time-left');
            if (display) startTimer(duration, display);
        };

        // Clear Selection Logic
        function clearSelection(rblId) {
            var rbl = document.getElementById(rblId);
            if (rbl) {
                var radioButtons = rbl.getElementsByTagName("input");
                for (var i = 0; i < radioButtons.length; i++) {
                    if (radioButtons[i].type === "radio") {
                        radioButtons[i].checked = false;
                    }
                }
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div style="width: 100%; display: flex; flex-direction: column; align-items: center; padding-top: 20px;">

        <div class="ec-glass-card shadow-sm" style="width: 100%; max-width: 850px; padding: 15px 25px; margin-bottom: 25px; display: flex; flex-direction: row; justify-content: space-between; align-items: center; position: sticky; top: 15px; z-index: 1000;">
            <div>
                <h5 class="mb-1 fw-bold text-main"><asp:Literal ID="litQuizTitle" runat="server"></asp:Literal></h5>
                <span class="text-muted" style="font-size: 0.8rem;"><i class="bi bi-book-half me-1"></i><asp:Literal ID="litCourseName" runat="server"></asp:Literal></span>
            </div>
            <div class="text-primary fw-bold" style="font-size: 1.3rem; font-variant-numeric: tabular-nums; background: var(--ec-bg-alt); padding: 6px 16px; border-radius: 8px; border: 1px solid var(--ec-border-subtle);">
                <i class="bi bi-stopwatch me-2"></i>
                <span id="time-left">00:00</span>
            </div>
        </div>

        <div style="width: 100%; max-width: 850px; padding-bottom: 50px;">
            
            <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound">
                <ItemTemplate>
                    <div class="ec-glass-card mb-4 p-4 p-md-5 border-0 shadow-sm" style="background: white; border-top: 4px solid var(--ec-primary) !important;">
                        
                        <h5 class="fw-bold mb-4 text-main" style="line-height: 1.6;">
                            <span class="text-primary me-2">Q<%# Container.ItemIndex + 1 %>.</span>
                            <%# Eval("question_text") %>
                        </h5>
                        
                        <asp:HiddenField ID="hfQuestionId" runat="server" Value='<%# Eval("Id") %>' />
                        
                        <div class="ec-option-list">
                            <asp:RadioButtonList ID="rblOptions" runat="server" 
                                DataTextField="option_text" DataValueField="Id" 
                                RepeatLayout="UnorderedList" CssClass="m-0 p-0">
                            </asp:RadioButtonList>
                        </div>
                        
                        <div class="text-end mt-2 border-top pt-3">
                            <a href="javascript:void(0);" class="text-muted text-decoration-none small fw-bold" onclick="clearSelection('<%# ((Control)Container.FindControl("rblOptions")).ClientID %>')">
                                <i class="bi bi-eraser-fill me-1"></i>Clear Selection
                            </a>
                        </div>

                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <div class="d-flex justify-content-center gap-3 mt-5">
                <asp:Button ID="btnCancelQuiz" runat="server" Text="Cancel Attempt" 
                    CssClass="btn btn-sub rounded-pill px-4 fw-bold shadow-sm" OnClick="btnCancelQuiz_Click" 
                    OnClientClick="return confirm('Exit quiz? Progress will not be saved.');" />
                
                <asp:Button ID="btnSubmitQuiz" runat="server" Text="Submit Assessment" 
                    CssClass="btn btn-primary rounded-pill px-5 fw-bold shadow-sm" OnClick="btnSubmitQuiz_Click" 
                    OnClientClick="return confirm('Are you sure you want to submit your final answers?');" />
            </div>

        </div>

    </div>

</asp:Content>