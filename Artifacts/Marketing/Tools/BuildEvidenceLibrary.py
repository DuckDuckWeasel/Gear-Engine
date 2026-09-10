#!/usr/bin/env python3
"""Build the marketing evidence manifest and contact sheets.

The script never mutates source media. It inventories first-party repository assets,
current visual-test captures, historical repository snapshots, and generated concepts.
"""

from __future__ import annotations

import hashlib
import json
import math
import subprocess
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from PIL import Image, ImageDraw, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parents[3]
MANIFEST_PATH = ROOT / "Artifacts/Marketing/Data/EvidenceManifest.json"
CONTACT_SHEET_DIR = ROOT / "Artifacts/Marketing/Evidence/ContactSheets"
VIDEO_EVOLUTION_DIR = ROOT / "Artifacts/Marketing/Evidence/VideoEvolution"
TIMELINE_FRAME_DIR = VIDEO_EVOLUTION_DIR / "TimelineFrames"
LATEST_FRAME_DIR = VIDEO_EVOLUTION_DIR / "LatestBuildFrames"
SUPPORTED_IMAGES = {".png", ".jpg", ".jpeg", ".webp"}
SUPPORTED_VIDEOS = {".mp4", ".mov", ".m4v", ".webm"}
SUPPORTED_VECTORS = {".svg"}

RECORDING_TIMELINE = [
    {
        "path": ROOT / "Recordings/Movie_011.mp4",
        "sequence": 1,
        "date": "2026-04-21T18:21:13-03:00",
        "dateBasis": "Filesystem timestamp corroborated by visible prototype state",
        "milestone": "Functional race prototype",
        "summary": "Debug HUD, orange prototype surface, race loop, result dialog, and gear cluster.",
        "evidenceLabel": "HistoricalBuild",
        "buildStatus": "HistoricalRecordedBuild",
        "timelineFrame": (24.0, "T_Movie011PrototypeRace.png"),
    },
    {
        "path": ROOT / "Recordings/Movie_015.mp4",
        "sequence": 2,
        "date": "2026-07-03T08:55:50-03:00",
        "dateBasis": "Filesystem timestamp corroborated by visible integrated product state",
        "milestone": "Integrated meta and race loop",
        "summary": "Storage, several track shapes, gear-grid loadout, race feedback, and reward selection.",
        "evidenceLabel": "HistoricalBuild",
        "buildStatus": "HistoricalRecordedBuild",
        "timelineFrame": (30.0, "T_Movie015IntegratedRace.png"),
    },
    {
        "path": ROOT / "Recordings/Movie_001.mp4",
        "sequence": 3,
        "date": "2026-08-28T21:17:30-03:00",
        "dateBasis": "Filesystem modified timestamp corroborated by the most complete visible product state",
        "milestone": "Current full-loop capture",
        "summary": "Loading, track selection, loadout, three races, results, upgrade choice, and storage.",
        "evidenceLabel": "CurrentBuild",
        "buildStatus": "CurrentRecordedBuild",
        "timelineFrame": (35.0, "T_Movie001CurrentRace.png"),
    },
]

LATEST_FRAME_SPECS = [
    (12.5, "T_CurrentLoadingSplash.png", "LoadingSplash", "StyleReference"),
    (17.5, "T_CurrentMenuFigureEight.png", "MenuFigureEight", "CompositionReference"),
    (22.5, "T_CurrentRaceStartFigureEight.png", "RaceStartFigureEight", "GameplayReference"),
    (35.0, "T_CurrentRaceActionFigureEight.png", "RaceActionFigureEight", "GameplayReference"),
    (62.5, "T_CurrentMenuOval.png", "MenuOval", "CompositionReference"),
    (67.5, "T_CurrentRaceStartOval.png", "RaceStartOval", "GameplayReference"),
    (85.0, "T_CurrentRaceActionOval.png", "RaceActionOval", "GameplayReference"),
    (97.0, "T_CurrentResult.png", "Result", "UiLayoutReference"),
    (102.5, "T_CurrentBuildDiamond.png", "BuildDiamond", "UiLayoutReference"),
    (135.0, "T_CurrentRaceActionDiamond.png", "RaceActionDiamond", "GameplayReference"),
    (150.0, "T_CurrentRewardSelection.png", "RewardSelection", "UiLayoutReference"),
    (157.5, "T_CurrentStorage.png", "Storage", "UiLayoutReference"),
    (164.0, "T_CurrentItemDetail.png", "ItemDetail", "UiLayoutReference"),
]


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def git_metadata(path: Path) -> tuple[str | None, str | None]:
    relative = path.relative_to(ROOT).as_posix()
    historical_index = ROOT / "Artifacts/Marketing/Data/HistoricalSourceIndex.json"
    if historical_index.exists():
        sources = json.loads(historical_index.read_text(encoding="utf-8"))
        if relative in sources:
            source = sources[relative]
            return source["commitId"], source["date"]
    result = subprocess.run(
        ["git", "log", "-1", "--format=%H%x00%cI", "--", relative],
        cwd=ROOT,
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0 or not result.stdout.strip():
        return None, None
    commit_id, date = result.stdout.strip().split("\x00", 1)
    return commit_id, date


def image_metadata(path: Path) -> dict[str, Any]:
    with Image.open(path) as image:
        return {
            "width": image.width,
            "height": image.height,
            "mode": image.mode,
            "format": image.format,
        }


def video_metadata(path: Path) -> dict[str, Any]:
    result = subprocess.run(
        [
            "ffprobe",
            "-v",
            "error",
            "-select_streams",
            "v:0",
            "-show_entries",
            "stream=width,height,codec_name:format=duration",
            "-of",
            "json",
            str(path),
        ],
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        return {"probeError": result.stderr.strip() or "ffprobe failed"}
    payload = json.loads(result.stdout)
    stream = (payload.get("streams") or [{}])[0]
    return {
        "width": stream.get("width"),
        "height": stream.get("height"),
        "codec": stream.get("codec_name"),
        "durationSeconds": float((payload.get("format") or {}).get("duration", 0)),
    }


def asset_id(prefix: str, path: Path, digest: str) -> str:
    clean = "".join(character for character in path.stem.title() if character.isalnum())
    return f"{prefix}{clean}{digest[:10]}"


def record_for(path: Path, collection: str, evidence_label: str, build_status: str) -> dict[str, Any]:
    digest = sha256(path)
    commit_id, commit_date = git_metadata(path)
    relative = path.relative_to(ROOT).as_posix()
    stat = path.stat()
    metadata: dict[str, Any] = {"bytes": stat.st_size}
    if path.suffix.lower() in SUPPORTED_IMAGES:
        metadata.update(image_metadata(path))
    elif path.suffix.lower() in SUPPORTED_VIDEOS:
        metadata.update(video_metadata(path))
    elif path.suffix.lower() in SUPPORTED_VECTORS:
        metadata.update({"format": "SVG", "scalable": True})
    return {
        "assetId": asset_id(collection, path, digest),
        "sourceType": "LocalRepository" if relative.startswith("Assets/") else "LocalArtifact",
        "sourceLocator": relative,
        "commitTaskId": commit_id,
        "date": commit_date or datetime.fromtimestamp(stat.st_mtime, timezone.utc).isoformat(),
        "buildStatus": build_status,
        "rights": "FirstPartyProject",
        "hash": {"algorithm": "SHA256", "value": digest},
        "evidenceLabel": evidence_label,
        "approvedUsages": ["EvidenceDossier", "InternalIdentityResearch"],
        "metadata": metadata,
    }


def extract_frame(source: Path, timestamp: float, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    result = subprocess.run(
        [
            "ffmpeg",
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-ss",
            f"{timestamp:.3f}",
            "-i",
            str(source),
            "-frames:v",
            "1",
            str(destination),
        ],
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        raise RuntimeError(f"Failed to extract {source.name} at {timestamp}s: {result.stderr.strip()}")


def build_labeled_sheet(
    entries: list[tuple[Path, str, str]],
    output: Path,
    columns: int,
    cell_size: tuple[int, int] = (280, 540),
) -> None:
    rows = math.ceil(len(entries) / columns)
    cell_width, cell_height = cell_size
    canvas = Image.new("RGB", (columns * cell_width, rows * cell_height + 72), "#0D1117")
    draw = ImageDraw.Draw(canvas)
    font = ImageFont.load_default()
    draw.text((24, 24), output.stem, fill="#F4F0E6", font=font)
    for index, (path, title, detail) in enumerate(entries):
        row, column = divmod(index, columns)
        x = column * cell_width
        y = 72 + row * cell_height
        preview = thumbnail(path, (cell_width - 24, cell_height - 78))
        canvas.paste(preview, (x + (cell_width - preview.width) // 2, y + 8))
        draw.text((x + 12, y + cell_height - 56), title[:42], fill="#33C8FF", font=font)
        draw.text((x + 12, y + cell_height - 36), detail[:48], fill="#F4F0E6", font=font)
    output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output, optimize=True)


def prepare_recording_evidence() -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[str]]:
    recording_records: list[dict[str, Any]] = []
    frame_records: list[dict[str, Any]] = []
    timeline_entries: list[tuple[Path, str, str]] = []

    for milestone in RECORDING_TIMELINE:
        source = milestone["path"]
        if not source.exists():
            continue
        record = record_for(
            source,
            "Recording",
            milestone["evidenceLabel"],
            milestone["buildStatus"],
        )
        record["date"] = milestone["date"]
        record["approvedUsages"] = ["EvidenceDossier", "GameplayClip", "PressKit", "TrailerSource"]
        record["metadata"].update(
            {
                "chronologySequence": milestone["sequence"],
                "dateBasis": milestone["dateBasis"],
                "milestone": milestone["milestone"],
                "summary": milestone["summary"],
            }
        )
        recording_records.append(record)

        timestamp, filename = milestone["timelineFrame"]
        timeline_path = TIMELINE_FRAME_DIR / filename
        extract_frame(source, timestamp, timeline_path)
        frame = record_for(
            timeline_path,
            "Frame",
            milestone["evidenceLabel"],
            f"FrameFrom{milestone['buildStatus']}",
        )
        frame["date"] = milestone["date"]
        frame["approvedUsages"] = ["EvidenceDossier", "PressKitCandidate", "AiImageReference"]
        frame["metadata"].update(
            {
                "derivedFrom": source.relative_to(ROOT).as_posix(),
                "sourceSha256": record["hash"]["value"],
                "timestampSeconds": timestamp,
                "milestone": milestone["milestone"],
            }
        )
        frame_records.append(frame)
        timeline_entries.append((timeline_path, milestone["milestone"], milestone["date"][:10]))

    latest_source = ROOT / "Recordings/Movie_001.mp4"
    latest_source_hash = sha256(latest_source) if latest_source.exists() else None
    latest_entries: list[tuple[Path, str, str]] = []
    if latest_source.exists():
        LATEST_FRAME_DIR.mkdir(parents=True, exist_ok=True)
        expected_names = {spec[1] for spec in LATEST_FRAME_SPECS}
        for existing in LATEST_FRAME_DIR.glob("T_Current*.png"):
            if existing.name not in expected_names:
                existing.unlink()
        for timestamp, filename, screen, role in LATEST_FRAME_SPECS:
            frame_path = LATEST_FRAME_DIR / filename
            extract_frame(latest_source, timestamp, frame_path)
            frame = record_for(frame_path, "Frame", "CurrentBuild", "FrameFromCurrentRecordedBuild")
            frame["date"] = RECORDING_TIMELINE[-1]["date"]
            frame["approvedUsages"] = [
                "EvidenceDossier",
                "PressKitCandidate",
                "AiImageReference",
                "StoreScreenshotSource",
            ]
            frame["metadata"].update(
                {
                    "derivedFrom": latest_source.relative_to(ROOT).as_posix(),
                    "sourceSha256": latest_source_hash,
                    "timestampSeconds": timestamp,
                    "screen": screen,
                    "referenceRole": role,
                }
            )
            frame_records.append(frame)
            latest_entries.append((frame_path, screen, f"{timestamp:.0f}s · {role}"))

    timeline_sheet = VIDEO_EVOLUTION_DIR / "T_VideoEvolutionTimeline.png"
    latest_sheet = VIDEO_EVOLUTION_DIR / "T_LatestBuildAiReferenceSheet.png"
    build_labeled_sheet(timeline_entries, timeline_sheet, columns=3, cell_size=(360, 700))
    build_labeled_sheet(latest_entries, latest_sheet, columns=5, cell_size=(240, 480))
    sheets = [
        timeline_sheet.relative_to(ROOT).as_posix(),
        latest_sheet.relative_to(ROOT).as_posix(),
    ]
    return recording_records, frame_records, sheets


def enumerate_media() -> tuple[list[dict[str, Any]], list[str]]:
    recording_records, frame_records, video_sheets = prepare_recording_evidence()
    records: list[dict[str, Any]] = recording_records + frame_records
    groups = [
        (
            ROOT / "Assets/GearEngine/Art",
            "Repo",
            "RepositoryAsset",
            "CurrentRepositoryAsset",
        ),
        (
            ROOT / "Artifacts/VisualTests/GearWorkspaceScreenSpace",
            "Current",
            "CurrentBuild",
            "WorkingTreeCapture",
        ),
        (
            ROOT / "Artifacts/VisualTests/UITransitionDemos",
            "Current",
            "CurrentBuild",
            "WorkingTreeCapture",
        ),
        (
            ROOT / "Artifacts/Marketing/Evidence/Historical",
            "History",
            "RepositoryAsset",
            "HistoricalRepositorySnapshot",
        ),
        (
            ROOT / "Artifacts/Marketing/Evidence/GeneratedConcepts",
            "Concept",
            "GeneratedConcept",
            "NotInBuild",
        ),
        (
            ROOT / "Artifacts/Marketing/BrandExploration",
            "Concept",
            "GeneratedConcept",
            "NotInBuild",
        ),
    ]
    for directory, prefix, label, status in groups:
        if not directory.exists():
            continue
        for path in sorted(directory.rglob("*")):
            if path.is_file() and path.suffix.lower() in SUPPORTED_IMAGES | SUPPORTED_VIDEOS | SUPPORTED_VECTORS:
                records.append(record_for(path, prefix, label, status))

    missing = [
        ("Screenshot20260727At141055", "019fa087-bc34-7f23-9fed-5a7134ccb426"),
        ("Screenshot20260727At141234", "019fa087-bc34-7f23-9fed-5a7134ccb426"),
        ("Screenshot20260727At131500", "019fa087-bc34-7f23-9fed-5a7134ccb426"),
    ]
    for name, task_id in missing:
        records.append(
            {
                "assetId": f"Missing{name}",
                "sourceType": "CodexTaskMetadata",
                "sourceLocator": f"codex-task:{task_id}/{name}.png",
                "commitTaskId": task_id,
                "date": "2026-07-27",
                "buildStatus": "ExpiredTemporaryCapture",
                "rights": "FirstPartyProject",
                "hash": None,
                "evidenceLabel": "MissingReference",
                "approvedUsages": ["EvidenceDossierMetadataOnly"],
                "metadata": {
                    "recoveryStatus": "Original temporary file is no longer available",
                    "reconstructionAllowed": False,
                },
            }
        )
    return sorted(records, key=lambda item: (item["evidenceLabel"], item["sourceLocator"])), video_sheets


def thumbnail(path: Path, size: tuple[int, int]) -> Image.Image:
    if path.suffix.lower() in SUPPORTED_IMAGES:
        with Image.open(path) as image:
            return ImageOps.contain(image.convert("RGB"), size, Image.Resampling.LANCZOS)
    if path.suffix.lower() in SUPPORTED_VECTORS:
        raster = path.with_name(f"{path.stem}512.png")
        if raster.exists():
            with Image.open(raster) as image:
                return ImageOps.contain(image.convert("RGB"), size, Image.Resampling.LANCZOS)
    frame = Image.new("RGB", size, "#1A2029")
    draw = ImageDraw.Draw(frame)
    draw.rectangle((20, 20, size[0] - 20, size[1] - 20), outline="#FFB347", width=4)
    draw.text((32, size[1] // 2 - 8), "VIDEO", fill="#FFB347")
    return frame


def build_contact_sheets(records: list[dict[str, Any]]) -> list[str]:
    CONTACT_SHEET_DIR.mkdir(parents=True, exist_ok=True)
    for old_sheet in CONTACT_SHEET_DIR.glob("EvidenceContactSheet*.png"):
        old_sheet.unlink()

    media_records = [
        record
        for record in records
        if record["evidenceLabel"] != "MissingReference"
        and (ROOT / record["sourceLocator"]).exists()
    ]
    cell_width, cell_height = 300, 250
    columns, rows = 5, 4
    per_page = columns * rows
    output_paths: list[str] = []
    font = ImageFont.load_default()

    for page_index in range(math.ceil(len(media_records) / per_page)):
        page_records = media_records[page_index * per_page : (page_index + 1) * per_page]
        canvas = Image.new("RGB", (columns * cell_width, rows * cell_height + 64), "#0D1117")
        draw = ImageDraw.Draw(canvas)
        title = f"Gear Engine Evidence Contact Sheet {page_index + 1}"
        draw.text((24, 22), title, fill="#F4F0E6", font=font)
        for index, record in enumerate(page_records):
            row, column = divmod(index, columns)
            x = column * cell_width
            y = 64 + row * cell_height
            source = ROOT / record["sourceLocator"]
            preview = thumbnail(source, (cell_width - 20, 178))
            canvas.paste(preview, (x + (cell_width - preview.width) // 2, y + 8))
            label = record["evidenceLabel"]
            name = source.name[:38]
            draw.text((x + 10, y + 192), label, fill="#33C8FF", font=font)
            draw.text((x + 10, y + 210), name, fill="#F4F0E6", font=font)
            draw.text((x + 10, y + 228), record["assetId"][:38], fill="#7E8998", font=font)
        output = CONTACT_SHEET_DIR / f"EvidenceContactSheet{page_index + 1:02d}.png"
        canvas.save(output, optimize=True)
        output_paths.append(output.relative_to(ROOT).as_posix())
    return output_paths


def main() -> None:
    records, video_sheets = enumerate_media()
    sheets = build_contact_sheets(records)
    counts: dict[str, int] = {}
    for record in records:
        counts[record["evidenceLabel"]] = counts.get(record["evidenceLabel"], 0) + 1
    payload = {
        "schemaVersion": "1.0.0",
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "project": "Gear Engine",
        "captureConstraint": (
            "Three first-party portrait recordings now prove gameplay evolution through a current full-loop "
            "capture. A fresh distributable build and clean device capture remain pending; no conceptual image "
            "is treated as gameplay proof."
        ),
        "counts": {"total": len(records), "byEvidenceLabel": counts},
        "contactSheets": sheets,
        "videoEvolutionSheets": video_sheets,
        "recordingChronology": [
            {
                "sequence": milestone["sequence"],
                "sourceLocator": milestone["path"].relative_to(ROOT).as_posix(),
                "date": milestone["date"],
                "dateBasis": milestone["dateBasis"],
                "milestone": milestone["milestone"],
                "summary": milestone["summary"],
            }
            for milestone in RECORDING_TIMELINE
            if milestone["path"].exists()
        ],
        "assets": records,
    }
    MANIFEST_PATH.parent.mkdir(parents=True, exist_ok=True)
    MANIFEST_PATH.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"manifest": str(MANIFEST_PATH), "counts": payload["counts"], "sheets": len(sheets)}))


if __name__ == "__main__":
    main()
