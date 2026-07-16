using UnityEngine;

namespace GearEngine.Core.Architecture.References
{
    /// <summary>
    /// Abstracts the resolution of global string-based target references (e.g., from Fungus Global Variables)
    /// without coupling the Core Architecture to third-party libraries.
    /// </summary>
    public interface ITargetResolver
    {
        GameObject Resolve(string globalVariableName);
    }
}
