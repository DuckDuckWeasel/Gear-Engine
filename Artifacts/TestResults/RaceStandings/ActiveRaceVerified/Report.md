# Unity Test Report

Generated: 2026-09-16T18:46:55.070313+00:00

## Test intent

Verify race setup, deferred start on car readiness, result then persistence and stalled persistence with current analytics dependency and coroutine timing.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Failed(Child) | 3 | 1 | 2 | 0 | 0 |

## Failures


- `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests.Initialize_CreatesSessionAndWaitsForCarBeforeStartingEngine` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Events.Contracts.IEventBus with Key:
- `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests.WhenTrackCompletes_OpensResultPopupAndCreditsCurrency` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Events.Contracts.IEventBus with Key:

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

No test-declared visual evidence was created during this run.
