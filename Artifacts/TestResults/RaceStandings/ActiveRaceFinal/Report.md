# Unity Test Report

Generated: 2026-09-16T18:45:38.995543+00:00

## Test intent

Verify active race session registration, deferred engine start after car readiness, result display then currency persistence, and result-before-persistence under a stalled service. Fixtures supply analytics and use coroutine waiting for the cinematic delay instead of blocking the Unity thread.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | No XML | 0 | 0 | 1 | 0 | 0 |

## Failures


- `No result file` — Unity did not write NUnit XML.

## Relevant Unity log events


- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/ActiveRaceViewModelTests.cs(339,29): error CS0308: The non-generic type 'List' cannot be used with type arguments
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/ActiveRaceViewModelTests.cs(339,29): error CS0308: The non-generic type 'List' cannot be used with type arguments
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/ActiveRaceViewModelTests.cs(339,29): error CS0308: The non-generic type 'List' cannot be used with type arguments
- `EditMode.log` — Scripts have compiler errors.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

No test-declared visual evidence was created during this run.
