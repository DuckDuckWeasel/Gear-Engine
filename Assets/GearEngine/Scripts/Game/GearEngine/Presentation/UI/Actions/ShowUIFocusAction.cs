using System;
using System.Collections.Generic;
using UnityEngine;
using GearEngine.Core.Actions;
using GearEngine.Core.Architecture.References;
using TriInspector;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    [Serializable]
    [DeclareBoxGroup("Target")]
    [DeclareBoxGroup("Preset")]
    [DeclareBoxGroup("Overrides")]
    public class ShowUIFocusAction : ActionBase
    {
        private bool ShowCustomAnchor => overridePresetLayout && indicatorAnchor == IndicatorAnchor.Custom;

        [Group("Target")]
        [Tooltip("The target UI element to focus on. Use Tags or Runtime Anchors.")]
        [SerializeField]
        private TargetReference target = new TargetReference();

        [Group("Target")]
        [Tooltip("If multiple objects have this tag, pick the one at this index. Default is 0. (Only applies if using Tags strategy)")]
        [SerializeField]
        private int targetIndex = 0;

        [Group("Preset")]
        [Tooltip("The visual preset to use for the focus overlay and effects.")]
        [SerializeField]
        private FocusPresetSO preset;

        [Group("Overrides")]
        [Tooltip("If true, overrides the layout properties (anchor, offset, etc.) defined in the FocusPresetSO.")]
        [SerializeField]
        private bool overridePresetLayout = false;

        [Group("Overrides")]
        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("Offset of the indicator relative to the anchor point on the target bounds.")]
        [SerializeField]
        private Vector2 indicatorOffset = Vector2.zero;

        [Group("Overrides")]
        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("Anchor point on the target UI bounds.")]
        [SerializeField]
        private IndicatorAnchor indicatorAnchor = IndicatorAnchor.TopCenter;

        [Group("Overrides")]
        [ShowIf(nameof(ShowCustomAnchor))]
        [Tooltip("Used only if IndicatorAnchor is set to Custom (e.g. 0.5, 1.0 is Top Center).")]
        [SerializeField]
        private Vector2 customIndicatorAnchor = new Vector2(0.5f, 1f);

        [Group("Overrides")]
        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("If true, rotates the indicator to aim precisely at the anchor center.")]
        [SerializeField]
        private bool aimToAnchor = true;

        [Group("Overrides")]
        [ShowIf(nameof(overridePresetLayout))]
        [Tooltip("Pushes the indicator towards the anchor (if negative) or away from the anchor (if positive) along the normal direction of the anchor.")]
        [SerializeField]
        private float directionOffset = 0f;

        public override void OnEnter()
        {
            if (!TryResolveFocusService(out TutorialFocusService focusService))
            {
                return;
            }

            RectTransform targetRect = ResolveTargetRect();
            if (!ValidateTargetRect(targetRect))
            {
                return;
            }

            ApplyFocus(focusService, targetRect);
            Continue();
        }

        private bool TryResolveFocusService(out TutorialFocusService focusService)
        {
            focusService = TutorialFocusService.Instance;
            if (preset != null && focusService != null)
            {
                return true;
            }

            string message = preset == null ? "Preset is missing." : "TutorialFocusService.Instance is null.";
            Debug.LogError($"[ShowUIFocusAction] {message}");
            Fail();
            return false;
        }

        private RectTransform ResolveTargetRect()
        {
            if (target.strategy == TargetResolutionStrategy.Tags && target.tagFilter.soTags.Count > 0)
            {
                return ResolveTaggedTarget();
            }

            return ResolveDirectTarget();
        }

        private RectTransform ResolveTaggedTarget()
        {
            List<TagComponent> matchingTags = GetMatchingTags();
            if (matchingTags.Count == 0 || targetIndex >= matchingTags.Count)
            {
                Debug.LogWarning($"[ShowUIFocusAction] No valid target found for the specified TargetReference at index {targetIndex}.");
                Fail();
                return null;
            }

            matchingTags.Sort((first, second) => first.transform.GetSiblingIndex().CompareTo(second.transform.GetSiblingIndex()));
            return matchingTags[targetIndex].GetComponent<RectTransform>();
        }

        private List<TagComponent> GetMatchingTags()
        {
            List<TagComponent> matchingTags = new List<TagComponent>();
            TagComponent[] allComponents = UnityEngine.Object.FindObjectsOfType<TagComponent>();
            foreach (TagComponent component in allComponents)
            {
                if (target.IsMatch(component.gameObject))
                {
                    matchingTags.Add(component);
                }
            }

            return matchingTags;
        }

        private RectTransform ResolveDirectTarget()
        {
            GameObject resolvedTarget = target.Resolve();
            if (resolvedTarget != null)
            {
                return resolvedTarget.GetComponent<RectTransform>();
            }

            Debug.LogWarning("[ShowUIFocusAction] Could not resolve target using the specified TargetReference.");
            Fail();
            return null;
        }

        private bool ValidateTargetRect(RectTransform targetRect)
        {
            if (targetRect == null)
            {
                Debug.LogError("[ShowUIFocusAction] Target does not have a RectTransform.");
                Fail();
                return false;
            }

            return true;
        }

        private void ApplyFocus(TutorialFocusService focusService, RectTransform targetRect)
        {
            IndicatorAnchor anchor = overridePresetLayout ? indicatorAnchor : preset.indicatorAnchor;
            Vector2 customAnchor = overridePresetLayout ? customIndicatorAnchor : preset.customIndicatorAnchor;
            Vector2 offset = overridePresetLayout ? indicatorOffset : preset.indicatorOffset;
            float dirOffset = overridePresetLayout ? directionOffset : preset.directionOffset;
            bool aim = overridePresetLayout ? aimToAnchor : preset.aimToAnchor;
            focusService.FocusOn(targetRect, preset, anchor, customAnchor, offset, dirOffset, aim);
        }
    }
}
