using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.ModuleFetchData;
using LiveOps.Modules.DTO.GameData;
using GameDataModel = LiveOps.DTO.GameModule.GameData;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;

namespace LiveOps.Modules.GameData
{
    public sealed class GameDataHandler : IGameApiHandler<GameDataRequest, GameDataResponse>
    {
        private readonly ILogger<GameDataHandler> _logger;
        private readonly IEnumerable<IGameModule> _modules;

        public GameDataHandler(ILogger<GameDataHandler> logger, IEnumerable<IGameModule> modules)
        {
            _logger = logger;
            _modules = modules;
        }

        public string[]? PlayerKeys() => ModulePrefetchKeys.UnionOrAll(_modules, static m => m.PlayerKeys());

        public string[]? ConfigKeys() => ModulePrefetchKeys.UnionOrAll(_modules, static m => m.ConfigKeys());

        public async Task<GameDataResponse> HandleAsync(GameApiSession session, GameDataRequest request)
        {
            GameDataModel gameData = new();
            IGameModule[] list = _modules?.Where(m => m != null).Cast<IGameModule>().ToArray() ?? Array.Empty<IGameModule>();

            IExecutionContext ctx = session.Context;
            _logger.LogInformation(
                "[GameDataHandler] GameData initialization started. PlayerId={PlayerId}, ProjectId={ProjectId}, EnvironmentId={EnvironmentId}",
                ctx.PlayerId,
                ctx.ProjectId,
                ctx.EnvironmentId);

            if (list.Length == 0)
            {
                _logger.LogWarning("[GameDataHandler] No IGameModule registrations found; returning empty GameData.");
                return new GameDataResponse(gameData, false, null);
            }

            string moduleSummary = string.Join(
                ", ",
                list.Select(m => $"{m.Key}:{m.GetType().FullName}"));
            _logger.LogInformation("[GameDataHandler] Modules found ({Count}): {Modules}", list.Length, moduleSummary);

            var loaded = new ConcurrentQueue<IGameModuleData>();
            var errors = new ConcurrentQueue<GameDataModuleError>();

            await Task.WhenAll(
                list.Select(
                    m => RunModuleAsync(
                        m,
                        session,
                        loaded,
                        errors))).ConfigureAwait(false);

            foreach (IGameModuleData part in loaded)
            {
                gameData.AddModuleData(part);
            }

            List<GameDataModuleError> errList = errors.ToList();
            int moduleDataAdded = loaded.Count;
            int nullModuleResults = list.Length - moduleDataAdded - errList.Count;
            _logger.LogInformation(
                "[GameDataHandler] GameData initialization finished. ModulesFound={ModulesFound}, ModuleDataAdded={ModuleDataAdded}, NullModuleResults={NullModuleResults}, Errors={Errors}",
                list.Length,
                moduleDataAdded,
                nullModuleResults,
                errList.Count);

            if (errList.Count > 0)
            {
                return new GameDataResponse(
                    gameData,
                    isPartial: true,
                    moduleLoadErrors: errList);
            }

            return new GameDataResponse(gameData, false, null);
        }

        private async Task RunModuleAsync(
            IGameModule gameModule,
            GameApiSession session,
            ConcurrentQueue<IGameModuleData> loaded,
            ConcurrentQueue<GameDataModuleError> errors)
        {
            try
            {
                _logger.LogInformation(
                    "[GameDataHandler] Starting module {ModuleKey} ({ModuleType}).",
                    gameModule.Key,
                    gameModule.GetType().FullName);

                IGameModuleData? moduleData = await gameModule
                    .InitializeAsync(session, default)
                    .ConfigureAwait(false);
                if (moduleData != null)
                {
                    loaded.Enqueue(moduleData);
                    _logger.LogInformation(
                        "[GameDataHandler] Module {ModuleKey} produced module data type {DataType}.",
                        gameModule.Key,
                        moduleData.GetType().FullName);
                }
                else
                {
                    _logger.LogWarning("[GameDataHandler] Module {ModuleKey} returned null module data.", gameModule.Key);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[GameDataHandler] Error on module {ModuleKey}: {Message}", gameModule.Key, e.Message);
                errors.Enqueue(
                    new GameDataModuleError
                    {
                        ModuleKey = gameModule.Key,
                        Message = e.Message
                    });
            }
        }
    }
}
