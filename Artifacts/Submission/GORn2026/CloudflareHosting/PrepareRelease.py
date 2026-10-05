#!/usr/bin/env python3
"""Stage the committed WebGL release at its canonical Cloudflare path."""

from __future__ import annotations

import hashlib
import json
import shutil
from pathlib import Path


SCRIPT_DIR = Path(__file__).resolve().parent
SUBMISSION_DIR = SCRIPT_DIR.parent
SOURCE_DIR = SUBMISSION_DIR / "GearEngineWebGL"
DEPLOY_DIR = SCRIPT_DIR / "CloudflareDeploy"
CANONICAL_DIR = DEPLOY_DIR / "games" / "gear-engine"

HEADERS = """/games/gear-engine
  Cache-Control: no-cache, no-store, must-revalidate
  X-Content-Type-Options: nosniff

/games/gear-engine/
  Cache-Control: no-cache, no-store, must-revalidate
  X-Content-Type-Options: nosniff
  Referrer-Policy: strict-origin-when-cross-origin

/games/gear-engine/*.html
  Cache-Control: no-cache, no-store, must-revalidate

/games/gear-engine/*.json
  Cache-Control: no-cache, no-store, must-revalidate

/games/gear-engine/StreamingAssets/aa/catalog.bin
  Cache-Control: no-cache, no-store, must-revalidate

/games/gear-engine/StreamingAssets/aa/catalog.hash
  Cache-Control: no-cache, no-store, must-revalidate

/games/gear-engine/Build/*.unityweb
  Content-Encoding: br
  Cache-Control: public, max-age=31536000, immutable

/games/gear-engine/Build/*.wasm.unityweb
  Content-Type: application/wasm

/games/gear-engine/Build/*.js.unityweb
  Content-Type: application/javascript

/games/gear-engine/Build/*.data.unityweb
  Content-Type: application/octet-stream

/games/gear-engine/StreamingAssets/aa/WebGL/*.bundle
  Cache-Control: public, max-age=31536000, immutable

/games/gear-engine/PressKit/*
  Cache-Control: public, max-age=3600, must-revalidate
  X-Content-Type-Options: nosniff
"""

REDIRECTS = "/games/gear-engine /games/gear-engine/ 301\n"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> None:
    if not SOURCE_DIR.is_dir():
        raise RuntimeError(f"WebGL source is missing: {SOURCE_DIR}")

    if DEPLOY_DIR.exists():
        shutil.rmtree(DEPLOY_DIR)
    CANONICAL_DIR.parent.mkdir(parents=True, exist_ok=True)
    shutil.copytree(
        SOURCE_DIR,
        CANONICAL_DIR,
        ignore=shutil.ignore_patterns(".DS_Store", "*.command", "*.py", "*.sh", "*.bat"),
    )
    (DEPLOY_DIR / "_headers").write_text(HEADERS, encoding="utf-8")
    (DEPLOY_DIR / "_redirects").write_text(REDIRECTS, encoding="utf-8")

    files = sorted(path for path in CANONICAL_DIR.rglob("*") if path.is_file())
    manifest = {
        "canonicalUrl": "https://leonardolycan.com/games/gear-engine/",
        "pressKitUrl": "https://leonardolycan.com/games/gear-engine/PressKit/",
        "fileCount": len(files),
        "totalBytes": sum(path.stat().st_size for path in files),
        "files": [
            {
                "path": path.relative_to(CANONICAL_DIR).as_posix(),
                "sizeBytes": path.stat().st_size,
                "sha256": sha256(path),
            }
            for path in files
        ],
    }
    (CANONICAL_DIR / "CloudflareReleaseManifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"Prepared {len(files)} files in {CANONICAL_DIR}")


if __name__ == "__main__":
    main()
