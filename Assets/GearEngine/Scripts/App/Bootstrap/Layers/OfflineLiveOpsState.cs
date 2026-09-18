using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using GearEngine.Perks.Config;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.GameData;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.Roguelike;
using LiveOps.Modules.DTO.Tracks;
using Newtonsoft.Json;
using Scaffold.AppFlow;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Layers
{
    internal sealed class OfflineLiveOpsState
    {
        public OfflineLiveOpsState(ILayerResolver layerResolver)
        {
            this.layerResolver = layerResolver ?? throw new ArgumentNullException(nameof(layerResolver));
        }

        private static readonly string[] s_preferredStartingGearIds =
        {
            "gear_core",
            "gear_base_1",
            "gear_score",
            "gear_speed",
        };

        private readonly ILayerResolver layerResolver;
        private readonly List<string> perkCatalogIds = new List<string>();
        private readonly List<string> roguelikeGearIds = new List<string>();
        private GameData gameData;
        private int rollOffset;

        public void UseGameData(GameData value)
        {
            gameData = value ?? throw new ArgumentNullException(nameof(value));
            RefreshLocalCatalogs();
        }

        public GameData CreateInitialGameData()
        {
            if (gameData != null)
            {
                return gameData;
            }

            RefreshLocalCatalogs();
            gameData = new GameData();
            gameData.AddModuleData(CreateCurrencyData());
            gameData.AddModuleData(CreateInventoryData());
            gameData.AddModuleData(new LoadoutGameData(
                new LoadoutPersistence(),
                new LoadoutConfig { BaseSlots = 6, MotorCogStartX = 2, MotorCogStartY = 2 }));
            gameData.AddModuleData(CreatePerkData());
            gameData.AddModuleData(CreateRoguelikeData());
            gameData.AddModuleData(CreateTrackData());
            return gameData;
        }

        public TResponse CreateResponse<TResponse>(ModuleRequest<TResponse> request)
            where TResponse : ModuleResponse
        {
            CreateInitialGameData();
            object requestObject = request;
            ModuleResponse response = requestObject switch
            {
                GameDataRequest => new GameDataResponse(gameData),
                AddCurrencyRequest add => CreateAddCurrencyResponse(add),
                SpendCurrencyRequest spend => CreateSpendCurrencyResponse(spend),
                SetInventoryRequest setInventory => CreateSetInventoryResponse(setInventory),
                SaveBoardLayoutRequest => CreateSaveBoardLayoutResponse(),
                ClearBoardRequest => new ClearBoardResponse(),
                PurchasePerkRequest => CreatePurchasePerkResponse(),
                BurnPerkRequest burn => CreateBurnPerkResponse(burn),
                DrawRoguelikeRollRequest => CreateDrawResponse(),
                RerollRoguelikeRollRequest => CreateRerollResponse(),
                ClaimRoguelikePickRequest claim => CreateClaimResponse(claim),
                SkipRoguelikePickRequest => new SkipRoguelikePickResponse { Success = true },
                RecordRaceResultRequest race => CreateRecordRaceResultResponse(race),
                _ => CreateEmptyResponse<TResponse>(),
            };
            return (TResponse)response;
        }

        public TResponse CreateEmptyResponse<TResponse>() where TResponse : ModuleResponse
        {
            try
            {
                return JsonConvert.DeserializeObject<TResponse>("{}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[OfflineLiveOpsState] Could not create an empty {typeof(TResponse).Name} response. {exception}");
                return null;
            }
        }

        private CurrencyGameData CreateCurrencyData()
        {
            CurrencyConfig config = new CurrencyConfig();
            config.AddEntry(new CurrencyConfigEntry { Id = "gold", Initial = 15, Min = 0 });
            CurrencyPersistence persistence = new CurrencyPersistence();
            persistence.Set("gold", 15);
            return new CurrencyGameData(persistence, config);
        }

        private InventoryGameData CreateInventoryData()
        {
            InventoryConfig config = new InventoryConfig { BaseSlots = 8 };
            InventoryPersistence persistence = new InventoryPersistence { StartingGearsSeeded = true };
            foreach (string id in ResolveStartingGearIds())
            {
                config.StartingGearIds.Add(id);
                persistence.Gears.Add(new OwnedGearEntry
                {
                    GearId = id,
                    InstanceId = $"offline_{id}",
                });
            }

            return new InventoryGameData(persistence, config);
        }

        private PerkGameData CreatePerkData()
        {
            PerkConfig config = new PerkConfig { BaseCost = 100, CostPerPurchaseGrowth = 50, BurnReward = 50 };
            config.Catalog.AddRange(perkCatalogIds);
            return new PerkGameData(new PerkPersistence(), config);
        }

        private RoguelikeGameData CreateRoguelikeData()
        {
            RoguelikeConfig config = new RoguelikeConfig { OptionsPerRoll = 3 };
            config.GearPool.AddRange(roguelikeGearIds);
            return new RoguelikeGameData(new RoguelikePersistence(), config);
        }

        private TrackGameData CreateTrackData()
        {
            TrackConfig config = new TrackConfig();
            if (layerResolver.TryResolve(out IReadOnlyList<TrackDefinition> tracks) && tracks != null)
            {
                foreach (TrackDefinition track in tracks)
                {
                    if (track != null && !string.IsNullOrEmpty(track.name))
                    {
                        config.AddEntry(new TrackConfigEntry { Id = track.name, BaseReward = 10 });
                    }
                }
            }

            TrackPersistence persistence = new TrackPersistence
            {
                CurrentTrackId = config.Entries.FirstOrDefault()?.Id ?? string.Empty,
            };
            return new TrackGameData(persistence, config);
        }

        private IEnumerable<string> ResolveStartingGearIds()
        {
            if (!layerResolver.TryResolve(out GearCatalogSO catalog) || catalog == null)
            {
                return s_preferredStartingGearIds;
            }

            HashSet<string> available = new HashSet<string>(
                catalog.All.Where(item => item != null).Select(item => item.Id),
                StringComparer.Ordinal);
            List<string> preferred = s_preferredStartingGearIds.Where(available.Contains).ToList();
            if (preferred.Count > 0)
            {
                return preferred;
            }

            return available.Take(4);
        }

        private void RefreshLocalCatalogs()
        {
            perkCatalogIds.Clear();
            if (layerResolver.TryResolve(out PerkCatalogSO perks) && perks != null)
            {
                perkCatalogIds.AddRange(perks.All.Where(item => item != null).Select(item => item.Id));
            }

            if (perkCatalogIds.Count == 0)
            {
                perkCatalogIds.AddRange(new[] { "Boost", "GearSize", "HotTires", "MaxSpeed", "Stations" });
            }

            roguelikeGearIds.Clear();
            if (layerResolver.TryResolve(out RoguelikeGearPoolSO pool) && pool != null)
            {
                roguelikeGearIds.AddRange(pool.All.Where(item => item != null).Select(item => item.Id));
            }

            if (roguelikeGearIds.Count == 0 && layerResolver.TryResolve(out GearCatalogSO gears) && gears != null)
            {
                roguelikeGearIds.AddRange(gears.All
                    .Where(item => item != null && item.Id != "gear_core")
                    .Select(item => item.Id));
            }
        }

        private AddCurrencyResponse CreateAddCurrencyResponse(AddCurrencyRequest request)
        {
            CurrencyWallet wallet = GetWallet(request.CurrencyId);
            long previous = wallet?.Current ?? 0;
            long next = previous + Math.Max(0, request.Amount);
            return new AddCurrencyResponse(request.CurrencyId, next, next - previous);
        }

        private SpendCurrencyResponse CreateSpendCurrencyResponse(SpendCurrencyRequest request)
        {
            CurrencyWallet wallet = GetWallet(request.CurrencyId);
            long previous = wallet?.Current ?? 0;
            long floor = wallet?.Min ?? 0;
            bool succeeded = request.Amount > 0 && previous - request.Amount >= floor;
            long next = succeeded ? previous - request.Amount : previous;
            return new SpendCurrencyResponse(request.CurrencyId, next, succeeded ? request.Amount : 0, succeeded);
        }

        private SetInventoryResponse CreateSetInventoryResponse(SetInventoryRequest request)
        {
            return new SetInventoryResponse
            {
                Gears = request.Gears != null
                    ? request.Gears.Select(CloneGearEntry).ToList()
                    : new List<OwnedGearEntry>(),
            };
        }

        private static SaveBoardLayoutResponse CreateSaveBoardLayoutResponse()
        {
            return new SaveBoardLayoutResponse
            {
                SavedAtUtcTicks = DateTime.UtcNow.Ticks,
                Rejected = false,
            };
        }

        private PurchasePerkResponse CreatePurchasePerkResponse()
        {
            PerkGameData perks = gameData.GetModuleData<PerkGameData>();
            CurrencyWallet wallet = GetWallet("gold");
            long cost = perks?.NextCost ?? 100;
            if (perks == null || wallet == null || wallet.Current < cost || perkCatalogIds.Count == 0)
            {
                return new PurchasePerkResponse { Success = false, Cost = cost, NextCost = cost };
            }

            string perkId = perkCatalogIds[perks.Unlocked.Count % perkCatalogIds.Count];
            int uniqueCount = perks.Unlocked.Append(perkId).Distinct(StringComparer.Ordinal).Count();
            return new PurchasePerkResponse
            {
                Success = true,
                UnlockedPerkId = perkId,
                Cost = cost,
                NextCost = 100 + (50 * uniqueCount),
                NewGoldBalance = wallet.Current - cost,
            };
        }

        private BurnPerkResponse CreateBurnPerkResponse(BurnPerkRequest request)
        {
            PerkGameData perks = gameData.GetModuleData<PerkGameData>();
            CurrencyWallet wallet = GetWallet("gold");
            bool success = perks?.Unlocked.Contains(request.PerkId) == true;
            long reward = success ? perks.BurnReward : 0;
            return new BurnPerkResponse
            {
                Success = success,
                GoldEarned = reward,
                NewGoldBalance = (wallet?.Current ?? 0) + reward,
            };
        }

        private DrawRoguelikeRollResponse CreateDrawResponse()
        {
            RoguelikeGameData data = gameData.GetModuleData<RoguelikeGameData>();
            List<string> ids = data?.CurrentRollIds?.Count > 0
                ? new List<string>(data.CurrentRollIds)
                : CreateNextRoll();
            return new DrawRoguelikeRollResponse { CurrentRollIds = ids };
        }

        private RerollRoguelikeRollResponse CreateRerollResponse()
        {
            return new RerollRoguelikeRollResponse { CurrentRollIds = CreateNextRoll() };
        }

        private ClaimRoguelikePickResponse CreateClaimResponse(ClaimRoguelikePickRequest request)
        {
            RoguelikeGameData data = gameData.GetModuleData<RoguelikeGameData>();
            return new ClaimRoguelikePickResponse
            {
                Success = data?.CurrentRollIds?.Contains(request.PickedGearId) == true,
            };
        }

        private List<string> CreateNextRoll()
        {
            List<string> result = new List<string>();
            int count = Math.Min(3, roguelikeGearIds.Count);
            for (int i = 0; i < count; i++)
            {
                result.Add(roguelikeGearIds[(rollOffset + i) % roguelikeGearIds.Count]);
            }

            if (roguelikeGearIds.Count > 0)
            {
                rollOffset = (rollOffset + count) % roguelikeGearIds.Count;
            }

            return result;
        }

        private RecordRaceResultResponse CreateRecordRaceResultResponse(RecordRaceResultRequest request)
        {
            TrackGameData tracks = gameData.GetModuleData<TrackGameData>();
            List<string> orderedTrackIds = tracks?.OrderedTrackIds;
            float previousBest = tracks?.BestTimeSec != null
                && tracks.BestTimeSec.TryGetValue(request.TrackId, out float best)
                    ? best
                    : float.PositiveInfinity;
            float newBest = Math.Min(previousBest, Math.Max(0, request.RaceTimeSec));
            int currentIndex = orderedTrackIds?.IndexOf(request.TrackId) ?? -1;
            string nextTrackId = orderedTrackIds != null
                && currentIndex >= 0
                && currentIndex + 1 < orderedTrackIds.Count
                ? orderedTrackIds[currentIndex + 1]
                : request.TrackId;
            RecordRaceResultResponse response = new RecordRaceResultResponse
            {
                NewBestTimeSec = newBest,
                MatchedBandIndex = 0,
                Reward = 10,
                Advanced = currentIndex >= 0,
                NextTrackId = nextTrackId,
            };
            CurrencyWallet wallet = GetWallet("gold");
            response.Responses.Add(new AddCurrencyResponse("gold", (wallet?.Current ?? 0) + 10, 10));
            return response;
        }

        private CurrencyWallet GetWallet(string currencyId)
        {
            return gameData.GetModuleData<CurrencyGameData>()?.GetWallet(currencyId);
        }

        private static OwnedGearEntry CloneGearEntry(OwnedGearEntry entry)
        {
            return entry == null
                ? null
                : new OwnedGearEntry { GearId = entry.GearId, InstanceId = entry.InstanceId };
        }
    }
}
