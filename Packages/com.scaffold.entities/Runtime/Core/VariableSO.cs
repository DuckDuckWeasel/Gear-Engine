using UnityEngine;

namespace Scaffold.Entities
{
    /// <summary>
    /// A ScriptableObject that serves as an identifier key for runtime entity variables.
    /// Assign instances of this SO to entity definitions or configs to define which
    /// variables an entity tracks at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Scaffold/Entity/Variable Key", fileName = "VariableKey")]
    public class VariableSO : ScriptableObject
    {
    }
}
