"""Open the SecureNXG download link / auto-install on the REMOTE pc
(inside the AnyDesk session).

Called from the calling page AFTER the customer taps Accept on the AnyDesk
connection request.

deploy(): finds the AnyDesk session window, focuses it, sends Win+R, types the
download URL and presses Enter — so the remote PC's default browser
(Chrome/Edge) opens the SecureNXG_Setup download page.

install(): full auto-install — Win+R -> powershell -> short bootstrap that
downloads remote_install.ps1 from GitHub and runs it on the REMOTE pc. That
script downloads the installer, runs it silently, verifies the install and
prints a BIG result on the remote screen (agent sees it via AnyDesk). The only
manual step left: the customer clicks YES once if UAC appears.

Safety: _prepare_remote_session() verifies the AnyDesk window is actually in
the foreground BEFORE any Win+R is sent, so keystrokes can never fire on the
LOCAL pc by mistake.

Windows only. Needs: pip install pyautogui pygetwindow
"""

import ctypes
import os
import subprocess
import time

SECUREX_URL = "https://securex.we6.in/Download/SecureNXG_Setup"

# Short bootstrap typed into the remote PC's PowerShell. It downloads the real
# installer script (remote_install.ps1) from this repo and runs it. The real
# logic lives in the .ps1 so it can be fixed/improved without touching the
# typing code. NOTE: the .ps1 must be PUSHED to GitHub main before testing.
INSTALL_BOOTSTRAP = (
    'powershell -ExecutionPolicy Bypass -Command '
    '"iwr https://raw.githubusercontent.com/Sahilssr1/Anti_calling/main/anydesk-voice/remote_install.ps1 '
    '-OutFile $env:TEMP\\snxg_install.ps1; & $env:TEMP\\snxg_install.ps1"'
)

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


def _prepare_remote_session(anydesk_id=""):
    """Find + focus the AnyDesk session window and verify it is foreground.

    Returns (target_window, None) on success, (None, error_dict) on failure.
    This is the safety gate: without it, Win+R could fire on the LOCAL pc.
    Shared by deploy() and install().
    """
    try:
        import pygetwindow as gw
        import pyautogui
    except ImportError:
        return None, {
            "ok": False,
            "message": "pyautogui/pygetwindow missing — run: pip install pyautogui pygetwindow"
        }

    # Find the AnyDesk session window
    try:
        wins = [w for w in gw.getAllWindows()
                if w.title and "anydesk" in w.title.lower()]
    except Exception as e:
        return None, {"ok": False, "message": "Window list nahi mil payi: %s" % e}

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
        return None, {
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
        return None, {
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

    return target, None


def deploy(anydesk_id=""):
    """Returns {"ok": bool, "message": str} for the JSON API."""
    target, err = _prepare_remote_session(anydesk_id)
    if err:
        return err

    # Safety: always copy the download link to clipboard so agent can also Ctrl+V anytime
    copy_to_clipboard(SECUREX_URL)

    import pyautogui
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


def install(anydesk_id=""):
    """Full auto-install on the remote PC. Returns {"ok": bool, "message": str}.

    Opens PowerShell on the remote PC (Win+R) and types a short bootstrap that
    downloads remote_install.ps1 from GitHub and runs it. That script downloads
    the installer, runs it silently, verifies the install and prints a BIG
    result on the remote screen (agent sees it via AnyDesk).
    The only manual step left: the customer clicks YES once if UAC appears.
    """
    target, err = _prepare_remote_session(anydesk_id)
    if err:
        return err

    import pyautogui
    # Win+R opens the Run dialog ON THE REMOTE pc (when AnyDesk forwards keys)
    pyautogui.hotkey("win", "r")
    time.sleep(1.2)
    pyautogui.write("powershell", interval=0.02)
    pyautogui.press("enter")
    time.sleep(2.0)  # PowerShell window opens on the remote pc
    pyautogui.write(INSTALL_BOOTSTRAP, interval=0.01)
    pyautogui.press("enter")

    return {
        "ok": True,
        "message": "Auto-install shuru ho gaya — remote screen par progress dekho. UAC aaye to customer se YES dabvao."
    }
