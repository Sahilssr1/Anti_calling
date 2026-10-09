# AnyDesk Voice Listener 🎤

Listens to the customer's spoken AnyDesk number on the call, extracts the
9-digit ID, and **automatically opens AnyDesk with a connection request** —
no typing needed. Triggered from the calling web page.

## How it works

1. Agent clicks **🎤 Auto AnyDesk ID** on the calling page (after the recorded
   message finishes and the customer speaks their number).
2. This service records what the agent hears (WASAPI loopback on the speaker —
   the mic is not captured, so only the customer's voice is heard).
3. `faster-whisper` transcribes it (Hindi / English / Hinglish).
4. The 9-digit AnyDesk ID is extracted — handles spoken words
   ("ek do teen…", "one two three…", "एक दो तीन…") and plain digits.
5. `anydesk.exe <ID>` is launched → AnyDesk opens a connection request.
   The customer still taps **Accept** on their side (AnyDesk requires it).

If no clear 9-digit ID is found, nothing is launched and the page shows the
transcript so the agent can type it manually.

## Setup (on the agent's Windows PC)

1. Install Python 3.10+ from python.org (tick **Add python.exe to PATH**).
2. Open a terminal in this folder and run:
   ```
   pip install -r requirements.txt
   ```
   First run downloads the Whisper `small` model (~500 MB, one time).
3. Start the service — double-click **`run.bat`**, or run `python server.py`.
   Leave the black window open while calling.
4. On the calling page, click **🎤 Auto AnyDesk ID** when the customer starts
   speaking their number. It listens up to ~25 seconds (stops early on silence).

## Settings (environment variables)

| Variable | Default | Purpose |
|---|---|---|
| `ANYDESK_VOICE_PORT` | `8787` | Port the service listens on |
| `WHISPER_MODEL` | `small` | `tiny`/`base`/`small`/`medium` — smaller = faster |
| `WHISPER_LANG` | auto | Set `hi` or `en` to skip language detection |
| `LISTEN_SECONDS` | `25` | Max recording window |
| `SILENCE_SECONDS` | `3` | Stop early after this much silence |
| `SPEAKER_DEVICE` | auto | Substring of the speaker to capture, e.g. `AB-EH01` |
| `ANYDESK_EXE` | auto-detected | Full path to `AnyDesk.exe` if not in the usual place |

## Test without a call

```
python test_digits.py      # digit extraction self-test
python anydesk_listener.py # one full listen cycle in the terminal
```

## Notes

- Accuracy depends on the customer speaking the digits clearly. Hindi, English
  and Hinglish number words are all understood.
- Nothing is typed anywhere except into AnyDesk's own address bar via its
  official command line (`anydesk.exe <ID>`).
- Only call customers who have agreed to be contacted.
