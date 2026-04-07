# Codex Execution Plans (ExecPlans)

**Repository status:** This project is treated as a **complete sample**. The rules below still apply if you author **new** ExecPlans for maintenance or extensions. Existing plans under `Plans/` are a **historical record** of how work was executed.

---

This document describes the requirements for an execution plan ("ExecPlan"), a design document that a coding agent can follow to deliver a working feature or system change.

## How to use ExecPlans and PLANS.md

When authoring an executable specification (ExecPlan), follow PLANS.md _to the letter_. Be thorough in reading (and re-reading) source material to produce an accurate specification.

When implementing an executable specification (ExecPlan), unless explicitly told by the user not to, prompt for next steps. Keep all sections up to date, add or split entries in the list at every stopping point.

## Requirements

NON-NEGOTIABLE REQUIREMENTS:

* Every ExecPlan must be fully self-contained.
* Every ExecPlan is a living document. Contributors are required to revise it as progress is made.
* Every ExecPlan must enable a complete novice to implement the feature end-to-end.
* Every ExecPlan must produce a demonstrably working behavior.
* Every ExecPlan must define every term of art in plain language.
* Every bug fix in an ExecPlan must include a regression test.

Plan file locations:

* ExecPlan main file: `Plans/[FeatureName]/[FeatureName]-ExecPlan.md`
* ExecPlan milestone detail file (optional): `Plans/[FeatureName]/milestones/ExecPlan-Milestone-[x].md`

## Skeleton of a Good ExecPlan

    # <Short, action-oriented description>

    This ExecPlan is a living document.

    ## Purpose / Big Picture
    ## Progress
    - [x] Example completed step.
    - [ ] Example incomplete step.

    ## Surprises & Discoveries
    ## Decision Log
    ## Outcomes & Retrospective
    ## Context and Orientation
    ## Plan of Work
    ## Concrete Steps
    ## Validation and Acceptance
    ## Idempotence and Recovery
    ## Artifacts and Notes
    ## Interfaces and Dependencies
