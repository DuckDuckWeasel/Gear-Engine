# Home standings and gear details

## Playable scene

- `HomeRuntime1080x2280.png` and `HomeRuntime1080x1680.png`: application launched from Main Scene, with the existing profile's saved best time. Historical races had no persisted score-star count, so the displayed stars are empty.
- `SetupRuntime1080x2280.png`: Home → Setup spot check. Home restores the shared track's original world pose before navigation. Setup's pre-existing presentation is outside this adjustment.

## Deterministic presentation fixtures

- `HomeUnraced*`: three rivals plus YOU without a recorded time; no earned stars.
- `HomeSavedProgress*`: third place and two earned score stars, independent of time. These isolated UI captures omit scene geometry and the toolbar.
- The updated Home fixtures also show the existing left/right selector art and the track
  counter in the responsive layout. The counter sits between the preview controls and
  RACE action at both captured heights. Runtime binding hides these controls for a
  single available track; focused ViewModel tests cover that state and multi-track cycling.
- `GearPopupQuantumLink*`: the project's real Quantum Link IV item definition (Epic), with four of five rarity slots filled, artwork, name, description and Select action. The regression also switches to Quantum Link II (Common), then back, and reopens the popup.

Both portrait sizes are 1080×2280 and 1080×1680. Adjacent evidence JSON files identify the capture scenario. Test fixtures live in `HomePresentationTests`; final NUnit results and the run report are under `Artifacts/TestResults/HomeStandings/Final`.

Track stars represent score (three slots). Gear stars represent rarity (five slots), not race score or the item's Roman-numeral name suffix.
