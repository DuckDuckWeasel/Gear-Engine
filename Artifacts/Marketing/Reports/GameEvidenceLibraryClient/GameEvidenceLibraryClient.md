# Game Evidence Library

## Decision Snapshot

The project now has a verifiable recorded evolution from prototype race logic to a complete current gameplay loop. The archive contains **255 evidence records**: **215 repository assets**, **21 current-build records**, **4 historical-build records**, **12 generated route-concept assets**, and **3 missing Codex references**. Nine repository records remain curated historical recoveries tied to representative milestones.

This dossier makes a strict distinction between what exists in the current build, what only exists as a repository asset, what was recovered from history, what was generated as a concept, and what is missing.

> **Capture constraint:** Three first-party portrait recordings prove the visible gameplay evolution and current loop. A fresh distributable build and clean device capture are still pending. No generated or reconstructed image is presented as gameplay proof.

## Evidence Vocabulary

| Label | Meaning | Public-use rule |
| --- | --- | --- |
| `CurrentBuild` | Captured from the current working project or an existing current visual-test artifact | May support a current-build claim only when its screen and capture context are named |
| `HistoricalBuild` | Captured from a historical playable build | May support historical claims; none recovered in this phase |
| `RepositoryAsset` | First-party image or video stored in the repository or recovered from a commit | Proves the asset existed, not that a screen was playable |
| `InferredReconstruction` | A reconstruction based on incomplete sources | Must be visibly labeled; none used in this phase |
| `GeneratedConcept` | AI-generated or otherwise concept-only visual | Never used as gameplay evidence or an authentic store screenshot |
| `MissingReference` | Metadata exists but the original media is unavailable | Metadata only; reconstruction is prohibited as proof |

## Current Evidence

`Movie_001.mp4` is the latest recorded-build source. It proves loading, track selection, loadout configuration, three track shapes, live race feedback, a result state, reward selection, and storage. Thirteen lossless frames and one timeline frame derived from that video preserve exact timestamps and source hashes. Six visual-test captures remain available for the gear workspace and UI transition demonstrations.

The recordings do not yet prove tutorial, pause, error, recovery, or a public distributable package. Those claims remain outside the current evidence boundary.

![Current gear workspace evidence](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/VisualTests/GearWorkspaceScreenSpace/Baseline.png>)

## Recorded Gameplay Evolution

The filenames do not encode chronology reliably. The ordering below uses filesystem timestamps corroborated by visible product maturity. `Movie_001` is therefore the latest capture despite its lower numeric suffix.

| Sequence | Recording | Evidence date | Duration | Visible milestone |
| --- | --- | --- | ---: | --- |
| 1 | `Movie_011.mp4` | 2026-04-21 | 50.77 s | Functional race prototype with debug HUD, orange test surface, gear cluster, race loop, and result dialog |
| 2 | `Movie_015.mp4` | 2026-07-03 | 175.43 s | Integrated storage, loadout grid, several track shapes, race feedback, and reward selection |
| 3 | `Movie_001.mp4` | 2026-08-28 | 173.72 s | Current full-loop capture with loading, track selection, three races, results, upgrade choice, and storage |

![Chronological gameplay evolution](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/Marketing/Evidence/VideoEvolution/T_VideoEvolutionTimeline.png>)

## AI Reference Frame Pack

Thirteen 1080×1920 PNG frames from `Movie_001.mp4` are prepared for image direction. Every frame records its source video hash, timestamp, screen name, and reference role in `EvidenceManifest.json`.

![Latest-build AI reference frames](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/Marketing/Evidence/VideoEvolution/T_LatestBuildAiReferenceSheet.png>)

| Reference role | Frames | Use |
| --- | --- | --- |
| Style | Loading splash | Establish the pop-art racing energy and red hero car |
| Composition | Figure-eight and oval menus | Preserve portrait framing, track silhouette, action hierarchy, and negative space |
| Gameplay | Figure-eight, oval, and diamond races | Preserve the actual track, car, loadout grid, HUD, and mechanical feedback relationships |
| UI layout | Result, reward, storage, and item detail | Preserve verified screen architecture while exploring finish, lighting, texture, and promotional staging |

For AI-generated key art, use the frames as composition, object, palette, and UI references rather than edit targets unless a specific screen must remain pixel-identical. Preserve the track geometry, red car identity, cream interface field, coral action color, and gear-cluster logic. Generated outputs must remain labeled `GeneratedConcept`; only the untouched extracted frames qualify as gameplay evidence or store-screenshot sources.

## Repository Visual Assets

The first-party art directory contains 205 PNG files and one MP4. The library includes gear sprites, perk and reward icons, rarity cards, UI surfaces, environmental textures, a pop-art car icon, an imported `Cog Runner` screen reference, and a ten-second portrait splash animation. The splash animation is not gameplay.

![Current game icon repository asset](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Assets/GearEngine/Art/Splash Screen/Game Icon.png>)

![Imported Cog Runner UI reference](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Assets/GearEngine/Art/UI/Cog Runner Screen 1 Ref.png>)

These two images are useful identity inputs. The first is a current repository icon; the second is an imported UI reference. Neither alone proves a current playable screen.

## Historical Recovery Timeline

| Milestone | Commit | Date | Recovered proof | Interpretation |
| --- | --- | --- | --- | --- |
| Project baseline | `1d8a07ae` | 2026-04-07 | No first-party media under the current art root | Establishes the repository starting point |
| Pre-refactor gear visuals | `8eca297d` | April 2026 | Base and core gear sprites | Mechanical vocabulary existed before the broader art import |
| Imported UI | `5b4413b1` | 2026-05-13 | `Cog Runner` reference and atlas | Establishes the historical UI/name direction, not runtime implementation |
| Design-system iteration | `9e691af5` | 2026-06-05 | New-track unlocked UI asset | Establishes a polished UI-pack milestone |
| July gameplay integration | `4d2aa3d5` | July 2026 | Pop-art game icon and boost gear icon | Establishes the combined racing and gear identity vocabulary |
| Result-screen VFX branch | `c555f825` | July 2026 | Reward background and legendary card | Establishes reward presentation assets on that branch |

The historical files are copied byte-for-byte from local Git or Git LFS objects. `HistoricalSourceIndex.json` preserves each original path, commit, date, and milestone. They remain labeled `RepositoryAsset` because asset presence is not equivalent to a historical playable capture.

## Missing References

Codex task `019fa087-bc34-7f23-9fed-5a7134ccb426` references three screenshots dated July 27, 2026:

- `Screenshot20260727At141055.png`
- `Screenshot20260727At141234.png`
- `Screenshot20260727At131500.png`

The temporary files no longer exist. The manifest retains their names, task ID, and date with `reconstructionAllowed: false`. They are not shown or replaced.

## Source Reach

| Source | Result |
| --- | --- |
| Current repository | 206 first-party media files inventoried |
| Current visual-test artifacts | 6 relevant captures inventoried |
| Local gameplay recordings | 3 portrait recordings inventoried; 2 historical and 1 current |
| Git and local Git LFS | 9 representative milestone assets recovered |
| Branch history | 106 local and remote branches inspected; no committed gameplay video discovered |
| Codex task history | Three expired screenshot references recorded as metadata |
| Connected Google Drive | Searches for project, naming, gameplay, and Unity terms returned no relevant archive |
| Authorized public search | No verified official public project archive discovered |

## Contact Sheets and Search

Thirteen general contact sheets plus two video-specific sheets provide a visual index of the local records. The canonical machine-searchable source is `Artifacts/Marketing/Data/EvidenceManifest.json`, where records can be filtered by `assetId`, `sourceType`, `commitTaskId`, `date`, `buildStatus`, `evidenceLabel`, chronology, timestamp, source hash, dimensions, or approved usage.

![Evidence contact sheet](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/Marketing/Evidence/ContactSheets/EvidenceContactSheet01.png>)

## Required Current Capture Set

The recordings cover most of the core loop. A fresh reproducible capture at 1080×1920 or higher must close the remaining gaps and preserve clean source frames before promotional editing:

1. Main or campaign entry.
2. Race setup and track selection.
3. Gear board or loadout configuration.
4. Race start, readable HUD, and at least two mechanical consequence moments.
5. Finish and result state.
6. Roguelike reward selection.
7. Inventory or upgrade state.
8. Store, if implemented in the submitted build.
9. Tutorial and first-time guidance.
10. Pause, loading, error, and recovery states where reproducible.

Each capture must record project commit, working-tree status, Unity version, scene, date, operator, device or Game View dimensions, and whether development overlays are present.

## Store and Pitch Readiness

The current evidence is sufficient for identity discovery, a gameplay-evolution dossier, a rough gameplay-first trailer, and an evidence-led pitch appendix. It is not yet sufficient for a public release claim, a permission-free downloadable build, or eight final Google Play screenshot compositions. Those deliverables remain gated on a verified build and clean capture pass.

The store package must comply with the official Google Play preview-asset requirements and show actual functionality. Concept art may support a feature graphic if clearly used as art direction, but it must not replace authentic gameplay screenshots or imply unavailable features.

## Integrity Rules

- Keep raw captures separate from promotional compositions.
- Preserve SHA-256 hashes and source locators.
- Never upgrade a `RepositoryAsset` to `CurrentBuild` without a reproducible capture.
- Never recreate an expired screenshot and call it historical proof.
- Label every generated visual as `GeneratedConcept` in the manifest and in reader-facing contexts.
- Validate every deck and store claim against the manifest before publication.

## Canonical Data

- `Artifacts/Marketing/Data/EvidenceManifest.json`
- `Artifacts/Marketing/Data/HistoricalSourceIndex.json`
- `Artifacts/Marketing/Evidence/ContactSheets/`
- `Artifacts/Marketing/Evidence/VideoEvolution/`
- `Artifacts/Marketing/Evidence/Historical/`
