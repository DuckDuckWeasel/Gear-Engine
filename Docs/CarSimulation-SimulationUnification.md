# Simulation Unification

## Overview

The `CarSimulation` module supports two distinct simulation pipelines for racing:

| Pipeline | Namespace | Runner | Config |
|---|---|---|---|
| **Physics** | `GearEngine.CarSimulation.PhysicsSimulation` | `SplineCarRunnerService` | `SplineCarRunnerConfigSO` |
| **Spline** | `GearEngine.CarSimulation.SplineSimulation` | `SplineEvaluateRunnerService` | `SplineDriverConfig` |

Both pipelines implement `ISimulationRunnerService` and derive their configs from `SimulationConfigBase`, allowing seamless switching at the scene level.

## How to Switch Pipelines

1. Create a new `ScriptableObject` asset via the Unity menu:
   - **Physics**: `GearEngine → Simulation → Physics Simulation Config`
   - **Spline**: `GearEngine → Simulation → Spline Simulation Config`

2. On the scene's `CarTrackScope` or `RaceScope`, assign the desired config asset to the **Simulation Config** field.

3. Press Play. The `CarTrackInstaller` uses pattern matching to register the correct runner service:
   ```csharp
   switch (config)
   {
       case SplineCarRunnerConfigSO physics:
           // registers SplineCarRunnerService
           break;
       case SplineDriverConfig spline:
           // registers SplineEvaluateRunnerService
           break;
   }
   ```

## Architecture

```
CarSimulation/
├── Definitions/
│   └── SimulationConfigBase.cs        ← abstract ScriptableObject base
├── Simulation/
│   ├── ISimulationRunnerService.cs    ← shared interface
│   ├── ISimulationInitParams.cs       ← marker for init params
│   ├── RaceManagerService.cs          ← depends on ISimulationRunnerService
│   ├── RaceState.cs
│   └── CarTelemetryData.cs
├── PhysicsSimulation/
│   ├── SplineCarRunnerService.cs      ← ISimulationRunnerService impl
│   ├── SplineCarRunnerConfigSO.cs     ← SimulationConfigBase impl
│   ├── PhysicsInitParams.cs           ← ISimulationInitParams impl
│   ├── RoguelikeCarStats.cs
│   ├── CarVariableSet.cs
│   ├── SplineCarRunnerContext.cs
│   ├── CarAreaModifier.cs
│   └── CarAreaSensor.cs
├── SplineSimulation/
│   ├── SplineEvaluateRunnerService.cs ← ISimulationRunnerService impl
│   ├── SplineDriverConfig.cs          ← SimulationConfigBase impl
│   ├── SplineInitParams.cs            ← ISimulationInitParams impl
│   ├── SplineEvaluateDriver.cs
│   ├── DriverPersonality.cs
│   ├── LaneProfile.cs
│   ├── SplineMotionState.cs
│   ├── SplineCurvatureHelper.cs
│   └── CurveMode.cs
└── Bootstrap/
    ├── CarTrackInstaller.cs           ← pattern-matching registration
    ├── CarTrackScope.cs               ← [InlineEditor] config field
    └── CarTrackBootstrap.cs
```

## Key Design Decisions

- **No enums**: The config class itself determines the pipeline via `is` pattern matching.
- **Odin `[InlineEditor]`**: Allows editing the polymorphic config directly in the Inspector.
- **Shared interface**: `RaceManagerService` is pipeline-agnostic — it only depends on `ISimulationRunnerService`.
- **Init params polymorphism**: Each pipeline defines its own `ISimulationInitParams` implementation carrying pipeline-specific data.
