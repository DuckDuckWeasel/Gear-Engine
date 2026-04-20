# Gear Catalog

> Complete reference of all Gear Abilities in the Roguelike Gear Engine.
> Auto-generated values are scaled per rarity via **The Forge** (`GearAssetGenerator.cs`).

---

## 🔴 Active Race Gears (Campaign — `ActiveRaceGearAbilitySO`)

These gears interact with the live race simulation via `RaceState` and `IGearEngineService`.

### Group A — High Risk / High Reward

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Burnout** | Stays at redline speed for X seconds → fires massive permanent buff, then self-destructs | `targetVariable` (VariableSO), `massiveBuffValue` = 300, `timeRequiredAtRedline` = 5s, `maxSpeedThreshold` = 90 |
| **Fragile Bomb** | Explodes after being triggered X times → applies a timed buff | `targetVariable` (VariableSO), `explosiveBuffValue` = 200, `buffDurationSeconds` = 5s, `requiredTriggersToExplode` = 5 |
| **Kamikaze Recovery** | Arms on first trigger; if car crashes (speed ≈ 0), fires a massive thrust rescue | `targetVariable` (VariableSO), `thrustValue` = 150, `thrustDurationSeconds` = 2s, `crashSpeedThreshold` = 0.1, `idleGracePeriod` = 2s |
| **Martyr** | When any neighbor gear is destroyed, permanently buffs a stat | `targetVariable` (VariableSO), `martyrBuffValue` = 50 |

### Group B — Spatial / Synergy

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Blackhole** | Pulls triggers from neighbors inward; on activation fires burst buff | `burstTarget` (VariableSO), `burstAmount` = 400, `pullForce` = -10, `eventDuration` = 1s, `buffDuration` = 4s |
| **Cursed Synergy** | Penalizes self but massively boosts adjacent neighbors | `penaltyVar` (VariableSO), `penaltyAmount` = -30, `penaltyDuration` = 3s, `neighborBoostEventForce` = 50, `eventDuration` = 1s |
| **Echo** | On trigger, re-fires the Execute() of all adjacent Active gears | *(no configurable params — reads neighbor abilities dynamically)* |
| **Quantum Link** | Charges all adjacent gears simultaneously when triggered | `injectedChargeAmount` = 50 |

### Group C — Scaling / Conversion

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Lap Scaler** | Buff grows multiplicatively with each completed lap | `stat` (VariableSO), `baseBuff` = 10, `buffDuration` = 5s |
| **Momentum Converter** | Converts sustained speed into stacking bonus over time | `penaltyStat` (VariableSO), `bonusStat` (VariableSO), `conversionThreshold` = 10s, `bonusIncrement` = 5 |
| **Ouroboros** | Cycles through a list of stats, buffing one per tick in rotation | `cycleStats` (List\<VariableSO\>), `buffVal` = 50, `buffDuration` = 6s |
| **Overheat** | Massive speed boost but increases drift penalty simultaneously | `speedStat` (VariableSO), `brakeStat` (VariableSO), `boostAmount` = 200, `boostDuration` = 15s |
| **Radioactive Engine** | Passively decays a stat every X seconds (risk/reward) | `decayTarget` (VariableSO), `passiveTickRate` = 2s |
| **Vampiric Engine** | Each trigger increases a stacking buff permanently | `targetStat` (VariableSO), `stackIncreaseVal` = 2 |

### Group D — Chaos / RNG

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Bipolar** | Randomly switches between buff and debuff every X seconds | `targ` (VariableSO), `stateSwitchInterval` = 5s, `buffAmount` = 50, `debuffAmount` = -30, `effectDuration` = 4s |
| **Mirage** | Randomizes the trigger pattern of adjacent gears on activation | *(dynamic — no configurable params)* |
| **Slot Machine** | 5% chance of jackpot (999 buff), otherwise minor buff | `stat` (VariableSO), `jackpotBonus` = 999, `minorBonus` = 5, `minorDuration` = 2s |
| **The Joker** | Buffs one stat massively while debuffing another. One-shot | `s1` (VariableSO) +300, `s2` (VariableSO) -150 |

### Group E — Conditional

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Brake To Burn** | Charges instantly when car is braking (speed < threshold) | `speedThreshold` = 45 |
| **Pacemaker** | Boosts stat only when car speed is within a sweet spot range | `stat` (VariableSO), `minSpeedThreshold` = 40, `maxSpeedThreshold` = 60, `boostAmount` = 100, `boostDuration` = 3s |

### Ungrouped — Utility

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Lap Trigger** | Permanent stacking buff per completed lap | `targetVariable` (VariableSO), `buffPerLap` = 5 |
| **Neighbor Overclock** | Periodically injects charge into all adjacent board gears | `overclockAmount` = 20, `intervalSeconds` = 2s |
| **Race Start Buff** | One-time permanent buff applied at race start | `targetVariable` (VariableSO), `buffValue` = 20 |
| **Recovery** | Triggers a short buff when speed drops below threshold | `speedThreshold` = 20, `cooldownTime` = 5s, `targetVariable` (VariableSO), `buffValue` = 30, `durationSeconds` = 1.5s |
| **Temporary Boost** | Timed buff that fires once on activation | `targetVariable` (VariableSO), `buffValue` = 50, `durationSeconds` = 3s |
| **Track Segment Boost** | Fires buff when car reaches a specific track progress % | `triggerProgress` = 0.5, `targetVariable` (VariableSO), `buffValue` = 40, `durationSeconds` = 2s |

---

## 🔵 Passive Race Gears (Campaign — `PassiveRaceGearAbilitySO`)

These gears modify `RoguelikeCarStats` directly via `ApplyPassiveStats()`. They don't need triggers.

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Adjacent Synergy** | Bonus per adjacent neighbor on the board | `baseBonusPerNeighbor` (PassiveStatModifier: Stat + Amount) |
| **Clone** | Flat stat boost (duplicates base stats) | `topSpeedMultiplier` = 50, `accelMultiplier` = 20 |
| **Ghost** | Copies passive stats from all adjacent gears | *(dynamic — reads neighbor PassiveRaceGearAbilitySO)* |
| **Greed** | Bonus per empty slot on the board | `topSpeedBonusPerSlot` = 10, `accelBonusPerSlot` = 5 |
| **Modifier Passive** | Generic stat modifier list (designer-friendly) | `modifiers` (List\<PassiveStatModifier\>) |

---

## ⚙️ Base Board Gears (`GearAbilitySO`)

Core engine abilities for the puzzle/board layer. Not tied to race simulation.

| Gear | Mechanic | Parameters |
|------|----------|------------|
| **Destroy Self** | Self-destructs on max charge, raises `GearDestroyedEvent` | — |
| **Inactive** | Disables the gear node; re-enables on deactivation | — |
| **Score** | Awards points when gear reaches max charge | `ScoreAmount` = 100 |
| **Speed Boost** | Multiplies the gear's spin speed on the board | `SpeedMultiplier` = 2.0 |

### 🚫 Debug Stubs (Skipped by The Forge)

These exist as legacy placeholders with only `Debug.Log` in their `Execute()`:

- `AccelerationAbility`
- `BoostAbility`
- `CurveDriftAbility`
- `DriftUpAbility`
- `GasAbility`
- `VelocityAbility`

---

## 🏭 The Forge — Generation Rules

When `GearAssetGenerator` runs, it applies the following logic:

| Tier | Rarity | Buff Multiplier | Duration/Threshold Divisor |
|------|--------|-----------------|---------------------------|
| 1 | Common | ×1.0 | ÷1.0 |
| 2 | Uncommon | ×1.5 | ÷1.5 |
| 3 | Rare | ×2.0 | ÷2.0 |
| 4 | Epic | ×2.5 | ÷2.5 |
| 5 | Legendary | ×3.0 | ÷3.0 |

**Multi-variant gears** (TemporaryBoost, RaceStartBuff, LapTrigger, Martyr, LapScaler, SlotMachine, Pacemaker) generate **3 variants** each targeting: `Speed`, `Acceleration`, `Handling`.

**Fixed-target gears** generate 1 variant with a pre-assigned `VariableSO`.

**Progression**: Each tier's `GearConfig.NextLevel` points to the next rarity tier, enabling merge/upgrade mechanics.

---

## 📊 Totals

| Category | Count |
|----------|-------|
| Active Race Gears | 25 |
| Passive Race Gears | 5 |
| Base Board Gears | 4 |
| Debug Stubs (skipped) | 6 |
| **Total Unique Classes** | **40** |
| **Forge Output (5 tiers × variants)** | ~200+ assets |
