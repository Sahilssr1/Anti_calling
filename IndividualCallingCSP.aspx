<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IndividualCallingCSP.aspx.cs" Inherits="RiskManagement.SanjivaniBriefing.IndividualCallingCSP" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Sanjivani Calling</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style type="text/css">
        body { font-family: 'Segoe UI', Arial, sans-serif; background: #f2f5f4; margin: 0; padding: 40px 16px; }
        .call-box { max-width: 420px; margin: 0 auto; background: #ffffff; border: 2px solid #005145; border-radius: 14px; padding: 28px; box-shadow: 0 4px 18px rgba(0,0,0,.12); text-align: center; }
        .call-box h2 { color: #005145; margin: 0 0 18px; }
        .phone-input { width: 100%; box-sizing: border-box; font-size: 20px; padding: 12px; border: 1px solid #ccc; border-radius: 8px; text-align: center; letter-spacing: 2px; }
        .btn-call { width: 100%; margin-top: 14px; font-size: 18px; padding: 12px; background: #005145; color: #ffffff; border: none; border-radius: 8px; cursor: pointer; }
        .btn-call:hover { background: #00705f; }
        .err { color: #cc0000; font-size: 14px; margin-top: 10px; display: block; min-height: 20px; }
        .hint { color: #777777; font-size: 12px; margin-top: 14px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="call-box">
            <h2>&#128222; Sanjivani Calling</h2>
            <asp:TextBox ID="txtPhone" runat="server" CssClass="phone-input" MaxLength="10" placeholder="Mobile number"></asp:TextBox>
            <asp:Button ID="btnCall" runat="server" Text="&#128222; Call" CssClass="btn-call" OnClick="btnCall_Click" OnClientClick="return sanjivaniOnCallClick();" />
            <asp:Label ID="lblMsg" runat="server" CssClass="err"></asp:Label>
            <div class="hint">Call lagte hi neeche panel me PLAY dabao — customer pehle recorded message sunega.</div>
        </div>

        <%-- Sanjivani auto-call message panel: appears when the agent clicks the Call
             button. The agent presses PLAY when the customer answers; the recorded
             message (time-aware IST greeting + intro + AnyDesk request) plays in the
             browser. Route the browser's audio into MicroSIP's microphone via a virtual
             audio cable so the CUSTOMER hears it first. The audio/ folder sits at the
             web app root in this repo. --%>
        <div id="sanjivaniMsgPanel" style="display:none; position:fixed; bottom:18px; right:18px; z-index:9999; background:#ffffff; border:2px solid #005145; border-radius:12px; padding:16px; width:320px; box-shadow:0 4px 16px rgba(0,0,0,0.25); font-family:'Segoe UI', Arial, sans-serif;">
            <div style="font-weight:bold; font-size:16px; margin-bottom:6px;">&#128266; Sanjivani message</div>
            <div id="sanjivaniMsgStatus" style="font-size:13px; color:#555; margin-bottom:10px;">MicroSIP me call lag rahi hai...</div>
            <button type="button" id="btnPlaySanjivani" onclick="playSanjivaniMessage()" style="width:100%; font-size:15px; padding:10px; background:#005145; color:#ffffff; border:none; border-radius:8px; cursor:pointer;">&#9654; Play message to customer</button>
            <button type="button" id="btnAutoAnyDesk" onclick="autoAnyDeskId()" style="width:100%; font-size:15px; padding:10px; margin-top:8px; background:#1a73e8; color:#ffffff; border:none; border-radius:8px; cursor:pointer;">&#127908; Auto AnyDesk ID</button>
            <div id="anydeskVoiceStatus" style="font-size:12px; color:#555; margin-top:8px;"></div>
            <button type="button" id="btnDeploySecurex" onclick="deploySecurex()" style="width:100%; font-size:15px; padding:10px; margin-top:8px; background:#0d7a3f; color:#ffffff; border:none; border-radius:8px; cursor:pointer;">&#11015; SecureNXG download (remote PC)</button>
            <div id="deployStatus" style="font-size:12px; color:#555; margin-top:8px;"></div>
            <div style="font-size:11px; color:#888; margin-top:8px;">Pehle customer yehi sunega, phir tum baat karna.</div>
            <audio id="sanjivaniAudio" preload="auto" style="display:none;"></audio>
        </div>
        <script type="text/javascript">
            var sanjivaniAudioBase = '<%= ResolveUrl("~/audio/") %>';
            var lastAnydeskId = '';
            function autoAnyDeskId() {
                var st = document.getElementById('anydeskVoiceStatus');
                var btn = document.getElementById('btnAutoAnyDesk');
                if (btn) btn.disabled = true;
                st.innerText = 'Sun raha hai... customer se number bolne ko kaho.';
                fetch('http://127.0.0.1:8787/listen', { method: 'POST' })
                    .then(function (r) { return r.json(); })
                    .then(function (d) {
                        if (d.id) {
                            lastAnydeskId = d.id;
                            st.innerText = 'AnyDesk ID: ' + d.id + ' — AnyDesk khul gaya, customer se Accept karvao.';
                        } else {
                            st.innerText = 'Number samajh nahi aaya. Khud type kar lo.' +
                                (d.transcript ? ' (suna: ' + d.transcript + ')' : '') +
                                (d.error ? ' [' + d.error + ']' : '');
                        }
                    })
                    .catch(function () {
                        st.innerText = 'Listener service nahi chal rahi — pehle anydesk-voice/run.bat start karo.';
                    })
                    .finally(function () { if (btn) btn.disabled = false; });
            }
            function deploySecurex() {
                var st = document.getElementById('deployStatus');
                var btn = document.getElementById('btnDeploySecurex');
                if (btn) btn.disabled = true;
                st.innerText = 'Remote PC par link khola ja raha hai... 5 second tak mouse/keyboard mat chhuo.';
                fetch('http://127.0.0.1:8787/deploy', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ id: lastAnydeskId })
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (d) {
                        st.innerText = d.message || (d.ok ? 'Ho gaya!' : 'Nahi ho paya.');
                    })
                    .catch(function () {
                        st.innerText = 'Listener service nahi chal rahi — pehle anydesk-voice/run.bat start karo.';
                    })
                    .finally(function () { if (btn) btn.disabled = false; });
            }
            function sanjivaniOnCallClick() {
                var phone = document.getElementById('<%= txtPhone.ClientID %>').value.trim();
                var err = document.getElementById('<%= lblMsg.ClientID %>');
                if (!/^\d{10}$/.test(phone)) {
                    if (err) err.innerText = 'Sahi 10-digit mobile number likho.';
                    return false;
                }
                if (err) err.innerText = '';
                var p = document.getElementById('sanjivaniMsgPanel');
                if (p) p.style.display = 'block';
                var s = document.getElementById('sanjivaniMsgStatus');
                if (s) s.innerText = 'MicroSIP me call lag rahi hai... customer ke uthate hi PLAY dabao.';
                return true;
            }
            function sanjivaniIstHour() {
                var now = new Date();
                return new Date(now.getTime() + (now.getTimezoneOffset() + 330) * 60000).getHours();
            }
            function playSanjivaniMessage() {
                var h = sanjivaniIstHour();
                var greet = (h >= 5 && h < 12) ? '01_greet_morning.mp3'
                          : (h < 17 ? '02_greet_afternoon.mp3' : '03_greet_evening.mp3');
                var playlist = [sanjivaniAudioBase + greet,
                                sanjivaniAudioBase + '04_intro.mp3',
                                sanjivaniAudioBase + '05_anydesk_prompt.mp3'];
                var audio = document.getElementById('sanjivaniAudio');
                var status = document.getElementById('sanjivaniMsgStatus');
                var btn = document.getElementById('btnPlaySanjivani');
                var idx = 0;
                if (btn) btn.disabled = true;
                status.innerText = 'Baj raha hai... customer sun raha hai.';
                audio.onended = function () {
                    idx++;
                    if (idx < playlist.length) { audio.src = playlist[idx]; audio.play(); }
                    else {
                        status.innerText = 'Message poora baj gaya. Ab tum baat kar sakte ho.';
                        if (btn) btn.disabled = false;
                    }
                };
                audio.onerror = function () {
                    status.innerText = 'Audio file nahi mili: ' + playlist[idx];
                    if (btn) btn.disabled = false;
                };
                audio.src = playlist[0];
                var pr = audio.play();
                if (pr && pr.catch) pr.catch(function () {
                    status.innerText = 'Play block hua — dobara PLAY dabao.';
                    if (btn) btn.disabled = false;
                });
            }
        </script>
    </form>
</body>
</html>
