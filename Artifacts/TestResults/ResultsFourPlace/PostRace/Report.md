# Unity Test Report

Generated: 2026-09-17T03:01:01.523720+00:00

## Test intent

Results always shows four positions; a player without a previous placement starts at fourth and animates to the current time-ranked position; Home compact standings behavior remains separate.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorResultsFourPlaceTests`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Passed | 3 | 3 | 0 | 0 | 0 |

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
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars; Four-position standings retained | [Campaign_ResultPopupView1080x1680.png](../../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars; Four-position standings retained | [Campaign_ResultPopupView1080x1920.png](../../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars; Four-position standings retained | [Campaign_ResultPopupView1080x2280.png](../../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | Campaign_ResultPopupView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars; Four-position standings retained | [Campaign_ResultPopupView1080x2400.png](../../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | FourthBeforePromotion | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars; Four-position standings retained | [FourthBeforePromotion1080x2280.png](../../../VisualTests/RewardsFlow/Portrait/FourthBeforePromotion1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | GearReward | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [GearReward1080x2280.png](../../../VisualTests/RewardsFlow/Portrait/GearReward1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x1680.png](../../../VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x1920.png](../../../VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x1920.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x2280.png](../../../VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData` | Passed | PFB_ReceivedRewardsView | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [PFB_ReceivedRewardsView1080x2400.png](../../../VisualTests/RewardsFlow/Portrait/PFB_ReceivedRewardsView1080x2400.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.Standings_ShowPoorPlacementWithThreeStarsAndUnchangedThird` | Passed | FourthPlaceThreeStars | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [FourthPlaceThreeStars1080x2280.png](../../../VisualTests/RewardsFlow/Portrait/FourthPlaceThreeStars1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.Standings_ShowPoorPlacementWithThreeStarsAndUnchangedThird` | Passed | UnchangedThirdPlace | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Time-based standings and score-only stars | [UnchangedThirdPlace1080x2280.png](../../../VisualTests/RewardsFlow/Portrait/UnchangedThirdPlace1080x2280.png) |

All media created during this run is associated with a test above.
