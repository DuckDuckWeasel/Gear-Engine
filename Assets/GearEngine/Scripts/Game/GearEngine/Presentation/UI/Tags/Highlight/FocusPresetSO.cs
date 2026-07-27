using UnityEngine;

using TriInspector;
using Coffee.UIEffects;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    [CreateAssetMenu(menuName = "GearEngine/Tutorial/Focus Preset", fileName = "NewFocusPreset")]
    public class FocusPresetSO : ScriptableObject
    {
        [Header("Overlay Settings")]
        public bool useDarkOverlay = true;

        [ShowIf(nameof(useDarkOverlay))]
        public Color overlayColor = new Color(0, 0, 0, 0.75f);

        [ShowIf(nameof(useDarkOverlay))]
        public bool blockClicksOutside = true;

        [Header("Visual Indicators")]
        [Tooltip("The prefab to spawn pointing at the focused UI element (e.g. an arrow).")]
        public GameObject indicatorPrefab;

        [Tooltip("Anchor point on the target UI bounds.")]
        public IndicatorAnchor indicatorAnchor = IndicatorAnchor.MiddleCenter;

        [Tooltip("Used only if IndicatorAnchor is set to Custom (e.g. 0.5, 1.0 is Top Center).")]
        [ShowIf(nameof(indicatorAnchor), IndicatorAnchor.Custom)]
        public Vector2 customIndicatorAnchor = new Vector2(0.5f, 1f);

        [Tooltip("Offset of the indicator relative to the anchor point on the target bounds.")]
        public Vector2 indicatorOffset = Vector2.zero;

        [Tooltip("Pushes the indicator towards the anchor (if negative) or away from the anchor (if positive) along the normal direction of the anchor.")]
        public float directionOffset = 0f;

        [Tooltip("If true, rotates the indicator to aim precisely at the anchor center.")]
        public bool aimToAnchor = true;

        [Header("Native UIEffect")]
        [Tooltip("If true, applies a UIEffect preset to the target UI element during focus.")]
        public bool useUIEffect = false;

        [Tooltip("The mob-sakai UIEffect preset to apply. Create one via right-click > Create > UIEffect > UIEffectPreset.")]
        [ShowIf(nameof(useUIEffect))]
        public UIEffectPreset uiEffectPreset;

    }
}
