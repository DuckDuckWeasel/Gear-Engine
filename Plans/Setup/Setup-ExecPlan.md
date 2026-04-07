# Foundation & Project Setup

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

Maintained in accordance with `PLANS.md`.

## Purpose / Big Picture

Bootstrap the Unity project with modular architecture, assembly definitions, VContainer DI, and initial documentation.

**Prerequisites**: Unity Editor installed, .NET SDK available for analyzer builds
**Estimate**: ~45 min

## Progress

- [ ] Project structure created
- [ ] All files from scope implemented
- [ ] Acceptance criteria verified

## Context and Orientation

MyProject is a MyProject — Unity project with Scaffold modular architecture.

This is the first ExecPlan — it establishes the foundational project structure.

## Plan of Work

- Create the Unity project structure (`Assets/App/`, `Assets/Core/`, `Assets/Infra/`)
- Define assembly definitions (`.asmdef`) for each module with correct references
- Set up VContainer as the DI container with a root `LifetimeScope`
- Create `Architecture.md` documenting module layout, dependencies, and boundaries
- Create the initial Roslyn analyzer project under `Analyzers/`
- Create the quality gate script `.agents/scripts/validate-changes.cmd`
- Create a sample module with EditMode tests to validate the pipeline

## Files to Create

### Architecture.md
- Module layout, dependency graph, and architectural boundaries. Single source of truth for directory structure.

### Assets/App/Scenes/Boot.unity
- Bootstrap scene with the root VContainer LifetimeScope.

### Assets/App/Boot/AppLifetimeScope.cs
- Root LifetimeScope registering core services via VContainer.

### Assets/App/App.asmdef
- Assembly definition for the App layer. References: Core, Infra, VContainer.

### Assets/Core/Core.asmdef
- Assembly definition for core gameplay modules. No App/Infra references.

### Assets/Infra/Infra.asmdef
- Assembly definition for infrastructure. References: Core.

### Assets/Core/Tests/CoreTests.asmdef
- EditMode test assembly for Core module.

### Assets/Core/Tests/SampleTests.cs
- Sample EditMode test to validate the test pipeline runs.

### Analyzers/Analyzers.csproj
- Roslyn analyzer project targeting netstandard2.0.

### .agents/scripts/validate-changes.cmd
- Quality gate script: runs EditMode tests, PlayMode tests, and analyzer checks.

### Docs/Architecture.md
- Detailed documentation of the architecture and module boundaries.

## Validation and Acceptance

- [ ] Unity Editor opens the project without compilation errors
- [ ] VContainer LifetimeScope initializes on Play without exceptions
- [ ] EditMode tests pass via `.agents/scripts/validate-changes.cmd`
- [ ] `dotnet build -c Release` on the Analyzers project compiles without errors
- [ ] All `.asmdef` files have correct references and no circular dependencies

## DO NOT Implement

- Gameplay logic (future ExecPlans)
- UI/UX screens or views (future ExecPlans)
- PlayMode test infrastructure (add when first PlayMode feature is implemented)
- CI/CD pipeline (separate ExecPlan)

## Surprises & Discoveries

_None yet._

## Decision Log

_No decisions recorded yet._

## Outcomes & Retrospective

_Not yet completed._
