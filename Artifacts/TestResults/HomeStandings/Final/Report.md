# Unity Test Report

Generated: 2026-09-17T00:26:10.917904+00:00

## Test intent

Verify Home time rankings independently from score stars, accepted star persistence across reload/lower scores, preview pose restoration on close/interruption, and real Quantum Link popup content/rarity/next/previous/reopening at 1080x2280 and 1080x1680.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `HomePresentationTests;TrackGameDataTests;RaceStandingsTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Passed | 24 | 24 | 0 | 0 | 0 |

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
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.GearPopup_ShowsSelectedCardOnOpenAndReopen` | Passed | GearPopupQuantumLink | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [GearPopupQuantumLink1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/GearPopupQuantumLink1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.GearPopup_ShowsSelectedCardOnOpenAndReopen` | Passed | GearPopupQuantumLink | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [GearPopupQuantumLink1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/GearPopupQuantumLink1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeSavedProgress | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeSavedProgress1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeSavedProgress1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeSavedProgress | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeSavedProgress1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeSavedProgress1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeUnraced | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeUnraced1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeUnraced1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeUnraced | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeUnraced1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeUnraced1080x2280.png) |

All media created during this run is associated with a test above.
