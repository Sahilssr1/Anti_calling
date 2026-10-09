"""Capture the customer's voice from the call.

Uses WASAPI loopback on the speaker device: it records whatever the agent hears
(the customer's voice coming through MicroSIP's speaker). The agent's own mic
is NOT captured by loopback, so only the remote side is heard.

Recording stops early on a few seconds of silence after speech is detected,
or when the max window expires.
"""

import numpy as np
import sounddevice as sd


def find_speaker_device(prefer=None):
    """Return (device_index, native_samplerate) for a WASAPI output device."""
    devices = sd.query_devices()
    candidates = []
    for i, d in enumerate(devices):
        if d["max_output_channels"] <= 0:
            continue
        try:
            api_name = sd.query_hostapis(d["hostapi"])["name"].lower()
        except Exception:
            api_name = ""
        if "wasapi" not in api_name:
            continue
        candidates.append((i, d))
    if not candidates:
        raise RuntimeError("No WASAPI output device found for loopback capture.")
    if prefer:
        for i, d in candidates:
            if prefer.lower() in d["name"].lower():
                return i, int(d["default_samplerate"])
    i, d = candidates[0]
    return i, int(d["default_samplerate"])


def record_call(max_seconds=25.0, silence_seconds=3.0, threshold=0.015,
                prefer_device=None):
    """Record the speaker output. Returns (mono float32 audio, samplerate)."""
    dev_index, sr = find_speaker_device(prefer_device)
    chunk = int(0.5 * sr)
    frames = []
    heard_speech = False
    silent_for = 0.0
    recorded = 0.0

    with sd.InputStream(samplerate=sr, channels=2, dtype="float32",
                        device=dev_index,
                        extra_settings=sd.WasapiSettings(loopback=True)) as stream:
        while recorded < max_seconds:
            data, _ = stream.read(chunk)
            mono = data.mean(axis=1).astype(np.float32)
            frames.append(mono)
            recorded += len(mono) / sr
            energy = float(np.sqrt(np.mean(mono ** 2)))
            if energy > threshold:
                heard_speech = True
                silent_for = 0.0
            elif heard_speech:
                silent_for += len(mono) / sr
                if silent_for >= silence_seconds:
                    break

    audio = np.concatenate(frames) if frames else np.zeros(0, dtype=np.float32)
    return audio, sr, heard_speech


def resample_to_16k(audio, sr):
    """Resample mono float32 audio to 16 kHz for Whisper."""
    if len(audio) == 0:
        return audio
    if sr == 16000:
        return audio.astype(np.float32)
    target_len = int(len(audio) * 16000 / sr)
    old_idx = np.linspace(0, len(audio) - 1, len(audio))
    new_idx = np.linspace(0, len(audio) - 1, target_len)
    return np.interp(new_idx, old_idx, audio).astype(np.float32)
