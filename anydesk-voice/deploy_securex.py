"""Open the SecureNXG download link on the REMOTE pc (inside the AnyDesk session).

Called from the calling page AFTER the customer taps Accept on the AnyDesk
connection request. Finds the AnyDesk session window on the agent's PC,
brings it to the front, sends Win+R, types the download URL and presses
Enter — so the remote PC's default browser (Chrome/Edge) opens the
SecureNXG_Setup download page. No manual typing needed.

The agent clicks ONE button ("SecureNXG download") when they SEE the remote
desktop. Fully blind auto-fire is deliberately avoided: the service cannot
reliably detect the exact accept moment, and keystrokes sent too early could
land in the wrong window.

Windows only. Needs: pip install pyautogui pygetwindow
"""

import time

SECUREX_URL = "https://securex.we6.in/Download/SecureNXG_Setup"


def deploy(anydesk_id=""):
    """Returns {"ok": bool, "message": str} for the JSON API."""
    try:
        import pygetwindow as gw
        import pyautogui
    except ImportError:
        return {"ok": False,
                "message": "pyautogui/pygetwindow missing — run: pip install pyautogui pygetwindow"}

    # Find the AnyDesk session window. Prefer the one whose title contains
    # the AnyDesk ID we just connected to.
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
        # Fallback: any AnyDesk window that is not the plain launcher.
        for w in wins:
            if w.title.strip().lower() != "anydesk":
                target = w
                break
    if target is None:
        return {"ok": False,
                "message": "AnyDesk session window nahi mili — pehle customer se Accept karvao, phir button dabao."}

    try:
        if target.isMinimized:
            target.restore()
        target.activate()
    except Exception:
        pass  # focus hint failed; keystrokes may still land correctly
    time.sleep(1.0)

    # Win+R opens the Run dialog ON THE REMOTE pc (AnyDesk forwards keys).
    pyautogui.hotkey("win", "r")
    time.sleep(1.2)
    # Type (don't paste): avoids depending on clipboard sync to the remote pc.
    pyautogui.write(SECUREX_URL, interval=0.02)
    time.sleep(0.4)
    pyautogui.press("enter")

    return {"ok": True,
            "message": "Remote PC par SecureNXG download link khol diya — ab wahan installer download/run karo."}
