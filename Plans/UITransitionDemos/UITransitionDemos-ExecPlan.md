# Create interactive UI transition demo scenes

This ExecPlan is a living document.

## Purpose / Big Picture

Create two connected Unity sample scenes that demonstrate the transition filters
already provided by Coffee UIEffect. A developer can enter Play Mode, cycle through
the curated transition presets, replay a complete cover-and-reveal animation, and
load the companion scene through the selected transition.

## Progress

- [x] Inspect the existing UIEffect presets, playback API, scene conventions, and
  assembly boundaries.
- [x] Add the reusable sample-only transition controller.
- [x] Add a deterministic Editor generator for both scenes.
- [x] Generate and register both scenes.
- [x] Document usage and ownership.
- [x] Run focused formatting, compilation, log checks, and visual verification.

## Surprises & Discoveries

- The repository already contains `UIEffectsForEachDemo`, but it cycles the entire
  effect catalog and is not focused on transitions or scene changes.
- UIEffect defines transition rate `0` as the unmodified visible graphic and rate
  `1` as the fully transitioned graphic. A full-screen cover therefore reveals by
  playing forward and covers by playing in reverse.

## Decision Log

- Use native `UIEffectPreset` assets and `UIEffectTweener`; do not duplicate shader
  logic.
- Create two sample scenes under `Assets/GearEngine/Scenes/Test/UITransitions`.
- Keep the controller in presentation code and avoid adding a production transition
  service in this sample-focused change.
- Add both scenes to Editor Build Settings so their scene-change button works in a
  player build as well as in the Editor.

## Outcomes & Retrospective

The implementation produced two connected sample scenes with eight curated UIEffect
presets, deterministic scene regeneration, and three 1920 x 1080 visual captures.
The sample remains isolated from production navigation while documenting the intended
VContainer-based production boundary. Final compiler and Unity-log results are
recorded in `Docs/UnityErrorCheckReport.md`.

## Context and Orientation

Coffee UIEffect lives under `Assets/3rdParty/UIEffect`. Project presentation scripts
live under `Assets/GearEngine/Scripts/Game/GearEngine/Presentation`. Editor generation
tools live in the matching `Editor` assembly. The two scenes are sample assets and do
not modify production navigation.

## Plan of Work

Implement a scene-owned controller that applies a curated list of transition
presets, previews cover/reveal playback, serializes scene references, blocks
overlapping requests, and loads the companion scene after the cover completes.

Implement an Editor generator that creates a camera, screen-space camera Canvas,
EventSystem, responsive content, transition overlay, controls, and persistent button
listeners. Generate gallery and destination variants with distinct visual themes.

## Concrete Steps

1. Add `UITransitionDemoController`.
2. Add `UITransitionDemoSceneGenerator`.
3. Run the generator in an isolated Unity project and copy the generated assets.
4. Add the scene GUIDs to Editor Build Settings.
5. Add `Docs/UIEffects/UITransitionDemoScenes.md`.
6. Format changed C# files, compile affected assemblies, scan Unity logs, and inspect
   both scenes in Play Mode.

## Validation and Acceptance

- Both scenes open without missing scripts.
- Previous, Replay, Next, and scene-change buttons are visible and interactive.
- Replay performs a complete cover and reveal.
- Scene change covers the current scene and the destination reveals itself.
- The preset label follows the selected transition.
- Changed C# files pass focused fix/check and source-structure validation.
- Affected runtime and Editor assemblies compile with zero errors.
- Visual evidence is stored under `Artifacts/VisualTests/UITransitionDemos`.

## Idempotence and Recovery

The generator recreates the two sample scenes deterministically and replaces only
those scene files. Existing build settings entries remain unchanged; the generator
adds the two sample entries only when missing.

## Artifacts and Notes

Expected scenes:

- `Assets/GearEngine/Scenes/Test/UITransitions/UITransitionsGallery.unity`
- `Assets/GearEngine/Scenes/Test/UITransitions/UITransitionsDestination.unity`

## Interfaces and Dependencies

- `Coffee.UIEffects.UIEffect`
- `Coffee.UIEffects.UIEffectPreset`
- `Coffee.UIEffects.UIEffectTweener`
- Unity UGUI, Input System UI, and Scene Management
