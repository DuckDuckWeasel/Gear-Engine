# Child Action Composite Controls Fix

## Issue

Action Invoker rendered child-level Execution, Await, Order, and repeat-guard controls in both its inspector and the compact Block command-list header. Those settings belong to the parent Block.

## Root Cause

`InvokeActionCommandEditor` drew the serialized composite fields directly. `CommandListAdaptor` also rendered execution and secondary enum popups for Action Invoker group rows.

## Fix

- Removed the inspector fields from `InvokeActionCommandEditor`.
- Removed the compact-row execution, secondary, and repeat-guard renderers.
- Preserved serialized runtime values for backward compatibility with existing scenes and prefabs.
- Added an editor regression test that asserts the child-control helper and compact popup renderers are absent.

## Verification

- Unity Editor log parser: no current compiler errors or exceptions.
- `Game.GearEngine.Editor`: builds with 0 errors.
- `Game.GearEngine.Tests`: builds with 0 errors.
- `ScaffoldEditor`: builds with 0 errors.
- Scoped formatting and style checks pass for the four modified C# files.

The focused Unity EditMode test was not launched because `Temp/UnityLockfile` shows the project is open in another Unity Editor instance. The repository PowerShell runner is also unavailable because `pwsh` is not installed.
