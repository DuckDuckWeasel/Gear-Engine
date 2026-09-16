# Corrected race results and rewards

These Unity runtime captures supersede the earlier Results time/laps/gold rows. They use the existing Circle track, stable synthetic rivals, real ViewModel bindings, and the authored Victor screen assets. They are isolated rendering/navigation checks, not a live-server session or physical-device safe-area certification.

## Representative states

- [Before fourth-to-third promotion](Runtime/FourthBeforePromotion1080x2280.png)
- [After promotion: player is third, no duplicate fourth row](Runtime/Campaign_ResultPopupView1080x2280.png)
- [Fourth place with three score stars](Runtime/FourthPlaceThreeStars1080x2280.png)
- [Already third: no rank animation](Runtime/UnchangedThirdPlace1080x2280.png)
- [Eligible gear reward: Upgrade opens the existing selection](Runtime/GearReward1080x2280.png)

## Portrait layout matrix

| Dimensions | Results | Gold reward | Progress |
|---|---|---|---|
| 1080×2280 | [Capture](Runtime/Campaign_ResultPopupView1080x2280.png) | [Capture](Runtime/PFB_ReceivedRewardsView1080x2280.png) | [Capture](Runtime/PFB_RaceProgressView1080x2280.png) |
| 1080×1920 | [Capture](Runtime/Campaign_ResultPopupView1080x1920.png) | [Capture](Runtime/PFB_ReceivedRewardsView1080x1920.png) | [Capture](Runtime/PFB_RaceProgressView1080x1920.png) |
| 1080×2400 | [Capture](Runtime/Campaign_ResultPopupView1080x2400.png) | [Capture](Runtime/PFB_ReceivedRewardsView1080x2400.png) | [Capture](Runtime/PFB_RaceProgressView1080x2400.png) |
| 1080×1680 | [Capture](Runtime/Campaign_ResultPopupView1080x1680.png) | [Capture](Runtime/PFB_ReceivedRewardsView1080x1680.png) | [Capture](Runtime/PFB_RaceProgressView1080x1680.png) |

## Behavior checked

Time determines placement; score determines stars. Results has one Continue button. Gold precedes an eligible gear reward. Eligibility is first place OR at least one star, with one selection even when both apply. Selecting gear resumes at that reward; skipping proceeds to Progress; Progress returns home. Closing mid-animation stops row movement; reopening resets it. Viewport checks cover visible text bounds at all four sizes.

The capture helper uses a render camera and a canvas scale corresponding to the prefab's height-based CanvasScaler. It does not emulate device cutouts or exercise physical pointer hit testing. Existing navigation tests substitute destinations, ads, and persistence services.

First-place-only track unlocking is planned in [the ExecPlan](../../../Plans/RaceStandings/ExecPlan.md), not implemented on the backend here.
