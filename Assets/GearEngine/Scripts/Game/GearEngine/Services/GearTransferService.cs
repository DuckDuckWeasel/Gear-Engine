using System;
using UnityEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Presentation.UI;

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
        private GearInventoryViewModel LinkedInventory;

        public GearTransferService(IGridManager gridManager, IGearEngineService engineService, BoardConfigSO boardConfig)
        {
            this.gridManager = gridManager;
            this.engineService = engineService;
            this.boardConfig = boardConfig;
        }

        public void LinkBoard(BoardViewModel board) => LinkedBoard = board;
        
        public void LinkInventory(GearInventoryViewModel inventory) => LinkedInventory = inventory;

        public void RequestTransferToBoard(Vector3 worldPos, GearConfigData gearData)
        {
            if (LinkedBoard == null || LinkedInventory == null) return;

            if (engineService != null && engineService.IsRunning) return;

            bool placed = LinkedBoard.HandleInventoryDrop(worldPos, gearData);
            if (placed)
            {
                LinkedInventory.ConsumeSpecificGear(gearData);
            }
        }

        public void RequestTransferToInventory(GearConfigData gearData, Vector3 dropWorldPos)
        {
            if (LinkedInventory == null) return;
            if (gearData != null)
            {
                LinkedInventory.AddGearToInventory(gearData);
            }
        }
    }
}
