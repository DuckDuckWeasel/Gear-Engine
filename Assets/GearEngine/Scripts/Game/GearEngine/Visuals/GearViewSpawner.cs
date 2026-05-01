using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.GearEngine.Visuals
{
    /// <summary>Single entry point for instantiating gear visuals from <see cref="GearItemData.ViewPrefab"/>.</summary>
    public static class GearViewSpawner
    {
        public static GearView Spawn(GearItemData config, Transform parent)
        {
            if (config?.ViewPrefab == null)
            {
                Debug.LogError($"[GearViewSpawner] Gear '{config?.Id}' missing ViewPrefab.");
                return null;
            }

            GearView view = Object.Instantiate(config.ViewPrefab, parent, false);
            view.transform.localPosition = Vector3.zero;
            view.transform.localRotation = Quaternion.identity;
            view.ApplyConfig(config);
            return view;
        }
    }
}
