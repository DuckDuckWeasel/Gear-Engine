#!/usr/bin/env python3
"""Recover a representative, provenance-locked set of historical repository media."""

from __future__ import annotations

import json
import re
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parents[3]
OUTPUT = ROOT / "Artifacts/Marketing/Evidence/Historical"
INDEX_PATH = ROOT / "Artifacts/Marketing/Data/HistoricalSourceIndex.json"

SELECTIONS = [
    (
        "InitialGearSystem",
        "8eca297d",
        "Assets/GearEngine/Art/Sprites/BaseGear.png",
        "BaseGear.png",
        "Pre-refactor mechanical visual vocabulary",
    ),
    (
        "InitialGearSystem",
        "8eca297d",
        "Assets/GearEngine/Art/Sprites/CoreGear.png",
        "CoreGear.png",
        "Pre-refactor core gear asset",
    ),
    (
        "ImportedUi",
        "5b4413b1",
        "Assets/GearEngine/Art/UI/Cog Runner Screen 1 Ref.png",
        "CogRunnerScreen1Ref.png",
        "Imported UI reference; not a runtime capture",
    ),
    (
        "ImportedUi",
        "5b4413b1",
        "Assets/GearEngine/Art/UI/Atlas Cog Runner.png",
        "AtlasCogRunner.png",
        "Imported UI atlas",
    ),
    (
        "DesignSystemIteration",
        "9e691af5",
        "Assets/GearEngine/Art/UI/New Track Unlocked.png",
        "NewTrackUnlocked.png",
        "Modern UI Pack milestone asset",
    ),
    (
        "JulyGameplayIntegration",
        "4d2aa3d5",
        "Assets/GearEngine/Art/Splash Screen/Game Icon.png",
        "GameIcon.png",
        "Pop-art racing identity introduced by July milestone",
    ),
    (
        "JulyGameplayIntegration",
        "4d2aa3d5",
        "Assets/GearEngine/Art/Sprites/GearIcons/Boost.png",
        "Boost.png",
        "Implemented gear-content visual asset",
    ),
    (
        "ResultScreenVfxBranch",
        "c555f825",
        "Assets/GearEngine/Art/UI/Cards&Rewards/Background_Resize.png",
        "RewardBackground.png",
        "Reward presentation asset present on result-screen VFX branch",
    ),
    (
        "ResultScreenVfxBranch",
        "c555f825",
        "Assets/GearEngine/Art/UI/Cards&Rewards/Card_Legendary_Goldenstar.png",
        "LegendaryRewardCard.png",
        "Legendary reward card asset present on result-screen VFX branch",
    ),
]


def commit_date(commit: str) -> str:
    result = subprocess.run(
        ["git", "show", "-s", "--format=%cI", commit],
        cwd=ROOT,
        check=True,
        capture_output=True,
        text=True,
    )
    return result.stdout.strip()


def recover() -> None:
    index: dict[str, dict[str, str]] = {}
    for milestone, commit, source, filename, note in SELECTIONS:
        destination = OUTPUT / milestone / filename
        destination.parent.mkdir(parents=True, exist_ok=True)
        result = subprocess.run(
            ["git", "show", f"{commit}:{source}"],
            cwd=ROOT,
            check=True,
            capture_output=True,
        )
        recovered_bytes = result.stdout
        if recovered_bytes.startswith(b"version https://git-lfs.github.com/spec/v1"):
            pointer = recovered_bytes.decode("utf-8")
            match = re.search(r"oid sha256:([0-9a-f]{64})", pointer)
            if match is None:
                raise RuntimeError(f"Invalid Git LFS pointer for {commit}:{source}")
            oid = match.group(1)
            object_path = ROOT / ".git/lfs/objects" / oid[:2] / oid[2:4] / oid
            if not object_path.exists():
                raise RuntimeError(f"Git LFS object is not available locally: {oid} ({commit}:{source})")
            recovered_bytes = object_path.read_bytes()
        destination.write_bytes(recovered_bytes)
        relative = destination.relative_to(ROOT).as_posix()
        index[relative] = {
            "commitId": subprocess.run(
                ["git", "rev-parse", commit],
                cwd=ROOT,
                check=True,
                capture_output=True,
                text=True,
            ).stdout.strip(),
            "date": commit_date(commit),
            "originalPath": source,
            "milestone": milestone,
            "note": note,
        }
    INDEX_PATH.parent.mkdir(parents=True, exist_ok=True)
    INDEX_PATH.write_text(json.dumps(index, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"recovered": len(index), "index": str(INDEX_PATH)}))


if __name__ == "__main__":
    recover()
