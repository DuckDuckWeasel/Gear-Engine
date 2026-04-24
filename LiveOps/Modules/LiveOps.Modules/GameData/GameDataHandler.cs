using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.GameModule;
using LiveOps.Modules.DTO.GameData;
using LiveOps.DTO.GameModule;
using Microsoft.Extensions.Logging;
using GameDataModel = LiveOps.DTO.GameModule.GameData;

namespace LiveOps.Modules.GameData
{
    /// <summary>
    /// Builds <see cref="GameDataResponse"/> by initializing every registered <see cref="IGameModule"/>.
    /// </summary>
    public sealed class GameDataHandler : IGameApiHandler<GameDataRequest, GameDataResponse>
    {
        private readonly ILogger<GameDataHandler> _logger;
        private readonly IEnumerable<IGameModule> _modules;

        public GameDataHandler(ILogger<GameDataHandler> logger, IEnumerable<IGameModule> modules)
        {
            _logger = logger;
            _modules = modules;
        }

        public async Task<GameDataResponse> HandleAsync(GameApiSession session, GameDataRequest request)
        {
            GameDataModel gameData = new GameDataModel();
            List<GameDataModuleError> errors = new List<GameDataModuleError>();

            foreach (IGameModule gameModule in _modules)
            {
                if (gameModule == null)
                {
                    continue;
                }

                try
                {
                    IGameModuleData moduleData = await gameModule.InitializeAsync(session, default);
                    if (moduleData != null)
                    {
                        gameData.AddModuleData(moduleData);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "[GameDataHandler] Error on module {ModuleKey}: {Message}", gameModule.Key, e.Message);
                    errors.Add(new GameDataModuleError { ModuleKey = gameModule.Key, Message = e.Message });
                }
            }

            if (errors.Count > 0)
            {
                return new GameDataResponse(gameData, true, errors);
            }

            return new GameDataResponse(gameData, false, null);
        }
    }
}
