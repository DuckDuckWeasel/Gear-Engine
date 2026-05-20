# Failing Tests — Audit & Remediation Plan

**Source:** EditMode run on 2026-05-20 (`Temp/audit-results.xml`). 196 total, 160 passed, **36 failed across 20 fixtures**.

None of the failures are in files touched by the offline-mode rework. They are pre-existing technical debt.

## Bucket overview

| Bucket | Failures | Description | Action shape |
|---|---|---|---|
| **A — Legitimate `LogAssert.Expect`** | 4 | Production code logs an error on a known failure path; test deliberately drives that path. Log is intentional, the test contract (exception/return value) is met. | Add `LogAssert.Expect(LogType.Error, …)` to suppress the documented log. **Does not mask anything** — the assertion is exercising the same contract production logs document. |
| **B — Test setup / infra out of date** | 22 | Test forgot to inject a dependency, or reflects a field/method that has since been renamed, or doesn't unwrap reflection exceptions. | Fix the test (inject the fake, update the reflected name, unwrap `TargetInvocationException`). Do **not** add Expect — the LogError/NRE is the symptom, not the contract. |
| **C — Real production bug or stale expectation** | 10 | Assertion on the actual behaviour fails. Either production drifted from contract, or the test's expected value was wrong from the start. Requires inspection to decide. | Read both sides; fix whichever is wrong. **Cannot resolve mechanically.** |

> **Why this matters:** the user's concern was correct — blanket `LogAssert.Expect` would mask real issues in Buckets B and C. Only Bucket A is safe to fix with `Expect`.

---

## Bucket A — Add `LogAssert.Expect`

These tests trigger documented error paths in production code. The `Debug.LogError` call is *production behaviour by design*; the test contract is the thrown exception / returned `null`, not the absence of the log.

### `CurrencyClientModuleTests.AddAsync_InvalidCurrencyId_Throws`
- **What:** Add `LogAssert.Expect(LogType.Error, new Regex("currencyId required"));` immediately before the `await` that triggers the error.
- **Why:** `CurrencyClientModule.AddAsync` has a `catch { Debug.LogError(...); throw; }` pattern (line 40). The test deliberately passes `""` to assert the throw. The log is correct production behaviour for any failing currency call — devs want to see them in player.log.
- **File:** `Assets/GearEngine/Scripts/App/Bootstrap/Tests/Editor/CurrencyClientModuleTests.cs:111`
- **Source:** `Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/Currency/CurrencyClientModule.cs:40`

### `CurrencyClientModuleTests.AddAsync_NonPositiveAmount_Throws`
- **What:** Same — `LogAssert.Expect(LogType.Error, new Regex("amount"));` before the call.
- **Why:** Same `catch+log+rethrow` pattern, same intentional production behaviour.
- **File:** `Assets/GearEngine/Scripts/App/Bootstrap/Tests/Editor/CurrencyClientModuleTests.cs:135`

### `CarEntityAndCarViewTests.CarView_Initialize_DoesNotThrowBeforeUnityStart`
- **What:** `LogAssert.Expect(LogType.Error, new Regex("Missing PrometeoCarController"));` before invoking `OnBind`.
- **Why:** Test asserts `OnBind` does **not throw** when the prefab is incomplete. Production logs an error and returns gracefully — that *is* the test contract ("graceful degradation").
- **File:** `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/CarEntityAndCarViewTests.cs`
- **Source:** `Assets/GearEngine/Scripts/Game/CarSimulation/Presentation/CarView.cs:20`

### `GearViewSpawnerTests.Spawn_ReturnsNull_WhenViewPrefabMissing`
- **What:** `LogAssert.Expect(LogType.Error, new Regex("missing ViewPrefab"));` before calling `Spawn`.
- **Why:** Test asserts `Spawn` *returns null* (not throws) when the prefab is missing. Production logs an error and returns null — both contracts (log visibility + null return) are intentional.
- **File:** `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/GearViewSpawnerTests.cs`
- **Source:** `Assets/GearEngine/Scripts/Game/GearEngine/Visuals/GearViewSpawner.cs:13`

---

## Bucket B — Fix the test, not the symptom

### B1: Tests don't inject `navigation` (or other deps) into ViewModels

**Pattern:** `ViewModelTestInject.InvokeInitialize(vm)` calls `Initialize` → `LoadItemsAsync` → references injected `navigation` field → null → either NREs or throws `ArgumentNullException`. The test's actual assertions are on `BurnItem` / `BuyRandom`, which don't need navigation. The init phase explodes first.

**Fix shape (apply to all five `ItemsViewModelTests`):**
1. Add a `FakeNavigation : INavigation` (or use an existing one — `RaceViewModelToggleTests.NoOpNavigation` is one).
2. After the other `InjectPrivateField` calls, add `ViewModelTestInject.InjectPrivateField(vm, "navigation", new NoOpNavigation());`
3. Re-run — the items list should populate from `perksClient` and the original assertions should pass.

**Do NOT** wrap with `LogAssert.Expect` — that would tell the test "ignore the broken init" while leaving the real test scenario (BurnItem etc.) running against a half-initialised VM.

#### Tests in this group
| Test | Notes |
|---|---|
| `ItemsViewModelTests.Initialize_BuildsItemListFromOwnedPerks` | After fix, asserts items count = 3 (currently 0 because LoadItemsAsync aborted). |
| `ItemsViewModelTests.BuyRandom_AddsNewItemWhenPerkIsNew` | Same. |
| `ItemsViewModelTests.BuyRandom_DoesNotModifyListOnFailedPurchase` | Same. |
| `ItemsViewModelTests.BurnPerk_CallsBurnOnClientAndUpdatesRevision` | Same. |
| `ItemsViewModelTests.BurnPerk_DoesNotUpdateRevisionOnFailure` | Same. |

**File:** `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/ItemsViewModelTests.cs`

#### Same root cause, different fixture
| Test | Notes |
|---|---|
| `CarTrackScreenViewModelTests.Initialize_DoesNotRegisterSessionsWithRaceManager` | `ArgumentNullException: navigation` thrown from constructor via reflection. Inject fake navigation. |
| `CarTrackScreenViewModelTests.ToggleRace_RegistersSessionsAndStartsPrimary` | Same. |

**File:** `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/CarTrackScreenViewModelTests.cs`

---

### B2: Tests use reflection on private fields that have been renamed

**Pattern:** `RaceViewModelToggleTests.InjectPrivateField` (line 247) does `Assert.That(field, Is.Not.Null)`. Four tests fail there — the looked-up field doesn't exist on the type, so the assertion fires with "Expected: not null But was: null". Almost certainly a field was renamed in `RaceViewModel` without updating the test.

**Fix:**
1. Open `RaceViewModel` and `RaceViewModelToggleTests`.
2. Diff: find the field name(s) the test is looking for (`engine`, `track`, `manager`, etc. — read the test).
3. Update the name string(s) in the test to match the current field, OR rename the field back if there was no good reason to change it.

| Test | File |
|---|---|
| `RaceViewModelToggleTests.Initialize_RegistersRaceStateToManager` | `Assets/GearEngine/Scripts/Game/Race/Tests/Editor/RaceViewModelToggleTests.cs` |
| `RaceViewModelToggleTests.ToggleRace_WhenStopped_StartsEngineAndTrack` | Same |
| `RaceViewModelToggleTests.ToggleRace_WhenRunning_StopsEngineAndPausesTrack` | Same |
| `RaceViewModelToggleTests.ToggleRace_ToggleTwice_ResumesCorrectly` | Same |

---

### B3: Tests use `FindPropertyRelative` on a SerializedField that was renamed

**Pattern:** `CreateGearConfig` does `dp.FindPropertyRelative("Id")` then `.stringValue = id`. The `FindPropertyRelative` returns null → NRE at line 200. Means `GearItemData.Id` field was renamed (e.g., to `id` or `gearId`) or moved out of `data`.

**Fix:**
1. Open `GearItemData` and check current field name(s).
2. Update the `"Id"` literal in tests to match.

| Test | File |
|---|---|
| `GearInventoryViewModelTests.Constructor_BuildsTray_FromOwnedWhenBoardEmpty` | `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/GearInventoryViewModelTests.cs:200` |
| `GearInventoryViewModelTests.NotifySlotDragAccepted_DoesNotRemoveGearFromInventory` | Same |
| `GearInventoryViewModelTests.RecreatingViewModel_DoesNotResetSharedInventory` | Same |
| `GearInventoryViewComponentTests.Bind_DoesNotPopulateSlots_UntilRebuildAndFitIsCalled` | `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/GearInventoryViewComponentTests.cs:199` |
| `GearInventoryViewComponentTests.RebuildListTwiceInOneFrame_KeepsOneSetOfSlots` | Same |

---

### B4: Test reflects a private member, exception is wrapped

**Pattern:** `SceneFoundationScopeTests.InvokeProtectedConfigure` does `methodInfo.Invoke(...)`. When the invoked method throws, .NET wraps it in `TargetInvocationException`. Tests use `Assert.Throws<InvalidOperationException>` which only matches the *outer* exception type → fails.

**Fix:**
1. In `InvokeProtectedConfigure`, catch and rethrow the inner exception:
   ```csharp
   try { configure.Invoke(scope, new object[] { builder }); }
   catch (TargetInvocationException tie) { throw tie.InnerException ?? tie; }
   ```
   *(Or, in NUnit: use `Assert.Throws<TargetInvocationException>` + `Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>())`.)*
2. The first option is simpler — one shared helper fix unlocks both tests.

| Test | File |
|---|---|
| `SceneFoundationScopeTests.Configure_MissingNavigationSettings_ThrowsInvalidOperationException` | `Assets/GearEngine/Scripts/Core/SceneFoundation/Tests/Editor/SceneFoundationScopeTests.cs:26` |
| `SceneFoundationScopeTests.Configure_MissingNavigationViewHolder_ThrowsInvalidOperationException` | Same file, line 46 |

Same fix shape would also address:
| Test | File |
|---|---|
| `RoguelikeViewModelTests.LoadRoll_PopulatesPerkOptions` | `TargetInvocationException → NRE` — the test's `Initialize` invocation via reflection wraps an NRE. Likely *also* missing a fake navigation/dependency injection (B1-style). Investigate together. |
| `RoguelikeViewModelTests.PickPerk_AddsItem_ConsumesRoll_OpensMain` | Same. |

**File:** `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs`

---

### B5: Test container doesn't register all transitive deps

**Pattern:** `GearMechanicsInstaller.Install` requires `IEventBus`. The test registers `IInventoryService`, `IBoardSlotCapacityProvider`, `BoardRulesSO`, `GearEngineFeatureToggleSO` — but **not** `IEventBus`. `BoardService` (constructed by VContainer) takes `IEventBus` through `GridMergeService` → VContainerException at resolve time.

**Fix:** add `new EventsInstaller().Install(builder);` (or register a fake `IEventBus`) before calling `GearMechanicsInstaller.Install`. Pattern is already used in `CampaignTestUtilities.GearMechanicsTestContext`.

| Test | File |
|---|---|
| `GearMechanicsInstallerTests.Install_RegistersBoardService_AndInventoryService` | `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/GearMechanicsInstallerTests.cs:14` |
| `GearMechanicsInstallerTests.Install_WithFeatureToggle_ResolvesToggle` | Same file, line 34 |

---

## Bucket C — Real bug or stale expectation (need eyes-on)

These can't be fixed mechanically. Each needs the test owner (or whoever last touched the production code) to decide which side is correct.

### `BoardPointerProjectionUtilityTests.TryProjectScreenPointToPlane_WithTopDownBoard_PreservesBoardLocalAxes`
- **Failure:** `Expected 1.0f ±0.001 but was 4.375f`.
- **Likely cause:** Either the projection math changed (refactor missed a scale factor) or the test setup's expected board / camera positions changed without updating the expected value.
- **Where to start:** Compare `BoardPointerProjectionUtility` to whatever commit introduced the test. If neither changed but other coordinate systems did, the test setup may be wrong.
- **File:** `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/BoardPointerProjectionUtilityTests.cs:63`

### `BoardPointerProjectionUtilityTests.TryProjectScreenPointToPlane_WithTopDownBoard_ReturnsBoardOriginAtScreenCenter`
- **Failure:** `Expected 0.0f ±0.001 but was 2.8125f`.
- Same root as above — investigate together.
- **File:** same, line 43.

### `ActiveRaceViewModelTests.WhenTrackCompletes_OpensResultPopupAndCreditsCurrency`
- **Failure:** `Expected: False But was: True`.
- **Likely:** A flag that should be reset after track completion isn't being reset. Could be a real regression in `ActiveRaceViewModel` or related state.
- **File:** `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/ActiveRaceViewModelTests.cs:138`

### `CrossPlatformTypeBinderTests.BindToType_AcceptsGameLiveOpsTracksDtoAssemblyForTrackGameData`
- **Failure:** `Disallowed $type assembly in JSON payload: Game.LiveOps.Tracks.DTO`.
- **Likely:** `CrossPlatformTypeBinder`'s allowlist hasn't been updated to include `Game.LiveOps.Tracks.DTO`. Production bug — JSON deserialization of TrackGameData will fail in real use too.
- **File:** `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/CrossPlatformTypeBinderTests.cs`
- **Source:** `LiveOps.DTO.Json.CrossPlatformTypeBinder.BindToType`

### `LiveOpsConfigBuilderAndRcTests.Builder_ConfigKeys_Match_ServerModule_Contract`
- **Failure:** `Expected "PerkItem" But was "PerkConfig"`.
- **Likely:** Perks module renamed its config key (or its config type), test/contract didn't follow. Decide which side is canonical — server module contract should win.
- **File:** `Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/LiveOpsConfigBuilderAndRcTests.cs:102`

### `SplineEvaluateDriverTests.Driver_NoLaneProfile_OffsetIsZero`
- **Failure:** `Expected 0.0 ±0.001 but was 0.0384`.
- **Likely:** `SplineEvaluateDriver` is applying a small default offset when no lane profile is present (regression — contract is "no profile ⇒ zero offset"). High likelihood of real bug.
- **File:** `Assets/GearEngine/Scripts/Game/CarSimulation/Tests/Editor/SplineEvaluateDriverTests.cs:255`

### `FrustumFitAnchorTests.Factory_ZeroExtentOnFittedAxes_ReturnsFalse`
- **Failure:** `Expected: False But was: True`.
- **Likely:** Edge case "plane mesh has no Y extent" no longer returns false from the factory. Either the factory's guard regressed, or the test's plane no longer has zero Y extent.
- **File:** `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/FrustumFitAnchorTests.cs:164`

### `RectContentFitterTests.Refit_ScalesChild_WithRenderer_ToNonDefaultScale`
- **Failure:** `Expected: less than 2.01 But was: 90.0`.
- **Likely:** Scale math is wildly off (factor of ~45). Either content fitter regressed, or a renderer's bounds are being measured differently. Real bug, not test setup.
- **File:** `Assets/GearEngine/Scripts/Game/GearEngine/Tests/Editor/RectContentFitterTests.cs:54`

### `CarPowerupBuildResolverTests.Resolve_SortsByPhaseThenAppliesMultipliers`
- **Failure:** `Expected: 6.0 ±0.001 but was 1.0`.
- **Likely:** Multipliers aren't being applied — resolver returns the base value. Production bug in `CarPowerupBuildResolver`.
- **File:** `Assets/GearEngine/Scripts/Game/Perks/Tests/Editor/CarPowerupBuildResolverTests.cs:40`

### `AssetPublisherDefinitionDrawerCacheTests.SourceTypeCache_Reset_CanRebuild`
- **Failure:** `Expected: > 1 But was: 1`.
- **Likely:** `ResetSourceTypeCacheForTests` no longer actually resets the rebuild counter (or `EnsureSourceTypeCacheForTests` short-circuits after the first call regardless of reset). The sibling test `SourceTypeCache_MultipleEnsure_RebuildsOnce` passes, suggesting the rebuild *can* happen — just not after the reset.
- **File:** `Assets/Packages/com.scaffold.appflow.publishers.addressables/Tests/Editor/AssetPublisherDefinitionDrawerCacheTests.cs:34`
- **Source:** `Scaffold.AppFlow.Publishers.Editor.AssetPublisherDefinitionDrawer.ResetSourceTypeCacheForTests` — check if it actually resets the rebuild counter.

---

## Recommended ordering

1. **Bucket A first** (4 tests, ~10 min total). Pure mechanical, zero risk. Gets the suite cleaner before tackling harder ones.
2. **Bucket B5** (GearMechanicsInstaller, 2 tests). Single one-line fix (`new EventsInstaller().Install(builder);`). Low risk.
3. **Bucket B4** (SceneFoundation + Roguelike, 4 tests). Single helper change in two test files. Then look at Roguelike to see if it's also B1.
4. **Bucket B3** (Gear inventory tests, 5 tests). Update one literal field name in two test files.
5. **Bucket B1** (Items + CarTrackScreen ViewModels, 7 tests). Inject `NoOpNavigation`. Reuse the one in `RaceViewModelToggleTests`.
6. **Bucket B2** (RaceViewModelToggle, 4 tests). Requires reading `RaceViewModel` to find the current field names.
7. **Bucket C** (10 tests). Real investigation. Triage them individually — `CrossPlatformTypeBinder` and `LiveOpsConfigBuilderAndRcTests` (key mismatch) are likely the highest-impact bugs since they affect runtime LiveOps deserialization.

Estimated effort: Buckets A + B = ~2-4h mechanical, Bucket C = unknown (each test is its own investigation).
