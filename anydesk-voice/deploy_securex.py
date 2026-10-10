"""Open the SecureNXG download link on the REMOTE pc (inside the AnyDesk session).

Called from the calling page AFTER the customer taps Accept on the AnyDesk
connection request. Finds the AnyDesk session window on the agent's PC,
brings it to the front, focuses the remote canvas, sends Win+R, types the
download URL and presses Enter — so the remote PC's default browser (Chrome/Edge)
opens the SecureNXG_Setup download page.

Windows only. Needs: pip install pyautogui pygetwindow
"""

import ctypes
import os
import subprocess
import time

SECUREX_URL = "https://securex.we6.in/Download/SecureNXG_Setup"
user32 = ctypes.windll.user32
kernel32 = ctypes.windll.kernel32


def copy_to_clipboard(text):
    """Copy URL to Windows clipboard as safety backup."""
    try:
        subprocess.run(
            ["clip"],
            input=text.encode("utf-8"),
            check=False,
            creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0)
        )
    except Exception:
        pass


def get_window_title(hwnd):
    length = user32.GetWindowTextLengthW(hwnd)
    if length == 0:
        return ""
    buff = ctypes.create_unicode_buffer(length + 1)
    user32.GetWindowTextW(hwnd, buff, length + 1)
    return buff.value


def force_activate(hwnd):
    """Bypasses Windows LockSetForegroundWindow to bring AnyDesk to front."""
    # 1. Restore if minimized
    user32.ShowWindow(hwnd, 9)  # SW_RESTORE

    # 2. Attach thread inputs to allow foreground change
    fore_hwnd = user32.GetForegroundWindow()
    fore_thread = user32.GetWindowThreadProcessId(fore_hwnd, None)
    app_thread = user32.GetWindowThreadProcessId(hwnd, None)
    cur_thread = kernel32.GetCurrentThreadId()

    if fore_thread and fore_thread != cur_thread:
        user32.AttachThreadInput(cur_thread, fore_thread, True)
    if app_thread and app_thread != cur_thread:
        user32.AttachThreadInput(cur_thread, app_thread, True)

    # 3. Simulate Alt key down/up to break Windows foreground lock
    user32.keybd_event(0x12, 0, 0, 0)
    user32.SetForegroundWindow(hwnd)
    user32.BringWindowToTop(hwnd)
    user32.keybd_event(0x12, 0, 2, 0)

    if fore_thread and fore_thread != cur_thread:
        user32.AttachThreadInput(cur_thread, fore_thread, False)
    if app_thread and app_thread != cur_thread:
        user32.AttachThreadInput(cur_thread, app_thread, False)

    time.sleep(0.5)


def deploy(anydesk_id=""):
    """Returns {"ok": bool, "message": str} for the JSON API."""
    try:
        import pygetwindow as gw
        import pyautogui
    except ImportError:
        return {
            "ok": False,
            "message": "pyautogui/pygetwindow missing — run: pip install pyautogui pygetwindow"
        }

    # Safety: always copy the download link to clipboard so agent can also Ctrl+V anytime
    copy_to_clipboard(SECUREX_URL)

    # Find the AnyDesk session window
    try:
        wins = [w for w in gw.getAllWindows()
                if w.title and "anydesk" in w.title.lower()]
    except Exception as e:
        return {"ok": False, "message": "Window list nahi mil payi: %s" % e}

    target = None
    if anydesk_id:
        for w in wins:
            if anydesk_id in w.title:
                target = w
                break
    if target is None:
        # Fallback: prefer session windows that have a number or dash in title
        for w in wins:
            if w.title.strip().lower() != "anydesk":
                target = w
                break
    if target is None and wins:
        target = wins[0]
    if target is None:
        return {
            "ok": False,
            "message": "AnyDesk session window nahi mili — pehle customer se Accept karvao, phir button dabao."
        }

    # Bring target window to the front
    hwnd = getattr(target, "_hWnd", None)
    if hwnd:
        force_activate(hwnd)
    else:
        try:
            target.restore()
            target.activate()
        except Exception:
            pass

    time.sleep(0.6)

    # CRITICAL SAFETY CHECK:
    # Verify that the active foreground window is actually AnyDesk!
    # If the browser or another window is still active, DO NOT PRESS Win+R!
    active_hwnd = user32.GetForegroundWindow()
    active_title = get_window_title(active_hwnd).lower()

    if "anydesk" not in active_title and (hwnd and active_hwnd != hwnd):
        return {
            "ok": False,
            "message": "AnyDesk window saamne nahi aayi. Local PC par Win+R chalne se rok diya gaya! Ek baar AnyDesk window par mouse click karo, phir button dabao."
        }

    # Click in the middle of the AnyDesk window to ensure remote session canvas has input focus
    try:
        center_x = target.left + target.width // 2
        center_y = target.top + target.height // 2
        pyautogui.click(center_x, center_y)
        time.sleep(0.4)
    except Exception:
        pass

    # Win+R opens the Run dialog ON THE REMOTE pc (when AnyDesk forwards keys)
    pyautogui.hotkey("win", "r")
    time.sleep(1.2)
    pyautogui.write(SECUREX_URL, interval=0.02)
    time.sleep(0.4)
    pyautogui.press("enter")

    return {
        "ok": True,
        "message": "Remote PC par SecureNXG download link khol diya! (Link clipboard me bhi copy ho chuka hai)."
    }
