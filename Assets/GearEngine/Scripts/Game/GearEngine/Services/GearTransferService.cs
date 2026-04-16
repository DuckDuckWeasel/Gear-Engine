using UnityEngine;

namespace GearEngine.GearEngine.Services
{
    /// <summary>
    /// Service responsible for validating and executing gear transfers 
    /// between the external Inventory and the Grid Board.
    /// This removes the reliance on Views to orchestrate cross-system logic.
    /// </summary>
    public class GearTransferService : IGearTransferService
    {
        private readonly IGridManager gridManager;
        private readonly IGearEngineService engineService;
        private readonly BoardConfigSO boardConfig;

        private BoardViewModel LinkedBoard;
        private Inventory.IInventoryService LinkedInventory;

        public GearTransferService(IGridManager gridManager, IGearEngineService engineService, BoardConfigSO boardConfig)
        {
            this.gridManager = gridManager;
            this.engineService = engineService;
            this.boardConfig = boardConfig;
        }

        public void LinkBoard(BoardViewModel board) => LinkedBoard = board;
        
        public void LinkInventory(Inventory.IInventoryService inventory) => LinkedInventory = inventory;

        public void RequestTransferToBoard(Vector3 worldPos, GearConfigData gearData)
        {
            if (LinkedBoard == null || LinkedInventory == null) return;

            if (engineService != null && engineService.IsRunning) return;

            Vector2Int gridPos = boardConfig.GetGridPosition(worldPos);
            bool placed = LinkedBoard.HandleInventoryDrop(gridPos, gearData);
            if (placed)
            {
                LinkedInventory.ConsumeSpecificItem(gearData);
            }
        }

        public void RequestTransferToInventory(GearConfigData gearData, Vector3 dropWorldPos)
        {
            if (LinkedInventory == null) return;
            if (gearData != null)
            {
                LinkedInventory.AddItem(gearData);
            }
        }
    }
}
