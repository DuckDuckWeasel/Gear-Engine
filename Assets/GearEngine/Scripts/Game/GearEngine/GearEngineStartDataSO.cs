using UnityEngine;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Authoring asset for <see cref="GearEngineStartData"/> (e.g. board layout on <c>RaceStartData</c>). Owned gear comes from LiveOps <c>IInventoryService</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "GearEngineStartData", menuName = "GearEngine/Gear Engine Start Data")]
    public sealed class GearEngineStartDataSO : ScriptableObject
    {
        [SerializeField]
        private GearEngineStartData data = new GearEngineStartData();

        public GearEngineStartData Data => data;
    }
}
