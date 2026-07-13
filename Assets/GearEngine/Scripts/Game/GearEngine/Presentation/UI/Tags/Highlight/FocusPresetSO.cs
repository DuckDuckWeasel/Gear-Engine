using UnityEngine;

using TriInspector;
using Coffee.UIEffects;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    [CreateAssetMenu(menuName = "Gear Engine/Tutorial/Focus Preset", fileName = "NewFocusPreset")]
    public class FocusPresetSO : ScriptableObject
    {
        [Header("Overlay Settings")]
        public bool useDarkOverlay = true;
        public Color overlayColor = new Color(0, 0, 0, 0.75f);
        public bool blockClicksOutside = true;

        [Header("Visual Indicators")]
        [Tooltip("The prefab to spawn pointing at the focused UI element (e.g. an arrow).")]
        public GameObject indicatorPrefab;

        [Header("Native UIEffect")]
        [Tooltip("If true, applies a UIEffect preset to the target UI element during focus.")]
        public bool useUIEffect = false;

        [Tooltip("The mob-sakai UIEffect preset to apply. Create one via right-click > Create > UIEffect > UIEffectPreset.")]
        [ShowIf(nameof(useUIEffect))]
        public UIEffectPreset uiEffectPreset;

    }
}
