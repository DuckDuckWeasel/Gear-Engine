# Race HUD layout validation

## Scope

The Race telemetry panel now sits between the track and the shared gear board. The
numeric red bar is labeled `RPM`, speed/gear/RPM values reset when a race binds, and
the total score includes active drift points while they accumulate.

## Automated checks

- Unity fixture: `GearEngine.Campaign.Tests.Editor.CampaignScreenReferenceTests`
- Result: **15 passed, 0 failed, 0 skipped**
- NUnit evidence: [`Green/results.xml`](Green/results.xml)
- New regression coverage:
  - `RaceBoardLayout_ReservesHudSpaceAndRestoresSharedBoard`
  - `RaceHud_SitsBetweenTrackAndGearBoardAndLabelsRpm`
  - `RaceScore_UpdatesWhileDriftPointsAccumulate`

The initial batch attempt under `Red/` could not start because this exact worktree was
already open in Unity. It is retained as infrastructure evidence and is not counted as
a semantic failing test. The final fixture was run in the open Editor and exported as
native NUnit XML.

## Runtime inspection

The playable flow was exercised from Main to Setup to Race in the open Unity Editor.
The track remained in the upper region, the telemetry panel rendered immediately below
it, and the shared gear board rendered below the telemetry panel. Live speed, gear,
RPM, and score values changed during the race. Play mode was stopped after inspection.

## Known unrelated Editor findings

The existing invalid package `.meta` GUID messages and the previously documented
`Scaffold.Entities.VariableSO` missing-reference exception remain outside this layout
change. The focused fixture compiled and completed without a failure.
