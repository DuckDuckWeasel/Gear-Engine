#!/usr/bin/env python3
"""Stage the committed WebGL release at its canonical Cloudflare path."""

from __future__ import annotations

import hashlib
import json
import re
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

/games/gear-engine/play
  Cache-Control: no-cache, no-store, must-revalidate

/games/gear-engine/play/
  Cache-Control: no-cache, no-store, must-revalidate
  X-Content-Type-Options: nosniff

/games/gear-engine/play/StreamingAssets/aa/catalog.bin
  Cache-Control: no-cache, no-store, must-revalidate

/games/gear-engine/play/StreamingAssets/aa/catalog.hash
  Cache-Control: no-cache, no-store, must-revalidate

/games/gear-engine/play/Build/*.unityweb
  Content-Encoding: br
  Cache-Control: public, max-age=31536000, immutable

/games/gear-engine/play/Build/*.wasm.unityweb
  Content-Type: application/wasm

/games/gear-engine/play/Build/*.js.unityweb
  Content-Type: application/javascript

/games/gear-engine/play/Build/*.data.unityweb
  Content-Type: application/octet-stream

/games/gear-engine/play/StreamingAssets/aa/WebGL/*.bundle
  Cache-Control: public, max-age=31536000, immutable

/games/gear-engine/PressKit/*
  Cache-Control: public, max-age=3600, must-revalidate
  X-Content-Type-Options: nosniff
"""

REDIRECTS = """/games/gear-engine /games/gear-engine/ 301
/games/gear-engine/PressKit /games/gear-engine/ 301
/games/gear-engine/PressKit/ /games/gear-engine/ 301
/games/gear-engine/PressKit/index.html /games/gear-engine/ 301
"""


def landing_html(source: Path) -> str:
    """Keep page anchors local while resolving press-kit resources from the root."""
    html = source.read_text(encoding="utf-8")
    html = re.sub(
        r'((?:src|href|poster)=[\"\'])(Media/|Fonts/|BrandKit\.html|GearEnginePublicPressKit\.zip)',
        r'\1PressKit/\2', html,
    )
    return re.sub(r'(url\([\"\']?)(Fonts/|Media/)', r'\1PressKit/\2', html)


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
    CANONICAL_DIR.mkdir(parents=True, exist_ok=True)
    shutil.copytree(
        SOURCE_DIR,
        CANONICAL_DIR / "play",
        ignore=shutil.ignore_patterns("PressKit", ".DS_Store", "*.command", "*.py", "*.sh", "*.bat"),
    )
    shutil.copytree(
        SOURCE_DIR / "PressKit", CANONICAL_DIR / "PressKit",
        ignore=shutil.ignore_patterns("index.html", ".DS_Store"),
    )
    (CANONICAL_DIR / "index.html").write_text(
        landing_html(SOURCE_DIR / "PressKit" / "index.html"), encoding="utf-8")
    (DEPLOY_DIR / "_headers").write_text(HEADERS, encoding="utf-8")
    (DEPLOY_DIR / "_redirects").write_text(REDIRECTS, encoding="utf-8")

    files = sorted(path for path in CANONICAL_DIR.rglob("*") if path.is_file())
    manifest = {
        "canonicalUrl": "https://leonardolycan.com/games/gear-engine/play",
        "landingPageUrl": "https://leonardolycan.com/games/gear-engine/",
        "pressKitUrl": "https://leonardolycan.com/games/gear-engine/",
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
