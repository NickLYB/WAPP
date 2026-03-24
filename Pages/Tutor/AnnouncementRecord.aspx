<%@ Page Title="Announcement History" Language="C#" MasterPageFile="~/Masters/Tutor.Master" AutoEventWireup="true" CodeBehind="AnnouncementRecord.aspx.cs" Inherits="WAPP.Pages.Tutor.AnnouncementRecord" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        
        <div class="ec-section-gap">
            <div class="ec-item-gap">
                <asp:SiteMapPath ID="SiteMapPath1" runat="server" SiteMapProvider="TutorMap" 
                    PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
            </div>
            
            <div class="ec-section-header border-0">
                <h1 class="ec-page-title m-0">Announcement History</h1>
            </div>
            <p class="ec-page-subtitle">Review all notifications and updates you've shared with your students.</p>
        </div>

        <div class="ec-section-gap">
            <asp:Label ID="lblMsg" runat="server" CssClass="d-block mb-3 fw-bold text-primary"></asp:Label>
            <div class="row mb-4">
                <div class="col-md-5 mb-3 mb-md-0">
                    <div class="input-group">
                        <span class="input-group-text bg-white border-end-0">
                            <i class="bi bi-search text-muted"></i>
                        </span>
                        <input type="text" id="liveSearchInput" class="form-control border-start-0" 
                            placeholder="Search titles or messages..." onkeyup="filterAnnouncements()" />
                    </div>
                </div>
                <div class="col-md-4 mb-3 mb-md-0">
                    <div class="input-group">
                        <span class="input-group-text bg-white">
                            <i class="bi bi-calendar-date text-muted"></i>
                        </span>
                        <input type="date" id="dateFilterInput" class="form-control" onchange="filterAnnouncements()" />
                    </div>
                </div>
                <div class="col-md-3">
                    <button type="button" class="btn btn-outline-secondary w-100" onclick="clearFilters()">
                        <i class="bi bi-x-circle me-1"></i> Clear Filters
                    </button>
                </div>
            </div>
            <asp:Repeater ID="rptAnnouncements" runat="server" OnItemCommand="rptAnnouncements_ItemCommand">
                <ItemTemplate>
                    <div class="ec-glass-card mb-4 announcement-item" data-date='<%# Eval("created_at", "{0:yyyy-MM-dd}") %>'>
        
                        <div class="ec-glass-card-header">
                            <div>
                                <h5 class="fw-bold text-dark m-0 announcement-title"><%# Eval("title") %></h5>
                                <div class="mt-2 d-flex align-items-center gap-3">
                                    <small class="text-muted">
                                        <i class="bi bi-calendar3 me-1"></i> <%# Eval("created_at", "{0:MMM dd, yyyy}") %>
                                    </small>
                                    <small class="text-muted">
                                        <i class="bi bi-clock me-1"></i> <%# Eval("created_at", "{0:h:mm tt}") %>
                                    </small>
                                    <span class='ec-status-pill <%# Eval("course_title") == DBNull.Value ? "ec-status-locked" : "ec-status-active" %>'>
                                        <i class='bi <%# Eval("course_title") == DBNull.Value ? "bi-globe" : "bi-book" %> me-1'></i>
                                        <%# Eval("course_title") == DBNull.Value ? "General Announcement" : "Course: " + Eval("course_title") %>
                                    </span>
                                </div>
                            </div>

                            <asp:LinkButton ID="btnArchive" runat="server" 
                                CommandName="Archive" 
                                CommandArgument='<%# Eval("Id") %>'
                                CssClass="text-danger text-decoration-none small fw-bold"
                                OnClientClick="return confirm('Are you sure you want to delete this announcement?');">
                                <i class="bi bi-trash3-fill"></i> Delete
                            </asp:LinkButton>
                        </div>

                        <div class="ec-glass-card-body">
                            <div class="p-3 bg-light rounded-3 text-dark announcement-message" style="white-space: pre-wrap; line-height: 1.6; border: 1px solid #f1f5f9;">
                                <%# Eval("message") %>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
                <div class="ec-empty-state">
                    <i class="bi bi-chat-left-dots"></i>
                    <h5 class="fw-bold text-dark">No history found</h5>
                    <p>When you post announcements, they will appear here for your reference.</p>
                </div>
            </asp:PlaceHolder>
            <asp:Label ID="lblEmpty" runat="server" Visible="false" CssClass="d-none"></asp:Label>
        </div>

    </div>
    <script type="text/javascript">
    function filterAnnouncements() {
        // Get filter values
        let searchText = document.getElementById('liveSearchInput').value.toLowerCase();
        let selectedDate = document.getElementById('dateFilterInput').value; // Format: yyyy-mm-dd
        
        // Get all announcement cards
        let announcements = document.querySelectorAll('.announcement-item');

        announcements.forEach(function(card) {
            // Get data from the card
            let titleText = card.querySelector('.announcement-title').innerText.toLowerCase();
            let messageText = card.querySelector('.announcement-message').innerText.toLowerCase();
            let cardDate = card.getAttribute('data-date'); // Format: yyyy-mm-dd

            // Check if it matches search text (either in title or message)
            let matchesSearch = titleText.includes(searchText) || messageText.includes(searchText);
            
            // Check if it matches date (or if no date is selected)
            let matchesDate = (selectedDate === "") || (cardDate === selectedDate);

            // Show or hide based on conditions
            if (matchesSearch && matchesDate) {
                card.style.display = "";
            } else {
                card.style.display = "none";
            }
        });
    }

    function clearFilters() {
        document.getElementById('liveSearchInput').value = "";
        document.getElementById('dateFilterInput').value = "";
        filterAnnouncements(); // Re-run to show all items
    }
    </script>
</asp:Content>