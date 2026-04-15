using UnityEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;

namespace GearEngine.GearEngine.Services
{
    public interface IGearTransferService
    {
        void LinkBoard(BoardViewModel board);
        void LinkInventory(GearInventoryViewModel inventory);
        
        void RequestTransferToBoard(Vector3 worldPos, GearConfigData gearData);
        void RequestTransferToInventory(GearConfigData gearData, Vector3 dropWorldPos);
    }
}
