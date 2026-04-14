# Race simulation simplification — design plan

This document records the agreed model for the single-car track simulation. It is a specification for implementation and future tuning, not a user guide.

## Goals

- Replace overly realistic dynamics with a small set of **orthogonal, tunable** stats.
- Keep **curve behaviour** readable (baked geometry + simple look-ahead).
- Keep **single-use acceleration** explicit (stackable bank, all-at-once apply, normal cap only for now).
- Structure code so **integration order** (accel → drift → cap → consume, etc.) can change without rewriting physics.

## Scope

- **One car** only. No slipstreaming or drafting.
- **Normal acceleration** only for this phase: push speed toward caps, never above cap. **Extra boost** (ignore cap) is explicitly deferred.

---

## Car stats (design intent)

| Stat | Role |
|------|------|
| **Max straight speed** | Speed ceiling on low-curvature / straight segments. |
| **Max curve speed** | Lower ceiling while the car is in a corner segment (or implied by curvature). |
| **Handling** | Drives **overshoot risk** (deterministic margin + small stochastic roll), not a second hidden grip simulation. |
| **Brake** | Rate of **automatic** slowdown when above the active cap (especially entering curves). |
| **Stability / recovery / drift penalty** | Can remain for **feel** (drift visuals, time/space cost) until replaced by the simpler overshoot outcome mix. |

*Open implementation detail:* whether “max curve speed” is a separate variable from `CarVariableSet` or derived from straight max × a tuning curve by segment class.

---

## Curve detection

1. **Primary:** Baked track samples provide **curvature per arc length**. Segment type (straight vs curve severity) comes from this map at startup or at sample time.
2. **Secondary (keep simple):** **Speed-based look-ahead** only — e.g. `lookAhead = max(minMetres, speed * factor)`, probe forward along distance, take **min** of upcoming caps so slowdown starts early enough. No braking-distance integral unless we add it later.

---

## Target speed and automatic slowdown

- Each tick, compute an **active cap**: `min(straightMax, curveMaxForSegment, lookAheadMinCap)` (exact composition once segment buckets exist).
- If current speed **above** cap → move toward cap using **brake** (automatic curve slowdown).
- If current speed **below** cap → move toward cap using **throttle acceleration** only where we still want continuous closing to cap on straights; **event acceleration** is separate (below).

---

## Overshoot (procedural + light RNG)

When lateral demand is high relative to what the corner allows (conceptually: **curve “force”** from curvature × speed, compared to a **safe envelope** from handling / curve cap):

1. **Deterministic component:** overshoot **potential** rises with entry margin (speed vs allowed curve speed) and curvature; **better handling** lowers that potential.
2. **Stochastic component:** small extra chance scaled by **handling** (worse handling → slightly more volatile outcomes).

**Outcomes (mix):** force a **drift** (wide line, time + space) or **correct** (tighten, mostly time, optional brief speed loss). Costs are primarily **time** and **space**, optionally **speed**.

*Tuning principle:* players should be able to trace *why* something happened (big entry + tight corner), not only RNG.

---

## Single-use acceleration (bank)

- **Stackable:** receiving `+10` twice with bank at `0` yields bank `20`.
- **All-at-once apply:** on apply, translate the **entire bank** to a speed delta (or single impulse), then **bank resets to `0`**. Spreading over multiple ticks is a later option.
- **Caps:** **normal** acceleration never exceeds the **active cap**. **Extra boost** that ignores cap is explicitly **not** in this phase.
- **Ordering contract (flexible in code):** perform intermediate steps in dedicated methods; apply **final cap once** near the end of the step; **consume bank to zero** after cap (or immediately after apply if apply is defined as “add then cap then clear” — pick one and document in code comments next to the pipeline).

Encapsulate each transformation in its **own method** so the main tick reads as a ordered pipeline and **order can be swapped** without merging unrelated logic.

---

## Suggested tick pipeline (illustrative)

Order is **not** frozen — only the **ideas** are. Example:

1. Sample track / build frame (`here`, look-ahead window).
2. `ComputeActiveSpeedCap(...)`
3. `IntegrateAutomaticAccelDecelTowardCap(...)` — closes gap to cap using brake / base accel.
4. `EvaluateOvershootAndApplyDriftOrCorrect(...)` — may adjust speed, drift intensity, lateral state.
5. `ApplyBankedAccelerationImpulse(...)` — add bank to speed (or velocity target).
6. `ClampSpeedToActiveCap(...)` — single clamp pass for “normal” rules.
7. `ConsumeAccelerationBank(...)` — zero bank after successful apply.

Any step that is a no-op today should still exist as a thin hook if we expect to reorder or enable “extra boost” later.

---

## Deferred / out of scope (named)

- **Extra boost** ignoring cap.
- **Bank consumption over N ticks** instead of all-at-once.
- **Multi-car** drafting or wake effects.
- **DRS-style** sectors (could be modeled later as a straight-cap modifier on segment flags).

---

## Implementation checklist (for PRs)

- [ ] Car / tuning assets: straight max, curve max (or derivation rule), handling-driven overshoot coefficients.
- [ ] Runner: curvature-based segment caps + simple look-ahead min cap.
- [ ] Runner: bank add API for gameplay events; apply + cap + consume pipeline with small methods.
- [ ] Tests: straight line reaches cap; corner forces cap down; bank stacks and clears on apply; cap order invariant under reorder of pre-cap steps (where applicable).

---

## Revision

Update this file when stats names, variable assets, or tick order contracts change.
