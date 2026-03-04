<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreateCourseModal.ascx.cs" Inherits="WAPP.Pages.Tutor.CreateCourseModal" %>
<div class="modal fade" id="createCourseModal" tabindex="-1" aria-labelledby="createCourseModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-centered"> <div class="modal-content">
            
            <div class="modal-header bg-light">
                <h5 class="modal-title" id="createCourseModalLabel" >CREATE NEW COURSE</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>

            <asp:UpdatePanel ID="upCreateCourse" runat="server">
                <ContentTemplate>
                    <div class="modal-body p-4">
                        
                        <div class="text-center mb-3">
                            <asp:Label ID="lblMsg" runat="server" ></asp:Label>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-center">
                            <asp:Label ID="lblEmpty" runat="server" Text="Course Title:" CssClass="form-label" />
                            <asp:TextBox ID="title" runat="server" CssClass="form-control flex-grow-1" required="true"></asp:TextBox>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-start">
                            <asp:Label ID="Label1" runat="server" Text="Course Description:" CssClass="form-label mt-1"  />
                            <asp:TextBox ID="description" runat="server" CssClass="form-control flex-grow-1" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-center">
                            <asp:Label ID="Label2" runat="server" Text="Course Type:" CssClass="form-label" />
                            <asp:DropDownList ID="type" runat="server" CssClass="form-select flex-grow-1" ></asp:DropDownList>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-center">
                            <asp:Label ID="Label3" runat="server" Text="Duration (Mins):" CssClass="form-label" />
                            <asp:TextBox ID="duration" runat="server" CssClass="form-control flex-grow-1" TextMode="Number"></asp:TextBox>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-center">
                            <asp:Label ID="Label4" runat="server" Text="Skill Level:" CssClass="form-label" />
                            <asp:DropDownList ID="skill" runat="server" CssClass="form-select flex-grow-1">
                                <asp:ListItem>-- Select Skill --</asp:ListItem>
                                <asp:ListItem>Beginner</asp:ListItem>
                                <asp:ListItem>Intermediate</asp:ListItem>
                                <asp:ListItem>Advanced</asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="form-row mb-3 d-flex align-items-center">
                            <asp:Label ID="Label5" runat="server" Text="Upload Image:" CssClass="form-label" />
                            <asp:FileUpload ID="FileUpload1" runat="server" CssClass="form-control flex-grow-1" />
                        </div>

                    </div>

                    <div class="modal-footer bg-light">
                        <asp:Button ID="Button2" runat="server" Text="Cancel" CssClass="btn btn-secondary" data-bs-dismiss="modal" OnClientClick="return false;" />
                        <asp:Button ID="Button1" runat="server" Text="Submit for Approval" OnClick="Button1_Click" CssClass="btn text-white" style="background-color: #a31d3d;" />
                    </div>
                </ContentTemplate>
                
                <Triggers>
                    <asp:PostBackTrigger ControlID="Button1" />
                </Triggers>

            </asp:UpdatePanel>
        </div>
    </div>
</div>