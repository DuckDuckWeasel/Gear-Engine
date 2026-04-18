namespace GearEngine.GearEngine.Services.Inventory
{
    public interface IInventoryService
    {
        InventoryModel GetInventory();

        bool TryAdd(IItem item);

        bool TryConsume(IItem item);
    }
}
