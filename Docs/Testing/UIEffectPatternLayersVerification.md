# UIEffect Pattern Layers Verification

## Scope

This report covers the four-layer native `Coffee.UIEffects` `Transition.Pattern` implementation and its migration, propagation, material binding, and ordered rendering paths.

## Automated Coverage

- Legacy `UIEffectPreset` and `UIEffect` fields migrate to layer `0`.
- Repeated migration preserves an existing layer array and remains idempotent.
- Preset load, save, append-compatible copy, and replica context paths retain all four layers.
- All four indexed material parameter sets receive independent values.
- Indexed access rejects values outside the fixed `0` through `3` range.
- Render-backed tests validate red/blue alpha-over ordering, disabled and zero-opacity layers, and multiplication by sampled texture alpha.

## Verification Results

| Check | Result | Evidence |
| --- | --- | --- |
| C# formatting | Passed | Fix and check modes passed for both added fixtures. |
| C# source structure | Passed | Each added fixture contains one matching top-level type. |
| Assembly-CSharp build | Passed | Scoped generated project build completed with zero errors after the package cache refresh. |
| Unity Editor log scan | Passed | The Unity log parser found no compilation errors or exceptions in the current `Editor.log`. |
| UIEffect preset catalog EditMode execution | Passed | `UIEffectPresetCatalogSceneVisualTests` applied all 78 configured presets through the Blackboard button-click path and generated a single contact sheet. |
| Repository validation gate | Pending | Must run after the focused Unity tests once the Editor is free. |

## Remaining Manual Gate

Run the `GearEngine.GearEngine.Tests.Editor.UIEffectPatternLayerTests` and `GearEngine.GearEngine.Tests.Editor.UIEffectPatternRenderingTests` fixtures. The render-backed fixture is the required shader compilation and ordered alpha-over validation gate. It emits `OrderedAlphaOver.png` and `SampledTextureAlpha.png` with JSON evidence sidecars.
