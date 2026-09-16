# Unity Test Report

Generated: 2026-09-16T03:12:38.201541+00:00

## Test intent

Victor standalone Results, Rewards, and Progress: real runtime data, registered navigation, visible text after reopening at four portrait sizes, one continuation destination, reward sequence, and result-before-persistence behavior. Rendering enters Play mode within the selected Editor coroutine. Network and ads are isolated.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.(ResultPopupViewModelTests|CampaignScreenReferenceTests|PostRaceScreenTests|RaceResultModelTests|ActiveRaceViewModelTests.WhenResultPersistenceStalls_StillOpensResultPopup)`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Failed(Child) | 25 | 24 | 1 | 0 | 0 |

## Failures


- `GearEngine.Campaign.Tests.Editor.RaceResultModelTests.WhenTrackHasTiers_ScoreAndGoldMatchTierReward` — Expected: 1

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

| Test | Result | Scenario | Criteria | Media |
| --- | --- | --- | --- | --- |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [Campaign_ResultPopupView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/Campaign_ResultPopupView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [Campaign_ResultPopupView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/Campaign_ResultPopupView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [Campaign_ResultPopupView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/Campaign_ResultPopupView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [Campaign_ResultPopupView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/Campaign_ResultPopupView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_RaceProgressView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_RaceProgressView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_RaceProgressView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_RaceProgressView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_RaceProgressView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_RaceProgressView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_RaceProgressView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_RaceProgressView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_ReceivedRewardsView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_ReceivedRewardsView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_ReceivedRewardsView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_ReceivedRewardsView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_ReceivedRewardsView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_ReceivedRewardsView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView after reopening | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; No invented placement | [PFB_ReceivedRewardsView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/VictorPostRaceScreens/Runtime/PFB_ReceivedRewardsView1080x2400.png) |

All media created during this run is associated with a test above.
