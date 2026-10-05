#!/usr/bin/env python3
"""Validate Cloudflare limits, routing files, WebGL payloads, and archive integrity."""

from __future__ import annotations

import re
import sys
import zipfile
from pathlib import Path


SCRIPT_DIR = Path(__file__).resolve().parent
DEPLOY_DIR = SCRIPT_DIR / "CloudflareDeploy"
CANONICAL_DIR = DEPLOY_DIR / "games" / "gear-engine"
CLOUDFLARE_FILE_LIMIT = 25 * 1024 * 1024
PRESS_KIT_LIMIT = 24 * 1024 * 1024
OLD_HOST = "gear-engine-gorn-2026.web.app"


def fail(message: str) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)


def main() -> None:
    required = [
        DEPLOY_DIR / "_headers",
        DEPLOY_DIR / "_redirects",
        CANONICAL_DIR / "index.html",
        CANONICAL_DIR / "PressKit" / "index.html",
        CANONICAL_DIR / "PressKit" / "GearEnginePublicPressKit.zip",
    ]
    for path in required:
        if not path.is_file():
            fail(f"Required deployment file is missing: {path}")

    oversized = [
        path
        for path in DEPLOY_DIR.rglob("*")
        if path.is_file() and path.stat().st_size > CLOUDFLARE_FILE_LIMIT
    ]
    if oversized:
        fail("Files exceed Cloudflare's 25 MiB limit: " + ", ".join(map(str, oversized)))

    press_kit = CANONICAL_DIR / "PressKit" / "GearEnginePublicPressKit.zip"
    if press_kit.stat().st_size > PRESS_KIT_LIMIT:
        fail(f"Press-kit archive exceeds the 24 MiB safety target: {press_kit.stat().st_size}")
    if not zipfile.is_zipfile(press_kit):
        fail("Press-kit download is not a valid ZIP archive.")
    with zipfile.ZipFile(press_kit) as archive:
        corrupt_entry = archive.testzip()
        if corrupt_entry:
            fail(f"Press-kit archive contains a corrupt entry: {corrupt_entry}")
        names = archive.namelist()
        for category in ("Brand/", "ConceptArt/", "Factsheet/", "KeyArt/", "Logos/", "Screenshots/", "Video/"):
            if not any(category in name for name in names):
                fail(f"Press-kit archive is missing category: {category}")

    text_extensions = {".html", ".json", ".md", ".txt", ".js", ".css"}
    for path in CANONICAL_DIR.rglob("*"):
        if path.is_file() and path.suffix.lower() in text_extensions:
            if OLD_HOST in path.read_text(encoding="utf-8", errors="ignore"):
                fail(f"Retired Firebase hostname remains in deployment content: {path}")

    headers = (DEPLOY_DIR / "_headers").read_text(encoding="utf-8")
    for required_header in ("Content-Encoding: br", "Content-Type: application/wasm"):
        if required_header not in headers:
            fail(f"Required WebGL header is missing: {required_header}")

    index = (CANONICAL_DIR / "index.html").read_text(encoding="utf-8")
    for required_asset in (
        ".data.unityweb",
        ".framework.js.unityweb",
        ".wasm.unityweb",
    ):
        if required_asset not in index:
            fail(f"WebGL index does not reference the compressed asset: {required_asset}")

    uncompressed_build_files = [
        path
        for path in (CANONICAL_DIR / "Build").iterdir()
        if path.is_file() and path.suffix in {".data", ".wasm"}
    ]
    if uncompressed_build_files:
        fail(
            "Uncompressed WebGL payloads remain in the release: "
            + ", ".join(map(str, uncompressed_build_files))
        )

    loader_match = re.search(r'loaderUrl = buildUrl \+ "/([^\"]+\.loader\.js)"', index)
    if not loader_match:
        fail("WebGL index does not declare a loader file.")
    loader = CANONICAL_DIR / "Build" / loader_match.group(1)
    if not loader.is_file():
        fail(f"WebGL loader referenced by the index is missing: {loader}")
    loader_source = loader.read_text(encoding="utf-8")
    if "hasUnityMarker:function(e){return!0}" not in loader_source:
        fail("WebGL loader is missing the Cloudflare Brotli fallback patch.")

    print(
        f"Validated {sum(1 for path in DEPLOY_DIR.rglob('*') if path.is_file())} files; "
        f"press kit is {press_kit.stat().st_size} bytes."
    )


if __name__ == "__main__":
    main()
