# Victor screen migration evidence

## Reading the captures

- `Reference`: AnimationAnimora authored Build, Race and explicitly named new reward compositions. These use demo data.
- `Before`: playable prefabs before migration, rendered independently with authored preview data.
- `After`: migrated prefabs with animations evaluated to their visible state. Results receives five representative result rows. Shared scene-owned board content is absent from these isolated renders.
- `Runtime`: the actual running campaign, Store, Garage and item popup with live data. These are the primary evidence for bindings and gameplay composition.

Each set uses 1080x2280, 1080x1920, 1080x2400 and 1080x1680. Runtime captures temporarily route screen-space canvases through a render camera and restore their settings. They do not emulate device notches or prove pointer hit regions. World-space geometry retains the running camera state. Source demo and runtime data are not identical; comparisons support composition and design provenance, not pixel-equivalence claims.

The final Race set uses one frozen simulation frame across all four dimensions. `Runtime/Flow.txt` records board identity, interaction state, EventSystem count and drag-handler results across separate play sessions.

Inactive blank reference renders and camera-only intermediate images were discarded. Validation details and known gaps are in `Plans/VictorScreensMigration/Validation.md`.

## Capture counts

- After: 28 PNGs
- Before: 28 PNGs
- Reference: 12 PNGs
- Runtime: 32 PNGs
