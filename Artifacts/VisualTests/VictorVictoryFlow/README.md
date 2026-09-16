# Victor post-race flow evidence

## Sequence

1. `Victory`: Victor's Photo Finish modal appears over the completed race and displays live time,
   score, laps and gold.
2. `Reward`: Continue, or completing Roguelike selection through Upgrade, opens the dedicated
   received-reward screen. The captures use the real Echo gear sprite as representative data.
3. `Progress`: Continue opens the dedicated star and track-progression screen with representative
   server outcome data.
4. Continue returns to Home.

## Captures

`After/` contains each state at 1080×1680, 1080×1920, 1080×2280 and 1080×2400.
`Reference/VictorResultsCanvas1080x2280.png` records Victor's source victory composition.

The captures instantiate the playable result prefab in an empty Unity scene and render through a
camera-backed canvas. The blue area behind Victory represents the completed race underlay; it is
not part of the modal. Reward and Progress are full-screen compositions. Representative values are
used to verify layout; runtime bindings provide the actual race, item and progression values.

## Acceptance criteria represented

- Correct Victor victory hierarchy and clean title treatment.
- Distinct Victory, Reward and Progress states.
- No clipping at the four requested aspect ratios.
- Reward icon and label have real runtime binding targets.
- Progress title, stars, track, tier, score and time have real runtime binding targets.
