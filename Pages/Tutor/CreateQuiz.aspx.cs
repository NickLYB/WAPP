using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP.Pages.Tutor
{
    [Serializable]
    public class QuestionItem
    {
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
                // 1. Always load the lessons for this course first
                LoadLessonDropDownList(CourseId);

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
                    // title heading if you want:
                    // (change your H1 to runat=server to set text)
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
                        if (mins == 9999) mins = 0; // your "no limit"

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

            // Optional: Append ID to current node's URL to be perfectly consistent
            clone.Url += $"?id={courseId}";

            // Walk up: Announcement -> Edit -> Courses
            if (clone.ParentNode != null)
            {
                // Update the title
                clone.ParentNode.Title = $"Edit - {courseTitle}";

                // CRITICAL FIX: Append the ID to the Parent Node's URL!
                // This ensures the breadcrumb link actually sends you back to the right course.
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
            if (QuizQuestions.Count == 0 || string.IsNullOrWhiteSpace(QuizQuestions[0].QuestionText))
            {
                lblMsg.Text = "Please add at least one complete question before saving.";
                return;
            }

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                // 3. Start a Transaction (If one insert fails, they ALL fail safely)
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // --- STEP A: Insert the Quiz ---
                        string quizSql = @"
                    INSERT INTO quiz (course_id, title, description, duration_minutes) 
                    VALUES (@courseId, @title, @desc, @duration); 
                    SELECT CAST(SCOPE_IDENTITY() as int);"; // Gets the new Quiz ID

                        int newQuizId = 0;
                        if (ddlTargetLesson.SelectedValue != "0")
                        {
                            string linkSql = "UPDATE learningResource SET quiz_id = @qid WHERE Id = @lid";
                            using (SqlCommand cmdLink = new SqlCommand(linkSql, con, transaction))
                            {
                                cmdLink.Parameters.AddWithValue("@qid", newQuizId);
                                cmdLink.Parameters.AddWithValue("@lid", ddlTargetLesson.SelectedValue);
                                cmdLink.ExecuteNonQuery();
                            }
                        }
                        using (SqlCommand cmd = new SqlCommand(quizSql, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@courseId", courseId);
                            cmd.Parameters.AddWithValue("@title", txtQuizTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@duration", duration);

                            newQuizId = (int)cmd.ExecuteScalar();
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
                            // Skip empty/blank questions that the user added but didn't fill out
                            if (string.IsNullOrWhiteSpace(q.QuestionText)) continue;

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
                                    cmdO.Parameters.AddWithValue("@optText", string.IsNullOrWhiteSpace(optText) ? "" : optText.Trim());
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

                        // Clear memory and redirect back to the Edit Course page
                        ViewState["QuizQuestions"] = null;
                        Response.Redirect($"~/Pages/Tutor/EditCourse.aspx?id={courseId}", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    catch (Exception ex)
                    {
                        // If anything broke, rollback all database changes so no corrupt data is left behind
                        transaction.Rollback();
                        lblMsg.Text = "Database Error: " + ex.Message;
                    }
                }
            }
        }
        private void UpdateQuizAndQuestions(int quizId)
        {
            lblMsg.ForeColor = System.Drawing.Color.Red;

            if (string.IsNullOrWhiteSpace(txtQuizTitle.Text) ||
    string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                lblMsg.Text = "Please fill in the Quiz Title and Description.";
                return;
            }
            int courseId = CourseId;
            int duration = Convert.ToInt32(ddlTimeLimit.SelectedValue);
            if (duration <= 0) duration = 9999;

            SaveCurrentRepeaterData();

            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    try
                    {
                        // 1) update quiz header
                        using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE quiz
                    SET title=@t, description=@d, duration_minutes=@dur
                    WHERE Id=@id", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@t", txtQuizTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@d", txtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@dur", duration);
                            cmd.Parameters.AddWithValue("@id", quizId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2) delete old options then questions
                        using (SqlCommand cmd = new SqlCommand(@"
                    DELETE ao
                    FROM answerOption ao
                    INNER JOIN question q ON ao.question_id = q.Id
                    WHERE q.quiz_id = @qid", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@qid", quizId);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand("DELETE FROM question WHERE quiz_id=@qid", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@qid", quizId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3) reinsert questions/options (same logic as create, but using existing quizId)
                        string questionSql = @"
                    INSERT INTO question (quiz_id, question_text)
                    VALUES (@quizId, @qText);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        string optionSql = @"
                    INSERT INTO answerOption (question_id, text, is_correct)
                    VALUES (@qId, @optText, @isCorrect);";

                        foreach (var q in QuizQuestions)
                        {
                            if (string.IsNullOrWhiteSpace(q.QuestionText)) continue;

                            int newQuestionId;
                            using (SqlCommand cmdQ = new SqlCommand(questionSql, con, tran))
                            {
                                cmdQ.Parameters.AddWithValue("@quizId", quizId);
                                cmdQ.Parameters.AddWithValue("@qText", q.QuestionText.Trim());
                                newQuestionId = (int)cmdQ.ExecuteScalar();
                            }

                            void InsertOption(string optText, bool isCorrect)
                            {
                                using (SqlCommand cmdO = new SqlCommand(optionSql, con, tran))
                                {
                                    cmdO.Parameters.AddWithValue("@qId", newQuestionId);
                                    cmdO.Parameters.AddWithValue("@optText", string.IsNullOrWhiteSpace(optText) ? "" : optText.Trim());
                                    cmdO.Parameters.AddWithValue("@isCorrect", isCorrect);
                                    cmdO.ExecuteNonQuery();
                                }
                            }

                            InsertOption(q.OptionA, q.CorrectAnswer == "A");
                            InsertOption(q.OptionB, q.CorrectAnswer == "B");
                            InsertOption(q.OptionC, q.CorrectAnswer == "C");
                            InsertOption(q.OptionD, q.CorrectAnswer == "D");
                        }

                        // --- STEP 4: Update Target Lesson Link ---
                        // 1. Clear old links to this quiz
                        using (SqlCommand cmdClear = new SqlCommand("UPDATE learningResource SET quiz_id = NULL WHERE quiz_id = @qid", con, tran))
                        {
                            cmdClear.Parameters.AddWithValue("@qid", quizId);
                            cmdClear.ExecuteNonQuery();
                        }

                        // 2. Set the new link (if they selected a lesson)
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

                        Response.Redirect($"~/Pages/Tutor/EditCourse.aspx?id={courseId}", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        lblMsg.Text = "Database Error: " + ex.Message;
                    }
                }
            }
        }

        private void LoadLessonDropDownList(int courseId)
        {
            string cs = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                string sql = "SELECT Id, title FROM learningResource WHERE course_id = @cid ORDER BY sequence_order";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@cid", courseId);
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
    }
}