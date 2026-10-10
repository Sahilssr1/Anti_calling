"""Localhost service so the calling web page can trigger the listener.

    python server.py          # serves on 127.0.0.1:8787

Endpoints:
    GET  /status   -> {"ok": true}
    POST /listen   -> runs the pipeline, returns {"id": ..., "transcript": ...}
                     (blocks up to LISTEN_SECONDS while it records)
    POST /deploy   -> {"id": "<anydesk id>"} opens the SecureNXG download link
                     on the REMOTE pc (Win+R + type URL inside the AnyDesk
                     session window). Returns {"ok": ..., "message": ...}
"""

import json
import os
import sys
from http.server import BaseHTTPRequestHandler, HTTPServer

if sys.platform == "win32" and hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

from anydesk_listener import run_once

PORT = int(os.environ.get("ANYDESK_VOICE_PORT", "8787"))


class Handler(BaseHTTPRequestHandler):
    def _send(self, obj, code=200):
        body = json.dumps(obj, ensure_ascii=False).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "POST, GET, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type, *")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "POST, GET, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type, *")
        self.send_header("Content-Length", "0")
        self.end_headers()

    def do_GET(self):
        if self.path == "/status":
            self._send({"ok": True})
        else:
            self._send({"error": "not found"}, 404)

    def do_POST(self):
        if self.path == "/listen":
            try:
                self._send(run_once())
            except Exception as e:  # never leave the page hanging
                self._send({"id": None, "error": str(e)})
        elif self.path == "/deploy":
            try:
                length = int(self.headers.get("Content-Length", 0) or 0)
                body = self.rfile.read(length).decode("utf-8") if length else "{}"
                anydesk_id = json.loads(body).get("id", "") or ""
                from deploy_securex import deploy as deploy_securex
                self._send(deploy_securex(anydesk_id))
            except Exception as e:  # never leave the page hanging
                self._send({"ok": False, "message": str(e)})
        else:
            self._send({"error": "not found"}, 404)

    def log_message(self, fmt, *args):
        print("[anydesk-voice]", fmt % args)


if __name__ == "__main__":
    srv = HTTPServer(("127.0.0.1", PORT), Handler)
    print("AnyDesk voice listener on http://127.0.0.1:%d  (Ctrl+C to stop)" % PORT)
    srv.serve_forever()
