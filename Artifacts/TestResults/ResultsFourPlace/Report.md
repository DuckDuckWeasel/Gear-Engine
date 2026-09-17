# Four-position Results standings validation

## Scope

Results keeps all four positions visible. When the player has no previous placement,
the player row starts in fourth and swaps upward until it reaches the current position
calculated from race time. Home keeps its separate compact Best Times behavior.

## Automated checks

- `PostRaceScreenTests`: **3 passed, 0 failed, 0 skipped**.
  - Confirms the player starts in fourth without a previous result.
  - Confirms the row reaches the current position and all four rows remain visible.
  - Confirms close/reopen resets the animation and repeated navigation remains safe.
- `RaceStandingsTests`: **13 passed, 0 failed, 0 skipped**.
  - Confirms no previous race time maps to fourth place.
  - Confirms current and previous times independently determine their positions.

The final post-race run reported no relevant Unity errors or exceptions. Native NUnit
XML and contextual runner reports are retained in `PostRace/` and `Model/`.

## Visual evidence

The focused test regenerated the Results screen at 1080×1680, 1080×1920,
1080×2280, and 1080×2400 under `Artifacts/VisualTests/RewardsFlow/Portrait/`.
`FourthBeforePromotion1080x2280.png` shows the player in fourth before the animation;
`Campaign_ResultPopupView1080x2280.png` shows the player in third afterward while the
displaced rival remains visible in fourth. Both inspected images keep the full board
inside the viewport.
