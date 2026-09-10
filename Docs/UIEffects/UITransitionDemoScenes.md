# UI Transition Demo Scenes

## Purpose

The UI transition samples demonstrate the native Coffee UIEffect transition presets
in a focused, interactive flow. They are presentation samples and do not replace the
project's production navigation architecture.

## Scenes

- `Assets/GearEngine/Scenes/Test/UITransitions/UITransitionsGallery.unity`
  presents the curated transition gallery.
- `Assets/GearEngine/Scenes/Test/UITransitions/UITransitionsDestination.unity`
  provides a second visual state and automatically reveals itself after loading.

Both scenes are registered in Editor Build Settings.

## Controls

- **Previous** and **Next** select a transition preset and immediately preview it.
- **Replay Transition** covers the scene, briefly holds, and reveals it again.
- **Open Destination** or **Return To Gallery** covers the current scene before
  loading its companion.

The sample includes Fade, Burn, Dissolve, Square, Diamond, Stripe, Melt, and Blaze
presets. The Square preset is the closest existing baseline for the planned lateral
race-flag transition.

## Architecture

`UITransitionDemoController` owns only scene-local presentation behavior. It uses the
existing `UIEffectPreset` and `UIEffectTweener` APIs, rejects overlapping playback,
and carries only the selected preset index between the two sample scenes.

Production integration should place scene loading behind the project's navigation
service and inject a transition orchestrator through VContainer. The demo deliberately
does not create a global singleton or modify production scene flow.
