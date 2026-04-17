using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards
{
    /// <summary>
    /// Player-persisted card slots (IDs + state). Full catalog and roll rules stay in remote config / services.
    /// </summary>
    public sealed class CardPersistence : IGameModuleData
    {
        /// <inheritdoc />
        public string Key => typeof(CardPersistence).Name;

        [JsonProperty("slots")]
        public List<CardSlotEntry> Slots { get; set; } = new List<CardSlotEntry>();
    }
}
