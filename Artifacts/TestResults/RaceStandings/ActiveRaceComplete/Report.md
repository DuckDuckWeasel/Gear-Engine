# Unity Test Report

Generated: 2026-09-16T18:48:22.425045+00:00

## Test intent

Race lifecycle checks with current Currency dependencies (analytics and event bus), deferred start, and nonblocking waits for the existing finish delay. Expected result-before-persistence and one server currency update.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Failed(Child) | 3 | 2 | 1 | 0 | 0 |

## Failures


- `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests.WhenTrackCompletes_OpensResultPopupAndCreditsCurrency` — Expected: 1

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

No test-declared visual evidence was created during this run.
