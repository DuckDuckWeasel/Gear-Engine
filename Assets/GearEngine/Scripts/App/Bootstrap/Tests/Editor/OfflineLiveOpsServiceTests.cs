using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Offline;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Roguelike;
using LiveOps.Modules.DTO.Tracks;
using NUnit.Framework;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class OfflineLiveOpsServiceTests
    {
        [Test]
        public void DefaultCtor_SeedsStubsForAllSixKnownModules()
        {
            var service = new OfflineLiveOpsService();

            Assert.That(service.GetModuleData<CurrencyGameData>(), Is.Not.Null);
            Assert.That(service.GetModuleData<TrackGameData>(), Is.Not.Null);
            Assert.That(service.GetModuleData<LoadoutGameData>(), Is.Not.Null);
            Assert.That(service.GetModuleData<InventoryGameData>(), Is.Not.Null);
            Assert.That(service.GetModuleData<PerkGameData>(), Is.Not.Null);
            Assert.That(service.GetModuleData<RoguelikeGameData>(), Is.Not.Null);
        }

        [Test]
        public void GetModuleData_UnknownType_Throws()
        {
            var service = new OfflineLiveOpsService(
                new Dictionary<Type, IGameModuleData>(),
                new Dictionary<Type, Func<ModuleRequest, OfflineLiveOpsService, ModuleResponse>>());

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => service.GetModuleData<CurrencyGameData>());
            Assert.That(ex.Message, Does.Contain("CurrencyGameData"));
            Assert.That(ex.Message, Does.Contain("OfflineStubs.CreateModules"));
        }

        [Test]
        public async Task CallAsync_AddCurrency_MutatesStubWallet()
        {
            var service = new OfflineLiveOpsService();
            long before = service.GetModuleData<CurrencyGameData>().GetWallet("gold").Current;

            AddCurrencyResponse response = await service.CallAsync(new AddCurrencyRequest("gold", 75), CancellationToken.None);

            Assert.That(response.NewAmount, Is.EqualTo(before + 75));
            Assert.That(response.Diff, Is.EqualTo(75));
            Assert.That(service.GetModuleData<CurrencyGameData>().GetWallet("gold").Current, Is.EqualTo(before + 75));
        }

        [Test]
        public async Task CallAsync_SpendCurrency_FailsWhenInsufficient()
        {
            var service = new OfflineLiveOpsService();
            long before = service.GetModuleData<CurrencyGameData>().GetWallet("gold").Current;

            SpendCurrencyResponse response = await service.CallAsync(new SpendCurrencyRequest("gold", before + 1), CancellationToken.None);

            Assert.That(response.Succeeded, Is.False);
            Assert.That(service.GetModuleData<CurrencyGameData>().GetWallet("gold").Current, Is.EqualTo(before),
                "Wallet must stay untouched when spend fails.");
        }

        [Test]
        public async Task CallAsync_SetInventory_OverwritesStubInventory()
        {
            var service = new OfflineLiveOpsService();
            var gears = new List<OwnedGearEntry>
            {
                new OwnedGearEntry { InstanceId = "motor", GearId = "motor_cog" },
                new OwnedGearEntry { InstanceId = "g1", GearId = "speed_buff" },
            };

            SetInventoryResponse response = await service.CallAsync(new SetInventoryRequest(gears), CancellationToken.None);

            Assert.That(response.Gears.Count, Is.EqualTo(2));
            Assert.That(service.GetModuleData<InventoryGameData>().Gears.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task CallAsync_DrawRoguelikeRoll_PopulatesCurrentRollIdsOnModuleData()
        {
            var service = new OfflineLiveOpsService();

            DrawRoguelikeRollResponse response = await service.CallAsync(new DrawRoguelikeRollRequest(), CancellationToken.None);

            Assert.That(response.CurrentRollIds, Is.Not.Empty);
            Assert.That(service.GetModuleData<RoguelikeGameData>().CurrentRollIds, Is.EqualTo(response.CurrentRollIds));
        }

        [Test]
        public async Task CallAsync_ClaimRoguelikePick_SucceedsForIdInCurrentRoll_AndClearsIt()
        {
            var service = new OfflineLiveOpsService();
            await service.CallAsync(new DrawRoguelikeRollRequest(), CancellationToken.None);
            string pick = service.GetModuleData<RoguelikeGameData>().CurrentRollIds[0];

            ClaimRoguelikePickResponse response = await service.CallAsync(new ClaimRoguelikePickRequest(pick), CancellationToken.None);

            Assert.That(response.Success, Is.True);
            Assert.That(service.GetModuleData<RoguelikeGameData>().CurrentRollIds, Is.Empty);
        }

        [Test]
        public async Task CallAsync_ClaimRoguelikePick_FailsForIdNotInCurrentRoll()
        {
            var service = new OfflineLiveOpsService();
            await service.CallAsync(new DrawRoguelikeRollRequest(), CancellationToken.None);

            ClaimRoguelikePickResponse response = await service.CallAsync(
                new ClaimRoguelikePickRequest("unknown_gear"), CancellationToken.None);

            Assert.That(response.Success, Is.False);
            Assert.That(service.GetModuleData<RoguelikeGameData>().CurrentRollIds, Is.Not.Empty,
                "Claim must not consume the roll when the picked id is not in it.");
        }

        [Test]
        public async Task CallAsync_SkipRoguelikePick_ClearsCurrentRoll()
        {
            var service = new OfflineLiveOpsService();
            await service.CallAsync(new DrawRoguelikeRollRequest(), CancellationToken.None);

            SkipRoguelikePickResponse response = await service.CallAsync(new SkipRoguelikePickRequest(), CancellationToken.None);

            Assert.That(response.Success, Is.True);
            Assert.That(service.GetModuleData<RoguelikeGameData>().CurrentRollIds, Is.Empty);
        }

        [Test]
        public async Task CallAsync_RerollRoguelikeRoll_ReturnsFreshRollIds()
        {
            var service = new OfflineLiveOpsService();

            RerollRoguelikeRollResponse response = await service.CallAsync(new RerollRoguelikeRollRequest(), CancellationToken.None);

            Assert.That(response.CurrentRollIds, Is.Not.Empty);
            Assert.That(service.GetModuleData<RoguelikeGameData>().CurrentRollIds, Is.EqualTo(response.CurrentRollIds));
        }

        [Test]
        public void CallAsync_UnknownRequest_ThrowsWithStubGuidance()
        {
            var service = new OfflineLiveOpsService();

            InvalidOperationException ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.CallAsync(new BurnPerkRequest { PerkId = "grip" }, CancellationToken.None));

            Assert.That(ex.Message, Does.Contain("BurnPerkRequest"));
            Assert.That(ex.Message, Does.Contain("OfflineStubs.CreateHandlers"));
        }

        [Test]
        public async Task CallAsync_RecordRaceResult_AdvancesToNextTrackInOrderedList()
        {
            var service = new OfflineLiveOpsService();
            TrackGameData track = service.GetModuleData<TrackGameData>();
            string firstTrackId = track.OrderedTrackIds[0];
            string expectedNextTrackId = track.OrderedTrackIds[1];

            RecordRaceResultResponse response = await service.CallAsync(
                new RecordRaceResultRequest(firstTrackId, 12.34f), CancellationToken.None);

            Assert.That(response.NextTrackId, Is.EqualTo(expectedNextTrackId));
            Assert.That(response.Advanced, Is.True);
            Assert.That(response.NewBestTimeSec, Is.EqualTo(12.34f));
        }

        [Test]
        public async Task CallAsync_RecordRaceResult_OnLastTrack_DoesNotAdvance()
        {
            var service = new OfflineLiveOpsService();
            TrackGameData track = service.GetModuleData<TrackGameData>();
            string lastTrackId = track.OrderedTrackIds[track.OrderedTrackIds.Count - 1];

            RecordRaceResultResponse response = await service.CallAsync(
                new RecordRaceResultRequest(lastTrackId, 9.5f), CancellationToken.None);

            Assert.That(response.NextTrackId, Is.Empty);
            Assert.That(response.Advanced, Is.False);
        }

        [Test]
        public async Task CallAsync_RecordRaceResult_KeepsExistingBestTime_WhenSlower()
        {
            var service = new OfflineLiveOpsService();
            TrackGameData track = service.GetModuleData<TrackGameData>();
            string trackId = track.OrderedTrackIds[0];
            track.BestTimeSec[trackId] = 5f;

            RecordRaceResultResponse response = await service.CallAsync(
                new RecordRaceResultRequest(trackId, 9.5f), CancellationToken.None);

            Assert.That(response.NewBestTimeSec, Is.EqualTo(5f), "Slower time must not overwrite best.");
        }

        [Test]
        public void CallAsync_NullRequest_Throws()
        {
            var service = new OfflineLiveOpsService();

            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await service.CallAsync<AddCurrencyResponse>(null, CancellationToken.None));
        }

        [Test]
        public void CallAsync_CancelledToken_Throws()
        {
            var service = new OfflineLiveOpsService();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.CatchAsync<OperationCanceledException>(
                async () => await service.CallAsync(new AddCurrencyRequest("gold", 1), cts.Token));
        }
    }
}
