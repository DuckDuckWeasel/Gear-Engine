using System;
using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards
{
    /// <summary>
    /// Client-facing cards payload (stub until module merges persistence + config like gold).
    /// </summary>
    public sealed class CardGameData : IGameModuleData
    {
        /// <inheritdoc />
        public string Key => typeof(CardGameData).Name;

        [JsonProperty("slots")]
        private List<CardSlotEntry> _slots = new List<CardSlotEntry>();

        [JsonIgnore]
        public IReadOnlyList<CardSlotEntry> Slots => _slots;

        [JsonConstructor]
        private CardGameData()
        {
        }

        public CardGameData(IEnumerable<CardSlotEntry> slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            _slots = new List<CardSlotEntry>(slots);
        }
    }
}
