# Tutorial Focus Offset Verification

## Context

Validate that the tutorial focus runtime uses the same screen-space offset units as the
FocusPreset inspector preview and places the indicator outside the target bounds.

## EditMode Results

- Full run: 304 total, 240 passed, 64 failed, 0 skipped.
- Focused fixture: 2 total, 2 passed, 0 failed.
- Passed:
  - `DirectionOffset_UsesTheSameScreenDistanceAsThePreview`
  - `PositionOffset_IsAppliedDirectlyInScreenSpace`
- The 64 failures are outside the tutorial focus layout fixture.

## Visual Inspection

- Scenario: Play `Test Tutorial Scene` and inspect the automatically focused
  `MOUSE OVER` target.
- Result: The indicator is above the target and no longer overlaps its content.
- Evidence:
  `../../VisualTests/TutorialFocusOffset/RuntimeOffsetAligned.jpeg`

## Artifacts

- NUnit XML: `EditModeResults.xml`
- Unity Editor log:
  `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Logs/Editor.log`
