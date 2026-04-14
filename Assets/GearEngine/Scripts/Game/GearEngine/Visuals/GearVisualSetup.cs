using UnityEngine;

namespace GearEngine.GearEngine.Visuals
{
    /// <summary>
    /// Shared visual setup utility used by both <see cref="GearView"/> (board) and inventory slots.
    /// Instantiates the gear's visual prefab and sets up the icon overlay with fill shader.
    /// </summary>
    public static class GearVisualSetup
    {
        private static Shader fillShaderCache;

        /// <summary>
        /// Instantiates the gear's visual prefab under <paramref name="parent"/>,
        /// optionally adding a UIIcon overlay with fill shader.
        /// </summary>
        /// <returns>The instantiated visual GameObject, or null if no visual prefab is defined.</returns>
        public static GameObject SetupVisual(Transform parent, GearConfigData configData, float scaleMultiplier = 1f, int baseSortingOrder = 50)
        {
            if (configData?.VisualPrefab == null || parent == null)
            {
                return null;
            }

            GameObject visualObj = Object.Instantiate(configData.VisualPrefab, parent);
            visualObj.name = "VisualInstance";
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

            SpriteRenderer[] srs = visualObj.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingOrder = baseSortingOrder;
            }

            if (configData.UIIcon != null)
            {
                SetupIconOverlay(visualObj.transform, configData, baseSortingOrder + 5);
            }

            return visualObj;
        }

        private static void SetupIconOverlay(Transform parent, GearConfigData configData, int sortingOrder)
        {
            GameObject iconObj = new GameObject("UIIcon");
            iconObj.transform.SetParent(parent, false);
            iconObj.transform.localPosition = new Vector3(0, 0, -1f);
            iconObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            SpriteRenderer iconRenderer = iconObj.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = configData.UIIcon;
            iconRenderer.sortingOrder = sortingOrder;

            Shader shader = GetFillShader();
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.SetFloat("_FillAmount", 1f);
                iconRenderer.material = mat;
            }
        }

        private static Shader GetFillShader()
        {
            if (fillShaderCache == null)
            {
                fillShaderCache = Shader.Find("GearEngine/Sprites/SpriteFillGrayscale");
            }

            return fillShaderCache;
        }
    }
}
