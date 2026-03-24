<%@ Page Title="Messages" Language="C#" MasterPageFile="~/Masters/Guest.Master" AutoEventWireup="true" CodeBehind="Chat.aspx.cs" Inherits="WAPP.Pages.Shared.Chat" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #videoCallModal .modal-content { background-color: #1a1d20; border-radius: 12px; }
        
        .video-wrapper {
            position: relative;
            width: 100%;
            height: 500px;
            background-color: #000;
            border-radius: 8px;
            overflow: hidden;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .video-main { width: 100%; height: 100%; object-fit: cover; display: none; }

        .video-local-pip {
            position: absolute;
            bottom: 20px;
            right: 20px;
            width: 130px;
            height: 180px;
            object-fit: cover;
            border: 2px solid #fff;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.5);
            z-index: 10;
            background: #222;
        }

        .video-remote-pip {
            position: absolute;
            bottom: 20px;
            left: 20px;
            width: 130px;
            height: 180px;
            object-fit: cover;
            border: 2px solid #0d6efd;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.5);
            z-index: 10;
            background: #222;
            display: none;
        }

        .call-status-overlay {
            position: absolute;
            color: white;
            font-size: 1.2rem;
            text-align: center;
            z-index: 5;
        }

        /* --- DARK MODE SPECIFIC OVERRIDES --- */
        
        /* Chat Container Background */
        html.dark-mode .ec-chat-layout {
            background: #1e1e1e !important;
            border-color: #333 !important;
        }

        /* Sidebar Styling */
        html.dark-mode .ec-chat-sidebar {
            background: #1a1a1a !important;
            border-right-color: #333 !important;
        }

        html.dark-mode .ec-chat-sidebar .bg-white {
            background-color: #1a1a1a !important;
            border-bottom-color: #333 !important;
        }

        /* Contact List Items */
        html.dark-mode .ec-chat-contact {
            border-bottom-color: #333 !important;
            color: #e2e8f0 !important;
        }

        html.dark-mode .ec-chat-contact:hover {
            background-color: #2d2d2d !important;
        }

        html.dark-mode .ec-chat-contact.active {
            background-color: #2d2d2d !important;
            color: var(--ec-primary) !important;
        }

        /* Chat Window Header & Footer */
        html.dark-mode .ec-chat-window {
            background-color: #1e1e1e !important;
        }

        html.dark-mode .ec-chat-window .bg-white {
            background-color: #1e1e1e !important;
            border-bottom-color: #333 !important;
        }

        html.dark-mode .ec-chat-footer {
            background-color: #1e1e1e !important;
            border-top-color: #333 !important;
        }

        /* Message Bubbles */
        html.dark-mode .received .ec-msg-bubble {
            background-color: #333 !important;
            color: #ffffff !important;
        }

        html.dark-mode .sent .ec-msg-bubble {
            background-color: var(--ec-primary) !important;
            color: #ffffff !important;
        }

        /* Text Fixes */
        html.dark-mode .text-dark, 
        html.dark-mode #lblChatHeader {
            color: #ffffff !important;
        }

        html.dark-mode .text-muted {
            color: #a1a1aa !important;
        }

        /* Input Field Fixes */
        html.dark-mode .login-input {
            background-color: #2d2d2d !important;
            border-color: #444 !important;
            color: #ffffff !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ec-content-wrapper">
        <div class="ec-item-gap">
            <asp:SiteMapPath ID="SiteMapPath1" runat="server" 
                PathSeparator=" > " CssClass="small text-muted text-decoration-none" RenderCurrentNodeAsLink="false" />
        </div>
        <div class="ec-section-gap">
            <h1 class="ec-page-title">Messages</h1>
            <p class="ec-page-subtitle">Connect and collaborate in real-time.</p>
        </div>

        <div class="ec-chat-layout">
            
            <div class="ec-chat-sidebar">
                <div class="p-3 border-bottom bg-white">
                    <asp:TextBox ID="txtSearch" runat="server" ClientIDMode="Static" CssClass="login-input m-0" 
                        placeholder="Search users..." style="padding: 10px 15px; font-size: 0.9rem;"
                        onkeyup="handleLiveSearch(event)"></asp:TextBox>
                </div>
    
                <div class="ec-chat-list">
                    <asp:UpdatePanel ID="upContacts" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Button ID="btnSearchTrigger" runat="server" ClientIDMode="Static" OnClick="btnSearchTrigger_Click" style="display:none;" />
                            <asp:Button ID="btnRefreshSidebar" runat="server" ClientIDMode="Static" OnClick="btnRefreshSidebar_Click" style="display:none;" />
            
                            <asp:Repeater ID="rptContacts" runat="server" OnItemCommand="rptContacts_ItemCommand">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkContact" runat="server" 
                                        CommandName="SelectContact" 
                                        CommandArgument='<%# Eval("UserId") + "|" + Eval("UserName") %>'
                                        CssClass='<%# Convert.ToInt32(Eval("UserId")) == ActiveContactId ? "ec-chat-contact active" : "ec-chat-contact" %>'>
                        
                                        <div class="d-flex justify-content-between align-items-center w-100">
                                            <div>
                                                <div class="small fw-bold"><%# Eval("UserName") %></div>
                                                <small class="text-muted">Tap to message</small>
                                            </div>
                            
                                            <asp:Label runat="server" 
                                                Visible='<%# Convert.ToInt32(Eval("UnreadCount")) > 0 %>' 
                                                CssClass="badge bg-danger rounded-pill">
                                                <%# Eval("UnreadCount") %>
                                            </asp:Label>
                                        </div>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:Repeater>
            
                            <asp:Label ID="lblNoContacts" runat="server" Visible="false" CssClass="text-muted small d-block text-center mt-4">
                                No conversations found.
                            </asp:Label>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>

            <div class="ec-chat-window">
                <asp:UpdatePanel ID="upChatArea" runat="server" UpdateMode="Conditional" style="height: 100%; display: flex; flex-direction: column;">
                    <ContentTemplate>
                        <asp:HiddenField ID="hfMyId" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hfCurrentContactId" runat="server" ClientIDMode="Static" />
                        <asp:Button ID="btnRefreshChat" runat="server" ClientIDMode="Static" OnClick="btnRefreshChat_Click" style="display:none;" />

                        <div class="p-3 border-bottom bg-white d-flex justify-content-between align-items-center">
                            <h6 class="fw-bold m-0 text-dark">
                                <i class="bi bi-person-circle me-2 text-primary"></i>
                                <asp:Label ID="lblChatHeader" runat="server" Text="SELECT A CONVERSATION"></asp:Label>
                            </h6>
                            
                            <button type="button" id="btnStartCall" class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold" onclick="startCall()" style="display:none;">
                                <i class="bi bi-camera-video-fill me-1"></i> Video Call
                            </button>
                        </div>

                        <div class="ec-chat-messages" id="chatMessagesContainer">
                            <asp:Repeater ID="rptMessages" runat="server">
                                <ItemTemplate>
                                    <div class='<%# Convert.ToInt32(Eval("sender_id")) == LoggedInUserId ? "ec-msg-wrapper sent" : "ec-msg-wrapper received" %>'>
                                        <div class="ec-msg-bubble shadow-sm">
                                            <%# Eval("message_text") %>
                                        </div>
                                        <small class="text-muted mt-1" style="font-size: 0.7rem;">
                                            <%# Eval("SenderName") %> • <%# Convert.ToDateTime(Eval("created_at")).ToString("MMM dd, hh:mm tt") %>
                                        </small>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:PlaceHolder ID="phNoMessages" runat="server" Visible="false">
                                <div class="ec-empty-state">
                                    <i class="bi bi-chat-dots"></i>
                                    <p>Start a conversation with this user.</p>
                                </div>
                            </asp:PlaceHolder>
                        </div>

                            <div class="ec-chat-footer d-flex w-100">
                                <asp:TextBox ID="txtMessage" runat="server" ClientIDMode="Static" 
                                    CssClass="login-input m-0 flex-grow-1" placeholder="Write your message..." 
                                    Enabled="false" 
                                    onkeydown="if(event.keyCode===13){ event.preventDefault(); document.getElementById('btnSend').click(); return false; }"></asp:TextBox>
        
                                <asp:Button ID="btnSend" runat="server" ClientIDMode="Static" Text="Send" 
                                    CssClass="btn-main btn-pill px-4 ms-2" OnClick="btnSend_Click" 
                                    Enabled="false" UseSubmitBehavior="false" />
                            </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

        </div>
    </div>

    <div class="modal fade" id="videoCallModal" data-bs-backdrop="static" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content border-0 shadow-lg">
                <div class="modal-header border-bottom-0 pb-0">
                    <h5 class="modal-title fw-bold text-white">Video Call</h5>
                    <button type="button" class="btn-close btn-close-white" onclick="endCall()"></button>
                </div>
                <div class="modal-body">
                    
                    <div class="video-wrapper">
                        <div class="call-status-overlay" id="videoCallOverlay">
                            <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;"></div>
                            <div id="videoCallStatusText" class="fw-bold">Calling partner...</div>
                        </div>

                        <video id="mainVideo" class="video-main" autoplay playsinline></video>
    
                        <video id="localVideo" class="video-local-pip" autoplay playsinline muted></video>

                        <video id="remotePipVideo" class="video-remote-pip" autoplay playsinline></video>
                    </div>

                </div>
                <div class="modal-footer border-top-0 justify-content-center pt-0 pb-4">
                    <button type="button" id="btnShareScreen" class="btn btn-secondary rounded-pill px-4 fw-bold me-2" onclick="toggleScreenShare()">
                        <i class="bi bi-display me-2"></i> Share Screen
                    </button>
                    <button type="button" class="btn btn-danger rounded-pill px-5 fw-bold" onclick="endCall()">
                        <i class="bi bi-telephone-x-fill me-2"></i> End Call
                    </button>
                </div>
            </div>
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="../../Scripts/jquery.signalR-2.4.3.min.js"></script>
    <script src="../../signalr/hubs"></script>

<script type="text/javascript">
    let typingTimer;
    let chatHub;

    // WebRTC Variables
    let peerConnection;
    let localStream;
    let screenStream = null;
    let isScreenSharing = false;
    let iceQueue = [];
    let currentCallPartnerId = null;
    let myIceServers = null;

    // Track multiple streams
    let remoteCamStream = null;
    let screenSender = null;

    // Cache UI Elements
    const uiMainVideo = document.getElementById('mainVideo');
    const uiLocalVideo = document.getElementById('localVideo');
    const uiRemotePipVideo = document.getElementById('remotePipVideo');
    const uiVideoOverlay = document.getElementById('videoCallOverlay');
    const uiCallStatusText = document.getElementById('videoCallStatusText');
    const btnShareScreen = $('#btnShareScreen');

    $.connection.hub.qs = { "ngrok-skip-browser-warning": "true" };
    $.ajaxSetup({ headers: { 'ngrok-skip-browser-warning': 'true' } });

    $(function () {
        fetch("https://wapp.metered.live/api/v1/turn/credentials?apiKey=94efd7cde7e88d15b9cbd72b03e8e8131147")
            .then(response => response.json())
            .then(data => { myIceServers = data; })
            .catch(err => console.error("Error fetching TURN:", err));

        chatHub = $.connection.chatHub;

        chatHub.client.receiveNewMessage = function (senderId, receiverId) {
            const myId = $('#hfMyId').val();
            const currentContactId = $('#hfCurrentContactId').val();

            //C# btnSend_Click method is already refreshing my screen.
            if (senderId === myId) {
                return;
            }

            // Only click the refresh buttons if the OTHER person sent a message
            if (receiverId === myId && senderId === currentContactId) {
                $('#btnRefreshChat').click();
            } else if (receiverId === myId) {
                $('#btnRefreshSidebar').click();
            }
        };

        chatHub.client.receiveVideoSignal = async function (senderId, receiverId, dataJson) {
            const myId = $('#hfMyId').val();
            if (myId != receiverId) return;

            const signal = JSON.parse(dataJson);
            currentCallPartnerId = senderId;

            if (signal.type === 'call_ended') {
                endCall(true); // Pass true so we know the partner ended it
                return;
            }

            if (signal.type === 'screen_share_started') {
                btnShareScreen.prop('disabled', true)
                    .removeClass('btn-secondary').addClass('btn-outline-secondary')
                    .html('<i class="bi bi-display me-2"></i> Partner Sharing...');
                return;
            } else if (signal.type === 'screen_share_stopped') {
                btnShareScreen.prop('disabled', false)
                    .removeClass('btn-outline-secondary').addClass('btn-secondary')
                    .html('<i class="bi bi-display me-2"></i> Share Screen');
                return;
            }

            if (signal.sdp && signal.sdp.type === 'offer') {
                if (!peerConnection) {
                    if (!confirm("Incoming video call! Do you want to answer?")) return;

                    $('#videoCallModal').modal('show');
                    resetVideoUI();
                    uiCallStatusText.innerText = "Connecting...";

                    const camReady = await setupCamera();
                    if (!camReady) return;

                    createPeerConnection();
                }

                await peerConnection.setRemoteDescription(new RTCSessionDescription(signal.sdp));
                const answer = await peerConnection.createAnswer();
                await peerConnection.setLocalDescription(answer);
                sendSignal({ sdp: peerConnection.localDescription });

                processIceQueue();
            } else if (signal.sdp && signal.sdp.type === 'answer') {
                await peerConnection.setRemoteDescription(new RTCSessionDescription(signal.sdp));
                processIceQueue();
            } else if (signal.ice) {
                if (peerConnection && peerConnection.remoteDescription) {
                    await peerConnection.addIceCandidate(new RTCIceCandidate(signal.ice));
                } else {
                    iceQueue.push(signal.ice);
                }
            }
        };

        $.connection.hub.start().done(function () { console.log("Chat Connected!"); });
    });


    //video function
    //setup
    function resetVideoUI() {
        uiMainVideo.style.display = 'none';
        uiRemotePipVideo.style.display = 'none';
        uiVideoOverlay.style.display = 'block';

        // Reset the spinner and text in case it was changed to "Call Ended" previously
        uiVideoOverlay.innerHTML = `
            <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;" id="loadingSpinner"></div>
            <div id="videoCallStatusText" class="fw-bold">Calling partner...</div>
        `;

        btnShareScreen.html('<i class="bi bi-display me-2"></i> Share Screen');
    }
    async function setupCamera() {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("Camera API blocked! Make sure you are using HTTPS.");
            return false;
        }
        try {
            const constraints = { video: { width: 640, height: 480, frameRate: 15 }, audio: true };
            localStream = await navigator.mediaDevices.getUserMedia(constraints);
            uiLocalVideo.srcObject = localStream;
            return true;
        } catch (err) {
            alert("Error accessing camera: " + err.message);
            return false;
        }
    }
    function createPeerConnection() {
        const rtcConfig = myIceServers
            ? { iceServers: myIceServers }
            : { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] };

        peerConnection = new RTCPeerConnection(rtcConfig);

        peerConnection.onnegotiationneeded = async () => {
            try {
                const offer = await peerConnection.createOffer();
                await peerConnection.setLocalDescription(offer);
                sendSignal({ sdp: peerConnection.localDescription });
            } catch (err) {
                console.error("Renegotiation error:", err);
            }
        };

        peerConnection.oniceconnectionstatechange = function () {
            if (peerConnection && (peerConnection.iceConnectionState === "disconnected" || peerConnection.iceConnectionState === "failed")) {
                // If WebRTC detects a drop before SignalR does, end the call
                endCall(true, "Connection lost.");
            }
        };

        if (localStream) {
            localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));
        }

        peerConnection.ontrack = event => {
            uiVideoOverlay.style.display = 'none';
            uiMainVideo.style.display = 'block';

            const incomingStream = event.streams[0];

            if (!remoteCamStream) {
                remoteCamStream = incomingStream;
                uiMainVideo.srcObject = remoteCamStream;
            }
            else if (incomingStream.id !== remoteCamStream.id && event.track.kind === 'video') {
                uiRemotePipVideo.srcObject = remoteCamStream;
                uiRemotePipVideo.style.display = 'block';
                uiMainVideo.srcObject = incomingStream;

                event.track.onmute = () => {
                    uiMainVideo.srcObject = remoteCamStream;
                    uiRemotePipVideo.style.display = 'none';
                    uiRemotePipVideo.srcObject = null;
                };
            }
        };

        peerConnection.onicecandidate = event => {
            if (event.candidate) sendSignal({ ice: event.candidate });
        };
    }
    async function processIceQueue() {
        while (iceQueue.length > 0) {
            await peerConnection.addIceCandidate(new RTCIceCandidate(iceQueue.shift()));
        }
    }
    function sendSignal(data) {
        const myId = $('#hfMyId').val();
        chatHub.server.sendVideoSignal(myId, currentCallPartnerId, JSON.stringify(data));
    }
    //start end call
    async function startCall() {
        const partnerId = $('#hfCurrentContactId').val();
        if (!partnerId || partnerId == "0") return;

        currentCallPartnerId = partnerId;

        $('#videoCallModal').modal('show');
        resetVideoUI();

        const camReady = await setupCamera();
        if (!camReady) {
            $('#videoCallModal').modal('hide');
            return;
        }

        createPeerConnection();
    }
    function endCall(isRemote = false, customMessage = null) {
        if (!peerConnection && !localStream) return; // Prevent double firing

        // 1. Tell the other person instantly if WE pressed the button
        if (!isRemote) {
            sendSignal({ type: 'call_ended' });
        }

        // 2. Shut off hardware and pipes immediately so video doesn't freeze
        if (isScreenSharing) stopScreenShare();

        if (peerConnection) {
            peerConnection.close();
            peerConnection = null;
        }
        if (localStream) {
            localStream.getTracks().forEach(track => track.stop());
            localStream = null;
        }

        iceQueue = [];
        remoteCamStream = null;
        screenSender = null;

        // 3. Clear the videos from the screen instantly
        uiMainVideo.style.display = 'none';
        uiRemotePipVideo.style.display = 'none';
        uiLocalVideo.srcObject = null;
        uiMainVideo.srcObject = null;
        uiRemotePipVideo.srcObject = null;

        // 4. Show the clean "Call Ended" UI
        uiVideoOverlay.style.display = 'block';

        // Remove the loading spinner, show only text
        const spinner = document.getElementById('loadingSpinner');
        if (spinner) spinner.style.display = 'none';

        const statusText = document.getElementById('videoCallStatusText');
        if (customMessage) {
            statusText.innerText = customMessage;
        } else {
            statusText.innerText = isRemote ? "Partner ended the call." : "Call ended.";
        }

        // Reset Share Button State for the next call
        btnShareScreen.prop('disabled', false)
            .removeClass('btn-outline-secondary').addClass('btn-secondary')
            .html('<i class="bi bi-display me-2"></i> Share Screen');

        // 5. Wait 2 seconds, then close the modal
        setTimeout(() => {
            $('#videoCallModal').modal('hide');
            currentCallPartnerId = null;
        }, 2000);
    }
    //button
    function checkCallButtonVisibility() {
        const partnerId = $('#hfCurrentContactId').val();
        if (partnerId && partnerId != "0" && partnerId != "") {
            $('#btnStartCall').show();
        } else {
            $('#btnStartCall').hide();
        }
    }
    //share screen
    async function toggleScreenShare() {
        if (!peerConnection) return;

        if (!isScreenSharing) {
            try {
                screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
                const screenTrack = screenStream.getVideoTracks()[0];

                screenSender = peerConnection.addTrack(screenTrack, screenStream);
                isScreenSharing = true;

                uiRemotePipVideo.srcObject = uiMainVideo.srcObject;
                uiRemotePipVideo.style.display = 'block';
                uiMainVideo.srcObject = screenStream;

                btnShareScreen.html('<i class="bi bi-camera-video me-2"></i> Stop Sharing');

                sendSignal({ type: 'screen_share_started' });

                screenTrack.onended = function () {
                    stopScreenShare();
                };
            } catch (err) {
                console.error("Error sharing screen:", err);
            }
        } else {
            stopScreenShare();
        }
    }
    function stopScreenShare() {
        if (!isScreenSharing) return;

        if (screenSender) {
            peerConnection.removeTrack(screenSender);
            screenSender = null;
        }

        if (screenStream) {
            screenStream.getTracks().forEach(track => track.stop());
            screenStream = null;
        }

        isScreenSharing = false;
        btnShareScreen.html('<i class="bi bi-display me-2"></i> Share Screen');

        uiMainVideo.srcObject = uiRemotePipVideo.srcObject;
        uiRemotePipVideo.style.display = 'none';
        uiRemotePipVideo.srcObject = null;

        sendSignal({ type: 'screen_share_stopped' });
    }
   
    //chat function
    function handleLiveSearch(event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            clearTimeout(typingTimer);
            document.getElementById('btnSearchTrigger').click();
            return false;
        }
        const key = event.keyCode || event.charCode;
        if (key >= 37 && key <= 40) return;

        clearTimeout(typingTimer);
        typingTimer = setTimeout(function () {
            document.getElementById('btnSearchTrigger').click();
        }, 400);
    }
    function scrollToBottom() {
        const chatContainer = document.getElementById("chatMessagesContainer");
        if (chatContainer) { chatContainer.scrollTop = chatContainer.scrollHeight; }
    }
    document.addEventListener("DOMContentLoaded", function () {
        scrollToBottom();
        checkCallButtonVisibility();
    });

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            scrollToBottom();
            checkCallButtonVisibility();
        });
    }
</script>
</asp:Content>