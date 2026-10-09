# Anti_calling — Sanjivani auto-call integration

Current mode: **call + recorded voice only** (no database saving right now).

1. **Call button (`btnsipcall_Click`)** — unchanged from before: launches MicroSIP
   via the `sip:` URI, MicroSIP dials the customer through `192.168.2.150`.
2. **Audio message panel**: clicking the SIP call button shows a floating
   "🔊 Sanjivani message" panel. When the customer answers, the agent presses
   **▶ Play message to customer** — the recorded message plays in the browser:
   time-aware IST greeting (morning/afternoon/evening) → intro → AnyDesk request.
   The customer hears it FIRST, then the agent talks.
3. **Audio files** are in `audio/` (MP3, same Sanjivani voice). Deploy this folder
   to `~/SanjivaniBriefing/audio/` on the web server (the page resolves it with
   `ResolveUrl("~/SanjivaniBriefing/audio/")`).

> Database files (`App_Code/PgDb.cs`, `db/schema.sql`, `Web.config.sample`) are
> kept in the repo for later, but currently **not wired in** — no data is saved.

## How the customer hears the message

A web page cannot inject audio into MicroSIP by itself. The trick is a
**virtual audio cable** on the agent's Windows PC, which routes the browser's
audio into MicroSIP's microphone:

1. Install **VB-Audio Virtual Cable** (free) on the agent's PC and reboot.
2. Windows Settings → System → Sound → Volume mixer → set your **browser's**
   output device to **CABLE Input (VB-Audio Virtual Cable)**.
3. MicroSIP → Menu → Settings → Audio → **Microphone = CABLE Output**.
   (Leave Speaker/Headphones as normal — you still hear the customer.)
4. Call flow: click the phone button → MicroSIP dials → customer answers →
   click **▶ Play message to customer** → customer hears the Sanjivani
   message FIRST → then you talk.

Tip: test once by calling your own mobile number.

## Files added/changed

| File | Change |
|------|--------|
| `IndividualCallingCSP.aspx` | `OnClientClick` on `btnsipcall`, floating message panel + JS playlist |
| `IndividualCallingCSP.aspx.cs` | unchanged behaviour (call only) |
| `audio/*.mp3` | Sanjivani voice segments (same voice as before) |
| `App_Code/PgDb.cs` | kept for later (PostgreSQL helper, currently unused) |
| `db/schema.sql` | kept for later (call_log table, currently unused) |
| `Web.config.sample` | kept for later (connection string template, currently unused) |

## Notes

- Only call customers who have agreed to be contacted.
