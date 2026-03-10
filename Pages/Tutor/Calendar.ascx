<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Calendar.ascx.cs" Inherits="WAPP.Pages.Tutor.Calendar" %>

<style>
    /* Modernizing the ASP.NET Calendar */
    .modern-calendar {
        width: 100%;
        border-collapse: collapse;
        border: none !important;
        background-color: transparent;
    }
    .modern-calendar th {
        padding: 10px;
        color: #64748b;
        font-weight: 600;
        text-transform: uppercase;
        font-size: 0.75rem;
        border-bottom: 2px solid #e2e8f0;
    }
    .modern-calendar td {
        padding: 5px;
        height: 60px;
        vertical-align: top !important;
        border: 1px solid #f1f5f9;
        transition: background-color 0.2s;
    }
    .modern-calendar td:hover {
        background-color: #f8fafc;
    }
    .modern-calendar a {
        text-decoration: none;
        color: #334155;
        font-weight: 600;
        display: block;
        text-align: right;
        padding-right: 5px;
    }
    .appt-badge {
        display: block;
        font-size: 0.65rem;
        padding: 4px;
        margin-top: 4px;
        border-radius: 4px;
        cursor: pointer;
        text-align: left;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        transition: transform 0.1s;
    }
    .appt-badge:hover {
        transform: scale(1.02);
    }
</style>

<div class="calendar-body">
    <asp:Calendar ID="Calendar1" runat="server" 
        CssClass="modern-calendar" 
        SelectionMode="Day"
        ShowGridLines="False"
        OnDayRender="Calendar1_DayRender"
        NextPrevFormat="ShortMonth"
        DayNameFormat="Short">

        <TitleStyle BackColor="Transparent" Font-Bold="True" Font-Size="1.1em" ForeColor="#0d6efd" Height="40px" HorizontalAlign="Center" VerticalAlign="Middle" />
        <NextPrevStyle ForeColor="#0d6efd" Font-Bold="true" Font-Size="0.9em" VerticalAlign="Middle" />
        <OtherMonthDayStyle ForeColor="#cbd5e1" />
        <TodayDayStyle BackColor="#eff6ff" BorderColor="#0d6efd" BorderWidth="1px" />
    </asp:Calendar>
</div>

<div class="modal fade" id="apptDetailsModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered modal-sm">
        <div class="modal-content border-0 shadow">
            <div class="modal-header border-bottom-0 pb-0">
                <h5 class="modal-title fw-bold text-dark"><i class="bi bi-calendar-event me-2 text-primary"></i>Session Details</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="text-muted small fw-bold mb-1">Status</label>
                    <div id="modalApptStatus"></div>
                </div>
                <div class="mb-3">
                    <label class="text-muted small fw-bold mb-1">Student</label>
                    <div id="modalApptStudent" class="fw-bold text-dark"></div>
                </div>
                <div class="mb-3">
                    <label class="text-muted small fw-bold mb-1">Subject</label>
                    <div id="modalApptSubject" class="text-dark"></div>
                </div>
                <div>
                    <label class="text-muted small fw-bold mb-1">Time</label>
                    <div id="modalApptTime" class="text-primary fw-bold"></div>
                </div>
            </div>
            <div class="modal-footer border-top-0 pt-0">
                <a href="TutorAppointments.aspx" class="btn btn-sm btn-outline-primary w-100 rounded-pill fw-bold">Manage Appointments</a>
            </div>
        </div>
    </div>
</div>

<script>
    function showApptModal(event, student, subject, time, status) {
        event.stopPropagation(); 

        document.getElementById('modalApptStudent').innerText = student;
        document.getElementById('modalApptSubject').innerText = subject;
        document.getElementById('modalApptTime').innerText = time;
        
        var statusHtml = "";
        if (status === "APPROVED") {
            statusHtml = "<span class='badge bg-success'>Approved</span>";
        } else {
            statusHtml = "<span class='badge bg-warning text-dark'>Pending</span>";
        }
        document.getElementById('modalApptStatus').innerHTML = statusHtml;

        document.body.appendChild(document.getElementById('apptDetailsModal'));

        var myModal = new bootstrap.Modal(document.getElementById('apptDetailsModal'));
        myModal.show();
    }
</script>