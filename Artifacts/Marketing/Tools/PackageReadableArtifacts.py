#!/usr/bin/env python3
"""Package readable HTML/PDF outputs with portable local media.

The global Markdown adapter preserves file URLs, but its loopback print server cannot
load those URLs. This deterministic post-render adapter copies every declared media file
into the static package, injects a live-DOM media resolver, and overlays the same authored
visuals onto the PDF's reserved visual areas.

The generated HTML contains serialized React state. Rewriting file URLs directly in that
document corrupts the hydration payload, so portable paths are applied only after the app
has initialized.
"""

from __future__ import annotations

import hashlib
import io
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path
from urllib.parse import quote

from PIL import Image
from pypdf import PdfReader, PdfWriter
from reportlab.pdfgen import canvas


ROOT = Path(__file__).resolve().parents[3]
REPORT_GENERATOR_OUTPUTS = Path("/Users/leonardosilva/Documents/HTML:PDF Edit/ReportGenerator/outputs")
REPORTS_ROOT = ROOT / "Artifacts/Marketing/Reports"


PACKAGES = [
    {
        "upstream": "gorn-submission-press-kit-client",
        "destination": "GORnSubmissionPressKitClient",
        "source": ROOT / "Docs/Marketing/GORnSubmissionPressKitClient.md",
        "pdf": "GORnSubmissionPressKitClient.pdf",
        "media": [
            ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentRaceActionFigureEight.png",
            ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentRewardSelection.png",
            ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentStorage.png",
            ROOT / "Assets/GearEngine/Art/Splash Screen/Game Icon.png",
        ],
        "masks": [
            (5, 390, 200, 360, 38, "#201C24"),
            (6, 370, 205, 438, 110, "#201C24"),
            (7, 370, 205, 438, 110, "#201C24"),
            (8, 370, 205, 438, 110, "#201C24"),
        ],
        "overlays": [
            (
                5,
                ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentRaceActionFigureEight.png",
                575,
                25,
                120,
                215,
            ),
            (
                6,
                ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentRewardSelection.png",
                500,
                80,
                180,
                330,
            ),
            (
                7,
                ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/LatestBuildFrames/T_CurrentStorage.png",
                500,
                80,
                180,
                330,
            ),
            (8, ROOT / "Assets/GearEngine/Art/Splash Screen/Game Icon.png", 515, 115, 170, 170),
        ],
    },
    {
        "upstream": "game-marketing-kit-exec-plan",
        "destination": "GameMarketingKitExecPlan",
        "source": ROOT / "Plans/GameMarketingKit/GameMarketingKit-ExecPlan.md",
        "pdf": "GameMarketingKit-ExecPlan.pdf",
        "media": [],
        "masks": [],
        "overlays": [],
    },
    {
        "upstream": "game-evidence-library-client",
        "destination": "GameEvidenceLibraryClient",
        "source": ROOT / "Docs/Marketing/GameEvidenceLibraryClient.md",
        "pdf": "GameEvidenceLibraryClient.pdf",
        "media": [
            ROOT / "Artifacts/VisualTests/GearWorkspaceScreenSpace/Baseline.png",
            ROOT / "Assets/GearEngine/Art/Splash Screen/Game Icon.png",
            ROOT / "Assets/GearEngine/Art/UI/Cog Runner Screen 1 Ref.png",
            ROOT / "Artifacts/Marketing/Evidence/ContactSheets/EvidenceContactSheet01.png",
            ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/T_VideoEvolutionTimeline.png",
            ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/T_LatestBuildAiReferenceSheet.png",
        ],
        "masks": [
            (5, 390, 225, 390, 60, "#201C24"),
            (7, 30, 335, 310, 25, "#121117"),
            (7, 30, 35, 310, 250, "#121117"),
            (8, 390, 200, 390, 35, "#201C24"),
            (11, 390, 190, 390, 35, "#201C24"),
            (12, 390, 220, 390, 85, "#201C24"),
            (18, 390, 175, 390, 25, "#201C24"),
        ],
        "overlays": [
            (5, ROOT / "Artifacts/VisualTests/GearWorkspaceScreenSpace/Baseline.png", 620, 170, 120, 180),
            (
                7,
                ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/T_VideoEvolutionTimeline.png",
                35,
                40,
                300,
                235,
            ),
            (
                8,
                ROOT / "Artifacts/Marketing/Evidence/VideoEvolution/T_LatestBuildAiReferenceSheet.png",
                45,
                20,
                275,
                190,
            ),
            (11, ROOT / "Assets/GearEngine/Art/Splash Screen/Game Icon.png", 680, 165, 60, 60),
            (12, ROOT / "Assets/GearEngine/Art/UI/Cog Runner Screen 1 Ref.png", 650, 205, 100, 140),
            (
                18,
                ROOT / "Artifacts/Marketing/Evidence/ContactSheets/EvidenceContactSheet01.png",
                35,
                10,
                300,
                180,
            ),
        ],
    },
    {
        "upstream": "game-concept-creative-brief-client",
        "destination": "GameConceptCreativeBriefClient",
        "source": ROOT / "Docs/Marketing/GameConceptCreativeBriefClient.md",
        "pdf": "GameConceptCreativeBriefClient.pdf",
        "media": [
            ROOT / "Artifacts/Marketing/Evidence/GeneratedConcepts/GearEngineRouteBoard.png",
            ROOT / "Artifacts/Marketing/BrandExploration/GearEngineRouteMark512.png",
            ROOT / "Artifacts/Marketing/Evidence/GeneratedConcepts/CogRunnerRouteBoard.png",
            ROOT / "Artifacts/Marketing/BrandExploration/CogRunnerRouteMark512.png",
            ROOT / "Artifacts/Marketing/Evidence/GeneratedConcepts/ClockworkApexRouteBoard.png",
            ROOT / "Artifacts/Marketing/BrandExploration/ClockworkApexRouteMark512.png",
        ],
        "masks": [
            (5, 35, 590, 410, 84, "#121117"),
            (7, 35, 590, 410, 84, "#121117"),
            (8, 35, 590, 410, 84, "#121117"),
        ],
        "overlays": [
            (5, ROOT / "Artifacts/Marketing/Evidence/GeneratedConcepts/GearEngineRouteBoard.png", 55, 596, 116, 65),
            (5, ROOT / "Artifacts/Marketing/BrandExploration/GearEngineRouteMark512.png", 190, 596, 65, 65),
            (7, ROOT / "Artifacts/Marketing/Evidence/GeneratedConcepts/CogRunnerRouteBoard.png", 55, 596, 116, 65),
            (7, ROOT / "Artifacts/Marketing/BrandExploration/CogRunnerRouteMark512.png", 190, 596, 65, 65),
            (
                8,
                ROOT / "Artifacts/Marketing/Evidence/GeneratedConcepts/ClockworkApexRouteBoard.png",
                55,
                596,
                116,
                65,
            ),
            (8, ROOT / "Artifacts/Marketing/BrandExploration/ClockworkApexRouteMark512.png", 190, 596, 65, 65),
        ],
    },
]


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def draw_contained(page: canvas.Canvas, path: Path, x: float, y: float, width: float, height: float) -> None:
    with Image.open(path) as image:
        source_width, source_height = image.size
    if height <= 0:
        height = width * source_height / source_width
    scale = min(width / source_width, height / source_height)
    drawn_width = source_width * scale
    drawn_height = source_height * scale
    page.drawImage(
        str(path),
        x + (width - drawn_width) / 2,
        y + (height - drawn_height) / 2,
        width=drawn_width,
        height=drawn_height,
        preserveAspectRatio=True,
        mask="auto",
    )


def overlay_pdf(
    pdf_path: Path,
    overlays: list[tuple[int, Path, float, float, float, float]],
    masks: list[tuple[int, float, float, float, float, str]],
) -> None:
    source = PdfReader(str(pdf_path))
    writer = PdfWriter()
    by_page: dict[int, list[tuple[Path, float, float, float, float]]] = {}
    for page_index, media, x, y, width, height in overlays:
        by_page.setdefault(page_index, []).append((media, x, y, width, height))
    masks_by_page: dict[int, list[tuple[float, float, float, float, str]]] = {}
    for page_index, x, y, width, height, color in masks:
        masks_by_page.setdefault(page_index, []).append((x, y, width, height, color))

    for page_index, source_page in enumerate(source.pages):
        page_overlays = by_page.get(page_index, [])
        page_masks = masks_by_page.get(page_index, [])
        if page_overlays or page_masks:
            width = float(source_page.mediabox.width)
            height = float(source_page.mediabox.height)
            buffer = io.BytesIO()
            overlay = canvas.Canvas(buffer, pagesize=(width, height))
            for x, y, box_width, box_height, color in page_masks:
                overlay.setFillColor(color)
                overlay.setStrokeColor(color)
                overlay.rect(x, y, box_width, box_height, fill=1, stroke=0)
            for media, x, y, box_width, box_height in page_overlays:
                draw_contained(overlay, media, x, y, box_width, box_height)
            overlay.save()
            buffer.seek(0)
            source_page.merge_page(PdfReader(buffer).pages[0])
        writer.add_page(source_page)

    temporary = pdf_path.with_suffix(".packaging.pdf")
    with temporary.open("wb") as stream:
        writer.write(stream)
    temporary.replace(pdf_path)


def rewrite_html(package: dict, destination: Path) -> list[dict[str, str]]:
    media_dir = destination / "assets/media"
    media_dir.mkdir(parents=True, exist_ok=True)
    html_path = destination / "index.html"
    original_html = html_path.read_text(encoding="utf-8")
    manifest_media: list[dict[str, str]] = []
    media_map: dict[str, str] = {}
    for source in package["media"]:
        target = media_dir / source.name.replace(" ", "")
        shutil.copy2(source, target)
        raw_uri = source.as_uri()
        encoded_uri = "file://" + quote(str(source))
        replacement = f"./assets/media/{target.name}"
        media_map[raw_uri] = replacement
        media_map[encoded_uri] = replacement
        manifest_media.append(
            {
                "source": source.relative_to(ROOT).as_posix(),
                "packaged": target.relative_to(destination).as_posix(),
                "sha256": sha256(source),
            }
        )

    if media_map:
        serialized_media_map = json.dumps(media_map, ensure_ascii=True).replace("</", "<\\/")
        interactive_path = destination / "InteractiveApp.html"
        interactive_path.write_text(original_html, encoding="utf-8")
        resolver_path = destination / "assets/PackagedMediaResolver.js"
        resolver = f"""(() => {{
  const mediaMap = {serialized_media_map};
  const resolveMedia = (value) => {{
    if (!value) return value;
    if (mediaMap[value]) return mediaMap[value];
    try {{
      const decoded = decodeURI(value);
      return mediaMap[decoded] || value;
    }} catch (_error) {{
      return value;
    }}
  }};
  const rewriteNode = (root) => {{
    if (!root || !root.querySelectorAll) return;
    root.querySelectorAll("img[src],source[src],video[poster]").forEach((element) => {{
      const attribute = element.hasAttribute("poster") ? "poster" : "src";
      const current = element.getAttribute(attribute);
      const resolved = resolveMedia(current);
      if (resolved && resolved !== current) element.setAttribute(attribute, resolved);
    }});
  }};
  window.addEventListener("load", () => {{
    [100, 500, 1500].forEach((delay) => setTimeout(() => rewriteNode(document), delay));
  }}, {{ once: true }});
}})();
"""
        resolver_path.write_text(resolver, encoding="utf-8")
        if "</head>" not in original_html:
            raise RuntimeError(f"Missing head close tag in {html_path}")
        script_tag = '<script defer src="./assets/PackagedMediaResolver.js"></script>'
        alignment_style = """<style id="gear-engine-media-alignment">
.visual-panel .prose p:has(> img) {
  display: grid;
  place-items: center;
  width: 100%;
  margin: 12px auto 0;
  overflow: hidden;
}
.visual-panel .prose p > img {
  position: static !important;
  display: block;
  width: auto !important;
  max-width: 100% !important;
  height: auto !important;
  max-height: clamp(110px, 22vh, 280px) !important;
  margin: 0 auto !important;
  object-fit: contain;
}
</style>"""
        html = original_html.replace("</head>", f"{alignment_style}{script_tag}</head>", 1)
    else:
        html = original_html
    html_path.write_text(html, encoding="utf-8")
    return manifest_media


def package_artifact(package: dict) -> dict:
    upstream = REPORT_GENERATOR_OUTPUTS / package["upstream"]
    destination = REPORTS_ROOT / package["destination"]
    if destination.exists():
        shutil.rmtree(destination)
    shutil.copytree(upstream, destination)
    media = rewrite_html(package, destination)
    pdf_path = destination / package["pdf"]
    if package["overlays"]:
        overlay_pdf(pdf_path, package["overlays"], package["masks"])
    source_hash = sha256(package["source"])
    output_markdown = next(destination.glob("*.md"))
    if sha256(output_markdown) != source_hash:
        raise RuntimeError(f"Markdown source mismatch for {package['destination']}")
    package_manifest = {
        "schemaVersion": "1.0.0",
        "template": "lycan-scrollytelling",
        "adapter": "Artifacts/Marketing/Tools/PackageReadableArtifacts.py",
        "canonicalSource": package["source"].relative_to(ROOT).as_posix(),
        "canonicalSourceSha256": source_hash,
        "upstreamOutput": str(upstream),
        "outputDirectory": destination.relative_to(ROOT).as_posix(),
        "builtAt": datetime.now(timezone.utc).isoformat(),
        "media": media,
        "html": {
            "path": "index.html",
            "sha256": sha256(destination / "index.html"),
            "interactiveApp": "InteractiveApp.html" if package["media"] else None,
        },
        "pdf": {"path": package["pdf"], "sha256": sha256(pdf_path)},
    }
    (destination / "ReadableArtifactManifest.json").write_text(
        json.dumps(package_manifest, indent=2) + "\n",
        encoding="utf-8",
    )
    return package_manifest


def main() -> None:
    results = [package_artifact(package) for package in PACKAGES]
    print(json.dumps({"packaged": [result["outputDirectory"] for result in results]}))


if __name__ == "__main__":
    main()
