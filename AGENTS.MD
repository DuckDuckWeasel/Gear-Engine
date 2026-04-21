# MyProject - AI Agent Instructions

This is the **primary context file** for all AI coding agents in this repository. Read it fully before making any changes. It takes precedence over any other instructions.

## Project Summary

MyProject is a MyProject — Unity project with Scaffold modular architecture.

## Tech Stack

- **Engine**: Unity
- **Language**: C#
- **Framework**: MVVM (Model-View-ViewModel)
- **DI**: VContainer
- **Code Enforcement**: Custom Roslyn Analyzers (Analyzers/)
- **Code Generation**: Roslyn Source Generators (Generators/)

## Directory and Module Reference

Directory layout, module relationships, and architectural structure are documented in [Architecture.md](Architecture.md). Use that file as the single source of truth for repository structure details.

## Code Rules (Non-Negotiable)

1. **English-Only Outputs**: All generated content (code comments, docs, plans, commit messages, test names, UI placeholder text, and agent replies) must be in English.
2. **Separation of Concerns**: Keep App/UI and scene-specific presentation separate from unrelated modules.
3. **Modular Integrity**: Declare all dependencies explicitly via `.asmdef`/`.csproj`. Never bypass architectural boundaries.
4. **MonoBehaviours**: Use them in bootstrap, App/Infra presentation, and in Core modules that own engine-facing gameplay building blocks. Do not add MonoBehaviours to assemblies that are intentionally Unity-free (`noEngineReferences: true`).
5. **Mandatory Tests**: Every module must have tests.
6. **Documentation**: Every module needs a `.md` file or folder under `Docs/`.
7. **Plans go in `Plans/`**: All planning documents belong there.
8. **New source generators go in `Generators/`**.
9. **Analyzer compliance**: Fix all Roslyn analyzer warnings/errors before committing.
10. **Bugfixes require regression tests**: Every bug fix must add or update an automated test that fails before the fix and passes after it.
11. **Pragma suppression approval**: Never introduce a new `#pragma warning disable` unless the user explicitly approves it.
12. **Error Handling**: Always use try/catch in async operations and service methods. Use `Debug.LogError` for Unity-side errors, never `Debug.Log` for error conditions. Validate inputs and null-check injected dependencies. Never swallow exceptions silently — at minimum log them. Prefer guard clauses (`if (x == null) throw ...`) over deep nesting.
13. **Never use `ConfigureAwait`:** Do not call `ConfigureAwait` on `Task` / `Task<T>` (or any awaitable) in this codebase. Unity and Unity Gaming Services rely on the Unity synchronization context; `ConfigureAwait(false)` resumes on thread pool threads and breaks main-thread-only APIs (for example `Application.get_cloudProjectId` inside UGS). Do not add `ConfigureAwait(true)` either—it is redundant with the default and adds noise.

## Error Handling Standards

All code must follow these error handling principles. Failing silently is **never acceptable**.

### Unity C# (MonoBehaviours and Services)

```csharp
// ✅ Correct — explicit error handling with Debug.LogError and safe defaults
public async UniTask<PlayerData> LoadDataAsync(string id)
{
    try
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID cannot be null or empty.");
        return await _dataService.FetchAsync(id);
    }
    catch (Exception ex)
    {
        Debug.LogError($"[PlayerLoad] Failed to load data for ID {id}: {ex.Message}\n{ex.StackTrace}");
        _eventBus.Publish(new ErrorEvent("Failed to load player data"));
        return PlayerData.Empty;
    }
}

// ❌ Incorrect — swallowing exceptions or using standard log
public async UniTask<PlayerData> LoadDataAsync(string id)
{
    try { return await _dataService.FetchAsync(id); }
    catch (Exception ex) { Debug.Log("error: " + ex.Message); return null; }
}
```

**Rules:**
1. **Always use try/catch** in async methods and network operations.
2. **Use `Debug.LogError`** for all caught exceptions so they appear in error tracking (Bugsnag/Crashlytics).
3. **Provide feedback** to the player/user via events (e.g. `_eventBus.Publish(new ErrorEvent(...))`) for non-recoverable errors.
4. **Fail fast** using guard clauses (`if (x == null) throw new ArgumentNullException();`).
5. **Return safe defaults** (`PlayerData.Empty`, empty lists) rather than `null` to prevent downstream `NullReferenceException`s.

## How to Build

- **Application**: Unity Editor (File > Build Settings) or Unity CLI batch mode

## How to Test

- **Quality gate**: Run `.agents/scripts/validate-changes.cmd` from the repository root.

## Workflows

### /create-module
Use the `/create-module` workflow (`.agents/workflows/create-module.md`). Create a new module with proper structure

### /create-custom-analyzer
Use the `/create-custom-analyzer` workflow (`.agents/workflows/create-custom-analyzer.md`). Create a custom Roslyn analyzer rule

### /check-analyzers
Use the `/check-analyzers` workflow (`.agents/workflows/check-analyzers.md`). Report analyzer diagnostics


## Where to Put Plans

### ExecPlans

When writing complex features or significant refactors, use an ExecPlan (as described in `PLANS.md`) from design to implementation. Use `MILESTONE.md` for milestone plan structure when a milestone needs its own file.

## Milestone Quality Loop

For every milestone, follow this sequence before marking it complete:

1. Implement the milestone scope.
2. If bug fix, add/update regression test that reproduces the bug and confirm it fails before the fix.
3. Re-run regression test and confirm it passes after the fix.
4. Run `.agents/scripts/validate-changes.cmd`.
5. If the gate fails, fix all reported failures.
6. Re-run until the gate is clean.
7. Commit the milestone changes.

## Where to Put Documentation

All module documentation lives in `Docs/`. Every module **must** have a corresponding `.md` file or subfolder there.

## Agent Behavior Guidelines

- Read architecture docs before touching any code.
- Read `PLANS.md` before authoring or executing any ExecPlan.
- Always fix warnings before committing.
- Prefer editing existing files over creating new ones.
- Keep changes minimal and focused — no speculative abstractions or premature generalization.
- Use existing modules as reference when unsure about patterns.
