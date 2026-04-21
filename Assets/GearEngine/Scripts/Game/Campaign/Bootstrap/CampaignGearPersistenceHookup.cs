using System;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.LayeredScope;

namespace GearEngine.Campaign.Bootstrap
{
    /// <summary>
    /// Subscribes to local board and inventory changes in the Campaign layer and persists to LiveOps modules registered in the parent scope.
    /// </summary>
    public sealed class CampaignGearPersistenceHookup : IAsyncInitializable, IDisposable
    {
        private readonly IBoardService board;
        private readonly IRaceInventoryService raceInventory;
        private readonly LoadoutClientModule loadout;
        private readonly InventoryClientModule inventory;

        public CampaignGearPersistenceHookup(
            IBoardService board,
            IRaceInventoryService raceInventory,
            LoadoutClientModule loadout,
            InventoryClientModule inventory)
        {
            this.board = board;
            this.raceInventory = raceInventory;
            this.loadout = loadout;
            this.inventory = inventory;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            board.BoardLayoutChanged += OnBoardLayoutChanged;
            raceInventory.ItemsChanged += OnRaceInventoryChanged;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            board.BoardLayoutChanged -= OnBoardLayoutChanged;
            raceInventory.ItemsChanged -= OnRaceInventoryChanged;
        }

        private void OnBoardLayoutChanged()
        {
            loadout.PersistBoardLayout(BoardLayoutData.FromNodes(board.GetAllNodes()));
        }

        private void OnRaceInventoryChanged()
        {
            inventory.PersistOwnedGearFromRaceInventory(raceInventory);
        }
    }
}
