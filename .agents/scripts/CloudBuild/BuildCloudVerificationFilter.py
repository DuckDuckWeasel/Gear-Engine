#!/usr/bin/env python3
"""Select Cloud Verification tests eligible for a Unity player target."""

from __future__ import annotations

import argparse
import json
from pathlib import Path


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--catalog", required=True, type=Path)
    parser.add_argument("--build-target", required=True)
    return parser.parse_args()


def normalize_target(build_target: str) -> str:
    normalized = build_target.lower()
    if "android" in normalized:
        return "Android"
    if "osx" in normalized or "macos" in normalized:
        return "MacOS"
    raise ValueError(f"Unsupported Cloud Verification build target: {build_target}")


def select_test_names(catalog: dict, build_target: str) -> list[str]:
    target = normalize_target(build_target)
    return [test["fullName"] for test in catalog.get("tests", []) if target in test.get("targets", [])]


def main() -> int:
    arguments = parse_arguments()
    catalog = json.loads(arguments.catalog.read_text(encoding="utf-8"))
    test_names = select_test_names(catalog, arguments.build_target)
    if not test_names:
        raise SystemExit("No Cloud Verification tests support the selected build target.")

    print(";".join(test_names))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
