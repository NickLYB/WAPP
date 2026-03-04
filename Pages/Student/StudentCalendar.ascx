<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StudentCalendar.ascx.cs" Inherits="WAPP.Pages.Student.StudentCalendar" %>

<div class="card calendar-card">
    <div class="calendar-body">
        <asp:Calendar ID="Calendar1" runat="server" 
            CssClass="ec-calendar" 
            Width="100%" 
            SelectionMode="Day"
            ShowGridLines="False">
    
            <TitleStyle BackColor="#CCCCCC" Font-Bold="True" Height="40px" HorizontalAlign="Center" VerticalAlign="Middle" />
            <DayStyle HorizontalAlign="Center" VerticalAlign="Middle" />
            <NextPrevStyle VerticalAlign="Middle" />
        </asp:Calendar>
    </div>
</div>