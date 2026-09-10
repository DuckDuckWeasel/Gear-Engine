# Animora Timeline Visual Verification

## Result

- Focused EditMode tests: 5 passed, 0 failed, 0 skipped, 5 total.
- Relevant Unity log events: none.
- Unity version: 6000.5.3f1.
- Scenario: the existing `LoopAnimoraPlayer` inspector with two adjacent `Scale`
  clips in `Main Scene`.

## Verified Behavior

- The idle row shows its description while the selected row shows timing details.
- Title and secondary text occupy separate fixed vertical slots.
- Clip height is 56 px with a 10 px inter-row gutter, keeping each description
  visibly inside its own clip.
- Clicking the timeline background removes the selection outline.
- Repeated selection remains stable.
- The track context menu opens with Delete, Duplicate, Copy, Toggle Active, and Solo.
- The right resize handle changes clip width.
- A shortened clip can be moved horizontally.
- A clip can be reordered vertically between the two rows.
- Each temporary resize, horizontal move, and reorder was restored with Unity Undo.

The two-row nested-label selection path is also covered by the focused Editor fixture:
the second row is selected first, then an event targeted at the first row's title must
select the first row and deselect the second.

## Evidence

- Full inspector screenshot:
  [AnimoraTimelineSelectionLayout.jpeg](AnimoraTimelineSelectionLayout.jpeg)
- Timeline detail:
  [AnimoraTimelineLabelsDetail.jpeg](AnimoraTimelineLabelsDetail.jpeg)
- Final adjacent-row separation:
  [AnimoraTimelineRowSeparation.jpeg](AnimoraTimelineRowSeparation.jpeg)
- NUnit report:
  [../TestResults/AnimoraTimelineInteraction/Report.md](../TestResults/AnimoraTimelineInteraction/Report.md)
- NUnit XML:
  [../TestResults/AnimoraTimelineInteraction/EditMode.xml](../TestResults/AnimoraTimelineInteraction/EditMode.xml)
- Focused Editor log:
  [../TestResults/AnimoraTimelineInteraction/EditMode.log](../TestResults/AnimoraTimelineInteraction/EditMode.log)

## Limitations

- The screenshot is an Editor-inspector check rather than a pixel-golden test.
- The open scene contained unrelated unsaved user changes. Verification did not save
  or discard the scene.
