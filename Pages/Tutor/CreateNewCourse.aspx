<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="CreateNewCourse.aspx.cs" Inherits="WAPP.Pages.Tutor.CreateNewCourse" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        <div class="content-wrapper">
        <div class="course-panel">
            <div class="course-panel-header">
                <h1 class="course-panel-title">CREATE NEW COURSE</h1>
            </div>
            <div class="course-panel-body">

                <div class="form-row">
                    <asp:Label ID="lblEmpty" runat="server" Text="Course Title:" CssClass="form-label" />
                    <asp:TextBox ID="title" runat="server" CssClass="form-input"></asp:TextBox>
                </div>

                <div class="form-row">
                    <asp:Label ID="Label1" runat="server" Text="Course Description:" CssClass="form-label" />
                    <asp:TextBox ID="description" runat="server" CssClass="form-input" TextMode="MultiLine" Rows="3"></asp:TextBox>
                </div>

                <div class="form-row">
                    <asp:Label ID="Label2" runat="server" Text="Course Type:" CssClass="form-label" />
                    <asp:DropDownList ID="type" runat="server" CssClass="form-input" ></asp:DropDownList>
                </div>

                <div class="form-row">
                    <asp:Label ID="Label3" runat="server" Text="Duration:" CssClass="form-label" />
                    <asp:TextBox ID="duration" runat="server" CssClass="form-input"></asp:TextBox>
                </div>

                <div class="form-row">
                    <asp:Label ID="Label4" runat="server" Text="Skill Level:" CssClass="form-label" />
                    <asp:DropDownList ID="skill" runat="server" CssClass="form-input">
                        <asp:ListItem>-- Select Skill --</asp:ListItem>
                        <asp:ListItem>Beginner</asp:ListItem>
                        <asp:ListItem>Intermediate</asp:ListItem>
                        <asp:ListItem>Advanced</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="form-row">
                    <asp:Label ID="Label5" runat="server" Text="Upload Course Image:" CssClass="form-label" />
                    <asp:FileUpload ID="FileUpload1" runat="server" CssClass="form-input" />
                </div>

            </div>

            <div class="course-panel-footer">
                <asp:Button ID="Button1" runat="server" Text="Submit for Approval" OnClick="Button1_Click" CssClass="btn-create"/>
                 <asp:Button ID="Button2" runat="server" Text="Cancel" CssClass="btn-create" OnClick="Button2_Click"/>
                <asp:Label ID="lblMsg" runat="server" Text="[errMessage]" ></asp:Label>
            </div>
        </div>

    </div>
</asp:Content>
