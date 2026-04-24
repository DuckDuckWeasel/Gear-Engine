using System;
using System.Collections.Generic;
using LiveOps.DTO.ModuleRequest;
using GameDataPayload = LiveOps.DTO.GameModule.GameData;

namespace LiveOps.Modules.DTO.GameData
{
    public sealed class GameDataModuleError
    {
        public string ModuleKey { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    public class GameDataResponse : ModuleResponse
    {
        public GameDataPayload GameData { get; protected set; }

        public bool IsPartial { get; set; }

        public IReadOnlyList<GameDataModuleError> ModuleLoadErrors { get; set; } = Array.Empty<GameDataModuleError>();

        public GameDataResponse(
            GameDataPayload gameData,
            bool isPartial = false,
            IReadOnlyList<GameDataModuleError>? moduleLoadErrors = null)
        {
            GameData = gameData;
            IsPartial = isPartial;
            if (moduleLoadErrors is { Count: > 0 })
            {
                IsPartial = true;
                ModuleLoadErrors = moduleLoadErrors;
            }
        }
    }
}
