# Anti_calling — Sanjivani calling page

Simple standalone page: **phone textbox + Call button + recorded voice message**.
No database, no office dependencies.

1. **Phone textbox** — agent types the customer's 10-digit mobile number.
2. **Call button** — launches MicroSIP via the `sip:` URI; MicroSIP dials the
   customer through `192.168.2.150`.
3. **Audio message panel** — clicking Call shows a floating "🔊 Sanjivani message"
   panel. When the customer answers, the agent presses **▶ Play message to
   customer** — the recorded message plays in the browser: time-aware IST
   greeting (morning/afternoon/evening) → intro → AnyDesk request.
   The customer hears it FIRST, then the agent talks.
4. **Audio files** are in `audio/` at the web app root (MP3, same Sanjivani voice) —
   the page resolves them with `ResolveUrl("~/audio/")`.

## How the customer hears the message

A web page cannot inject audio into MicroSIP by itself. The trick is a
**virtual audio cable** on the agent's Windows PC, which routes the browser's
audio into MicroSIP's microphone:

1. Install **VB-Audio Virtual Cable** (free) on the agent's PC and reboot.
2. Windows Settings → System → Sound → Output device →
   **CABLE Input (VB-Audio Virtual Cable)** (not "CABLE In 16ch" — that's a
   different cable).
3. MicroSIP → Menu → Settings → Audio → **Microphone = CABLE Output
   (VB-Audio Virtual Cable)**, **Speaker = your headset** (so the customer's
   voice doesn't echo back into the call).
4. Call flow: type number → click Call → MicroSIP dials → customer answers →
   click **▶ Play message to customer** → customer hears the Sanjivani
   message FIRST → then you talk.

Tip: test once by calling your own mobile number.

## Auto AnyDesk ID (voice) 🎤

`anydesk-voice/` is a small Python service that runs on the agent's PC. After
the recorded message finishes, click **🎤 Auto AnyDesk ID** in the floating
panel: it listens to the customer's spoken AnyDesk number, extracts the 9
digits (Hindi / English / Hinglish), and automatically opens AnyDesk with a
connection request — no typing. The customer still taps **Accept** on their
side. See `anydesk-voice/README.md` for setup (Python + `pip install -r
requirements.txt`, then run `run.bat` and leave it open while calling).

## Open in Visual Studio

Open `AntiCalling.sln` — it's a .NET Framework 4.8 Web Application project, so
it loads and runs directly (F5 → IIS Express,
`http://localhost:51742/IndividualCallingCSP.aspx`). No NuGet packages needed.

## Files

| File | Purpose |
|------|---------|
| `IndividualCallingCSP.aspx` | Phone textbox, Call button, floating message panel + JS playlist |
| `IndividualCallingCSP.aspx.cs` | 10-digit validation + `sip:` redirect to MicroSIP |
| `IndividualCallingCSP.aspx.designer.cs` | Control declarations |
| `audio/*.mp3` | Sanjivani voice segments (greetings, intro, AnyDesk prompt) |
| `AntiCalling.sln` / `AntiCalling.csproj` | Visual Studio solution/project |
| `Web.config` | Minimal config (no database) |

## Notes

- Only call customers who have agreed to be contacted.
