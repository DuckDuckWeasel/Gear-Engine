# Unity Test Report

Generated: 2026-09-16T18:30:40.797834+00:00

## Test intent

Reproduce first configured track remaining locked when a fresh player has a locally valid saved track that no longer belongs to the remote catalog; also verify the normal empty-save case.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.MainTrackNavigationTests.FreshPlayer_AlwaysStartsWithFirstConfiguredTrackUnlocked`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Failed(Child) | 2 | 1 | 1 | 0 | 0 |

## Failures


- `GearEngine.Campaign.Tests.Editor.MainTrackNavigationTests.FreshPlayer_AlwaysStartsWithFirstConfiguredTrackUnlocked("Track2")` — Expected: True

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

No test-declared visual evidence was created during this run.
