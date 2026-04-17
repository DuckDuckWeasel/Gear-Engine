using GearEngine.GearEngine.Config;

namespace GearEngine.GearEngine.Services
{
    // todo: narrow interface if board/inventory flows split further.
    public interface IGearPresentationTransferService
    {
        void AddReturnedBoardGearToInventory(GearConfigData config);

        void TrashInventoryGear(GearConfigData gear);
    }
}
