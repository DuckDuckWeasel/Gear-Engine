---
audience: internal
title: End-of-Match Regression Test
date: 2026-08-29
---

# End-of-Match Regression Test

The result popup opens before remote race-result persistence completes.

## Scope

- Platform: Unity EditMode
- Fixture: `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests`
- Test: `WhenResultPersistenceStalls_StillOpensResultPopup`
- Expected behavior: the player sees the end-of-match popup even when
  `RecordResultAsync` remains pending.

## Result

| Total | Passed | Failed | Skipped | Inconclusive | Duration |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 1 | 0 | 0 | 0 | 2.06 s |

The NUnit run passed. Unity's final test log contains no relevant errors or
exceptions.

## Evidence

- NUnit XML: `Artifacts/TestResults/20260829-RegressionFinal/EditMode.xml`
- Unity log: `Artifacts/TestResults/20260829-RegressionFinal/EditMode.log`
- Public release: `https://gear-engine-gorn-2026.web.app`
