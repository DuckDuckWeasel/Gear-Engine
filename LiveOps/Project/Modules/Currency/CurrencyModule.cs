using System;
using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModule.Response;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.ModuleRequests;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Currency
{
    public class CurrencyModule : GameModule<CurrencyGameData>
    {
        private const string PersistenceKey = nameof(CurrencyPersistence);
        private const string ConfigKey = nameof(CurrencyConfig);

        private readonly ILogger<CurrencyModule> _logger;
        private readonly ModuleRequestHandler _handler;

        public CurrencyModule(ILogger<CurrencyModule> logger, ModuleRequestHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData Player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            CurrencyConfig config = await remoteConfig.Get(context, ConfigKey, new CurrencyConfig());
            CurrencyPersistence persistence = await Player.Get(context, PersistenceKey, new CurrencyPersistence());

            bool dirty = CurrencyPersistenceSeeder.SeedAndClampInPlace(persistence, config);
            if (dirty)
            {
                await Player.Set(context, PersistenceKey, persistence);
            }

            return new CurrencyGameData(persistence, config);
        }

        [CloudCodeFunction(nameof(AddCurrencyRequest))]
        public async Task<AddCurrencyResponse> AddCurrency(IExecutionContext context, IPlayerData Player, IRemoteConfig remoteConfig, AddCurrencyRequest request)
        {
            string id = request?.CurrencyId ?? string.Empty;
            long amount = request?.Amount ?? 0;
            CurrencyChangedResponse change = await AddToPlayer(context, Player, remoteConfig, id, amount, enqueueNestedResponse: false);
            AddCurrencyResponse response = new AddCurrencyResponse(change.CurrencyId, change.NewAmount, change.Diff);
            return await _handler.ResolveResponse(context, request, response);
        }

        [CloudCodeFunction(nameof(SpendCurrencyRequest))]
        public async Task<SpendCurrencyResponse> SpendCurrency(IExecutionContext context, IPlayerData Player, IRemoteConfig remoteConfig, SpendCurrencyRequest request)
        {
            string id = request?.CurrencyId ?? string.Empty;
            long amount = request?.Amount ?? 0;
            (bool ok, CurrencyChangedResponse change) = await TrySpendFromPlayer(context, Player, remoteConfig, id, amount, enqueueNestedResponse: false);
            SpendCurrencyResponse response = new SpendCurrencyResponse(id, change.NewAmount, ok ? amount : 0, ok);
            return await _handler.ResolveResponse(context, request, response);
        }

        public async Task<CurrencyChangedResponse> AddToPlayer(IExecutionContext context, IPlayerData Player, IRemoteConfig remoteConfig, string id, long amount, bool enqueueNestedResponse)
        {
            CurrencyConfig config = await remoteConfig.Get(context, ConfigKey, new CurrencyConfig());
            CurrencyPersistence persistence = await Player.Get(context, PersistenceKey, new CurrencyPersistence());

            if (string.IsNullOrEmpty(id) || amount <= 0 || !config.TryGet(id, out CurrencyConfigEntry? entry) || entry == null)
            {
                long val = persistence.TryGet(id, out long v0) ? v0 : 0;
                if (string.IsNullOrEmpty(id) || amount <= 0)
                {
                    _logger.LogWarning("[CurrencyModule] AddCurrency rejected (id='{Id}', amount={Amount})", id, amount);
                }
                else
                {
                    _logger.LogWarning("[CurrencyModule] Unknown currency id {Id}", id);
                }

                return new CurrencyChangedResponse(id, val, 0);
            }

            long previous = persistence.TryGet(id, out long pv) ? pv : entry.Initial;
            long next = entry.Max.HasValue ? Math.Min(previous + amount, entry.Max.Value) : previous + amount;
            persistence.Set(id, next);
            Player.AddToCache(PersistenceKey);

            CurrencyChangedResponse resp = new CurrencyChangedResponse(id, next, next - previous);
            if (enqueueNestedResponse)
            {
                _handler.AddResponse(resp);
            }

            return resp;
        }

        public async Task<(bool ok, CurrencyChangedResponse resp)> TrySpendFromPlayer(IExecutionContext context, IPlayerData Player, IRemoteConfig remoteConfig, string id, long amount, bool enqueueNestedResponse)
        {
            CurrencyConfig config = await remoteConfig.Get(context, ConfigKey, new CurrencyConfig());
            CurrencyPersistence persistence = await Player.Get(context, PersistenceKey, new CurrencyPersistence());

            if (string.IsNullOrEmpty(id) || amount <= 0 || !config.TryGet(id, out CurrencyConfigEntry? entry) || entry == null)
            {
                long val = persistence.TryGet(id, out long v) ? v : 0;
                return (false, new CurrencyChangedResponse(id, val, 0));
            }

            long previous = persistence.TryGet(id, out long pv) ? pv : entry.Initial;
            long floor = entry.Min ?? 0;
            long next = previous - amount;
            if (next < floor)
            {
                return (false, new CurrencyChangedResponse(id, previous, 0));
            }

            persistence.Set(id, next);
            Player.AddToCache(PersistenceKey);

            CurrencyChangedResponse resp = new CurrencyChangedResponse(id, next, next - previous);
            if (enqueueNestedResponse)
            {
                _handler.AddResponse(resp);
            }

            return (true, resp);
        }

        public async Task<CurrencyChangedResponse> SetForPlayer(IExecutionContext context, IPlayerData Player, IRemoteConfig remoteConfig, string id, long value, bool enqueueNestedResponse)
        {
            CurrencyConfig config = await remoteConfig.Get(context, ConfigKey, new CurrencyConfig());
            CurrencyPersistence persistence = await Player.Get(context, PersistenceKey, new CurrencyPersistence());

            if (string.IsNullOrEmpty(id) || !config.TryGet(id, out CurrencyConfigEntry? entry) || entry == null)
            {
                long val = persistence.TryGet(id, out long v) ? v : 0;
                return new CurrencyChangedResponse(id, val, 0);
            }

            long previous = persistence.TryGet(id, out long pv) ? pv : entry.Initial;
            long clamped = value;
            if (entry.Min.HasValue && clamped < entry.Min.Value)
            {
                clamped = entry.Min.Value;
            }

            if (entry.Max.HasValue && clamped > entry.Max.Value)
            {
                clamped = entry.Max.Value;
            }

            persistence.Set(id, clamped);
            Player.AddToCache(PersistenceKey);

            CurrencyChangedResponse resp = new CurrencyChangedResponse(id, clamped, clamped - previous);
            if (enqueueNestedResponse)
            {
                _handler.AddResponse(resp);
            }

            return resp;
        }
    }
}
