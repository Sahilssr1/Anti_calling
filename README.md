# Anti_calling — Sanjivani auto-call integration

Modifications on top of `IndividualCallingCSP` (SanjivaniBriefing calling page):

1. **Call button (`btnsipcall_Click`)** still launches MicroSIP via the `sip:` URI
   (unchanged behaviour), but now also logs the call start to a **local PostgreSQL**
   database (`sanjivani_calls`, table `call_log`).
2. **Audio message panel**: clicking the SIP call button shows a floating
   "🔊 Sanjivani message" panel. When the customer answers, the agent presses
   **▶ Play message to customer** — the recorded message plays in the browser:
   time-aware IST greeting (morning/afternoon/evening) → intro → AnyDesk request.
   When playback finishes, the page marks `audio_played = true` in PostgreSQL.
3. **Audio files** are in `audio/` (MP3). Deploy this folder to
   `~/SanjivaniBriefing/audio/` on the web server (the page resolves it with
   `ResolveUrl("~/SanjivaniBriefing/audio/")`).

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

## PostgreSQL setup (local)

1. Install PostgreSQL on the machine that runs this web app
   (Windows: EDB installer from postgresql.org).
2. Create the database and tables:
   ```
   createdb -U postgres sanjivani_calls
   psql -U postgres -d sanjivani_calls -f db/schema.sql
   ```
3. Merge `Web.config.sample` into your `Web.config` and set the real password.
4. Install the Npgsql driver (NuGet Package Manager Console):
   ```
   Install-Package Npgsql -Version 6.0.11
   ```
   (v6.x targets netstandard2.0, so it works on .NET Framework Web Forms.)
5. `App_Code/PgDb.cs` is auto-compiled by ASP.NET — no project changes needed.

Check the log:
```sql
SELECT id, call_time, mobile_no, caller_name, greeting_used, audio_played
FROM call_log ORDER BY id DESC LIMIT 20;
```

## Files added/changed

| File | Change |
|------|--------|
| `IndividualCallingCSP.aspx` | ScriptManager `EnablePageMethods`, `OnClientClick` on `btnsipcall`, floating message panel + JS playlist |
| `IndividualCallingCSP.aspx.cs` | `btnsipcall_Click` logs to PostgreSQL; new `[WebMethod] MarkAudioPlayed` |
| `App_Code/PgDb.cs` | Npgsql helper: `LogCallStart`, `MarkAudioPlayed`, IST greeting |
| `db/schema.sql` | `call_log` table |
| `Web.config.sample` | `SanjivaniPg` connection string template |
| `audio/*.mp3` | Sanjivani voice segments (same voice as before) |

## Notes

- DB writes are wrapped in try/catch — if PostgreSQL is down, the call still goes through.
- Only call customers who have agreed to be contacted.
