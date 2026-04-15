# Race simulation — arcade curve-band model

Specification for the single-car track simulation. This doc matches the current `TrackSimulationRunner` implementation.

## Motion state (`CarMotionState`)

| Field | Role |
|------|------|
| `Distance` | Progress along the baked spline. |
| `Speed` | Scalar speed toward the active cap. |
| `HeadingErrorDeg` | Signed turn debt: how much the car lags the ideal line. |
| `SlipAngle` / `LateralOffset` | Visual pose derived from normalized heading error. |
| `SampleIndex` | Nearest baked sample index. |

## Car stats (per car)

| Stat | Role |
|------|------|
| **Max straight speed** | Target cap when curve difficulty is zero. |
| **Max curve speed** | Target cap when curve difficulty is one. |
| **Handling** | How much turn demand (deg) can be absorbed per second; scales recovery toward zero heading error. |
| **Brake** / **Acceleration** | Rates to move `Speed` toward the active cap. |

Handling does **not** change the speed cap. Caps come only from **curve bands** and lookahead.

## Tuning (`TrackSimulationTuning`)

### Global

- `LookAheadMinMetres`, `LookAheadSpeedFactor`, `AheadProbeStep` — lookahead window and probe spacing.
- `HandlingNormalizationScale` — raw handling divided by this to get `handling01` (clamped 0–1).
- `HandlingTurnRateDegPerSec` — max turn absorption per second at `handling01 = 1`.
- `RecoveryRateDegPerSec` — heading error pulled toward zero per second, scaled by `handling01`.
- `MaxHeadingErrorDeg` — normalizes error for pace and visuals.
- `SpeedPenaltyScale` — `effectiveSpeed = Speed * (1 - error01 * scale)`.
- `SlipAngleScale`, `LateralOffsetScale` — visual magnitude from `error01`.
- `IsDriftingThreshold`, `IsOvershotThreshold` — compared to normalized `error01` for HUD flags.

### Curve bands (`CurveBandDefinition[]`)

Each band: `MinCurvature`, `MaxCurvature`, `Difficulty01` (0 = straight, 1 = hardest).

Default table: straight / easy / medium / hard ranges on unsigned baked curvature.

If the asset list is empty, the runner uses the same defaults in code.

## Tick pipeline

1. `SimulationFrame.Create` — sample track, read car stats and tuning, compute `handling01`.
2. **Lookahead** — `lookAhead = max(LookAheadMin, Speed * LookAheadSpeedFactor)`.
3. **Active band** — most severe `Difficulty01` among samples from `Distance` to `Distance + lookAhead` (step `AheadProbeStep`).
4. **Target cap** — `Lerp(MaxStraightSpeed, MaxCurveSpeed, activeBand.Difficulty01)`.
5. Integrate `Speed` toward cap with accel/brake.
6. **Turn demand** — `|SignedCurvature| * Speed * dt * Rad2Deg * Difficulty01`.
7. **Handled turn** — `handling01 * HandlingTurnRateDegPerSec * dt`.
8. **Heading error** — add `sign(SignedCurvature) * max(0, demand - handled)`, then `MoveTowards(0, RecoveryRate * handling01 * dt)`.
9. **Pace** — `error01 = clamp01(|HeadingErrorDeg| / MaxHeadingErrorDeg)`; `effectiveSpeed` from `SpeedPenaltyScale`.
10. **Visuals** — `SlipAngle` and `LateralOffset` from `error01` and sign of heading error.
11. **Race HUD** — `IsDrifting` / `IsOvershot` from `error01` vs thresholds; advance `Distance` with `effectiveSpeed`.

## Acceleration modifiers

Temporary acceleration uses `CarEntity` modifiers; each frame reads **Acceleration** from the entity when building the frame. Speed still respects the active cap after integration.

## Race scene ticking

`RaceBootstrap.Update` calls `ITrackSimulationRunner.Tick()` so the race scene advances the same solver as the car-track test bootstrap (which ticks its own runner instances).

## Scope

- Single car; no drafting.
- No RNG in the core loop (`IRaceRandom` remains injected for future use).

## Revision

Update this file when stat names, tuning fields, or tick order change.
