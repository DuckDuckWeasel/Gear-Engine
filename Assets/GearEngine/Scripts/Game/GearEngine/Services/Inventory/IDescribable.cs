namespace GearEngine.GearEngine.Services.Inventory
{
    /// <summary>
    /// Implemented by abilities to provide a runtime-generated rich text description
    /// that dynamically interpolates the ability's serialized variables.
    /// </summary>
    public interface IDescribable
    {
        string GetRichTextDescription();
        string GetFloatingTextDescription();
    }
}
