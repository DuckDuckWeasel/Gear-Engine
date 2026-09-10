# Deliver the Game Marketing Kit

This ExecPlan is a living document.

## Purpose / Big Picture

Build a truthful, reusable marketing system for the game without changing Unity runtime content. The finished system will let a reviewer distinguish playable proof from repository history, generated design exploration, business hypotheses, and roadmap commitments. It will include an evidence archive, identity, UI/UX rebrand specification, Google Play package, press kit, player presentation, publisher pitch, investor overview, and a 30–45 second trailer animatic.

## Progress

- [x] Preserve and record the pre-existing dirty working tree.
- [x] Inventory first-party media, Git/LFS history, branches, Codex metadata, Drive search results, and public naming conflicts.
- [x] Recover representative historical repository assets with commit and date provenance.
- [x] Generate the evidence manifest and contact sheets.
- [x] Document the current Unity capture blocker without fabricating proof.
- [x] Create three independent identity routes and preliminary naming-risk notes.
- [x] Produce the client evidence library and creative brief sources.
- [ ] Approval gate: select the canonical identity route.
- [ ] Build the approved brand system, deterministic vector masters, icon suite, and tokens.
- [ ] Complete the design-only UI/UX audit, flows, mockups, states, and implementation specification.
- [ ] Capture the full current runtime flow after Unity 6000.5.3f1 becomes available.
- [ ] Produce authentic Google Play imagery and copy from the selected route and verified capture set.
- [ ] Assemble the press kit and its source manifest.
- [ ] Produce the player, publisher, and investor presentations.
- [ ] Produce the trailer storyboard, shot list, safe-area guide, and rough animatic.
- [ ] Run final media, HTML, PDF, accessibility, claim, and provenance verification.

## Surprises & Discoveries

- The host does not contain the Unity 6000.5.3f1 editor required by the project. Runtime capture is therefore blocked, while static repository and existing visual-test evidence remain inspectable.
- The current first-party art library contains 205 PNG files and one MP4. The MP4 is a ten-second portrait splash animation, not gameplay.
- The project identity is inconsistent across configuration and art: `Gear Engine`, `Cog Runner`, `Scaffold`, and `DDW` all appear in first-party material.
- Three July 27 Codex screenshots are referenced by task history, but their temporary files have expired. They remain `MissingReference` records and are not reconstructed.
- `Cog Runner` has a marketplace-search risk because an unrelated App Store product uses the phrase as a mini-game name.
- `Redline Relay`, the first new-name hypothesis, was rejected during discovery because an unrelated Unity game already uses that exact title.

## Decision Log

- Decision: treat the route-selection gate as binding. Production identity assets do not proceed until one route is selected.
  Rationale: mixing three visual systems into store or pitch assets would create rework and invalidate consistency checks.
- Decision: label imported references and historical sprites as `RepositoryAsset`, not `HistoricalBuild`.
  Rationale: a repository asset proves that a visual existed, but it does not prove a playable screen was rendered at that commit.
- Decision: use generated imagery only as `GeneratedConcept` during route exploration.
  Rationale: generated imagery cannot establish current functionality or replace missing gameplay evidence.
- Decision: use static verification for this phase.
  Rationale: no runtime files change and the required Unity version is unavailable.

## Outcomes & Retrospective

Phase one produced a searchable manifest, twelve contact sheets, nine Git/LFS milestone recoveries, six current visual-test captures, three missing-reference records, three generated route boards, three deterministic route-mark sketches, and two client-facing source documents. Later outcomes will be added after the identity decision and runtime capture unblock.

## Context and Orientation

Canonical human-readable sources live under `Docs/Marketing/`. Generated HTML, PDF, manifests, evidence, and media live under `Artifacts/Marketing/`. `Artifacts/Marketing/Data/EvidenceManifest.json` is the provenance source of truth. `Artifacts/Marketing/Data/ProjectDirectionManifest.json` records the selected route and the boundary between fact, reconstruction, hypothesis, and concept.

The verified campaign loop is: configure a gear system, start a race, observe mechanical consequences, receive a result, select a roguelike reward, and repeat. A complete runtime capture must include Main, Setup, Gear Board, Race, Result, Reward, Inventory or Upgrade, Store, tutorial, pause, loading, error, and recovery states when those states are reproducible.

## Plan of Work

First, maintain the evidence archive and close the Unity capture gap. Second, select exactly one identity route and develop its vector system, typography, palette, iconography, imagery, voice, and usage rules. Third, design the complete mobile UI/UX package without modifying Unity. Fourth, create only store imagery supported by a reproducible build. Fifth, package the press materials, three audience-specific presentations, and modular trailer animatic. Every public claim must resolve to an evidence-manifest asset or carry an explicit concept, hypothesis, projection, or roadmap label.

## Concrete Steps

1. Run `Artifacts/Marketing/Tools/RecoverHistoricalEvidence.py` to recover the curated milestone set from local Git/LFS objects.
2. Run `Artifacts/Marketing/Tools/BuildEvidenceLibrary.py` to rebuild hashes, metadata, counts, and contact sheets.
3. Review `Docs/Marketing/GameConceptCreativeBriefClient.md` and choose Route A, B, or C.
4. Update `ProjectDirectionManifest.json` with the approved route before generating production assets.
5. Install or make Unity 6000.5.3f1 available, capture the declared flow in portrait resolution, and rerun the evidence builder.
6. Produce the remaining artifacts in the sequence defined by the approval gate and validate them against the manifest.

## Validation and Acceptance

Phase one is accepted when every local media record has a SHA-256 hash and probe metadata, historical selections resolve to a commit/date/original path, missing media has no reconstructed substitute, generated boards are labeled `GeneratedConcept`, Markdown/HTML/PDF variants are synchronized, and all output filenames follow the repository naming policy.

Final acceptance additionally requires valid Google Play dimensions and copy limits, evidence-backed screenshots, verified icon and adaptive layers, accessible UI specifications, no unmarked claims in decks, PDF image inspection, responsive and keyboard-usable HTML, and a 30–45 second animatic with shot provenance and 16:9/9:16 safe areas.

## Idempotence and Recovery

The recovery and evidence scripts are repeatable. They overwrite only their own derived outputs. Source media is read-only. If a Git LFS object is unavailable, recovery must stop rather than substituting a current file. If generated concepts are regenerated, retain the prior file or update the manifest so hashes and reviews remain traceable. Never clean or reset the working tree to prepare this package.

## Artifacts and Notes

- `Artifacts/Marketing/Data/EvidenceManifest.json`: evidence records, counts, hashes, labels, and capture constraint.
- `Artifacts/Marketing/Data/HistoricalSourceIndex.json`: original Git path, commit, date, and milestone for recovered files.
- `Artifacts/Marketing/Evidence/ContactSheets/`: paginated visual index.
- `Docs/Marketing/GameEvidenceLibraryClient.md`: readable evidence dossier.
- `Docs/Marketing/GameConceptCreativeBriefClient.md`: route comparison and approval decision.

## Interfaces and Dependencies

No Unity runtime API, C# type, prefab, scene, or production UI asset is changed. Local dependencies are Git, Git LFS objects already present on disk, Pillow for contact sheets, `ffprobe` for video metadata, the repository ReportGenerator for synchronized readable artifacts, and Poppler for PDF rendering and inspection.
