# Unity Test Report

Generated: 2026-07-27T00:19:48.637382+00:00

## Test intent

Final affected EditMode gate after Active Race read-only binding: validate screen-space mapping, staggered rows, UI-only targets, referenced Gear prefabs, workspace ownership, Safe Area, UI rendering, and visual evidence.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine`
- Platform mode: `edit`
- Selector: `GearEngine.GearEngine.Tests.Editor.BoardLayoutScreenSpaceTests;GearEngine.GearEngine.Tests.Editor.BoardScreenPositionUtilityTests;GearEngine.GearEngine.Tests.Editor.DragTargetFinderUiTests;GearEngine.GearEngine.Tests.Editor.GearWorkspaceAssetTests;GearEngine.GearEngine.Tests.Editor.SafeAreaRectTransformTests;GearEngine.GearEngine.Tests.Editor.GearViewTests;GearEngine.GearEngine.Tests.Editor.GearViewSpawnerTests;GearEngine.GearEngine.Tests.Editor.GearInventoryViewComponentTests;GearEngine.GearEngine.Tests.Editor.BoardGearAnimatorTests;GearEngine.GearEngine.Tests.Editor.GearWorkspaceVisualTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Passed | 31 | 31 | 0 | 0 | 0 |

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
| `GearEngine.GearEngine.Tests.Editor.GearWorkspaceVisualTests.Workspace_PortraitResolution_RendersInsideSafeArea` | Not found in NUnit XML | Baseline portrait workspace at 1080x1920 | Board, Inventory, Trash, and Gear UI remain inside the configured Safe Area using screen-space rendering. | [Baseline.png](/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/VisualTests/GearWorkspaceScreenSpace/Baseline.png) |
| `GearEngine.GearEngine.Tests.Editor.GearWorkspaceVisualTests.Workspace_PortraitResolution_RendersInsideSafeArea` | Not found in NUnit XML | Short portrait workspace at 1080x1680 | Board, Inventory, Trash, and Gear UI remain inside the configured Safe Area using screen-space rendering. | [Short.png](/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/VisualTests/GearWorkspaceScreenSpace/Short.png) |
| `GearEngine.GearEngine.Tests.Editor.GearWorkspaceVisualTests.Workspace_PortraitResolution_RendersInsideSafeArea` | Not found in NUnit XML | Tall portrait workspace at 1080x2400 | Board, Inventory, Trash, and Gear UI remain inside the configured Safe Area using screen-space rendering. | [Tall.png](/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/VisualTests/GearWorkspaceScreenSpace/Tall.png) |

All media created during this run is associated with a test above.
