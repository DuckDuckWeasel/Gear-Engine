using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.Cards;
using GearEngine.GearEngine;
using UnityEngine;

namespace GearEngine.Race
{
    [Serializable]
    public sealed class RaceStartData
    {
        public RaceStartData()
        {
        }

        public RaceStartData(TrackDefinition trackDefinition, CarDefinition carDefinition, GearEngineStartData gearEngineData = null, CardCatalogSO cardsCatalog = null, PlayerCardInventoryState cardsInventory = null)
        {
            this.trackDefinition = trackDefinition;
            this.carDefinition = carDefinition;
            this.gearEngineData = gearEngineData;
            this.cardsCatalog = cardsCatalog;
            this.cardsInventory = cardsInventory ?? new PlayerCardInventoryState();
        }

        public TrackDefinition TrackDefinition => trackDefinition;

        public CarDefinition CarDefinition => carDefinition;

        public GearEngineStartData GearEngineData => gearEngineData;

        public CardCatalogSO CardsCatalog => cardsCatalog;

        public PlayerCardInventoryState CardsInventory => cardsInventory;

        [SerializeField]
        private TrackDefinition trackDefinition;

        [SerializeField]
        private CarDefinition carDefinition;

        [SerializeField]
        private GearEngineStartData gearEngineData;

        [SerializeField]
        private CardCatalogSO cardsCatalog;

        [SerializeField]
        private PlayerCardInventoryState cardsInventory = new PlayerCardInventoryState();
    }
}
