#!/usr/bin/env python3
"""Serve the Gear Engine WebGL build with Unity-compatible Brotli headers."""

from __future__ import annotations

import functools
import http.server
import os
import pathlib
import threading
import webbrowser


HOST = "127.0.0.1"
PORT = 8080
URL = f"http://{HOST}:{PORT}"


class GearEngineRequestHandler(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path: str) -> str:
        if path.endswith((".wasm.br", ".wasm.unityweb")):
            return "application/wasm"
        if path.endswith((".js.br", ".js.unityweb")):
            return "application/javascript"
        if path.endswith((".data.br", ".data.unityweb")):
            return "application/octet-stream"
        return super().guess_type(path)

    def end_headers(self) -> None:
        if self.path.endswith((".br", ".unityweb")):
            self.send_header("Content-Encoding", "br")
        super().end_headers()


def main() -> None:
    build_directory = pathlib.Path(__file__).resolve().parent
    handler = functools.partial(GearEngineRequestHandler, directory=str(build_directory))
    server = http.server.ThreadingHTTPServer((HOST, PORT), handler)
    print(f"Gear Engine is available at {URL}")
    print("Keep this window open while playing. Press Control+C to stop the server.")
    if os.environ.get("GEAR_ENGINE_NO_BROWSER") != "1":
        threading.Timer(0.75, webbrowser.open, args=(URL,)).start()
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nStopping Gear Engine server.")
    finally:
        server.server_close()


if __name__ == "__main__":
    main()
