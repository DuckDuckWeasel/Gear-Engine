# Unity Test Report

Generated: 2026-07-27T21:27:55.044254+00:00

## Test intent

Capture the final unfiltered repository EditMode baseline after the Blackboard refactor; affected Blackboard assemblies are verified separately by the clean selected-assembly gate.

## Selection

- Project: `/Users/leonardosilva/.codex/worktrees/a62b/Gear Engine`
- Platform mode: `edit`
- Selector: `All tests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | Failed(Child) | 278 | 226 | 52 | 0 | 0 |

## Failures


- `GearEngine.App.Bootstrap.Tests.Editor.CurrencyClientModuleTests.AddAsync_InvalidCurrencyId_Throws` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.App.Bootstrap.Tests.Editor.CurrencyClientModuleTests.AddAsync_NonPositiveAmount_Throws` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.App.Bootstrap.Tests.Editor.CurrencyClientModuleTests.AddAsync_UpdatesCachedWallet_FromResponse` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.App.Bootstrap.Tests.Editor.CurrencyClientModuleTests.TrySpendAsync_False_DoesNotChangeWallet` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.App.Bootstrap.Tests.Editor.CurrencyClientModuleTests.TrySpendAsync_True_UpdatesWallet` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests.Initialize_CreatesSessionRegistersRunnerAndStartsEngine` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.ActiveRaceViewModelTests.WhenTrackCompletes_OpensResultPopupAndCreditsCurrency` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.CrossPlatformTypeBinderTests.BindToType_AcceptsGameLiveOpsTracksDtoAssemblyForTrackGameData` — System.IO.InvalidDataException : Disallowed $type assembly in JSON payload: Game.LiveOps.Tracks.DTO
- `GearEngine.Campaign.Tests.Editor.ItemsViewModelTests.BurnPerk_CallsBurnOnClientAndUpdatesRevision` — Unhandled log message: '[Error] [ItemsViewModel] LoadItemsAsync failed: Value cannot be null.
- `GearEngine.Campaign.Tests.Editor.ItemsViewModelTests.BurnPerk_DoesNotUpdateRevisionOnFailure` — Unhandled log message: '[Error] [ItemsViewModel] LoadItemsAsync failed: Value cannot be null.
- `GearEngine.Campaign.Tests.Editor.ItemsViewModelTests.BuyRandom_AddsNewItemWhenPerkIsNew` — Expected: 1
- `GearEngine.Campaign.Tests.Editor.ItemsViewModelTests.BuyRandom_DoesNotModifyListOnFailedPurchase` — Unhandled log message: '[Error] [ItemsViewModel] LoadItemsAsync failed: Value cannot be null.
- `GearEngine.Campaign.Tests.Editor.ItemsViewModelTests.Initialize_BuildsItemListFromOwnedPerks` — Expected: 3
- `GearEngine.Campaign.Tests.Editor.LiveOpsConfigBuilderAndRcTests.Builder_ConfigKeys_Match_ServerModule_Contract` — Expected string length 8 but was 10. Strings differ at index 4.
- `GearEngine.Campaign.Tests.Editor.LiveOpsConfigBuilderAndRcTests.TrackConfigBuilderSO_Default_Build_Matches_TrackRc` — Run Window → LiveOps → Configs → Sync for TrackConfig. Expected:
- `GearEngine.Campaign.Tests.Editor.RaceResultModelTests.WhenTrackHasTiers_ScoreAndGoldMatchTierReward` — Expected: 1
- `GearEngine.Campaign.Tests.Editor.ResultPopupViewModelTests.Continue_WhenGoodResult_OpensMain` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.ResultPopupViewModelTests.Continue_WhenPoorResult_OpensMain` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.ResultPopupViewModelTests.Upgrade_OpensRoguelikeViewModel` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.RoguelikeViewModelTests.LoadRoll_PopulatesPerkOptions` — System.Reflection.TargetInvocationException : Exception has been thrown by the target of an invocation.
- `GearEngine.Campaign.Tests.Editor.RoguelikeViewModelTests.PickPerk_AddsItem_ConsumesRoll_OpensMain` — System.Reflection.TargetInvocationException : Exception has been thrown by the target of an invocation.
- `GearEngine.Campaign.Tests.Editor.TrackGameDataTests.TracksClientModule_InitializeAsync_WhenCurrentTrackIdEmpty_RepairsToFirstOrderedTrackInCatalog` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Campaign.Tests.Editor.TrackGameDataTests.TracksClientModule_InitializeAsync_WhenRemoteTrackListEmpty_RepairsToFirstCatalogTrack` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.Perks.Tests.Editor.PerkSampleViewModelTests.RefreshDisplay_UsesCurrencyWalletGold` — VContainer.VContainerException : Failed to resolve GearEngine.Currency.CurrencyClientModule : No such registration of type: Scaffold.Analytics.IAnalyticsService with Key:
- `GearEngine.CarSimulation.Tests.CarEntityAndCarViewTests.CarView_Initialize_DoesNotThrowBeforeUnityStart` — Unhandled log message: '[Error] [CarView] Missing PrometeoCarController. Cannot bind AI logic.'. Use UnityEngine.TestTools.LogAssert.Expect
- `GearEngine.CarSimulation.Tests.CarTrackScreenViewModelTests.Initialize_DoesNotRegisterSessionsWithRaceManager` — System.Reflection.TargetInvocationException : Exception has been thrown by the target of an invocation.
- `GearEngine.CarSimulation.Tests.CarTrackScreenViewModelTests.ToggleRace_RegistersSessionsAndStartsPrimary` — System.Reflection.TargetInvocationException : Exception has been thrown by the target of an invocation.
- `GearEngine.CarSimulation.Tests.SplineEvaluateDriverTests.Driver_NoLaneProfile_OffsetIsZero` — Without a LaneProfile, lateral offset should be zero.
- `GearEngine.GearEngine.Tests.Editor.BoardPointerProjectionUtilityTests.TryProjectScreenPointToPlane_WithTopDownBoard_PreservesBoardLocalAxes` — Expected: 1.0f +/- 0.00100000005f
- `GearEngine.GearEngine.Tests.Editor.BoardPointerProjectionUtilityTests.TryProjectScreenPointToPlane_WithTopDownBoard_ReturnsBoardOriginAtScreenCenter` — Expected: 0.0f +/- 0.00100000005f
- `GearEngine.GearEngine.Tests.Editor.FrustumFitAnchorTests.Factory_ZeroExtentOnFittedAxes_ReturnsFalse` — A plane mesh has no Y extent; XY fit axes must fail gracefully.
- `GearEngine.GearEngine.Tests.Editor.GearInventoryViewComponentTests.Bind_DoesNotPopulateSlots_UntilRebuildAndFitIsCalled` — System.NullReferenceException : Object reference not set to an instance of an object
- `GearEngine.GearEngine.Tests.Editor.GearInventoryViewComponentTests.RebuildListTwiceInOneFrame_KeepsOneSetOfSlots` — System.NullReferenceException : Object reference not set to an instance of an object
- `GearEngine.GearEngine.Tests.Editor.GearInventoryViewModelTests.Constructor_BuildsTray_FromOwnedWhenBoardEmpty` — System.NullReferenceException : Object reference not set to an instance of an object
- `GearEngine.GearEngine.Tests.Editor.GearInventoryViewModelTests.NotifySlotDragAccepted_DoesNotRemoveGearFromInventory` — System.NullReferenceException : Object reference not set to an instance of an object
- `GearEngine.GearEngine.Tests.Editor.GearInventoryViewModelTests.RecreatingViewModel_DoesNotResetSharedInventory` — System.NullReferenceException : Object reference not set to an instance of an object
- `GearEngine.GearEngine.Tests.Editor.GearMechanicsInstallerTests.Install_RegistersBoardService_AndInventoryService` — Expected: No Exception to be thrown
- `GearEngine.GearEngine.Tests.Editor.GearMechanicsInstallerTests.Install_WithFeatureToggle_ResolvesToggle` — Expected: No Exception to be thrown
- `GearEngine.GearEngine.Tests.Editor.GearViewSpawnerTests.Spawn_ReturnsNull_WhenViewPrefabMissing` — Unhandled log message: '[Error] [GearViewSpawner] Gear 'x' missing ViewPrefab.'. Use UnityEngine.TestTools.LogAssert.Expect
- `GearEngine.GearEngine.Tests.Editor.RectContentFitterTests.Refit_ScalesChild_WithRenderer_ToNonDefaultScale` — Expected: less than 2.00999999f
- `GearEngine.GearEngine.Tests.Editor.UIEffectPatternLayerTests.MaterialBinding_AllFourLayerSlotsReceiveIndependentValues` — Expected: 0.200000003f +/- 0.00100000005f
- `GearEngine.GearEngine.Tests.Editor.UIEffectPatternLayerTests.PresetPaths_FourLayersSurviveLoadSaveAndReplica` — System.NullReferenceException : Object reference not set to an instance of an object
- `GearEngine.GearEngine.Tests.Editor.UIEffectPatternRenderingTests.OrderedAlphaOver_DisabledAndZeroOpacityLayersDoNotContribute` — Expected: 0.5f +/- 0.119999997f
- `GearEngine.GearEngine.Tests.Editor.UIEffectPatternRenderingTests.SampledTextureAlpha_ScalesLayerOpacity` — Expected: 0.25f +/- 0.119999997f
- `GearEngine.Perks.Tests.Editor.CarPowerupBuildResolverTests.Resolve_SortsByPhaseThenAppliesMultipliers` — Expected: 6.0f +/- 0.00100000005f
- `GearEngine.Race.Tests.Editor.RaceViewModelToggleTests.Initialize_RegistersRaceStateToManager` — Expected: not null
- `GearEngine.Race.Tests.Editor.RaceViewModelToggleTests.ToggleRace_ToggleTwice_ResumesCorrectly` — Expected: not null
- `GearEngine.Race.Tests.Editor.RaceViewModelToggleTests.ToggleRace_WhenRunning_StopsEngineAndPausesTrack` — Expected: not null
- `GearEngine.Race.Tests.Editor.RaceViewModelToggleTests.ToggleRace_WhenStopped_StartsEngineAndTrack` — Expected: not null
- `GearEngine.SceneFoundation.Tests.Editor.SceneFoundationScopeTests.Configure_MissingNavigationSettings_ThrowsInvalidOperationException` — Expected: <System.InvalidOperationException>
- `GearEngine.SceneFoundation.Tests.Editor.SceneFoundationScopeTests.Configure_MissingNavigationViewHolder_ThrowsInvalidOperationException` — Expected: <System.InvalidOperationException>
- `Scaffold.AppFlow.Publishers.Addressables.Tests.Editor.AssetPublisherDefinitionDrawerCacheTests.SourceTypeCache_Reset_CanRebuild` — Expected: greater than 1

## Relevant Unity log events

None.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

| Test | Result | Scenario | Criteria | Media |
| --- | --- | --- | --- | --- |
| `GearEngine.GearEngine.Tests.Editor.UIEffectPresetCatalogSceneVisualTests.CatalogScene_ButtonClick_AppliesAndRendersEveryPreset` | Passed | Each configured UIEffect preset is applied by the scene Blackboard's button-click path and captured in one contact sheet. | All 78 configured presets are reached through Button.onClick.; Each thumbnail contains the rendered scene after its corresponding preset was applied.; The contact sheet uses 8 columns and 10 rows. | [AllPresetsContactSheet.png](Media/AllPresetsContactSheet.png) |

All media created during this run is associated with a test above.
