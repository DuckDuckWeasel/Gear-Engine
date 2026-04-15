using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using UnityEngine;

namespace GearEngine.GearEngine.Services
{
    public interface IGearTrashService
    {
        BoardViewModel LinkedBoard { get; set; }
        GearInventoryViewModel LinkedInventory { get; set; }
        void RequestTrashDrop(IGridNode node);
        void HandleInventoryGearDropped(GearConfigData gearData);
    }
}
