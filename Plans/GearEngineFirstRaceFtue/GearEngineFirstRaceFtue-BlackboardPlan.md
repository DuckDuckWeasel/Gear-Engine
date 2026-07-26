# Gear Engine first-race FTUE — minimal Blackboard plan

## Implementation status

Implemented in `PFB_FirstRaceTutorial.prefab` and configured through
`FirstRaceTutorial.asset`. The Campaign Setup screen starts the configured tutorial through
the existing `TutorialController`.

## Goal

Teach only the two actions required to reach the first race:

1. Drag one or more gears from the inventory to the board.
2. Press the Race button.

The sequence belongs to the tutorial Blackboard. Gameplay code must not contain tutorial
phases, tutorial-specific gear IDs, or tutorial-specific board coordinates.

## Existing building blocks

Use the tutorial prefab structure already supported by Scaffold:

- `Blackboard`
- `TutorialProgressController`
- `ScaffoldTutorialAdapter`

Use the existing Blackboard actions:

- `Show UI Focus`
- `Clear UI Focus`
- `Wait For Target Drop`
- `Wait For Target Drop At Index` only when a specific board cell is a real design
  requirement
- `Wait For Target Click`
- `Stop Blackboard`

`ScaffoldTutorialAdapter` reports the Blackboard block name as the tutorial step name, so
the block names below are also the analytics step IDs.

## Target references

Configure targets through tags or runtime anchors:

| Target | Purpose |
| --- | --- |
| `TutorialGear` | The gear that the player can drag |
| `TutorialBoard` | Any valid board drop area |
| `RaceButton` | The button that starts the race |

The first version does not filter by gear catalog ID or board coordinate. If the design
later requires a specific cell, replace only the drop action with
`Wait For Target Drop At Index`; do not add a phase manager.

## Blackboard blocks

### `FTUE_01_PLACE_GEAR`

Action order:

1. `Show UI Focus`
   - Target: `TutorialGear`
   - Preset: hand/arrow plus inventory highlight
2. `Wait For Target Drop`
   - Drag target: `TutorialGear`
   - Drop target: `TutorialBoard`
   - This action also restricts input to the configured drag and drop targets.
3. `Clear UI Focus`

Result: the player has performed the core board interaction once.

### `FTUE_02_PLACE_ANOTHER_GEAR` (optional)

Include this block only when one placement does not communicate the board interaction
well enough.

Action order:

1. `Show UI Focus`
   - Target: the next available `TutorialGear`
2. `Wait For Target Drop`
   - Drag target: `TutorialGear`
   - Drop target: `TutorialBoard`
3. `Clear UI Focus`

Result: the player repeats the learned action with less explanation.

### `FTUE_03_START_RACE`

Action order:

1. `Show UI Focus`
   - Target: `RaceButton`
   - Preset: button spotlight/arrow
2. `Wait For Target Click`
   - Target: `RaceButton`
3. `Clear UI Focus`

Result: the player starts the first race.

### `FTUE_04_COMPLETE`

Action order:

1. Complete the current `TutorialProgressController`.
2. `Stop Blackboard`

The small generic `Complete Tutorial` action calls
`TutorialProgressController.CompleteProgress(false)` on the tutorial prefab. A matching
skip variant is unnecessary for this short sequence.

## Minimal runtime integration

- Start the existing `TutorialController` when Setup opens for an eligible first-time
  player.
- Let the tutorial prefab own all visual focus, input filtering, waits, and action order.
- Keep the existing `TutorialController` and `TutorialProgressController`; do not add
  another FTUE session manager.
- Use the current tutorial step analytics produced by `ScaffoldTutorialAdapter`.
- Do not add ad gating, reward staging, race-result phases, tutorial board seeding, or
  resume reconstruction in this first version.

## Validation

- The focused gear can be dragged and other unrelated UI is blocked during the wait.
- Dropping on the board advances the tutorial without requiring a specific cell.
- The optional second placement can be removed without code changes.
- The Race button is the only clickable target during its step.
- Focus and input filters are cleared when each wait completes.
- Completing the last block reports tutorial completion and stops the Blackboard.

## Optional follow-up

If UI drop detection proves insufficient because a visually valid drop can still be
rejected by gameplay, add one generic `Wait For Tutorial Event` action and publish a
single `gear_placed` event after `IBoardService.GearPlaced`. Keep the event name in the
Blackboard; do not introduce a Gear Engine tutorial phase enum.
