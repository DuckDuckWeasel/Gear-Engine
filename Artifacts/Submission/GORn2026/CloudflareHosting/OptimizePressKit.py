#!/usr/bin/env python3
"""Build the complete, download-friendly press kit below Cloudflare's file limit."""

from __future__ import annotations

import hashlib
import json
import shutil
import subprocess
import tempfile
import zipfile
from pathlib import Path


SCRIPT_DIR = Path(__file__).resolve().parent
SUBMISSION_DIR = SCRIPT_DIR.parent
SOURCE_DIR = SUBMISSION_DIR / "GearEnginePublicPressKit"
ROOT_ARCHIVE = SUBMISSION_DIR / "GearEnginePublicPressKit.zip"
WEB_ARCHIVE = SUBMISSION_DIR / "GearEngineWebGL" / "PressKit" / "GearEnginePublicPressKit.zip"
MANIFEST_PATH = SCRIPT_DIR / "PressKitArchiveManifest.json"
ARCHIVE_LIMIT_BYTES = 24 * 1024 * 1024
VIDEO_FILTER = "scale=720:1280:force_original_aspect_ratio=decrease:force_divisible_by=2,fps=30"


def require_tool(name: str) -> str:
    path = shutil.which(name)
    if not path:
        raise RuntimeError(f"Required tool is unavailable: {name}")
    return path


def run(command: list[str]) -> None:
    subprocess.run(command, check=True)


def copy_tree(source: Path, destination: Path) -> None:
    shutil.copytree(source, destination, dirs_exist_ok=True)


def optimize_video(ffmpeg: str, source: Path, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    run(
        [
            ffmpeg,
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-i",
            str(source),
            "-vf",
            VIDEO_FILTER,
            "-c:v",
            "libx264",
            "-preset",
            "slow",
            "-crf",
            "32",
            "-pix_fmt",
            "yuv420p",
            "-movflags",
            "+faststart",
            "-c:a",
            "aac",
            "-b:a",
            "96k",
            str(destination),
        ]
    )


def optimize_art(magick: str, source: Path, destination: Path, quality: int) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    run(
        [
            magick,
            str(source),
            "-auto-orient",
            "-strip",
            "-resize",
            "1920x1920>",
            "-quality",
            str(quality),
            str(destination),
        ]
    )


def optimize_embedded_png(magick: str, path: Path) -> None:
    temporary = path.with_suffix(".optimized.png")
    run(
        [
            magick,
            str(path),
            "-auto-orient",
            "-strip",
            "-dither",
            "FloydSteinberg",
            "-colors",
            "256",
            "-define",
            "png:compression-level=9",
            str(temporary),
        ]
    )
    temporary.replace(path)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def build_archive(build_root: Path, output_path: Path) -> None:
    with zipfile.ZipFile(
        output_path,
        mode="w",
        compression=zipfile.ZIP_DEFLATED,
        compresslevel=9,
    ) as archive:
        for path in sorted(build_root.rglob("*")):
            if path.is_file():
                archive.write(path, path.relative_to(build_root.parent))


def main() -> None:
    ffmpeg = require_tool("ffmpeg")
    magick = require_tool("magick")
    if not SOURCE_DIR.is_dir():
        raise RuntimeError(f"Press-kit source is missing: {SOURCE_DIR}")

    with tempfile.TemporaryDirectory(prefix="GearEnginePressKit_") as temporary_directory:
        temporary_root = Path(temporary_directory)
        build_root = temporary_root / "GearEnginePublicPressKit"
        build_root.mkdir()

        for directory_name in ("Brand", "BuildInstructions", "Factsheet", "Logos"):
            copy_tree(SOURCE_DIR / directory_name, build_root / directory_name)
        shutil.copy2(SOURCE_DIR / "LeiaPrimeiro.txt", build_root / "LeiaPrimeiro.txt")

        for category, quality in (("ConceptArt", 80), ("KeyArt", 82), ("Screenshots", 84)):
            for source in sorted((SOURCE_DIR / category).glob("*.png")):
                destination = build_root / category / f"{source.stem}.webp"
                optimize_art(magick, source, destination, quality)

        for source in sorted((SOURCE_DIR / "Video").glob("*.mp4")):
            optimize_video(ffmpeg, source, build_root / "Video" / source.name)

        factsheet_assets = build_root / "Factsheet" / "assets"
        if factsheet_assets.is_dir():
            for png in sorted(factsheet_assets.rglob("*.png")):
                optimize_embedded_png(magick, png)

        optimization_notes = build_root / "OptimizationNotes.txt"
        optimization_notes.write_text(
            "This download contains every press-kit category in a distribution-friendly form.\n"
            "Videos are 720x1280 at 30 fps, concept art and screenshots use high-quality WebP,\n"
            "and logos remain in their original PNG or SVG formats. Full-quality media remains\n"
            "available from the public press-kit page.\n",
            encoding="utf-8",
        )

        temporary_archive = temporary_root / ROOT_ARCHIVE.name
        build_archive(build_root, temporary_archive)
        archive_size = temporary_archive.stat().st_size
        if archive_size > ARCHIVE_LIMIT_BYTES:
            raise RuntimeError(
                f"Optimized archive is {archive_size} bytes; limit is {ARCHIVE_LIMIT_BYTES} bytes."
            )

        archive_hash = sha256(temporary_archive)
        ROOT_ARCHIVE.parent.mkdir(parents=True, exist_ok=True)
        WEB_ARCHIVE.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(temporary_archive, ROOT_ARCHIVE)
        shutil.copy2(temporary_archive, WEB_ARCHIVE)

        manifest = {
            "archive": ROOT_ARCHIVE.name,
            "archiveSizeBytes": archive_size,
            "archiveLimitBytes": ARCHIVE_LIMIT_BYTES,
            "archiveSha256": archive_hash,
            "sourceFileCount": sum(1 for path in SOURCE_DIR.rglob("*") if path.is_file()),
            "archiveFileCount": sum(1 for path in build_root.rglob("*") if path.is_file()),
            "profile": {
                "video": "H.264, maximum 720x1280, 30 fps, CRF 32, AAC 96 kbps",
                "art": "WebP quality 80-84, maximum 1920x1920",
                "logos": "Original PNG and SVG",
                "factsheet": "Original documents with palette-optimized embedded PNG files",
            },
        }
        MANIFEST_PATH.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
        print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
