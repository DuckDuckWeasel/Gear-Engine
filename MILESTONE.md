# Milestone Plans

Use this file as a short guide for writing milestone plan docs referenced by an ExecPlan.

Milestone plan file path:

`Plans/[FeatureName]/milestones/ExecPlan-Milestone-[x].md`

When to create one:

Create a milestone plan when a milestone is too complex for a simple paragraph in the parent ExecPlan.

## Goal

State the milestone objective in 2-4 sentences.

## Deliverable

List the concrete outputs expected at the end of the milestone.

## Plan

Describe the execution sequence in short, concrete steps:

1. Implement the milestone scope.
2. If bug fix, add/update regression test that reproduces the bug and confirm it fails before the fix.
3. Re-run regression test and confirm it passes after the fix.
4. Run `.agents/scripts/validate-changes.cmd`.
5. If the gate fails, fix all reported failures.
6. Re-run until the gate is clean.
7. Commit the milestone changes.

## Snippets and Samples

Add short examples only when useful. Keep examples concise and focused on verification.
