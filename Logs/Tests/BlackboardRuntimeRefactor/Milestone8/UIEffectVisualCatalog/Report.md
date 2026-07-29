# Unity Test Report

Generated: 2026-07-27T20:04:22.861158+00:00

## Test intent

Validate explicit Direct GameObject data survives managed scene deserialization, runtime cloning, and Button-triggered UI effect execution.

## Selection

- Project: `/Users/leonardosilva/.codex/worktrees/a62b/Gear Engine`
- Platform mode: `edit`
- Selector: `GearEngine.GearEngine.Tests.Editor.UIEffectPresetCatalogSceneVisualTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Passed | 1 | 1 | 0 | 0 | 0 |

## Failures

None.

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

| Test | Result | Scenario | Criteria | Media |
| --- | --- | --- | --- | --- |
| `GearEngine.GearEngine.Tests.Editor.UIEffectPresetCatalogSceneVisualTests.CatalogScene_ButtonClick_AppliesAndRendersEveryPreset` | Passed | Each configured UIEffect preset is applied by the scene Blackboard's button-click path and captured in one contact sheet. | All 78 configured presets are reached through Button.onClick.; Each thumbnail contains the rendered scene after its corresponding preset was applied.; The contact sheet uses 8 columns and 10 rows. | [AllPresetsContactSheet.png](Media/AllPresetsContactSheet.png) |

All media created during this run is associated with a test above.
