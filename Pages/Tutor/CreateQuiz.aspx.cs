using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WAPP.Utils;

namespace WAPP.Pages.Tutor
{
    [Serializable]
    public class QuestionItem
    {
        public int QuestionId { get; set; } = 0;
        public string QuestionText { get; set; } = "";
        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";
        public string CorrectAnswer { get; set; } = "A";
    }

    public partial class CreateNewQuiz : System.Web.UI.Page
    {
        private List<QuestionItem> QuizQuestions
        {
            get
            {
                if (ViewState["QuizQuestions"] == null)
                    ViewState["QuizQuestions"] = new List<QuestionItem>();
                return (List<QuestionItem>)ViewState["QuizQuestions"];
            }
            set
            {
                ViewState["QuizQuestions"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] == null || Session["role_id"] == null || (int)Session["role_id"] != 3)
            {
                Response.Redirect("~/Pages/Guest/Home.aspx");
            }

            if (!IsPostBack)
            {
                // 1. Load lessons, passing the current QuizId (0 if creating new) so the assigned lesson isn't hidden
                LoadLessonDropDownList(CourseId, IsEditMode ? QuizId : 0);

                // 2. Check for Lesson ID in QueryString (The "Lock" logic)
                if (int.TryParse(Request.QueryString["lessonId"], out int lockedLessonId))
                {
                    if (ddlTargetLesson.Items.FindByValue(lockedLessonId.ToString()) != null)
                    {
                        ddlTargetLesson.SelectedValue = lockedLessonId.ToString();
                        ddlTargetLesson.Enabled = false; // Lock it
                        lblLessonLockHint.Visible = true;
                    }
                }
                pageTitle.InnerText = IsEditMode ? "EDIT QUIZ" : "CREATE NEW QUIZ";
                if (CourseId <= 0)
                {
                    lblMsg.Text = "Error: Missing Course ID.";
                    return;
                }

                if (IsEditMode)
                {
                    // edit mode
                    PreselectCurrentLesson(QuizId);
                    LoadQuizHeader(QuizId);
                    LoadQuizQuestions(QuizId);
                    Button1.Text = "Update Quiz";

                    // ---> TURN ON DELETE BUTTON IN EDIT MODE <---
                    btnDeleteQuiz.Visible = true;
                }
                else
                {
                    // create mode
                    QuizQuestions = new List<QuestionItem> { new QuestionItem() };
                    BindQuestionRepeater();
                    Button1.Text = "Save Quiz";
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SiteMap.SiteMapResolve += SiteMap_Resolve;
        }
        protected override void OnUnload(EventArgs e)
        {
            SiteMap.SiteMapResolve -= SiteMap_Resolve;
            base.OnUnload(e);
        }

        private string GetCourseName(int courseId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 title FROM course WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", courseId);
                con.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private void LoadQuizHeader(int quizId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT title, description, duration_minutes
        FROM quiz
        WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", quizId);
                con.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        txtQuizTitle.Text = r["title"]?.ToString();
                        txtDescription.Text = r["description"]?.ToString();

                        int mins = Convert.ToInt32(r["duration_minutes"]);
                        if (mins == 9999) mins = 0;

                        if (ddlTimeLimit.Items.FindByValue(mins.ToString()) != null)
                            ddlTimeLimit.SelectedValue = mins.ToString();
                        else
                            ddlTimeLimit.SelectedValue = "0";
                    }
                }
            }
        }

        private void LoadQuizQuestions(int quizId)
        {
            var list = new List<QuestionItem>();

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT 
            q.Id AS QuestionId,
            q.question_text,
            ao.text AS option_text,
            ao.is_correct
        FROM question q
        LEFT JOIN answerOption ao ON ao.question_id = q.Id
        WHERE q.quiz_id = @QuizId
        ORDER BY q.Id ASC, ao.Id ASC", con))
            {
                cmd.Parameters.AddWithValue("@QuizId", quizId);
                con.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    int currentQid = -1;
                    QuestionItem current = null;
                    int optIndex = 0;

                    while (r.Read())
                    {
                        int qid = Convert.ToInt32(r["QuestionId"]);

                        if (qid != currentQid)
                        {
                            // new question
                            currentQid = qid;
                            current = new QuestionItem
                            {
                                QuestionId = qid,
                                QuestionText = r["question_text"]?.ToString() ?? "",
                                CorrectAnswer = "A"
                            };
                            list.Add(current);
                            optIndex = 0;
                        }

                        // option may be null if no options found
                        string optText = r["option_text"]?.ToString() ?? "";
                        bool isCorrect = r["is_correct"] != DBNull.Value && Convert.ToBoolean(r["is_correct"]);

                        // map by order to A/B/C/D
                        if (optIndex == 0) { current.OptionA = optText; if (isCorrect) current.CorrectAnswer = "A"; }
                        if (optIndex == 1) { current.OptionB = optText; if (isCorrect) current.CorrectAnswer = "B"; }
                        if (optIndex == 2) { current.OptionC = optText; if (isCorrect) current.CorrectAnswer = "C"; }
                        if (optIndex == 3) { current.OptionD = optText; if (isCorrect) current.CorrectAnswer = "D"; }

                        optIndex++;
                    }
                }
            }

            // if quiz has no questions, still show 1 blank
            if (list.Count == 0) list.Add(new QuestionItem());

            QuizQuestions = list;
            BindQuestionRepeater();
        }

        private SiteMapNode SiteMap_Resolve(object sender, SiteMapResolveEventArgs e)
        {
            var ctx = e.Context;
            if (ctx?.Request == null) return SiteMap.CurrentNode;

            string path = ctx.Request.Path;
            if (!path.EndsWith("/CreateQuiz.aspx", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith("/CreateQuiz", StringComparison.OrdinalIgnoreCase))
                return SiteMap.CurrentNode;

            SiteMapNode current = SiteMap.CurrentNode;
            if (current == null) return null;

            // Clone(true) clones the current node AND its ancestors (parents)
            SiteMapNode clone = current.Clone(true);

            // get course id
            if (!int.TryParse(ctx.Request.QueryString["id"], out int courseId))
                return clone;

            string courseTitle = GetCourseName(courseId);
            if (string.IsNullOrWhiteSpace(courseTitle))
                return clone;

            clone.Url += $"?id={courseId}";

            // Walk up: Announcement -> Edit -> Courses
            if (clone.ParentNode != null)
            {
                // Update the title
                clone.ParentNode.Title = $"Edit - {courseTitle}";

                clone.ParentNode.Url += $"?id={courseId}";
            }

            return clone;
        }

        private void BindQuestionRepeater()
        {
            rptQuestions.DataSource = QuizQuestions;
            rptQuestions.DataBind();
        }

        private void SaveCurrentRepeaterData()
        {
            List<QuestionItem> currentList = new List<QuestionItem>();

            foreach (RepeaterItem item in rptQuestions.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    HiddenField hfQId = (HiddenField)item.FindControl("hfQuestionId");
                    int qId = 0;
                    if (hfQId != null) int.TryParse(hfQId.Value, out qId);

                    // Find the textboxes inside this specific repeater row
                    TextBox txtQ = (TextBox)item.FindControl("txtQuestionText");
                    TextBox txtA = (TextBox)item.FindControl("txtOptionA");
                    TextBox txtB = (TextBox)item.FindControl("txtOptionB");
                    TextBox txtC = (TextBox)item.FindControl("txtOptionC");
                    TextBox txtD = (TextBox)item.FindControl("txtOptionD");
                    DropDownList ddlCorrect = (DropDownList)item.FindControl("ddlCorrectAnswer");

                    // Save the data to a new object
                    currentList.Add(new QuestionItem
                    {
                        QuestionId = qId,
                        QuestionText = txtQ.Text,
                        OptionA = txtA.Text,
                        OptionB = txtB.Text,
                        OptionC = txtC.Text,
                        OptionD = txtD.Text,
                        CorrectAnswer = ddlCorrect.SelectedValue
                    });
                }
            }

            // Update ViewState memory with the fresh user inputs
            QuizQuestions = currentList;
        }

        protected void btnAddQuestion_Click(object sender, EventArgs e)
        {
            SaveCurrentRepeaterData(); // Save their existing work

            QuizQuestions.Add(new QuestionItem()); // Add 1 blank question at the bottom

            BindQuestionRepeater(); // Redraw screen
        }

        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Remove")
            {
                SaveCurrentRepeaterData(); // Save their existing work

                // Get the index of the row they clicked remove on
                int indexToRemove = Convert.ToInt32(e.CommandArgument);

                // Don't let them delete the very last question (force at least 1)
                if (QuizQuestions.Count > 1)
                {
                    QuizQuestions.RemoveAt(indexToRemove);
                    BindQuestionRepeater();
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                UpdateQuizAndQuestions(QuizId);
            }
            else
            {
                CreateQuizAndQuestions();
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            // Cancel logic: Just redirect back to the Edit Course page
            string idStr = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(idStr))
            {
                Response.Redirect($"~/Pages/Tutor/EditCourse.aspx?id={idStr}");
            }
            else
            {
                Response.Redirect("~/Pages/Tutor/Teaching.aspx");
            }
        }

        private bool IsEditMode => int.TryParse(Request.QueryString["quizId"], out _);

        private int CourseId
        {
            get
            {
                int.TryParse(Request.QueryString["id"], out int cid);
                return cid;
            }
        }

        private int QuizId
        {
            get
            {
                int.TryParse(Request.QueryString["quizId"], out int qid);
                return qid;
            }
        }

        private void CreateQuizAndQuestions()
        {
            lblMsg.ForeColor = System.Drawing.Color.Red;

            if (ddlTargetLesson.SelectedValue == "0")
            {
                lblMsg.Text = "Please select a target lesson for this quiz.";
                return;
            }

            // 1. Basic Form Validation
            if (string.IsNullOrWhiteSpace(txtQuizTitle.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                lblMsg.Text = "Please fill in the Quiz Title and Description.";
                return;
            }

            if (!int.TryParse(Request.QueryString["id"], out int courseId))
            {
                lblMsg.Text = "Error: Cannot find Course ID.";
                return;
            }

            // Handle "No Limit" constraint (Value "0" will break your DB CHECK constraint, so we make it 9999)
            int duration = Convert.ToInt32(ddlTimeLimit.SelectedValue);
            if (duration <= 0) duration = 9999;

            // 2. Lock in the latest typed questions from the screen
            SaveCurrentRepeaterData();

            // Ensure at least one valid question exists
            if (QuizQuestions.Count == 0)
            {
                lblMsg.Text = "Please add at least one complete question before saving.";
                return;
            }

            for (int i = 0; i < QuizQuestions.Count; i++)
            {
                var q = QuizQuestions[i];
                if (string.IsNullOrWhiteSpace(q.QuestionText) ||
                    string.IsNullOrWhiteSpace(q.OptionA) ||
                    string.IsNullOrWhiteSpace(q.OptionB) ||
                    string.IsNullOrWhiteSpace(q.OptionC) ||
                    string.IsNullOrWhiteSpace(q.OptionD))
                {
                    lblMsg.Text = $"Question {i + 1} is incomplete. The question text and all options (A, B, C, D) must be filled.";
                    return;
                }
            }

            int? currentUserId = Session["UserId"] != null ? (int?)Convert.ToInt32(Session["UserId"]) : null;
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                string checkSql = "SELECT quiz_id FROM learningResource WHERE Id = @lid";
                using (SqlCommand cmdCheck = new SqlCommand(checkSql, con))
                {
                    cmdCheck.Parameters.AddWithValue("@lid", ddlTargetLesson.SelectedValue);
                    object existingQuiz = cmdCheck.ExecuteScalar();

                    if (existingQuiz != null && existingQuiz != DBNull.Value)
                    {
                        lblMsg.Text = "The selected lesson already has a quiz assigned. One lesson can only have one quiz.";
                        return;
                    }
                }

                // 3. Start a Transaction (If one insert fails, they ALL fail safely)
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // --- STEP A: Insert the Quiz FIRST ---
                        string quizSql = @"
                    INSERT INTO quiz (course_id, title, description, duration_minutes) 
                    VALUES (@courseId, @title, @desc, @duration); 
                    SELECT CAST(SCOPE_IDENTITY() as int);"; // Gets the new Quiz ID

                        int newQuizId = 0;

                        using (SqlCommand cmd = new SqlCommand(quizSql, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@courseId", courseId);
                            cmd.Parameters.AddWithValue("@title", txtQuizTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@duration", duration);

                            newQuizId = (int)cmd.ExecuteScalar();
                        }

                        if (ddlTargetLesson.SelectedValue != "0")
                        {
                            string linkSql = "UPDATE learningResource SET quiz_id = @qid WHERE Id = @lid";
                            using (SqlCommand cmdLink = new SqlCommand(linkSql, con, transaction))
                            {
                                cmdLink.Parameters.AddWithValue("@qid", newQuizId); // Now it has the real ID!
                                cmdLink.Parameters.AddWithValue("@lid", ddlTargetLesson.SelectedValue);
                                cmdLink.ExecuteNonQuery();
                            }
                        }

                        // --- STEP B: Loop through and insert Questions and Options ---
                        string questionSql = @"
                    INSERT INTO question (quiz_id, question_text) 
                    VALUES (@quizId, @qText); 
                    SELECT CAST(SCOPE_IDENTITY() as int);"; // Gets the new Question ID

                        string optionSql = @"
                    INSERT INTO answerOption (question_id, text, is_correct) 
                    VALUES (@qId, @optText, @isCorrect)";

                        foreach (QuestionItem q in QuizQuestions)
                        {
                            int newQuestionId = 0;

                            // Insert the Question
                            using (SqlCommand cmdQ = new SqlCommand(questionSql, con, transaction))
                            {
                                cmdQ.Parameters.AddWithValue("@quizId", newQuizId);
                                cmdQ.Parameters.AddWithValue("@qText", q.QuestionText.Trim());
                                newQuestionId = (int)cmdQ.ExecuteScalar();
                            }

                            // Local helper function to insert options cleanly
                            void InsertOption(string optText, bool isCorrect)
                            {
                                using (SqlCommand cmdO = new SqlCommand(optionSql, con, transaction))
                                {
                                    cmdO.Parameters.AddWithValue("@qId", newQuestionId);
                                    cmdO.Parameters.AddWithValue("@optText", optText.Trim());
                                    cmdO.Parameters.AddWithValue("@isCorrect", isCorrect);
                                    cmdO.ExecuteNonQuery();
                                }
                            }

                            // Insert the 4 options and check if they match the CorrectAnswer Dropdown (A, B, C, D)
                            InsertOption(q.OptionA, q.CorrectAnswer == "A");
                            InsertOption(q.OptionB, q.CorrectAnswer == "B");
                            InsertOption(q.OptionC, q.CorrectAnswer == "C");
                            InsertOption(q.OptionD, q.CorrectAnswer == "D");
                        }

                        // --- STEP C: If everything worked, commit the changes to the database! ---
                        transaction.Commit();

                        SystemLogService.Write("QUIZ_CREATED",
                            $"Tutor created quiz '{txtQuizTitle.Text.Trim()}' (Quiz ID: {newQuizId}) for Course ID {courseId}.",
                            LogLevel.INFO, currentUserId);

                        // Clear memory and redirect back to the Edit Course page
                        ViewState["QuizQuestions"] = null;
                        Response.Redirect($"~/Pages/Tutor/EditCourse.aspx?id={courseId}", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    catch (Exception ex)
                    {
                        // If anything broke, rollback all database changes so no corrupt data is left behind
                        transaction.Rollback();

                        SystemLogService.Write("QUIZ_CREATE_ERROR",
                            $"DB Error creating quiz for Course ID {courseId}: {ex.Message}",
                            LogLevel.ERROR, currentUserId);

                        lblMsg.Text = "Database Error while saving the quiz: " + ex.Message;
                    }
                }
            }
        }

        private void UpdateQuizAndQuestions(int quizId)
        {
            lblMsg.ForeColor = System.Drawing.Color.Red;

            // FIX 1: Must select a lesson
            if (ddlTargetLesson.SelectedValue == "0")
            {
                lblMsg.Text = "Please select a target lesson for this quiz.";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQuizTitle.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                lblMsg.Text = "Please fill in the Quiz Title and Description.";
                return;
            }

            int courseId = CourseId;
            int duration = Convert.ToInt32(ddlTimeLimit.SelectedValue);
            if (duration <= 0) duration = 9999;

            SaveCurrentRepeaterData();

            if (QuizQuestions.Count == 0)
            {
                lblMsg.Text = "Please add at least one complete question before saving.";
                return;
            }

            // FIX 2: Check that all questions and options A-D are filled
            for (int i = 0; i < QuizQuestions.Count; i++)
            {
                var q = QuizQuestions[i];
                if (string.IsNullOrWhiteSpace(q.QuestionText) ||
                    string.IsNullOrWhiteSpace(q.OptionA) ||
                    string.IsNullOrWhiteSpace(q.OptionB) ||
                    string.IsNullOrWhiteSpace(q.OptionC) ||
                    string.IsNullOrWhiteSpace(q.OptionD))
                {
                    lblMsg.Text = $"Question {i + 1} is incomplete. The question text and all options (A, B, C, D) must be filled.";
                    return;
                }
            }

            int? currentUserId = Session["UserId"] != null ? (int?)Convert.ToInt32(Session["UserId"]) : null;
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                // FIX 3: 1 Lesson can only have 1 Quiz Check (Allow if it's assigned to THIS quiz)
                string checkSql = "SELECT quiz_id FROM learningResource WHERE Id = @lid";
                using (SqlCommand cmdCheck = new SqlCommand(checkSql, con))
                {
                    cmdCheck.Parameters.AddWithValue("@lid", ddlTargetLesson.SelectedValue);
                    object existingQuiz = cmdCheck.ExecuteScalar();

                    if (existingQuiz != null && existingQuiz != DBNull.Value)
                    {
                        int assignedQuizId = Convert.ToInt32(existingQuiz);
                        if (assignedQuizId != quizId)
                        {
                            lblMsg.Text = "The selected lesson already has a different quiz assigned. One lesson can only have one quiz.";
                            return;
                        }
                    }
                }

                using (SqlTransaction tran = con.BeginTransaction())
                {
                    try
                    {
                        // 1. Update quiz header
                        using (SqlCommand cmd = new SqlCommand("UPDATE quiz SET title=@t, description=@d, duration_minutes=@dur WHERE Id=@id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@t", txtQuizTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@d", txtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@dur", duration);
                            cmd.Parameters.AddWithValue("@id", quizId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Delete questions that the Tutor removed via the UI (if any)
                        var keptIds = QuizQuestions.Where(q => q.QuestionId > 0).Select(q => q.QuestionId.ToString()).ToList();
                        string keptIdsStr = string.Join(",", keptIds);
                        string deleteFilter = string.IsNullOrEmpty(keptIdsStr) ? "" : $"AND Id NOT IN ({keptIdsStr})";

                        using (SqlCommand cmdDelOpts = new SqlCommand($"DELETE FROM answerOption WHERE question_id IN (SELECT Id FROM question WHERE quiz_id = @qid {deleteFilter})", con, tran))
                        {
                            cmdDelOpts.Parameters.AddWithValue("@qid", quizId);
                            cmdDelOpts.ExecuteNonQuery();
                        }
                        using (SqlCommand cmdDelQ = new SqlCommand($"DELETE FROM question WHERE quiz_id = @qid {deleteFilter}", con, tran))
                        {
                            cmdDelQ.Parameters.AddWithValue("@qid", quizId);
                            cmdDelQ.ExecuteNonQuery();
                        }

                        // 3. Process Questions (Update existing, Insert new)
                        string insertQuestionSql = "INSERT INTO question (quiz_id, question_text) VALUES (@quizId, @qText); SELECT CAST(SCOPE_IDENTITY() as int);";
                        string insertOptionSql = "INSERT INTO answerOption (question_id, text, is_correct) VALUES (@qId, @optText, @isCorrect);";

                        string updateQuestionSql = "UPDATE question SET question_text = @qText WHERE Id = @qId;";

                        string updateOptionsSql = @"
                            WITH OrderedOptions AS (
                                SELECT Id, text, is_correct, ROW_NUMBER() OVER (ORDER BY Id ASC) as RowNum
                                FROM answerOption WHERE question_id = @qId
                            )
                            UPDATE OrderedOptions
                            SET text = CASE RowNum WHEN 1 THEN @oA WHEN 2 THEN @oB WHEN 3 THEN @oC WHEN 4 THEN @oD END,
                                is_correct = CASE RowNum WHEN 1 THEN @cA WHEN 2 THEN @cB WHEN 3 THEN @cC WHEN 4 THEN @cD END;";

                        foreach (var q in QuizQuestions)
                        {
                            if (q.QuestionId > 0)
                            {
                                // --- UPDATE EXISTING QUESTION ---
                                using (SqlCommand cmdUpdQ = new SqlCommand(updateQuestionSql, con, tran))
                                {
                                    cmdUpdQ.Parameters.AddWithValue("@qText", q.QuestionText.Trim());
                                    cmdUpdQ.Parameters.AddWithValue("@qId", q.QuestionId);
                                    cmdUpdQ.ExecuteNonQuery();
                                }
                                using (SqlCommand cmdUpdO = new SqlCommand(updateOptionsSql, con, tran))
                                {
                                    cmdUpdO.Parameters.AddWithValue("@qId", q.QuestionId);
                                    cmdUpdO.Parameters.AddWithValue("@oA", q.OptionA.Trim());
                                    cmdUpdO.Parameters.AddWithValue("@oB", q.OptionB.Trim());
                                    cmdUpdO.Parameters.AddWithValue("@oC", q.OptionC.Trim());
                                    cmdUpdO.Parameters.AddWithValue("@oD", q.OptionD.Trim());
                                    cmdUpdO.Parameters.AddWithValue("@cA", q.CorrectAnswer == "A");
                                    cmdUpdO.Parameters.AddWithValue("@cB", q.CorrectAnswer == "B");
                                    cmdUpdO.Parameters.AddWithValue("@cC", q.CorrectAnswer == "C");
                                    cmdUpdO.Parameters.AddWithValue("@cD", q.CorrectAnswer == "D");
                                    cmdUpdO.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // --- INSERT NEW QUESTION ---
                                int newQuestionId;
                                using (SqlCommand cmdInsQ = new SqlCommand(insertQuestionSql, con, tran))
                                {
                                    cmdInsQ.Parameters.AddWithValue("@quizId", quizId);
                                    cmdInsQ.Parameters.AddWithValue("@qText", q.QuestionText.Trim());
                                    newQuestionId = (int)cmdInsQ.ExecuteScalar();
                                }

                                void InsertOpt(string optText, bool isCorrect)
                                {
                                    using (SqlCommand cmdInsO = new SqlCommand(insertOptionSql, con, tran))
                                    {
                                        cmdInsO.Parameters.AddWithValue("@qId", newQuestionId);
                                        cmdInsO.Parameters.AddWithValue("@optText", optText.Trim());
                                        cmdInsO.Parameters.AddWithValue("@isCorrect", isCorrect);
                                        cmdInsO.ExecuteNonQuery();
                                    }
                                }

                                InsertOpt(q.OptionA, q.CorrectAnswer == "A");
                                InsertOpt(q.OptionB, q.CorrectAnswer == "B");
                                InsertOpt(q.OptionC, q.CorrectAnswer == "C");
                                InsertOpt(q.OptionD, q.CorrectAnswer == "D");
                            }
                        }

                        // 4. Update Target Lesson Link
                        using (SqlCommand cmdClear = new SqlCommand("UPDATE learningResource SET quiz_id = NULL WHERE quiz_id = @qid", con, tran))
                        {
                            cmdClear.Parameters.AddWithValue("@qid", quizId);
                            cmdClear.ExecuteNonQuery();
                        }

                        if (ddlTargetLesson.SelectedValue != "0")
                        {
                            using (SqlCommand cmdLink = new SqlCommand("UPDATE learningResource SET quiz_id = @qid WHERE Id = @lid", con, tran))
                            {
                                cmdLink.Parameters.AddWithValue("@qid", quizId);
                                cmdLink.Parameters.AddWithValue("@lid", ddlTargetLesson.SelectedValue);
                                cmdLink.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();

                        SystemLogService.Write("QUIZ_UPDATED", $"Tutor modified quiz '{txtQuizTitle.Text.Trim()}' (Quiz ID: {quizId}).", LogLevel.NOTICE, currentUserId);

                        Response.Redirect($"~/Pages/Tutor/EditCourse.aspx?id={courseId}", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    catch (SqlException sqlEx)
                    {
                        tran.Rollback();
                        if (sqlEx.Number == 547)
                        {
                            lblMsg.Text = "You cannot REMOVE a question because a student has already answered it. Please put the question back, or you must create a new quiz.";
                        }
                        else
                        {
                            lblMsg.Text = "Database Error: " + sqlEx.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        SystemLogService.Write("QUIZ_UPDATE_ERROR", $"DB Error updating Quiz ID {quizId}: {ex.Message}", LogLevel.ERROR, currentUserId);
                        lblMsg.Text = "Error: " + ex.Message;
                    }
                }
            }
        }

        private void LoadLessonDropDownList(int courseId, int currentQuizId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                // Filter: Only get lessons where quiz_id is NULL, OR where the quiz_id matches the one we are currently editing
                string sql = @"
            SELECT Id, title 
            FROM learningResource 
            WHERE course_id = @cid 
              AND (quiz_id IS NULL OR quiz_id = @qid)
            ORDER BY sequence_order";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@cid", courseId);
                cmd.Parameters.AddWithValue("@qid", currentQuizId);
                con.Open();

                ddlTargetLesson.DataSource = cmd.ExecuteReader();
                ddlTargetLesson.DataTextField = "title";
                ddlTargetLesson.DataValueField = "Id";
                ddlTargetLesson.DataBind();

                ddlTargetLesson.Items.Insert(0, new ListItem("-- Select Lesson --", "0"));
            }
        }

        private void PreselectCurrentLesson(int quizId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                // Find the lesson that currently points to this quiz
                string sql = "SELECT Id FROM learningResource WHERE quiz_id = @qid";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@qid", quizId);
                con.Open();

                object lessonId = cmd.ExecuteScalar();

                if (lessonId != null)
                {
                    // Make sure the lesson exists in the dropdown list
                    if (ddlTargetLesson.Items.FindByValue(lessonId.ToString()) != null)
                    {
                        // Select it
                        ddlTargetLesson.SelectedValue = lessonId.ToString();

                        // LOCK IT for Edit Mode
                        ddlTargetLesson.Enabled = false;
                        lblLessonLockHint.Visible = true;
                    }
                }
            }
        }

        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            if (!IsEditMode) return;

            int quizId = QuizId;
            int courseId = CourseId;
            int? currentUserId = Session["UserId"] != null ? (int?)Convert.ToInt32(Session["UserId"]) : null;

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    try
                    {
                        // 1. Unlink the quiz from the lesson
                        using (SqlCommand cmdClear = new SqlCommand("UPDATE learningResource SET quiz_id = NULL WHERE quiz_id = @qid", con, tran))
                        {
                            cmdClear.Parameters.AddWithValue("@qid", quizId);
                            cmdClear.ExecuteNonQuery();
                        }

                        // 2. Delete all Answer Options for these questions
                        using (SqlCommand cmdDelOpts = new SqlCommand("DELETE FROM answerOption WHERE question_id IN (SELECT Id FROM question WHERE quiz_id = @qid)", con, tran))
                        {
                            cmdDelOpts.Parameters.AddWithValue("@qid", quizId);
                            cmdDelOpts.ExecuteNonQuery();
                        }

                        // 3. Delete all Questions
                        using (SqlCommand cmdDelQ = new SqlCommand("DELETE FROM question WHERE quiz_id = @qid", con, tran))
                        {
                            cmdDelQ.Parameters.AddWithValue("@qid", quizId);
                            cmdDelQ.ExecuteNonQuery();
                        }

                        // 4. Finally, delete the Quiz itself
                        using (SqlCommand cmdDelQuiz = new SqlCommand("DELETE FROM quiz WHERE Id = @qid", con, tran))
                        {
                            cmdDelQuiz.Parameters.AddWithValue("@qid", quizId);
                            cmdDelQuiz.ExecuteNonQuery();
                        }

                        tran.Commit();

                        SystemLogService.Write("QUIZ_DELETED", $"Tutor deleted quiz ID: {quizId}.", LogLevel.NOTICE, currentUserId);

                        // Kick them back to the Edit Course page
                        Response.Redirect($"~/Pages/Tutor/EditCourse.aspx?id={courseId}", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    catch (SqlException sqlEx)
                    {
                        tran.Rollback();
                        // 547 is the Foreign Key violation error (meaning a student has already answered this quiz!)
                        if (sqlEx.Number == 547)
                        {
                            lblMsg.Text = "You cannot delete this quiz because students have already attempted it.";
                        }
                        else
                        {
                            lblMsg.Text = "Database Error: " + sqlEx.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        SystemLogService.Write("QUIZ_DELETE_ERROR", $"DB Error deleting Quiz ID {quizId}: {ex.Message}", LogLevel.ERROR, currentUserId);
                        lblMsg.Text = "Error: " + ex.Message;
                    }
                }
            }
        }
    }
}