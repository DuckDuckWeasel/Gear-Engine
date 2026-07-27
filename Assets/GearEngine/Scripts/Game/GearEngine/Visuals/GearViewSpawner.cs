using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.GearEngine.Visuals
{
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
            if (view.transform is not RectTransform rect)
            {
                Debug.LogError($"[GearViewSpawner] Gear '{config.Id}' ViewPrefab must use RectTransform.");
                Object.Destroy(view.gameObject);
                return null;
            }

            ResetRectTransform(rect);
            view.ApplyConfig(config);
            return view;
        }

        private static void ResetRectTransform(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
    }
}
