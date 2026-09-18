# Unity Test Report

Generated: 2026-09-16T18:32:02.639733+00:00

## Test intent

Verify that first configured track is unlocked, selected, and playable for new players with empty or outdated saved selection, and retain carousel wraparound, locking, replay, and unlock history behavior.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.MainTrackNavigationTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Passed | 10 | 10 | 0 | 0 | 0 |

## Failures

None.

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

No test-declared visual evidence was created during this run.

## Regression and validation

Before the fix, the empty-save case passed and the outdated saved-track case failed
because track one was locked (see `../Before/Report.md`). After the fix, all ten
focused tests passed. The first configured track is unlocked, selected, and can
open setup without any race history.

Scoped C# formatter fix/check and `validate-changes.ps1 -SkipTests` passed, including
fresh Unity compilation and analyzer diagnostics (`TOTAL:0`, `BLOCKERS:0`).
The full game test suites were skipped; the focused fixture above was executed.
