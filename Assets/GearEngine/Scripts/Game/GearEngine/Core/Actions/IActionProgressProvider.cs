namespace GearEngine.Core.Actions
{
    /// <summary>
    /// Exposes real, normalized progress for actions whose duration is measurable.
    /// </summary>
    public interface IActionProgressProvider
    {
        bool TryGetExecutionProgress(out float progress);
    }
}
