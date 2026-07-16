using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Tags
{
    /// <summary>
    /// Interface that defines how to evaluate if a target GameObject matches specific tag criteria.
    /// This abstracts away the underlying tag implementation (SO, Native, Enum, etc.).
    /// </summary>
    public interface ITagFilter
    {
        bool IsMatch(GameObject target);
    }
}
