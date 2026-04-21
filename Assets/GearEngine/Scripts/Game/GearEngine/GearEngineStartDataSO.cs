using UnityEngine;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Authoring asset for <see cref="GearEngineStartData"/> (board seed) used by campaign installers. Owned gear comes from LiveOps <c>IInventoryService</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "GearEngineStartData", menuName = "GearEngine/Gear Engine Start Data")]
    public sealed class GearEngineStartDataSO : ScriptableObject
    {
        [SerializeField]
        private GearEngineStartData data = new GearEngineStartData();

        public GearEngineStartData Data => data;
    }
}
