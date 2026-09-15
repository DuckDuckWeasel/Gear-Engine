# Git Maintenance Report

Date: 2026-09-15

Baseline: `main` and `origin/main` at `bcda1ccf`

Maintenance branch: `codex/repository-maintenance`

## Scope and safety boundary

This report records the archive-then-prune maintenance performed locally and the shared GitHub cleanup that remains behind an explicit approval gate. No history was rewritten. No remote tag was pushed, no pull request was closed, and no remote branch was deleted.

Local archive tags are annotated tags. The tag object has its own object ID, while the peeled target below is the original branch or stash commit that the tag preserves.

## Local archive tags

| Archive tag | Preserved target | Disposition |
| --- | --- | --- |
| `archive/2026-09-15/stash-fix-tutorial-command-inspector-ui` | `bf53c05775009f63c66420abb454ce288f1cac5f` | Verified, then the corresponding stash was dropped. |
| `archive/2026-09-15/stash-feature-design-system` | `c5e0a060c28fb054476d33dffff72f397244941a` | Verified, then the corresponding stash was dropped. |
| `archive/2026-09-15/stash-feature-arcade` | `1c159fb7fb6e961f5b93ab7ebfe423eb62256588` | Verified, then the corresponding stash was dropped. |
| `archive/2026-09-15/stash-feature-android` | `639d8b653ae0901e86fd1d874a990f70cfa77993` | Verified, then the corresponding stash was dropped. |
| `archive/2026-09-15/branch-ftue-blackboard-tutorial` | `99a76003e762171acba3aed2ad2b7bba73ee1f09` | Verified, then the local branch was removed. Preserve for a future FTUE gap-analysis task; do not treat it as shipped behavior. |
| `archive/2026-09-15/branch-feature-design-system` | `9e691af5d5b2f155a30daf2a99ccfb2c15cb02b0` | Verified, then the local branch was removed. |
| `archive/2026-09-15/branch-feature-new-ui` | `351052333c3ddae4775c9b9d12a3cf590f2b30ce` | Verified, then the local branch was removed. |

The stash list is now empty. Each removed stash remains inspectable with `git show <archive-tag>` or can be recovered onto a temporary branch with `git branch <recovery-branch> <archive-tag>`. A stash commit can have multiple parents, so inspect its parent structure before attempting to reapply it as a patch.

## Removed local branches

The following three unmerged local branch refs were removed only after their archive tags were created and verified:

- `codex/ftue-blackboard-tutorial`
- `feature/design-system`
- `feature/new-ui`

The following 40 branches were already fully merged and were removed with Git's safe branch-deletion mode:

- `Art/AnimoraAnimations`
- `Art/Audio`
- `agent/ui-effect-pattern-layers`
- `codex/action-invoker-inspector-rendering`
- `codex/blackboard-runtime-refactor`
- `codex/cloud-verification`
- `codex/grid-board-inventory-screen-space`
- `codex/tutorial`
- `create-test-worktree`
- `cursor/simplified-race-sim-9917`
- `feature/adjust-prefabs`
- `feature/ads`
- `feature/analytics`
- `feature/android`
- `feature/arcade`
- `feature/boot-loading-manager`
- `feature/car-entity`
- `feature/contetti-vfx`
- `feature/delete-gear`
- `feature/gear-skills`
- `feature/item-popup`
- `feature/new_race`
- `feature/race-car`
- `feature/race-simplification`
- `feature/roguelike-options`
- `feature/talent-perks`
- `fix-tutorial-command-inspector-ui`
- `fix/gear-drag`
- `merge-features`
- `merge-resolve`
- `origin/feature/screens-update`
- `port-fungus-tutorial-plugin`
- `test-feature-environment`
- `test-global-rule`
- `test-worktree`
- `test-worktree-3`
- `test-worktree-4`
- `test-worktree-5`
- `test-worktree-6`
- `untitled-worktree`

The remaining local branches are intentionally limited to:

| Branch | Commit | Purpose |
| --- | --- | --- |
| `main` | `bcda1ccf` | Unmodified integration baseline. |
| `codex/repository-hygiene` | `f962651c` | Focused hygiene milestone. |
| `codex/result-screen-celebration` | `66ede040` | Focused result-screen feature milestone. |
| `codex/repository-maintenance` | `66ede040` before this report commit | Current archive-and-prune documentation milestone. |

## Integrity verification

- `git fsck --full` exited successfully. It reported dangling commits, trees, and blobs, which is expected after dropping stashes and deleting refs. No garbage collection was run, because the verified archive tags are the intended durable recovery mechanism.
- `git lfs fsck` reported `Git LFS fsck OK`.
- All seven archive tags resolve to the exact preserved targets recorded above.
- The working tree was clean after local pruning and before this report was authored.

## Proposed merged remote-branch cleanup

The following remote-tracking branches are ancestors of `origin/main` and are candidates for shared branch deletion. This list is prepared only; no deletion has been executed.

| Remote branch | Tip | Last commit date |
| --- | --- | --- |
| `origin/Art/Animations` | `317d406e` | 2026-05-29 |
| `origin/Art/AnimoraAnimations` | `7ea88694` | 2026-06-05 |
| `origin/Art/Audio` | `95754357` | 2026-06-17 |
| `origin/Art/Buttons` | `88b64153` | 2026-05-25 |
| `origin/Art/import-new-ui` | `5b4413b1` | 2026-05-13 |
| `origin/agent/ui-effect-pattern-layers` | `15588bc2` | 2026-07-24 |
| `origin/codex/blackboard-runtime-refactor` | `d7db0e20` | 2026-07-28 |
| `origin/codex/cloud-verification` | `80d5b4fd` | 2026-07-26 |
| `origin/codex/grid-board-inventory-screen-space` | `f93582c8` | 2026-07-27 |
| `origin/cursor/card-powerup-system-f131` | `bdc14b86` | 2026-04-16 |
| `origin/cursor/news-state-services-guide-19c8` | `c5440b5c` | 2026-04-22 |
| `origin/cursor/publisher-drawer-perf-c415` | `741e75b1` | 2026-04-25 |
| `origin/cursor/simplified-race-sim-9917` | `8d83e21a` | 2026-04-16 |
| `origin/cursor/standard-and-news-guide-refresh-e5e0` | `64b08874` | 2026-04-23 |
| `origin/feature/FrustrumFitRevamp` | `ef2faadf` | 2026-04-18 |
| `origin/feature/adjust-prefabs` | `bb0f641b` | 2026-06-30 |
| `origin/feature/ads` | `82c0c134` | 2026-05-04 |
| `origin/feature/analytics` | `7f2caf7f` | 2026-06-17 |
| `origin/feature/android` | `9c35c23d` | 2026-04-21 |
| `origin/feature/arcade` | `235da825` | 2026-04-29 |
| `origin/feature/boot-loading-manager` | `897ad3fa` | 2026-05-04 |
| `origin/feature/car-entity` | `b31925bd` | 2026-04-21 |
| `origin/feature/car-simulation` | `ba5a01ef` | 2026-04-10 |
| `origin/feature/cheats` | `e0562bdb` | 2026-04-21 |
| `origin/feature/configs` | `38c57675` | 2026-04-21 |
| `origin/feature/contetti-vfx` | `33c78c2b` | 2026-05-04 |
| `origin/feature/default_cog` | `400a5d85` | 2026-04-23 |
| `origin/feature/delete-gear` | `19e05ebc` | 2026-04-13 |
| `origin/feature/full-scene-flow` | `cf269b3c` | 2026-04-18 |
| `origin/feature/gear-skills` | `cbbbb769` | 2026-04-15 |
| `origin/feature/gear_engine_unification` | `e8730e8a` | 2026-04-17 |
| `origin/feature/gold-plan` | `3a3e2f86` | 2026-04-20 |
| `origin/feature/grid_drag_refactor` | `042f341e` | 2026-04-19 |
| `origin/feature/item-popup` | `58e8e2e0` | 2026-05-04 |
| `origin/feature/new_race` | `b11e24d3` | 2026-04-18 |
| `origin/feature/race-car` | `75b4c3b3` | 2026-04-18 |
| `origin/feature/race-simplification` | `f482eb20` | 2026-04-13 |
| `origin/feature/race_simulation` | `f341a071` | 2026-04-14 |
| `origin/feature/roguelike-options` | `b64a380e` | 2026-04-30 |
| `origin/feature/talent-cards` | `102b9381` | 2026-04-30 |
| `origin/feature/talent-perks` | `b9881cd1` | 2026-05-03 |
| `origin/feature/ui-panels` | `855d79a3` | 2026-04-20 |
| `origin/feature/unification` | `d1b4f9cd` | 2026-04-12 |
| `origin/fix-tutorial-command-inspector-ui` | `3b6c907e` | 2026-07-20 |
| `origin/fix/gear-drag` | `8eca297d` | 2026-04-14 |
| `origin/merge-features` | `806e404d` | 2026-04-13 |
| `origin/merge-resolve` | `7e747f4d` | 2026-04-16 |
| `origin/origin/feature/audio` | `703bc0b8` | 2026-07-09 |
| `origin/origin/feature/screens-update` | `9e56d673` | 2026-07-27 |
| `origin/port-fungus-tutorial-plugin` | `0c5dd8e0` | 2026-07-13 |

`origin/main` and the symbolic `origin/HEAD` ref are deliberately excluded.

## Remote branches to preserve for review

These remote branches are not ancestors of `origin/main` and must not be deleted as part of merged-branch cleanup:

| Remote branch | Tip | Last commit date |
| --- | --- | --- |
| `origin/claude/offline-mode-sample-data-apzRw` | `fcfe6677` | 2026-05-21 |
| `origin/cursor/cloud-agent-1777166607272-8azoj` | `0dbc4ff2` | 2026-04-26 |
| `origin/cursor/cloud-agent-1777166997326-b9r11` | `cebf2ec9` | 2026-04-25 |
| `origin/cursor/race-flow-implementation-9a6d` | `19935cc5` | 2026-04-10 |
| `origin/cursor/race-reward-system-4327` | `ff125a24` | 2026-04-16 |
| `origin/cursor/scaffold-entities-state-bridge-2ee9` | `15d5e9b5` | 2026-04-29 |
| `origin/cursor/simple-waypoint-driver-e41c` | `dddb3695` | 2026-04-16 |
| `origin/feature/design-system` | `9e691af5` | 2026-06-05 |
| `origin/feature/new-ui` | `35105233` | 2026-05-21 |
| `origin/feature/result-screen-vfx` | `c555f825` | 2026-08-03 |

The result-screen experiment remains unmerged. Its one approved production texture has been selectively recovered into `codex/result-screen-celebration`; the large experimental branch itself is preserved pending a separate disposition decision.

## Open pull requests requiring disposition

| Pull request | State | Head -> base | Last update | Proposed disposition |
| --- | --- | --- | --- | --- |
| [#14 Offline mode sample data](https://github.com/DuckDuckWeasel/Gear-Engine/pull/14) | Open | `claude/offline-mode-sample-data-apzRw` -> `main` | 2026-05-21 | Review before closing; head is unmerged. |
| [#13 Scaffold entities state bridge](https://github.com/DuckDuckWeasel/Gear-Engine/pull/13) | Draft | `cursor/scaffold-entities-state-bridge-2ee9` -> `main` | 2026-04-29 | Review before closing; head is unmerged. |
| [#12 LiveOps configs](https://github.com/DuckDuckWeasel/Gear-Engine/pull/12) | Draft | `cursor/cloud-agent-1777166607272-8azoj` -> `main` | 2026-04-26 | Review before closing; head is unmerged. |
| [#10 Standard and news guide refresh](https://github.com/DuckDuckWeasel/Gear-Engine/pull/10) | Draft | `cursor/standard-and-news-guide-refresh-e5e0` -> `cursor/news-state-services-guide-19c8` | 2026-04-23 | Its head and base are already merged into `main`; candidate for closure. |
| [#9 News state services guide](https://github.com/DuckDuckWeasel/Gear-Engine/pull/9) | Draft | `cursor/news-state-services-guide-19c8` -> `feature/default_cog` | 2026-04-22 | Its head and base are already merged into `main`; candidate for closure. |
| [#5 Race reward system](https://github.com/DuckDuckWeasel/Gear-Engine/pull/5) | Draft | `cursor/race-reward-system-4327` -> `feature/race_simulation` | 2026-04-16 | Review before closing; head is unmerged although base is merged. |
| [#3 Simple waypoint driver](https://github.com/DuckDuckWeasel/Gear-Engine/pull/3) | Draft | `cursor/simple-waypoint-driver-e41c` -> `feature/race_simulation` | 2026-04-16 | Review before closing; head is unmerged although base is merged. |

## Explicit external-state approval gate

The following actions require an explicit user decision and remain unexecuted:

- push the seven `archive/2026-09-15/*` tags to `origin`;
- close any or all of pull requests #3, #5, #9, #10, #12, #13, and #14;
- delete the 50 merged remote branches listed above;
- delete, archive, or otherwise change any unmerged remote branch; and
- push, publish, or merge any roadmap implementation branch.

After approval, execute only the selected actions, re-fetch with pruning, and append the exact GitHub-side outcome to this report.
