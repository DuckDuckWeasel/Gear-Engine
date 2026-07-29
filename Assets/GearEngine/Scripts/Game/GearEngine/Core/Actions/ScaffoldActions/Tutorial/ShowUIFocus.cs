using System;
using System.Collections.Generic;
using UnityEngine;
using GearEngine.Core.Actions;
using GearEngine.Core.Architecture.References;
using TriInspector;
using GearEngine.GearEngine.Presentation.UI.Tags;

namespace Scaffold
{
    [CommandInfo("Tutorial",
                 "Show UI Focus",
                 "Shows a focus indicator using a FocusPresetSO.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ShowUIFocus : ActionBase
    {
        [Tooltip("The target UI element to focus on. Use Tags or Runtime Anchors.")]
        [SerializeField]
        private TargetReference target = new TargetReference();

        [Tooltip("If multiple objects have this tag, pick the one at this index. Default is 0.")]
        [SerializeField]
        private int targetIndex = 0;

        [Tooltip("The visual preset to use for the focus overlay and effects.")]
        [SerializeField, InlineEditor]
        private GearEngine.GearEngine.Presentation.UI.Tags.Highlight.FocusPresetSO preset;

        [Header("Layout Overrides")]
        [Tooltip("If true, overrides the layout properties defined in the FocusPresetSO.")]
        [SerializeField]
        private bool overridePresetLayout = false;

        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("Anchor point on the target UI bounds.")]
        [SerializeField]
        private GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor indicatorAnchor = GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor.TopCenter;

        [ShowIf(nameof(ShowCustomAnchor))]
        [Tooltip("Used only if IndicatorAnchor is set to Custom.")]
        [SerializeField]
        private Vector2 customIndicatorAnchor = new Vector2(0.5f, 1f);

        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("Offset of the indicator relative to the anchor point on the target bounds.")]
        [SerializeField]
        private Vector2 indicatorOffset = Vector2.zero;

        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("Pushes the indicator towards/away from the anchor.")]
        [SerializeField]
        private float directionOffset = 0f;

        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("If true, rotates the indicator to aim precisely at the anchor center.")]
        [SerializeField]
        private bool aimToAnchor = true;

        [Tooltip("The Show custom anchor")]
        private bool ShowCustomAnchor => overridePresetLayout && indicatorAnchor == GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor.Custom;

        public override void OnEnter()
        {
            if (preset == null)
            {
                Debug.LogError("[ShowUIFocus] Preset is missing.");
                Continue();
                return;
            }

            GearEngine.GearEngine.Presentation.UI.Tags.Highlight.TutorialFocusService focusService = GearEngine.GearEngine.Presentation.UI.Tags.Highlight.TutorialFocusService.Instance;
            if (focusService == null)
            {
                Debug.LogError("[ShowUIFocus] TutorialFocusService.Instance is null.");
                Continue();
                return;
            }

            List<TagComponent> matchingTags = new List<TagComponent>();
            RectTransform targetRect = null;

            if (target.strategy == TargetResolutionStrategy.Tags && target.tagFilter.soTags.Count > 0)
            {
                foreach (TagComponent comp in TagComponent.Instances)
                {
                    if (IsTargetMatch(target, comp.gameObject))
                    {
                        matchingTags.Add(comp);
                    }
                }

                if (matchingTags.Count == 0 || targetIndex >= matchingTags.Count)
                {
                    Debug.LogWarning($"[ShowUIFocus] No valid target found for the specified TargetReference at index {targetIndex}.");
                    Continue();
                    return;
                }

                matchingTags.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                targetRect = matchingTags[targetIndex].GetComponent<RectTransform>();
            }
            else
            {
                GameObject resolvedTarget = ResolveTarget(target);
                if (resolvedTarget != null)
                {
                    targetRect = resolvedTarget.GetComponent<RectTransform>();
                }
                else
                {
                    Debug.LogWarning($"[ShowUIFocus] Could not resolve target using the specified TargetReference.");
                    Continue();
                    return;
                }
            }

            if (targetRect == null)
            {
                Debug.LogError("[ShowUIFocus] Target does not have a RectTransform.");
                Continue();
                return;
            }

            GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor anchor = overridePresetLayout ? indicatorAnchor : preset.indicatorAnchor;
            Vector2 customAnchor = overridePresetLayout ? customIndicatorAnchor : preset.customIndicatorAnchor;
            Vector2 offset = overridePresetLayout ? indicatorOffset : preset.indicatorOffset;
            float dirOffset = overridePresetLayout ? directionOffset : preset.directionOffset;
            bool aim = overridePresetLayout ? aimToAnchor : preset.aimToAnchor;

            focusService.FocusOn(targetRect, preset, anchor, customAnchor, offset, dirOffset, aim);

            Continue();
        }

        public override string GetSummary()
        {
            if (preset == null)
            {
                return "Error: No Preset";
            }

            return $"Focus using {preset.name}";
        }

        public override Color GetButtonColor()
        {
            return new Color32(255, 204, 153, 255);
        }

        public override bool IsPropertyVisible(string propertyName)
        {
            if (propertyName == nameof(targetIndex))
            {
                return target != null && target.strategy == TargetResolutionStrategy.Tags;
            }
            if (propertyName == nameof(indicatorOffset) ||
                propertyName == nameof(indicatorAnchor) ||
                propertyName == nameof(aimToAnchor) ||
                propertyName == nameof(directionOffset))
            {
                return overridePresetLayout;
            }
            if (propertyName == nameof(customIndicatorAnchor))
            {
                return overridePresetLayout && indicatorAnchor == GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor.Custom;
            }
            return base.IsPropertyVisible(propertyName);
        }
    }
}
