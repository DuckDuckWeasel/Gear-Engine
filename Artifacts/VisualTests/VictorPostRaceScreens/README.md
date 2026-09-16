# Victor post-race screens

The earlier Photo Finish implementation is superseded. This correction uses separate Results, Rewards, and Progress screens registered through the same navigation configuration as the other campaign screens.

## Runtime screenshots

Final focused validation: **25 passed, 0 failed**; repository compilation/analyzer gate passed.

These captures use the actual prefab Views bound to runtime ViewModels, after opening, closing, and reopening in Play mode. The deterministic scenario uses an existing three-tier track, 48.32 seconds, three laps, and 5,438 score. The game calculates the stars and gold from that track configuration. Network, ads, and navigation destinations are isolated by the focused test.

| Screen | 1080 × 2280 | 1080 × 1920 | 1080 × 2400 | 1080 × 1680 |
|---|---|---|---|---|
| Results | [Image](Runtime/Campaign_ResultPopupView1080x2280.png) | [Image](Runtime/Campaign_ResultPopupView1080x1920.png) | [Image](Runtime/Campaign_ResultPopupView1080x2400.png) | [Image](Runtime/Campaign_ResultPopupView1080x1680.png) |
| Rewards | [Image](Runtime/PFB_ReceivedRewardsView1080x2280.png) | [Image](Runtime/PFB_ReceivedRewardsView1080x1920.png) | [Image](Runtime/PFB_ReceivedRewardsView1080x2400.png) | [Image](Runtime/PFB_ReceivedRewardsView1080x1680.png) |
| Progress | [Image](Runtime/PFB_RaceProgressView1080x2280.png) | [Image](Runtime/PFB_RaceProgressView1080x1920.png) | [Image](Runtime/PFB_RaceProgressView1080x2400.png) | [Image](Runtime/PFB_RaceProgressView1080x1680.png) |

## Reference provenance

`Reference/` is rendered from `9e56d673:Assets/Lana Studio/AnimationAnimora.unity` with its original campaign/button prefab dependencies. The snapshots have temporary asset identities so current migration edits cannot change the reference. Captures show the authored demonstration data; they are not a claim of identical runtime metrics.

Results retains Victor's full-screen header, stars, large score, track panel, row artwork, and patterned background. Runtime time/laps/gold replace demo leaderboard entries because the current game does not provide race placement. Progress adapts the same visual components to actual tier targets. Rewards uses the explicitly named `new` composition. Old and alternate demo variants are excluded from runtime navigation.

## Scope and limitations

The focused runtime test checks visible content, on-screen text bounds, repeated opening, and single navigation after repeated continuation. Model tests cover the reward sequence and persistence wait; configuration tests cover screen registration. These are Editor captures, not physical-device safe-area proof or a live backend race/ad session. Existing shared-board behavior is unchanged by this correction.
