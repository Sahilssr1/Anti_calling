"""Capture the customer's voice from the call.

Uses WASAPI loopback on the speaker device: it records whatever the agent hears
(the customer's voice coming through MicroSIP's speaker). The agent's own mic
is NOT captured by loopback, so only the remote side is heard.

Recording stops early on a few seconds of silence after speech is detected,
or when the max window expires.
"""

import numpy as np
import soundcard as sc


def get_loopback_mic(prefer=None):
    """Return a Loopback microphone for recording what is played to the agent."""
    if prefer:
        for spk in sc.all_speakers():
            if prefer.lower() in spk.name.lower():
                return sc.get_microphone(id=str(spk.name), include_loopback=True)
    # Default to the system's default speaker (e.g. Headset Earphone)
    spk = sc.default_speaker()
    # If default speaker has 'cable' in name, try finding headset/earphone/speaker
    if "cable" in spk.name.lower():
        for s in sc.all_speakers():
            if "cable" not in s.name.lower() and any(w in s.name.lower() for w in ["headset", "earphone", "speaker", "headphone"]):
                return sc.get_microphone(id=str(s.name), include_loopback=True)
    return sc.get_microphone(id=str(spk.name), include_loopback=True)


def record_call(max_seconds=25.0, silence_seconds=3.0, threshold=0.015,
                prefer_device=None, samplerate=48000):
    """Record the speaker output. Returns (mono float32 audio, samplerate, heard_speech)."""
    mic = get_loopback_mic(prefer_device)
    sr = samplerate
    chunk_frames = int(0.5 * sr)
    frames = []
    heard_speech = False
    silent_for = 0.0
    recorded = 0.0

    with mic.recorder(samplerate=sr) as recorder:
        while recorded < max_seconds:
            data = recorder.record(numframes=chunk_frames)
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
