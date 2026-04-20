using UnityEngine;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Authoring asset for <see cref="GearEngineStartData"/> (inventory + board seed) used by campaign installers.
    /// </summary>
    [CreateAssetMenu(fileName = "GearEngineStartData", menuName = "GearEngine/Gear Engine Start Data")]
    public sealed class GearEngineStartDataSO : ScriptableObject
    {
        [SerializeField]
        private GearEngineStartData data = new GearEngineStartData();

        public GearEngineStartData Data => data;
    }
}
