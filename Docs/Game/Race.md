# Race module

Runtime code lives in `Assets/Scripts/Game/Race/` (`Game.Race` assembly). It composes the gear board (`Game.GearEngine`) and car track simulation (`Game.CarSimulation`) behind a single screen.

## Flow

- **`RaceScope`** — Inherits **`SceneFoundationScope`** (shared Addressables, navigation, events, cross-layer resolver); installs gear mechanics (`GearMechanicsInstaller`), car track (`CarTrackInstaller`), and registers **`RaceBootstrap`**. See [`SceneFoundation.md`](SceneFoundation.md).
- **`RaceBootstrap`** — On startup, calls `navigation.Open(new RaceViewModel(startData))` with serialized **`RaceStartData`** (track definition, car definition, optional `GearEngineStartData` for board layout).
- **`RaceViewModel`** — Injects domain services, builds `BoardViewModel` and `TrackViewModel`, and exposes **`ToggleRace()`** (toggles the gear engine and track simulation on/off together).
- **`RaceView`** — Binds **`BoardView`**, **`Track`**, and a **Race** UI button. Does not auto-start the simulation on bind (unlike `CarTrackTestView`).

## Track themes

- Each `TrackDefinition` can reference a reusable `TrackThemeDefinition` asset. A theme can replace the environment prefab, road and ground materials, and the prop-placement rules without duplicating the spline or race setup.
- Theme prop rules use the existing `SplinePropGenerator` rule types (`All`, `Curves`, `Straights`, `Inside`, and `Outside`). Disable **Use Default Props** on the theme to replace the shared-prefab defaults; an empty custom rule list intentionally produces no props.
- `TrackViewComponent` applies the theme and generates its props whenever a track is initialized, so the same look appears in campaign preview, setup, active race, and player builds. The active-race FrustumFit completion can regenerate the props after the final fit without changing this shared runtime fallback.
- Leave `Theme` empty on a track to preserve the current shared Desert presentation. This fallback keeps existing track assets compatible while themed assets are authored incrementally.
- Large themed scenery stays outside the drivable corridor: edge props are centered 11 units from the spline with only 0.5 units of lateral jitter, interior props start 12 units from their spawning segment, and every candidate is rejected if it falls within 8.5 units of any other part of the spline. Small traffic cones use a separate sparse rule on straight sections, centered 4.8 units from the spline with a 3.8-unit global clearance, so they can be contacted without forming a hard wall across the racing line.
- Each race keeps one biome, while `TrackDefinition.VisualVariant` applies one of four subtle ground/environment tints. The theme exporter assigns variants across tracks in each biome and renders four preview images per biome with different prop layouts.
- Themes can define non-colliding surface zones and an ambient particle prefab. Surface zones are visible patches on the road; the active spline driver reads their normalized position and width to apply a bounded speed change and, for ice, a small lateral slide. Desert quicksand slows to 73%, forest grass to 90%, mountain mud to 82%, and glacial ice to 92% with a lateral nudge. The ambient VFX prefabs use a serialized particle material and texture so they are included in player builds.

## Navigation and scene layout

- Register **`RaceViewModel` → `RaceView`** via a **`ViewConfig`** asset and **`Navigation Settings`** (same pattern as `GearEngineViewConfig`).
- A minimal **navigation stub prefab** may back the ViewConfig `asset` field if required; the **playable hierarchy** is expected to live in the scene.
- Place a **`RaceView`** instance under the **navigation view holder** transform assigned on `RaceScope`. Scaffold navigation resolves and binds this pre-placed view when `Open` runs.
- Use a **`World`** (or similar) branch for prefab instances of **`Track.prefab`** and **`GridBoardCollider.prefab`** (gear grid with `BoardView`). Wire **`RaceView`** serialized references to those scene objects and to the Race button.

## Tests

EditMode tests: `Assets/Scripts/Game/Race/Tests/Editor/` (`Game.Race.Tests`).
