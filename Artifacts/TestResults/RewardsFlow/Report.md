# Unity Test Report

Generated: 2026-09-16T22:56:03.674397+00:00

## Test intent

Verify Results -> Gold -> eligible gear selection -> awarded gear -> Home, skipping gear directly Home, no repeated reward/pick/navigation, and changed post-race portrait rendering at four heights. Progress must not be entered.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.(ResultPopupViewModelTests|PostRaceScreenTests)`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Passed | 12 | 12 | 0 | 0 | 0 |

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
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [Campaign_ResultPopupView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | FourthBeforePromotion | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [FourthBeforePromotion1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/FourthBeforePromotion1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | GearReward | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [GearReward1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/GearReward1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x1920.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x2400.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.Standings_ShowPoorPlacementWithThreeStarsAndUnchangedThird` | Passed | FourthPlaceThreeStars | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [FourthPlaceThreeStars1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/FourthPlaceThreeStars1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.Standings_ShowPoorPlacementWithThreeStarsAndUnchangedThird` | Passed | UnchangedThirdPlace | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [UnchangedThirdPlace1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Portrait/UnchangedThirdPlace1080x2280.png) |

All media created during this run is associated with a test above.
