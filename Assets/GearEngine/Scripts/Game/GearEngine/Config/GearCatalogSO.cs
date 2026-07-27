using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    /// <summary>
    /// Resolves gear ids from LiveOps persistence to <see cref="GearItem"/> assets.
    /// </summary>
    [CreateAssetMenu(fileName = "GearCatalog", menuName = "GearEngine/Config/Gear Catalog")]
    public sealed class GearCatalogSO : ScriptableObject
    {
        [SerializeField]
        private GearItem[] gears = Array.Empty<GearItem>();

        private readonly Dictionary<string, GearItem> _byId = new Dictionary<string, GearItem>(StringComparer.Ordinal);

        private void OnEnable()
        {
            RebuildLookup();
        }

        public void SetRuntimeEntries(GearItem[] gearConfigs)
        {
            gears = gearConfigs != null ? gearConfigs : Array.Empty<GearItem>();
            RebuildLookup();
        }

        /// <summary>Serialized catalog entries (may contain nulls or entries without ids; callers should filter).</summary>
        public IReadOnlyList<GearItem> All => gears != null ? gears : Array.Empty<GearItem>();

        private void RebuildLookup()
        {
            _byId.Clear();
            if (gears == null)
            {
                return;
            }

            foreach (GearItem g in gears)
            {
                if (g == null || string.IsNullOrEmpty(g.Id))
                {
                    continue;
                }

                _byId[g.Id] = g;
            }
        }

        public GearItem Get(string gearId)
        {
            if (string.IsNullOrEmpty(gearId))
            {
                return null;
            }

            return _byId.TryGetValue(gearId, out GearItem g) ? g : null;
        }

        public bool TryGet(string gearId, out GearItem gear)
        {
            if (string.IsNullOrEmpty(gearId))
            {
                gear = null;
                return false;
            }

            return _byId.TryGetValue(gearId, out gear);
        }
    }
}
