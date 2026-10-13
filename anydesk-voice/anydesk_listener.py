"""One-shot pipeline: listen -> transcribe -> extract AnyDesk ID -> open AnyDesk.

Run directly to test:
    python anydesk_listener.py
"""

import os
import shutil
import subprocess

from audio_capture import record_call, resample_to_16k
from digit_extract import extract_anydesk_id

WHISPER_MODEL = os.environ.get("WHISPER_MODEL", "small")
WHISPER_LANG = os.environ.get("WHISPER_LANG") or None  # None = auto-detect
LISTEN_SECONDS = float(os.environ.get("LISTEN_SECONDS", "25"))
SILENCE_SECONDS = float(os.environ.get("SILENCE_SECONDS", "3"))
SPEAKER_DEVICE = os.environ.get("SPEAKER_DEVICE")  # substring, e.g. "AB-EH01"
ANYDESK_EXE = os.environ.get("ANYDESK_EXE")

_model = None


def transcribe(audio16k):
    """Transcribe 16 kHz mono float32 audio with faster-whisper."""
    global _model
    if _model is None:
        from faster_whisper import WhisperModel
        # device="auto" uses the NVIDIA GPU when present, else CPU
        _model = WhisperModel(WHISPER_MODEL, device="auto", compute_type="int8")
    segments, _info = _model.transcribe(audio16k, language=WHISPER_LANG,
                                       beam_size=5)
    return " ".join(s.text.strip() for s in segments).strip()


def copy_to_clipboard(text):
    """Copy text to Windows clipboard using clip.exe."""
    try:
        subprocess.run(
            ["clip"],
            input=text.encode("utf-8"),
            check=False,
            creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0)
        )
    except Exception:
        pass


def find_anydesk():
    if ANYDESK_EXE and os.path.exists(ANYDESK_EXE):
        return ANYDESK_EXE

    # 1. Check if AnyDesk is already running and grab its exact exe path
    try:
        cmd = ['powershell', '-NoProfile', '-Command',
               '(Get-Process AnyDesk -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Path)']
        running_path = subprocess.check_output(cmd, text=True, timeout=2).strip()
        if running_path and os.path.exists(running_path):
            return running_path
    except Exception:
        pass

    # 2. Check permanent and standard paths
    candidates = [
        r"E:\Anti\AnyDesk\AnyDesk.exe",
        os.path.expanduser(r"~\Downloads\AnyDesk\AnyDesk.exe"),
        os.path.expanduser(r"~\Downloads\anydesk-mod_v9.7.5-pc-an1.ca (1)\AnyDesk.exe"),
        r"C:\Program Files (x86)\AnyDesk\AnyDesk.exe",
        r"C:\Program Files\AnyDesk\AnyDesk.exe",
    ]

    # 3. Check user Downloads and Desktop directories dynamically
    for folder in [r"~\Downloads", r"~\Desktop"]:
        base_dir = os.path.expanduser(folder)
        if os.path.exists(base_dir):
            for root, dirs, files in os.walk(base_dir):
                if "AnyDesk.exe" in files:
                    candidates.append(os.path.join(root, "AnyDesk.exe"))
                if len(candidates) >= 15:
                    break

    # 4. Check %TEMP% directories in case AnyDesk is extracted from a zip
    temp_dir = os.environ.get("TEMP", "")
    if temp_dir and os.path.exists(temp_dir):
        try:
            for item in os.listdir(temp_dir):
                if "anydesk" in item.lower():
                    item_path = os.path.join(temp_dir, item)
                    if os.path.isdir(item_path):
                        exe_cand = os.path.join(item_path, "AnyDesk.exe")
                        if os.path.exists(exe_cand):
                            candidates.append(exe_cand)
        except Exception:
            pass

    for p in candidates:
        if os.path.exists(p):
            return p
    return shutil.which("anydesk.exe")


def connect_anydesk(anydesk_id):
    # Copy ID to clipboard so the agent can also Ctrl+V anytime
    copy_to_clipboard(anydesk_id)

    exe = find_anydesk()
    if not exe:
        raise FileNotFoundError(
            "AnyDesk.exe not found. Set the ANYDESK_EXE environment variable.")
    # Official CLI: anydesk.exe <ID> opens a connection request to that ID.
    # The customer still has to click Accept on their side.
    subprocess.Popen([exe, anydesk_id])
    return exe


def run_once():
    """Full pipeline. Returns a dict with id / transcript / error."""
    try:
        audio, sr, heard = record_call(max_seconds=LISTEN_SECONDS,
                                       silence_seconds=SILENCE_SECONDS,
                                       prefer_device=SPEAKER_DEVICE)
    except Exception as e:
        return {"id": None, "error": "audio capture failed: %s" % e}

    if not heard:
        return {"id": None, "error": "no speech detected — customer didn't speak?"}

    audio16k = resample_to_16k(audio, sr)
    try:
        transcript = transcribe(audio16k)
    except Exception as e:
        return {"id": None, "error": "transcription failed: %s" % e}

    anydesk_id, digits = extract_anydesk_id(transcript)
    if not anydesk_id:
        return {"id": None, "transcript": transcript, "digits": digits,
                "error": "couldn't find a 9-digit AnyDesk ID in: %s" % transcript}

    try:
        connect_anydesk(anydesk_id)
    except Exception as e:
        return {"id": anydesk_id, "transcript": transcript,
                "error": "found ID but couldn't launch AnyDesk: %s" % e}

    return {"id": anydesk_id, "transcript": transcript}


if __name__ == "__main__":
    import json
    print(json.dumps(run_once(), indent=2, ensure_ascii=False))
