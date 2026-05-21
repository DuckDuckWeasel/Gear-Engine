# Unify Physics & Spline Simulation Under a Common Interface

This ExecPlan is a living document.

## Purpose / Big Picture

Currently two completely separate simulation pipelines exist in two different assemblies:

| | **Physics Pipeline** (`Game.CarSimulation`) | **Spline Pipeline** (`Game.SplineEvaluate`) |
|---|---|---|
| **Runner Service** | `SplineCarRunnerService` (Rigidbody + Prometeo) | `SplineEvaluateRunnerService` (pure math) |
| **Config** | `SplineCarRunnerConfigSO` (60+ fields) | `SplineDriverConfig` (17 fields) |
| **Race Lifecycle** | `RaceManagerService` / `RaceState` | None (ad-hoc) |
| **Bootstrap** | `CarTrackBootstrap` + `CarTrackScope` + `CarTrackInstaller` | `SplineEvaluateBootstrap` + `SplineEvaluateScope` + `SplineEvaluateInstaller` |

**Goal**: Merge everything into `Game.CarSimulation` so that a scene can switch between physics and spline simulation **purely by changing which config ScriptableObject is loaded** into the installer. The code must be separated into clear subfolders — one for physics, one for spline — but share common abstractions.

## Progress

- [x] M0: Create shared interfaces & abstractions
- [x] M1: Move SplineEvaluate simulation code into CarSimulation/SplineSimulation/
- [x] M2: Create abstract config hierarchy
- [x] M3: Unify the Bootstrap/Installer/Scope
- [x] M4: Wire RaceManagerService to work with both pipelines
- [x] M5: Clean up old `Game.SplineEvaluate` assembly
- [x] M6: Delete Test2 scene and scripts (temporary scaffolding)
- [x] M7: Documentation

## Surprises & Discoveries

_(To be filled during implementation.)_

## Decision Log

| # | Decision | Rationale |
|---|----------|-----------|
| D1 | Use interface-based strategy, not inheritance on services | Runner services have fundamentally different internal state (Rigidbody vs pure float). An interface is the cleanest seam. |
| D2 | Config uses abstract base `SimulationConfigBase` with two concrete subclasses | Odin Inspector `[InlineEditor]` handles polymorphic SO references in the inspector natively. The installer inspects the concrete type to choose which runner to register. |
| D3 | `LaneProfile` stays as-is (optional) | It's only consumed by the spline pipeline and all its curves are zeroed in practice. It remains an optional spline-only concern, not promoted to the shared interface. |
| D4 | Keep `SplineDriverConfig` fields that are unused dead — don't purge now | Avoids breaking serialized assets. We can deprecate them later. |
| D5 | `RaceManagerService` depends on `ISimulationRunnerService`, not concrete types | This is the key inversion point — it receives lap events from whichever runner is active. |

## Context and Orientation

### Key Terms

- **Physics Pipeline**: Uses `PrometeoCarController` + `Rigidbody` + reflection-injected touch inputs. The car is a real physics object steered by AI waypoints. ~630 lines of simulation logic.
- **Spline Pipeline**: Pure mathematical evaluation along a `UnityEngine.Splines.Spline`. No Rigidbody, no physics. Position/rotation/visual effects are calculated directly from spline `t`. ~870 lines of simulation logic.
- **Runner Service**: The service that manages active car instances and ticks them each frame (`ITickable`).
- **Config SO**: A `ScriptableObject` with tuning parameters for the runner.
- **`CarEntity`**: A `Scaffold.Entities.EntityInstance<CarDefinition>` — a lightweight data bag. Used by **both** pipelines as an identity key.

### Current File Map

```
Game.CarSimulation/
├── Bootstrap/
│   ├── CarTrackBootstrap.cs          ← scene launcher (physics)
│   ├── CarTrackInstaller.cs          ← VContainer registration
│   └── CarTrackScope.cs              ← LifetimeScope
├── Definitions/
│   ├── CarDefinition.cs              ← shared: car prefab + entity def
│   ├── SplineCarRunnerConfigSO.cs    ← physics-only config
│   ├── TrackDefinition.cs            ← shared: track spline + laps
│   ├── RaceSessionConfig.cs          ← shared: session options
│   └── RoguelikeCarStats.cs          ← physics-only: stat source
├── Entity/
│   ├── CarEntity.cs                  ← shared
│   └── CarEntityFactory.cs           ← shared
├── Simulation/
│   ├── SplineCarRunnerService.cs     ← physics runner (Prometeo)
│   ├── SplineCarRunnerContext.cs     ← physics per-car state
│   ├── RaceManagerService.cs         ← shared race lifecycle
│   ├── RaceState.cs                  ← shared race state
│   └── CarTelemetryData.cs           ← shared telemetry struct
└── ...

Game.SplineEvaluate/                  ← SEPARATE ASSEMBLY (to be absorbed)
├── Bootstrap/
│   ├── SplineEvaluateBootstrap.cs
│   ├── SplineEvaluateInstaller.cs
│   └── SplineEvaluateScope.cs
├── Definitions/
│   ├── DriverPersonality.cs          ← spline-only: 5 stats
│   ├── LaneProfile.cs               ← spline-only: per-track curves
│   └── SplineDriverConfig.cs         ← spline-only config
├── Simulation/
│   ├── SplineEvaluateDriver.cs       ← spline driver (main logic)
│   ├── SplineEvaluateRunnerService.cs
│   ├── SplineMotionState.cs
│   └── SplineCurvatureHelper.cs
└── Presentation/
    ├── SplineEvaluateHUD.cs
    └── SplineEvaluateGizmos.cs
```

### Target File Map (After)

```
Game.CarSimulation/
├── Bootstrap/
│   ├── CarTrackBootstrap.cs          ← UPDATED: works with ISimulationRunnerService
│   ├── CarTrackInstaller.cs          ← UPDATED: reads config type to pick runner
│   └── CarTrackScope.cs              ← UPDATED: holds SimulationConfigBase
├── Definitions/
│   ├── CarDefinition.cs              ← unchanged
│   ├── TrackDefinition.cs            ← unchanged
│   ├── RaceSessionConfig.cs          ← unchanged
│   ├── RoguelikeCarStats.cs          ← unchanged
│   └── SimulationConfigBase.cs       ← NEW: abstract base for both configs
├── Entity/                           ← unchanged
├── Simulation/
│   ├── ISimulationRunnerService.cs   ← NEW: shared interface
│   ├── RaceManagerService.cs         ← UPDATED: depends on ISimulationRunnerService
│   ├── RaceState.cs                  ← unchanged
│   └── CarTelemetryData.cs           ← unchanged
├── PhysicsSimulation/                ← NEW FOLDER
│   ├── PhysicsSimulationConfig.cs    ← RENAMED from SplineCarRunnerConfigSO
│   ├── SplineCarRunnerService.cs     ← MOVED here
│   ├── SplineCarRunnerContext.cs     ← MOVED here
│   └── CarAreaSensor.cs              ← MOVED here
├── SplineSimulation/                 ← NEW FOLDER (from Game.SplineEvaluate)
│   ├── SplineSimulationConfig.cs     ← RENAMED from SplineDriverConfig
│   ├── SplineEvaluateRunnerService.cs
│   ├── SplineEvaluateDriver.cs
│   ├── SplineMotionState.cs
│   ├── SplineCurvatureHelper.cs
│   ├── DriverPersonality.cs          ← MOVED from SplineEvaluate/Definitions
│   └── LaneProfile.cs               ← MOVED from SplineEvaluate/Definitions
├── Presentation/                     ← existing + moved SplineEvaluate presentation
└── ...
```

## Plan of Work

### M0 — Shared Interface & Abstractions

Create the contract that both simulation pipelines implement.

**Files to create:**

1. `Simulation/ISimulationRunnerService.cs`
2. `Definitions/SimulationConfigBase.cs`

#### `ISimulationRunnerService`

```csharp
/// <summary>
/// Common contract for both physics and spline simulation runners.
/// The bootstrap calls InitializeRun with pipeline-specific params;
/// RaceManagerService only sees this interface.
/// </summary>
public interface ISimulationRunnerService
{
    event Action<CarEntity> OnLapCompleted;

    /// <summary>
    /// Initializes a car on a track. The concrete ISimulationInitParams
    /// carries pipeline-specific data (Rigidbody vs pure-math).
    /// </summary>
    void InitializeRun(ISimulationInitParams initParams);

    void SetPaused(CarEntity entity, bool paused);
    bool GetTelemetry(CarEntity entity, out CarTelemetryData data);
    void RemoveDriver(CarEntity entity);
    void Tick();
}

/// <summary>
/// Marker interface for pipeline-specific initialization parameters.
/// Each pipeline defines its own concrete struct/class.
/// </summary>
public interface ISimulationInitParams
{
    CarEntity Entity { get; }
    SplineContainer Track { get; }
    Transform CarTransform { get; }
}
```

Concrete init params per pipeline:

```csharp
// PhysicsSimulation/PhysicsInitParams.cs
public sealed class PhysicsInitParams : ISimulationInitParams
{
    public CarEntity Entity { get; set; }
    public SplineContainer Track { get; set; }
    public Transform CarTransform { get; set; }
    public PrometeoCarController Controller { get; set; }
    public RoguelikeCarStats Stats { get; set; }
}

// SplineSimulation/SplineInitParams.cs
public sealed class SplineInitParams : ISimulationInitParams
{
    public CarEntity Entity { get; set; }
    public SplineContainer Track { get; set; }
    public Transform CarTransform { get; set; }
    public DriverPersonality Personality { get; set; }
    public LaneProfile LaneProfile { get; set; }
}
```

Each runner casts to its own params type inside `InitializeRun`. The bootstrap constructs the correct params based on which config is loaded.

#### `SimulationConfigBase`

```csharp
/// <summary>
/// Abstract base for all simulation config ScriptableObjects.
/// The concrete type itself is the discriminator — no enum needed.
/// The installer uses pattern matching (is PhysicsSimulationConfig)
/// to register the correct runner.
/// </summary>
public abstract class SimulationConfigBase : ScriptableObject { }
```

### M1 — Move SplineEvaluate Simulation Into CarSimulation

Move files from `Game.SplineEvaluate/` into `Game.CarSimulation/SplineSimulation/`:

| Source | Destination |
|--------|-------------|
| `SplineEvaluate/Simulation/SplineEvaluateDriver.cs` | `CarSimulation/SplineSimulation/SplineEvaluateDriver.cs` |
| `SplineEvaluate/Simulation/SplineEvaluateRunnerService.cs` | `CarSimulation/SplineSimulation/SplineEvaluateRunnerService.cs` |
| `SplineEvaluate/Simulation/SplineMotionState.cs` | `CarSimulation/SplineSimulation/SplineMotionState.cs` |
| `SplineEvaluate/Simulation/SplineCurvatureHelper.cs` | `CarSimulation/SplineSimulation/SplineCurvatureHelper.cs` |
| `SplineEvaluate/Definitions/DriverPersonality.cs` | `CarSimulation/SplineSimulation/DriverPersonality.cs` |
| `SplineEvaluate/Definitions/LaneProfile.cs` | `CarSimulation/SplineSimulation/LaneProfile.cs` |
| `SplineEvaluate/Definitions/SplineDriverConfig.cs` | `CarSimulation/SplineSimulation/SplineSimulationConfig.cs` |

**Namespace changes**: All moved files change from `GearEngine.SplineEvaluate.*` to `GearEngine.CarSimulation.SplineSimulation`.

Move the physics runner into its own subfolder:

| Source | Destination |
|--------|-------------|
| `CarSimulation/Simulation/SplineCarRunnerService.cs` | `CarSimulation/PhysicsSimulation/SplineCarRunnerService.cs` |
| `CarSimulation/Simulation/SplineCarRunnerContext.cs` | `CarSimulation/PhysicsSimulation/SplineCarRunnerContext.cs` |
| `CarSimulation/Definitions/SplineCarRunnerConfigSO.cs` | `CarSimulation/PhysicsSimulation/PhysicsSimulationConfig.cs` |
| `CarSimulation/CarAreaModifier.cs` | `CarSimulation/PhysicsSimulation/CarAreaModifier.cs` |
| `CarSimulation/Simulation/CarAreaSensor.cs` | `CarSimulation/PhysicsSimulation/CarAreaSensor.cs` |

**Namespace changes**: Physics files move to `GearEngine.CarSimulation.PhysicsSimulation`.

### M2 — Config Hierarchy

Make both configs inherit from `SimulationConfigBase`:

```csharp
// PhysicsSimulation/PhysicsSimulationConfig.cs (formerly SplineCarRunnerConfigSO)
public class PhysicsSimulationConfig : SimulationConfigBase
{
    // ... all existing fields unchanged ...
}

// SplineSimulation/SplineSimulationConfig.cs (formerly SplineDriverConfig)
public sealed class SplineSimulationConfig : SimulationConfigBase
{
    // ... all existing fields unchanged ...
}
```

Both runners implement `ISimulationRunnerService`:

```csharp
// PhysicsSimulation/SplineCarRunnerService.cs
public class SplineCarRunnerService : ISimulationRunnerService, ITickable { ... }

// SplineSimulation/SplineEvaluateRunnerService.cs
public sealed class SplineEvaluateRunnerService : ISimulationRunnerService, ITickable { ... }
```

### M3 — Unify Bootstrap/Installer/Scope

#### `CarTrackScope` (updated)

```csharp
public sealed class CarTrackScope : SceneFoundationScope
{
    [SerializeField] private CarTrackBootstrap sceneBootstrap;
    
    [Header("Simulation Config (determines pipeline)")]
    [InlineEditor]
    [SerializeField] private SimulationConfigBase simulationConfig; // THE SWITCH — Odin draws the full config inline
    
    protected override void InstallFeatureServices(IContainerBuilder builder)
    {
        builder.RegisterInstance(simulationConfig);
        new CarTrackInstaller().Install(builder, simulationConfig);
        builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
    }
}
```

#### `CarTrackInstaller` (updated)

```csharp
public sealed class CarTrackInstaller
{
    public void Install(IContainerBuilder builder, SimulationConfigBase config)
    {
        builder.Register<TrackSimulationFactory>(Lifetime.Singleton);
        builder.RegisterEntryPoint<RaceManagerService>(Lifetime.Singleton).AsSelf();

        switch (config)
        {
            case PhysicsSimulationConfig physics:
                builder.RegisterInstance(physics);
                builder.RegisterEntryPoint<SplineCarRunnerService>(Lifetime.Singleton)
                       .As<ISimulationRunnerService>().AsSelf();
                break;
                
            case SplineSimulationConfig spline:
                builder.RegisterInstance(spline);
                builder.RegisterEntryPoint<SplineEvaluateRunnerService>(Lifetime.Singleton)
                       .As<ISimulationRunnerService>().AsSelf();
                break;
                
            default:
                throw new InvalidOperationException(
                    $"[CarTrackInstaller] Unknown config type: {config.GetType().Name}");
        }
    }
}
```

**This is the key change**: Dragging a `PhysicsSimulationConfig` asset vs a `SplineSimulationConfig` asset into the Scope inspector field changes the entire simulation pipeline at play time.

### M4 — Wire RaceManagerService

```csharp
public sealed class RaceManagerService : ITickable
{
    private readonly ISimulationRunnerService runner;

    public RaceManagerService(ISimulationRunnerService runner)
    {
        this.runner = runner;
        this.runner.OnLapCompleted += HandleLapCompleted;
    }

    public void StartRace(RaceState state)
    {
        state.Phase = SimulationLifecycleState.Running;
        runner.SetPaused(state.Car, false);
    }
    // ... rest unchanged ...
}
```

### M5 — Clean Up Old `Game.SplineEvaluate` Assembly

After all code is moved:

1. Delete the `Game.SplineEvaluate.asmdef` and all moved files.
2. Keep `SplineEvaluate/Presentation/` (HUD, Gizmos) if they have scene-specific UI — OR move them into `CarSimulation/Presentation/` if they can be generalized.
3. Keep `SplineEvaluate/Editor/` (SplineEvaluateSceneSetup) — move to `CarSimulation/Editor/`.
4. Update any remaining `using GearEngine.SplineEvaluate.*` to `GearEngine.CarSimulation.SplineSimulation`.

### M6 — Delete Test2 Scene & Scripts

Test2 was temporary scaffolding to test the spline pipeline in isolation. After unification, the main scene can run both pipelines via config swap, making Test2 redundant.

1. Delete `Assets/Test2/Test2Bootstrap.cs`, `Test2HUD.cs`, `Test2Gizmos.cs`.
2. Delete `Assets/Test2/SplineTrack_Scene.unity` and associated assets.
3. Delete the entire `Assets/Test2/` folder.
4. Update `SplineEvaluateDriverTests.cs` to use new namespaces (`GearEngine.CarSimulation.SplineSimulation`).
5. Run `validate-changes.cmd` — all green.

### M7 — Documentation

1. Create `Docs/CarSimulation.md` documenting both pipelines.
2. Update `Architecture.md` if it references the old `Game.SplineEvaluate` assembly.

## Concrete Steps

### M0 Steps
1. Create `CarSimulation/Simulation/ISimulationRunnerService.cs`
2. Create `CarSimulation/Definitions/SimulationConfigBase.cs`
3. Compile — verify no errors.

### M1 Steps
1. Create `CarSimulation/SplineSimulation/` folder.
2. Create `CarSimulation/PhysicsSimulation/` folder.
3. Move spline files (7 files) with namespace updates.
4. Move physics files (5 files) with namespace updates.
5. Fix all `using` statements in moved files.
6. Compile — verify no errors.

### M2 Steps
1. Rename `SplineCarRunnerConfigSO` to `PhysicsSimulationConfig`, inherit `SimulationConfigBase`.
2. Rename `SplineDriverConfig` to `SplineSimulationConfig`, inherit `SimulationConfigBase`.
3. Make both runners implement `ISimulationRunnerService`.
4. Compile — verify no errors.
5. **Update serialized assets** — the script GUIDs in `.meta` files must stay the same (move files, don't recreate).

### M3 Steps
1. Update `CarTrackInstaller.Install()` to accept `SimulationConfigBase` and branch.
2. Update `CarTrackScope` to hold `SimulationConfigBase` instead of `SplineCarRunnerConfigSO`.
3. Delete `SplineEvaluateScope`, `SplineEvaluateInstaller`, `SplineEvaluateBootstrap`.
4. Compile — verify no errors.

### M4 Steps
1. Update `RaceManagerService` constructor to take `ISimulationRunnerService`.
2. Update all call sites.
3. Compile — verify no errors.

### M5 Steps
1. Move remaining Presentation files.
2. Delete `Game.SplineEvaluate.asmdef`.
3. Fix all remaining references.
4. Compile — verify no errors.

### M6 Steps
1. Delete `Assets/Test2/` folder entirely.
2. Update `SplineEvaluateDriverTests.cs` namespaces to `GearEngine.CarSimulation.SplineSimulation`.
3. Run `SplineEvaluateDriverTests` — all green.
4. Run validate-changes script.

### M7 Steps
1. Write `Docs/CarSimulation.md`.
2. Update `Architecture.md`.

## Validation and Acceptance

| Criterion | How to verify |
|-----------|---------------|
| Physics pipeline works unchanged | Load main race scene with `PhysicsSimulationConfig` — car drives with Rigidbody as before |
| Spline pipeline works unchanged | Load scene with `SplineSimulationConfig` — car drives with pure math as before |
| Switching pipeline is config-only | Swap the config asset in the Scope inspector — different simulation runs |
| `RaceManagerService` lap counting works with both | Complete a lap in both modes — `RaceState.CurrentLap` increments |
| All existing tests pass | `validate-changes.cmd` clean |
| Test2 folder deleted | `Assets/Test2/` does not exist |
| No broken serialized assets | Open scenes/prefabs — no "Missing Script" or null-reference warnings |

## Idempotence and Recovery

- **File moves preserve `.meta` GUIDs** — Unity tracks assets by GUID, not path. Moving files (not recreating) ensures all scene/prefab/asset references stay intact.
- **Namespace renames** are mechanical find-replace operations. If any step fails, revert the namespace change.
- **Config migration** — existing `SplineCarRunnerConfigSO` assets gain a new base class but keep all serialized fields. Unity handles polymorphic SO serialization natively.

## Artifacts and Notes

### Important GUIDs to Preserve

| Asset | GUID | Notes |
|-------|------|-------|
| `SplineCarRunnerConfigSO.cs` | check `.meta` | Becomes `PhysicsSimulationConfig` — keep GUID |
| `SplineDriverConfig.cs` | check `.meta` | Becomes `SplineSimulationConfig` — keep GUID |
| `SplineEvaluateDriver.cs` | check `.meta` | Moves folder — keep GUID |
| `SplineEvaluateRunnerService.cs` | check `.meta` | Moves folder — keep GUID |

## Interfaces and Dependencies

### Upstream (consumed by this plan)
- `Scaffold.Entities` — `EntityInstance<T>`, `EntityDefinition`
- `VContainer` — DI registration
- `Game.SceneFoundation` — `SceneFoundationScope` base
- `PROMETEO` — `PrometeoCarController` (physics pipeline only)
- `Unity.Splines` — `Spline`, `SplineContainer`, `SplineUtility`

### Downstream (consumers of this plan's output)
- **Main Race Scene** — uses `CarTrackScope` + config swap
- **Any future scene** — drops a config asset to choose simulation
