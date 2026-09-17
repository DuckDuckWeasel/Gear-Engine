# Unity Test Report

Generated: 2026-09-17T01:06:40.663706+00:00

## Test intent

Recapture the Home track controls after moving the counter clear of the RACE button at 1080x2280 and 1080x1680.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings`

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
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeSavedProgress | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeSavedProgress1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeSavedProgress1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeSavedProgress | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeSavedProgress1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeSavedProgress1080x2280.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeUnraced | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeUnraced1080x1680.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeUnraced1080x1680.png) |
| `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings` | Passed | HomeUnraced | Runtime ViewModel bindings; Rendered text inside viewport; Animation restart; Selected gear card visibility and rarity | [HomeUnraced1080x2280.png](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/HomeStandings/HomeUnraced1080x2280.png) |

All media created during this run is associated with a test above.
