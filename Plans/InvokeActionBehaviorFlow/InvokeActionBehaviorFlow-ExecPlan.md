# Add behavior-tree flow and interruption to Invoke Action

This ExecPlan is a living document.

## Purpose / Big Picture

Extend `InvokeActionCommand` from a completion-only list into a small behavior-tree-style composite. Designers can select Sequence, Selector, Parallel, Parallel Selector, or Utility Selector, and actions can finish with success or failure. A new Perform Interruption action can stop one or more running nested actions in the current Invoke Action or in another explicitly referenced Invoke Action. Block uses the same composite runtime for its visible Commands while retaining the existing command-list view.

## Progress

- [x] Review the existing Invoke Action runtime, editor, tests, and Opsive reference behavior.
- [x] Add backward-compatible status and interruption contracts to the action layer.
- [x] Implement Sequence, Selector, Parallel, and Parallel Selector semantics.
- [x] Add stable nested-action identifiers and keep them aligned during add, remove, move, and reorder operations.
- [x] Add Return Status and Perform Interruption actions with inspector task selection.
- [x] Add automated regression tests for all execution modes and interruption.
- [x] Add Utility Selector with variable-backed scores, per-frame reevaluation, and Block During Execution.
- [x] Format and compile the modified code, run focused Unity tests, and attempt the repository validation gate.
- [x] Review the Block Inspector ExecPlan and trace the relationship between Block, CommandTrack, and Invoke Action.
- [x] Make composite execution visible and editable inside every Invoke Action, including empty and single-action groups.
- [x] Make behavior-tree Flow actions discoverable by their CommandInfo category and name.
- [x] Add editor regression coverage, compile, and rerun the focused Unity tests.
- [x] Extract the composite scheduler into Scaffold so Block and Invoke Action use one runtime engine.
- [x] Add Await to Invoke Action and make it applicable only to parallel composites.
- [x] Add Ordered, weighted Random, and unweighted Shuffle ordering to Sequence and Selector.
- [x] Replace Block command execution internals with the shared composite settings while preserving the command-list view.
- [x] Add contextual tooltips and regression coverage for combined Execution, Await, and Order selections.
- [x] Add an optional cross-execution repeat guard for Random and Shuffle, visible only with multiple children.
- [x] Rename the designer-facing composite wrapper to Action Invoker and auto-balance nested-action Random weights with manual overrides.
- [x] Add live execution feedback and action validation badges to Block and Action Invoker lists.
- [x] Preserve per-row success/failure colors until Stop or a new execution resets them.

## Surprises & Discoveries

- `IAction.Execute` currently exposes only a parameterless completion callback. Existing actions therefore have no way to report failure.
- Some actions are asynchronous and already implement `ActionBase.OnStopExecuting`, so interruption can reuse that lifecycle without coupling the composite to individual action implementations.
- Utility Selector required a score source usable by every existing action. Per-action `FloatData` metadata provides constants, Blackboard variables, and ScriptableObject values without forcing action subclasses to implement a new scoring interface.
- A Block cannot literally instantiate a hidden `InvokeActionCommand` because Scaffold is an upstream assembly. The equivalent architecture is to adapt each visible `Command` to the same Scaffold-owned runner used by Invoke Action.
- The Invoke Action editor hid its execution selector for most single-action commands. This made the completed runtime modes look absent in the normal authoring workflow.
- The Block Inspector layout plan intentionally preserved the old command/detail split. That preservation decision accidentally kept the new composite configuration below the resize divider and easy to miss.
- `Scaffold` is an upstream runtime assembly and `Game.GearEngine` depends on it. Block therefore cannot instantiate `InvokeActionCommand` directly without an assembly cycle. The reusable unit is a Scaffold-owned composite runner with adapters for Command and IAction children.
- Applying the shared composite to CommandTrack objects made Random and Shuffle ineffective for the common one-track Block. The correct hidden children are the visible Commands, flattened in track order.
- Migrated flow actions inspect the command list through `InvokeActionCommand` wrappers. Condition and End discovery must unwrap nested actions before testing their behavior-tree type.
- Random ordering and Utility selection need different metadata. Random uses a designer-authored relative percentage weight from 0 to 100, while Utility continues to use a variable-backed score and runtime reevaluation.

## Decision Log

- Preserve the numeric serialized values of Sequence (`0`) and Parallel (`1`). New enum values are appended.
- Existing `IAction` implementations remain source- and behavior-compatible and are treated as successful when they complete.
- Status-aware and interruptible behavior is opt-in through narrow interfaces. `ActionBase` implements both so migrated Scaffold actions gain the capability automatically.
- Nested action references use stable serialized IDs instead of list indexes so interruption targets survive reordering.
- Perform Interruption accepts a null target command to mean the current Invoke Action and an explicit target for event-branch interruption.
- Utility Selector reevaluates scores from `Update`, retains the running action on ties, and interrupts it only when another eligible action has a strictly higher score. Block During Execution suppresses this interruption.
- Utility execution remains inside the serialized `InvokeActionCommand` because that component already owns child lifecycle, stable IDs, interruption, and backward-compatible serialized data. If another reactive composite is added, extract the mode-specific runtime into Strategy objects instead of extending the mode branches further.
- The earlier decision to keep Block track scheduling separate is superseded. Block now treats each visible Command as a composite child, and Commands without an explicit status remain backward-compatible successes.
- Use `CommandInfoAttribute` category and command name for the Invoke Action add menu. This places the new tasks under `Flow/Perform Interruption` and `Flow/Return Status` instead of exposing implementation type names.
- Supersede the earlier decision to keep Block and Invoke Action runtime schedulers separate. Both hosts will use a new Scaffold-owned `CompositeExecutionRunner`; adapters preserve their different child types and avoid a circular assembly dependency.
- Await applies only to Parallel and Parallel Selector. Wait All derives the final AND/OR composite result after every child, Wait Any returns the first completed child result while other children continue, and Wait None returns success immediately after launch while children continue.
- Order applies only to Sequence and Selector. Ordered preserves list order, Random produces a weighted permutation without replacement, and Shuffle produces a uniform permutation without weights. Utility Selector retains utility-driven ordering.
- Avoid Repeating Last compares the first randomized child with the last child started in the previous execution. When they match and another enabled child exists, the runner swaps in the next randomized candidate. Invoke Action tracks the child by stable action ID; Block tracks it by Command reference, so reordering does not corrupt the guard.
- Ordered Sequence translates `Continue(index)` into a shared-runner handoff, preserving If/Else, End, loops, and jump behavior without retaining a second scheduler.
- Keep `InvokeActionCommand` as the serialized runtime type and rename only its designer-facing label to Action Invoker. This avoids breaking existing scenes and prefabs while making the wrapper distinct from its nested actions.
- For Random order, enabled nested actions without an override split the remaining 100% equally. Manual overrides reserve their requested share; if overrides exceed 100%, normalize them proportionally and assign automatic actions 0%.
- The parent Block is the sole owner of visible Execution, Await, and Order controls. Action Invoker retains its serialized settings for backward compatibility, but its inspector and compact child row do not expose composite controls.
- Execution feedback never fabricates time progress. Ordered Sequence and Selector highlight the running row, and actions implementing `IActionProgressProvider` may provide a real normalized fill. Other modes show their active waiting rule instead.
- Action validation uses existing `GetSummary()` output: `Error:` and `Warning:` prefixes map to compact built-in Unity badges with the remaining summary as the tooltip.

## Outcomes & Retrospective

Invoke Action now exposes Sequence, Selector, Parallel, Parallel Selector, and Utility Selector with behavior-tree success/failure semantics. Legacy actions remain compatible and report success, while `ActionBase` actions can explicitly fail and can be interrupted safely without a late callback resuming execution.

Perform Interruption can target multiple tasks in its current Invoke Action or in another explicitly referenced Invoke Action. Stable serialized task IDs preserve those selections across editor reordering and cross-group moves.

The disposable Unity project compiled without errors, and the focused EditMode suite passed 44 of 44 tests, including Utility Selector score changes, fallback, blocking, Blackboard-variable reevaluation, and metadata movement. `dotnet format` was run for every modified assembly. The repository validation script was attempted but could not start because PowerShell 7 (`pwsh`) is not installed on this machine; the script exited before running any checks.

The follow-up editor audit found that runtime completion was not enough: the execution selector was conditional and the outer Block exposed an unrelated field with the same label. The follow-up milestone makes the Invoke Action composite controls persistent and labels the outer setting according to its actual CommandTrack responsibility.

The follow-up disposable Unity run compiled without C# errors and passed 46 of 46 focused EditMode tests. This includes the original runtime behavior suite plus regressions proving that one-action composites retain their controls and that Perform Interruption appears at `Flow/Perform Interruption`. The repository validation shell wrapper was run through `bash`, but it still cannot start its checks because PowerShell 7 (`pwsh`) is unavailable.

The shared-runtime milestone adds Await, Ordered/Random/Shuffle, dynamic selection tooltips, weighted command/action metadata, command-level Block adaptation, and the optional cross-execution repeat guard. The disposable project compiles cleanly; 58 focused Invoke Action EditMode tests and 10 Block PlayMode tests pass. Wrapped Condition/End discovery is also covered so flow-control actions continue to work through Invoke Action commands.

The final inspector refinement renames the wrapper to Action Invoker, leaving common nested actions with only their own inputs. Random weights now appear beside each nested enabled toggle, default to an equal automatic distribution, and can be overridden or restored to automatic balancing. Project-level C# builds and deterministic style checks passed. A fresh disposable Unity test project reached its initial import but did not emit a test result XML, so the focused EditMode run remains infrastructure-blocked.

The follow-up visibility correction removes Execution, Await, Order, and repeat-guard controls from Action Invoker's inspector and compact Block-list header. These are Block-owned controls; Action Invoker continues to preserve serialized values for existing content while presenting only its action list.

The execution-feedback milestone gives ordered lists a blue running state and real left-to-right progress for measurable actions such as Wait. Random, shuffled, parallel, and utility execution instead explain the active waiting rule. Missing actions and action summaries prefixed with `Error:` or `Warning:` display row-level validation badges without expanding the action.

## Context and Orientation

Runtime action contracts live under `Assets/GearEngine/Scripts/Game/GearEngine/Core/Actions/`. The shared scheduler and settings live under `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Components/`. The composite command is `Presentation/UI/Tags/Input/InvokeActionCommand.cs`. Its custom inspector is `Editor/InvokeActionCommandEditor.cs`, and the compact Block Inspector rendering is in `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Editor/CommandListAdaptor.cs`. EditMode coverage is in `Tests/Editor/InvokeActionCommandTests.cs`; Block integration coverage is in `Tests/PlayMode/BlockTrackExecutionTests.cs`.

## Plan of Work

Introduce `ActionExecutionStatus`, `IActionWithStatus`, and `IInterruptibleAction`. Update `ActionBase` so `Continue()` means success, `Fail()` means failure, and interruption invokes `OnStopExecuting` without allowing a late callback to resume the composite.

Refactor `InvokeActionCommand` to track running action indexes and an execution generation. Sequential composites advance according to status; parallel composites terminate according to AND/OR behavior and interrupt remaining children when their result is decided. Utility Selector chooses the highest variable-backed score, retries the next-highest action after failure, and reevaluates scores every frame. Add public interruption by stable action ID.

Add `ReturnActionStatus` and `PerformInterruption` actions in the Scaffold namespace. Extend the Invoke Action editor to maintain action IDs and draw Perform Interruption targets as task checkboxes. Update the compact group header to display every enum name.

Extract `CompositeExecutionRunner` and the shared Execution, Await, and Order enums into Scaffold. Adapt IAction and Command children separately, and keep ordered flow-control routing by translating Command continuation indexes into runner task indexes.

## Concrete Steps

1. Add action status/interruption contracts and adapt `ActionBase`.
2. Refactor `InvokeActionCommand`, preserving legacy enum values and existing serialized fields.
3. Synchronize `actionIds` in runtime list operations and the custom editor.
4. Add the two flow actions and the Perform Interruption property drawer.
5. Add the shared runner, Await, Random/Shuffle, Block command adapter, and contextual editor controls.
6. Add regression tests, format modified C# files, compile affected projects, and run focused EditMode and PlayMode tests in a disposable Unity clone.

## Validation and Acceptance

- Sequence stops after the first failure and succeeds only when all enabled children succeed.
- Selector stops after the first success and fails only when all enabled children fail.
- Parallel starts its children together. Wait All waits for all and applies AND status; Wait Any returns the first completion status; Wait None returns success immediately.
- Parallel Selector starts its children together. Wait All waits for all and applies OR status; Wait Any returns the first completion status; Wait None returns success immediately.
- Utility Selector starts the highest-scoring eligible child, falls back in descending utility order after failure, and interrupts a running child when a strictly higher score appears.
- Utility Selector respects Block During Execution and preserves utility settings through reorder and cross-group moves.
- Legacy `IAction` children still work and count as success.
- Perform Interruption stops all selected running tasks and returns its configured status.
- Reordering actions does not change interruption targets.
- A stopped parent Block interrupts all running nested actions.
- Focused Unity tests pass with zero compilation errors.
- Empty, single-action, and multi-action Invoke Action inspectors always show the composite execution selector.
- An expanded Invoke Action group exposes its execution method without requiring the user to find the selected-command panel below the resize divider.
- The Block summary keeps the existing Event/Execution/secondary-control layout while configuring the same composite semantics used by Invoke Action.
- The Invoke Action add menu contains `Flow/Perform Interruption` and `Flow/Return Status`.
- Invoke Action and Block expose the same serialized Execution, Await, and Order enum types and delegate execution to the same composite runner.
- Await is shown only for Parallel and Parallel Selector; Order is shown only for Sequence and Selector.
- Random displays a 0–100 Weight for each applicable child and preserves that metadata through reordering and cross-group moves.
- Shuffle changes child order once per execution without displaying or consulting weights.
- Execution and secondary-control tooltips describe the currently selected combination.
- Random and Shuffle expose Avoid Repeating Last only when more than one child exists, and the first child of a new run does not immediately repeat the previous run's last child when another enabled child is available.
- Action Invoker does not render Execution, Await, Order, or repeat-guard controls in either editor presentation.

## Idempotence and Recovery

Existing Invoke Action assets migrate lazily: missing action IDs are generated when the command or inspector synchronizes metadata. Re-running synchronization is safe and preserves non-empty IDs. Legacy execution enum values are unchanged.

## Artifacts and Notes

Reference semantics come from Opsive Behavior Designer Pro documentation for Flow, Sequence, Selector, Parallel, Parallel Selector, and Perform Interruption.

## Interfaces and Dependencies

No new assembly dependency is required. Shared runtime additions stay in `Scaffold`; IAction adaptation stays in `Game.GearEngine`; editor presentation remains in the existing editor assemblies.
