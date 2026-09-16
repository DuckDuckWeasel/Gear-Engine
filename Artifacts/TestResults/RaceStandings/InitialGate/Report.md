# Unity Test Report

Generated: 2026-09-16T18:34:08.398777+00:00

## Test intent

Verify score-only stars independently from time rankings; stable synthetic rivals and top-three/player visibility; fourth-to-third and unchanged-third presentation; one gear pick for first place OR at least one star; gear return/skip and result-before-persistence; prefab references and four portrait layouts. Network, ads and destinations use existing test substitutes.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.(RaceStandingsTests|RaceResultModelTests|ResultPopupViewModelTests|PostRaceScreenTests|CampaignScreenReferenceTests|RoguelikeViewModelTests|ActiveRaceViewModelTests.WhenResultPersistenceStalls_StillOpensResultPopup)`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Failed(Child) | 44 | 42 | 2 | 0 | 0 |

## Failures


- `GearEngine.Campaign.Tests.Editor.RoguelikeViewModelTests.LoadRoll_PopulatesPerkOptions` — System.Reflection.TargetInvocationException : Exception has been thrown by the target of an invocation.
- `GearEngine.Campaign.Tests.Editor.RoguelikeViewModelTests.PickPerk_AddsItem_ConsumesRoll_OpensMain` — System.Reflection.TargetInvocationException : Exception has been thrown by the target of an invocation.

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

| Test | Result | Scenario | Criteria | Media |
| --- | --- | --- | --- | --- |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/Campaign_ResultPopupView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/Campaign_ResultPopupView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/Campaign_ResultPopupView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/Campaign_ResultPopupView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | FourthBeforePromotion | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [FourthBeforePromotion1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/FourthBeforePromotion1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | GearReward | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [GearReward1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/GearReward1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_RaceProgressView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_RaceProgressView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_RaceProgressView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_RaceProgressView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_RaceProgressView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_RaceProgressView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_RaceProgressView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_RaceProgressView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_RaceProgressView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_ReceivedRewardsView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_ReceivedRewardsView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_ReceivedRewardsView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/PFB_ReceivedRewardsView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.Standings_ShowPoorPlacementWithThreeStarsAndUnchangedThird` | Passed | FourthPlaceThreeStars | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [FourthPlaceThreeStars1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/FourthPlaceThreeStars1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.Standings_ShowPoorPlacementWithThreeStarsAndUnchangedThird` | Passed | UnchangedThirdPlace | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [UnchangedThirdPlace1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RaceStandings/Runtime/UnchangedThirdPlace1080x2280.png) |

All media created during this run is associated with a test above.
