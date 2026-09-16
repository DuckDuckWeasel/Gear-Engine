# Unity Test Report

Generated: 2026-09-16T18:43:39.010430+00:00

## Test intent

Final post-cleanup verification: score stars independent of time-ranked standings; single gear selection for first place OR one star; gear resume/skip; all active-race lifecycle tests including result-before-persistence; shared-board reference regression; repeated opening/closing and four portrait captures. Includes the mandatory analyzer-driven method extraction, without backend changes.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.(RaceStandingsTests|RaceResultModelTests|ResultPopupViewModelTests|PostRaceScreenTests|CampaignScreenReferenceTests|RoguelikeViewModelTests|ActiveRaceViewModelTests)`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Failed(Child) | 46 | 44 | 2 | 0 | 0 |

## Failures


- `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests.Initialize_CreatesSessionRegistersRunnerAndStartsEngine` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests.WhenTrackCompletes_OpensResultPopupAndCreditsCurrency` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:

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
