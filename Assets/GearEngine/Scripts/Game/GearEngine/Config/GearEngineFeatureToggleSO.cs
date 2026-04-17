using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(menuName = "GearEngine/Feature Toggle", fileName = "GearEngineFeatureToggle")]
    public sealed class GearEngineFeatureToggleSO : ScriptableObject
    {
        [Header("Trash / Scrap Feature")]
        [Tooltip("Enables the drag-to-trash gear deletion mechanic.")]
        public bool EnableTrashDeletion = true;
    }
}
