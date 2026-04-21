using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    /// <summary>
    /// Resolves gear ids from LiveOps persistence to <see cref="GearConfig"/> assets.
    /// </summary>
    [CreateAssetMenu(fileName = "GearCatalog", menuName = "GearEngine/Gear Catalog")]
    public sealed class GearCatalogSO : ScriptableObject
    {
        [SerializeField]
        private GearConfig[] gears = Array.Empty<GearConfig>();

        private readonly Dictionary<string, GearConfig> _byId = new Dictionary<string, GearConfig>(StringComparer.Ordinal);

        private void OnEnable()
        {
            RebuildLookup();
        }

        public void SetRuntimeEntries(GearConfig[] gearConfigs)
        {
            gears = gearConfigs != null ? gearConfigs : Array.Empty<GearConfig>();
            RebuildLookup();
        }

        private void RebuildLookup()
        {
            _byId.Clear();
            if (gears == null)
            {
                return;
            }

            foreach (GearConfig g in gears)
            {
                if (g == null || string.IsNullOrEmpty(g.Id))
                {
                    continue;
                }

                _byId[g.Id] = g;
            }
        }

        public GearConfig Get(string gearId)
        {
            if (string.IsNullOrEmpty(gearId))
            {
                return null;
            }

            return _byId.TryGetValue(gearId, out GearConfig g) ? g : null;
        }

        public bool TryGet(string gearId, out GearConfig gear)
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
