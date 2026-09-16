# Rewards flow from app entry

The redundant Progress screen has been removed from post-race navigation. Results already shows score and stars. This supersedes the older `PFB_RaceProgressView1080x1680` screenshot.

**App entry → Home → Setup → Race → Results → Gold → Gear reward → Gear selection → Awarded gear → Home.**

Without an eligible gear reward: Gold → Home. Skipping the gear pick also returns Home. Eligibility remains first place OR at least one star, with one pick when both apply.

## Live walkthrough

Captured on 2026-09-16 from `Assets/GearEngine/Scenes/Main Scene.unity`, Unity 6000.5.9f1, using the real bootstrap, navigation, saved loadout, race simulation, and configured LiveOps service. Actions invoked the live UI buttons through the Unity CLI. The loading phase was observed at 33% in LiveOpsLayer; the first retained image is the Home screen after startup.

The screenshots render the complete live Game view at 1080×2280, including UI overlays. Unity's framebuffer orientation was corrected during readback; no UI was composited or retouched. Editor debug controls visible in Race/Results are retained. This is a walkthrough, not a claim that every existing screen is visually complete.

### 1. Home

Normal Main Scene startup completed through the existing UGS/LiveOps bootstrap. RoundedSquare and the saved six-cog board were used.

![Home](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/Home1080x2280.png)

### 2. Setup

Clicked Race on Home, then used the saved engine layout.

![Setup](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/Setup1080x2280.png)

### 3. Race

Clicked Race in Setup and let the car complete the race normally. No score, time, or result was injected.

![Race](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/Race1080x2280.png)

### 4. Results

Finished first in 31.02 seconds with score 432 and zero stars. First place alone qualified for one gear pick.

![Results](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/Results1080x2280.png)

### 5. Gold reward

Continue opened the recorded 10 Gold reward, page 1/2.

![Gold reward](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/GoldReward1080x2280.png)

### 6. Gear reward

Continue showed the gear reward, page 2/2. Upgrade opened the existing selection.

![Gear reward](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/GearReward1080x2280.png)

### 7. Gear selection

The existing roll offered Quantum Link IV, Adjacent Synergy III, and Base Gear II.

![Gear selection](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/GearSelection1080x2280.png)

### 8. Gear details

Clicked Quantum Link IV and then Select in the existing item popup.

![Gear details](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/GearDetails1080x2280.png)

### 9. Awarded gear

The reward page displayed the selected Quantum Link IV, still page 2/2. Gold was not repeated.

![Awarded gear](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/AwardedGear1080x2280.png)

### 10. Returned Home

Continue returned directly to MainViewModel, with one EventSystem and zero Progress views. The existing server progression selected SCurve.

![Returned Home](/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration/Artifacts/VisualTests/RewardsFlow/Runtime/ReturnedHome1080x2280.png)

## Findings outside this navigation change

- Home's Best Times panel is empty in the live scene, and Setup's title overlaps the header.
- The existing gear detail popup shows its description/Select action but omits the card title/artwork in this capture.
- Race execution logged two `MissingReferenceException` errors from `TemporaryBoostGearAbilitySO.Execute` and `RaceStartBuffGearAbilitySO.Execute`, referring to destroyed `VariableSO` objects. These unchanged components did not prevent the race, rewards, or return Home from completing. Their origin was not isolated in this change.
- The track catalog warned that `SquareTrack` has no matching TrackDefinition. Server progression remains unchanged and advanced the saved campaign to SCurve in this run.

These findings are recorded rather than hidden by isolated prefab screenshots. Physical pointer input, device cutouts, and production ad delivery were not verified. [End state](Runtime/EndState.json) records the final controller, EventSystem count, and absence of Progress views.

## Scoped validation

The two affected EditMode fixtures passed all 12 cases. Results and Gold also have isolated prefab captures at four portrait heights (1680, 1920, 2280, 2400), separate from the live walkthrough above. [Validation details](../../../Plans/RaceStandings/RewardsFlowValidation.md) and [test report](../../TestResults/RewardsFlow/Report.md).
