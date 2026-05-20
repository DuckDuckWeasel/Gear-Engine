using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Roguelike;
using LiveOps.Modules.DTO.Tracks;
using Newtonsoft.Json;

namespace GearEngine.App.Bootstrap.Offline
{
    /// <summary>
    /// Hand-authored stubs that <see cref="OfflineLiveOpsService"/> serves in place of UGS / LiveOps.
    /// Edit the JSON literals below to change the data offline runs see; add a handler entry to make
    /// a new <see cref="ModuleRequest"/> succeed locally instead of throwing.
    /// </summary>
    internal static class OfflineStubs
    {
        public static Dictionary<Type, IGameModuleData> CreateModules()
        {
            return new Dictionary<Type, IGameModuleData>
            {
                [typeof(CurrencyGameData)] = JsonConvert.DeserializeObject<CurrencyGameData>(
                    "{\"wallets\":[{\"id\":\"gold\",\"current\":500,\"min\":0,\"max\":null},{\"id\":\"gems\",\"current\":50,\"min\":0,\"max\":null}]}"),

                [typeof(TrackGameData)] = JsonConvert.DeserializeObject<TrackGameData>(
                    "{\"currentTrackId\":\"CircleTrack\",\"orderedTrackIds\":[\"CircleTrack\",\"OvalTrack\"],\"bestTimeSec\":{}}"),

                [typeof(LoadoutGameData)] = JsonConvert.DeserializeObject<LoadoutGameData>(
                    "{\"board\":[],\"baseSlots\":9,\"motorCogStartX\":2,\"motorCogStartY\":2}"),

                [typeof(InventoryGameData)] = JsonConvert.DeserializeObject<InventoryGameData>(
                    "{\"gears\":[{\"instanceId\":\"motor\",\"gearId\":\"gear_core\"}],\"baseSlots\":9,\"motorCogGearId\":\"gear_core\"}"),

                [typeof(PerkGameData)] = JsonConvert.DeserializeObject<PerkGameData>(
                    "{\"unlocked\":[],\"nextCost\":100,\"burnReward\":25}"),

                [typeof(RoguelikeGameData)] = JsonConvert.DeserializeObject<RoguelikeGameData>(
                    "{\"currentRollIds\":[],\"optionsPerRoll\":3}"),
            };
        }

        public static Dictionary<Type, Func<ModuleRequest, OfflineLiveOpsService, ModuleResponse>> CreateHandlers()
        {
            return new Dictionary<Type, Func<ModuleRequest, OfflineLiveOpsService, ModuleResponse>>
            {
                [typeof(AddCurrencyRequest)] = (r, s) => HandleAddCurrency((AddCurrencyRequest)r, s),
                [typeof(SpendCurrencyRequest)] = (r, s) => HandleSpendCurrency((SpendCurrencyRequest)r, s),
                [typeof(SetInventoryRequest)] = (r, s) => HandleSetInventory((SetInventoryRequest)r, s),
                [typeof(RecordRaceResultRequest)] = (r, s) => HandleRecordRaceResult((RecordRaceResultRequest)r, s),
                [typeof(DrawRoguelikeRollRequest)] = (r, s) => HandleDrawRoguelikeRoll((DrawRoguelikeRollRequest)r, s),
                [typeof(ClaimRoguelikePickRequest)] = (r, s) => HandleClaimRoguelikePick((ClaimRoguelikePickRequest)r, s),
                [typeof(SkipRoguelikePickRequest)] = (r, s) => HandleSkipRoguelikePick((SkipRoguelikePickRequest)r, s),
                [typeof(RerollRoguelikeRollRequest)] = (r, s) => HandleRerollRoguelikeRoll((RerollRoguelikeRollRequest)r, s),
            };
        }

        // Fixed pool the Roguelike draw/reroll handlers serve. Keep this in sync with real gear ids in
        // the catalog so the resulting picks can actually be claimed.
        private static readonly string[] RoguelikePool = new[] { "gear_base_1", "gear_base_2", "gear_score" };

        private static AddCurrencyResponse HandleAddCurrency(AddCurrencyRequest request, OfflineLiveOpsService service)
        {
            CurrencyGameData currency = service.GetModuleData<CurrencyGameData>();
            CurrencyWallet wallet = currency.GetWallet(request.CurrencyId);
            if (wallet == null || request.Amount <= 0)
            {
                return new AddCurrencyResponse(request.CurrencyId, wallet?.Current ?? 0, 0);
            }

            long previous = wallet.Current;
            long next = wallet.Max.HasValue ? Math.Min(previous + request.Amount, wallet.Max.Value) : previous + request.Amount;
            wallet.Current = next;
            return new AddCurrencyResponse(request.CurrencyId, next, next - previous);
        }

        private static SpendCurrencyResponse HandleSpendCurrency(SpendCurrencyRequest request, OfflineLiveOpsService service)
        {
            CurrencyGameData currency = service.GetModuleData<CurrencyGameData>();
            CurrencyWallet wallet = currency.GetWallet(request.CurrencyId);
            if (wallet == null || !wallet.CanSpend(request.Amount))
            {
                return new SpendCurrencyResponse(request.CurrencyId, wallet?.Current ?? 0, 0, false);
            }

            wallet.Current -= request.Amount;
            return new SpendCurrencyResponse(request.CurrencyId, wallet.Current, request.Amount, true);
        }

        private static SetInventoryResponse HandleSetInventory(SetInventoryRequest request, OfflineLiveOpsService service)
        {
            List<OwnedGearEntry> gears = request.Gears != null
                ? new List<OwnedGearEntry>(request.Gears)
                : new List<OwnedGearEntry>();

            InventoryGameData inventory = service.GetModuleData<InventoryGameData>();
            inventory.Gears = new List<OwnedGearEntry>(gears);

            return new SetInventoryResponse { Gears = gears };
        }

        private static DrawRoguelikeRollResponse HandleDrawRoguelikeRoll(DrawRoguelikeRollRequest request, OfflineLiveOpsService service)
        {
            RoguelikeGameData roguelike = service.GetModuleData<RoguelikeGameData>();
            var ids = new List<string>(RoguelikePool);
            roguelike.CurrentRollIds = new List<string>(ids);
            return new DrawRoguelikeRollResponse { CurrentRollIds = ids };
        }

        private static ClaimRoguelikePickResponse HandleClaimRoguelikePick(ClaimRoguelikePickRequest request, OfflineLiveOpsService service)
        {
            RoguelikeGameData roguelike = service.GetModuleData<RoguelikeGameData>();
            bool succeeded = !string.IsNullOrEmpty(request.PickedGearId)
                && roguelike.CurrentRollIds != null
                && roguelike.CurrentRollIds.Contains(request.PickedGearId);

            if (succeeded)
            {
                roguelike.CurrentRollIds = new List<string>();
            }

            return new ClaimRoguelikePickResponse { Success = succeeded };
        }

        private static SkipRoguelikePickResponse HandleSkipRoguelikePick(SkipRoguelikePickRequest request, OfflineLiveOpsService service)
        {
            RoguelikeGameData roguelike = service.GetModuleData<RoguelikeGameData>();
            roguelike.CurrentRollIds = new List<string>();
            return new SkipRoguelikePickResponse { Success = true };
        }

        private static RerollRoguelikeRollResponse HandleRerollRoguelikeRoll(RerollRoguelikeRollRequest request, OfflineLiveOpsService service)
        {
            RoguelikeGameData roguelike = service.GetModuleData<RoguelikeGameData>();
            var ids = new List<string>(RoguelikePool);
            roguelike.CurrentRollIds = new List<string>(ids);
            return new RerollRoguelikeRollResponse { CurrentRollIds = ids };
        }

        private static RecordRaceResultResponse HandleRecordRaceResult(RecordRaceResultRequest request, OfflineLiveOpsService service)
        {
            TrackGameData track = service.GetModuleData<TrackGameData>();
            track.BestTimeSec.TryGetValue(request.TrackId, out float previousBest);
            bool isNewBest = previousBest == 0f || request.RaceTimeSec < previousBest;
            float newBest = isNewBest ? request.RaceTimeSec : previousBest;

            // Always offer the next track in the ordered list so offline runs can play through progression.
            string nextTrackId = string.Empty;
            if (track.OrderedTrackIds != null)
            {
                int idx = track.OrderedTrackIds.IndexOf(request.TrackId);
                if (idx >= 0 && idx + 1 < track.OrderedTrackIds.Count)
                {
                    nextTrackId = track.OrderedTrackIds[idx + 1];
                }
            }

            return new RecordRaceResultResponse
            {
                NewBestTimeSec = newBest,
                MatchedBandIndex = -1,
                Reward = 0,
                Advanced = !string.IsNullOrEmpty(nextTrackId),
                NextTrackId = nextTrackId,
            };
        }
    }
}
