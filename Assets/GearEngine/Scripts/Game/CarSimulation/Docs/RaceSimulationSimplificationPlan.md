# Race simulation simplification — design plan

This document records the agreed model for the single-car track simulation. It is a specification for implementation and future tuning, not a user guide.

## Locked decisions (do not reopen without an explicit design pass)

These choices are **fixed** for the current implementation:

- **Geometry caps:** `MaxStraightSpeed` and `MaxCurveSpeed` are first-class car stats. The active cap blends between them from baked curvature; caps are **not** derived from handling.
- **Handling:** Affects **overshoot risk and outcomes** only (deterministic margin + stochastic roll). It does **not** define the corner speed limit.
- **Removed core stats:** `Stability`, `Recovery`, and `DriftPenalty` are **not** part of the gameplay model. Pace loss from mistakes uses tuning `OvershootPenaltyScale` instead of a per-car drift-penalty stat.
- **Acceleration events:** Temporary acceleration uses the **normal** `CarEntity` attribute / modifier path (`AddModifier` / `RemoveModifier`). There is no dedicated speed-bank field on motion state; integration reads acceleration from the entity each tick in `SimulationFrame.Create`.
- **Integration boundary:** `TrackSimulation` → `TrackSimulationRunner` → presentation stays the public seam unless a later task proves otherwise.

## Conversation context (compact)

**What we were trying to achieve.** The existing simulation felt **too realistic and complex** for the product goal. We wanted a lighter model that still reads as racing: clear knobs for **how fast on straights**, **how fast in corners**, and **how likely / severe corner mistakes are**, plus **acceleration** split into **automatic** slowdown when the track forces a lower cap and **gameplay-driven** bursts that do not require simulating full vehicle physics.

**Direction we converged on.** Separate **geometry caps** (straight max vs curve max) from **execution risk** (handling → overshoot). Slowdown toward the curve cap should feel **automatic**; handling should mainly gate **line error** (wide vs tight) rather than duplicate the cap logic. **Single-car races** were confirmed, so **drafting / slipstream between cars is out of scope** (no second car wake). Solo “DRS zones” or push-to-pass remain possible later as **segment modifiers**, not as drafting.

**Decisions captured here (historical table).**

| Topic | Decision |
|--------|-----------|
| Look-ahead | **As simple as possible:** distance from speed (and a floor), not a full braking-distance integral unless we add it later. |
| Overshoot | **Procedural core:** curve demand × entry margin vs local geometry cap → overshoot **potential**, scaled by **handling**, plus a **small random component** (`IRaceRandom`) also scaled by handling. **Outcomes** feed **OvershootIntensity**; visuals lerp slip and lateral offset from that signal. |
| Event acceleration | **Normal modifier path** only for this phase: read **Acceleration** from the entity each tick; events add modifiers; **clamp to active cap** once per step after integration. No separate motion-state “bank”. |

The sections below spell out the same contract in implementation-oriented detail.

## Goals

- Replace overly realistic dynamics with a small set of **orthogonal, tunable** stats.
- Keep **curve behaviour** readable (baked geometry + simple look-ahead).
- Keep **acceleration** stackable via modifiers while **respecting the active cap** after integration.
- Structure code as an explicit **pipeline** so integration order can evolve without merging unrelated logic.

## Scope

- **One car** only. No slipstreaming or drafting.
- **Normal acceleration** only for this phase: push speed toward caps, never above cap after the final clamp. **Extra boost** ignoring cap remains explicitly deferred.

---

## Car stats (design intent)

| Stat | Role |
|------|------|
| **Max straight speed** | Speed ceiling on low-curvature / straight segments. |
| **Max curve speed** | Lower ceiling while curvature is high (blended with straight max by curvature). |
| **Handling** | Drives **overshoot risk** (deterministic margin + stochastic roll), not the geometry speed cap. |
| **Brake** | Rate of **automatic** slowdown when above the active cap (especially entering curves). |
| **Acceleration** | Rate of closing toward the active cap when below it; also receives temporary modifiers from gameplay. |

Tuning on `TrackSimulationTuning` (not per-car stats): `OvershootDecayRate`, `OvershootPenaltyScale`, `ActiveCapCurvatureSpan`, look-ahead and probe parameters.

---

## Curve detection

1. **Primary:** Baked track samples provide **curvature per arc length**.
2. **Secondary (keep simple):** **Speed-based look-ahead** — `lookAhead = max(minMetres, speed * factor)`, probe forward along distance, take **min** of upcoming geometry caps so slowdown starts early enough.

---

## Target speed and automatic slowdown

- Each tick, compute a **geometry cap** at the car sample by blending `MaxStraightSpeed` → `MaxCurveSpeed` from curvature, then an **active cap** as the minimum of that value and the **minimum cap found in the look-ahead window**.
- If current speed **above** cap → move toward cap using **brake**.
- If current speed **below** cap → move toward cap using **acceleration** (including any modifiers on the acceleration attribute).

---

## Overshoot (procedural + light RNG)

Overshoot evaluation uses **pre-integration speed** vs the **local geometry cap at the current sample** so entry “too hot” into a corner still registers even when the same step brakes toward the (lookahead-limited) active cap.

1. **Deterministic component:** margin over the **local** geometry cap, scaled by curvature; **better handling** lowers growth into `OvershootIntensity`.
2. **Stochastic component:** small variation from injected **`IRaceRandom`**, scaled by handling (tests use a fixed sequence stub).

**Presentation:** `ApplyOvershootVisuals` lerps `SlipAngle` and `LateralOffset` from `OvershootIntensity` (same rates as the former drift visuals for v1).

**Pace:** `AdvanceRace` uses `effectiveSpeed = Speed * (1 - OvershootPenaltyScale * OvershootIntensity)` (tuning replaces the old per-car drift penalty stat).

`RaceRuntimeState.IsDrifting` remains a **presentation** flag: `OvershootIntensity >0.12f`.

---

## Target tick pipeline (implemented)

1. `SimulationFrame.Create` — read stats and tuning from the car entity and `TrackSimulationTuning`.
2. `ComputeActiveSpeedCap` — geometry blend + look-ahead minimum.
3. `IntegrateAutomaticAccelDecelTowardCap` — brake / acceleration toward the active cap.
4. `EvaluateOvershootAndApplyOutcome` — update `OvershootIntensity` using pre-integration speed vs **local** geometry cap; RNG via `IRaceRandom`.
5. `ApplyOvershootVisuals` — presentation only.
6. `ClampSpeedToActiveCap` — final guard on speed.
7. `AdvanceRace` — distance using effective speed; update HUD-facing race fields.

There is **no** separate “apply acceleration bank” stage; modifiers on the acceleration attribute are picked up automatically when the frame is built.

---

## Deferred / out of scope (named)

- **Extra boost** ignoring cap.
- **Multi-car** drafting or wake effects.
- **DRS-style** sectors (could be modeled later as a straight-cap modifier on segment flags).

---

## Implementation checklist (for PRs)

- [x] Car / tuning assets: straight max, curve max, handling-driven overshoot; tuning for decay, penalty scale, cap curvature span.
- [x] Runner: curvature-based geometry caps + look-ahead min cap; overshoot + visuals + clamp order.
- [x] Acceleration via entity modifiers; cap after integration.
- [x] Tests: stat extraction; straight / curve caps; handling vs overshoot with fixed RNG; decay; effective speed; modifier + cap; remodeled `CarDefinition` asset smoke test.

---

## Revision

Update this file when stats names, variable assets, or tick order contracts change.
