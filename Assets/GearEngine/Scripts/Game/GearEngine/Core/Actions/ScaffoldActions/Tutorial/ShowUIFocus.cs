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
        private TargetReference _target = new TargetReference();

        [Tooltip("If multiple objects have this tag, pick the one at this index. Default is 0.")]
        [SerializeField]
        private int _targetIndex = 0;

        [Tooltip("The visual preset to use for the focus overlay and effects.")]
        [SerializeField, InlineEditor]
        private GearEngine.GearEngine.Presentation.UI.Tags.Highlight.FocusPresetSO _preset;

        [Header("Layout Overrides")]
        [Tooltip("If true, overrides the layout properties defined in the FocusPresetSO.")]
        [SerializeField]
        private bool _overridePresetLayout = false;

        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("Anchor point on the target UI bounds.")]
        [SerializeField]
        private GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor _indicatorAnchor = GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor.TopCenter;

        [ShowIf(nameof(ShowCustomAnchor))]
        [Tooltip("Used only if IndicatorAnchor is set to Custom.")]
        [SerializeField]
        private Vector2 _customIndicatorAnchor = new Vector2(0.5f, 1f);

        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("Offset of the indicator relative to the anchor point on the target bounds.")]
        [SerializeField]
        private Vector2 _indicatorOffset = Vector2.zero;

        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("Pushes the indicator towards/away from the anchor.")]
        [SerializeField]
        private float _directionOffset = 0f;

        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("If true, rotates the indicator to aim precisely at the anchor center.")]
        [SerializeField]
        private bool _aimToAnchor = true;

        [Tooltip("The Show custom anchor")]
        private bool ShowCustomAnchor => _overridePresetLayout && _indicatorAnchor == GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor.Custom;

        public override void OnEnter()
        {
            if (_preset == null)
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

            if (_target.strategy == TargetResolutionStrategy.Tags && _target.tagFilter.soTags.Count > 0)
            {
                foreach (TagComponent comp in TagComponent.Instances)
                {
                    if (_target.IsMatch(comp.gameObject))
                    {
                        matchingTags.Add(comp);
                    }
                }

                if (matchingTags.Count == 0 || _targetIndex >= matchingTags.Count)
                {
                    Debug.LogWarning($"[ShowUIFocus] No valid target found for the specified TargetReference at index {_targetIndex}.");
                    Continue();
                    return;
                }

                matchingTags.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                targetRect = matchingTags[_targetIndex].GetComponent<RectTransform>();
            }
            else
            {
                GameObject resolvedTarget = _target.Resolve();
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

            GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor anchor = _overridePresetLayout ? _indicatorAnchor : _preset.indicatorAnchor;
            Vector2 customAnchor = _overridePresetLayout ? _customIndicatorAnchor : _preset.customIndicatorAnchor;
            Vector2 offset = _overridePresetLayout ? _indicatorOffset : _preset.indicatorOffset;
            float dirOffset = _overridePresetLayout ? _directionOffset : _preset.directionOffset;
            bool aim = _overridePresetLayout ? _aimToAnchor : _preset.aimToAnchor;

            focusService.FocusOn(targetRect, _preset, anchor, customAnchor, offset, dirOffset, aim);

            Continue();
        }

        public override string GetSummary()
        {
            if (_preset == null)
            {
                return "Error: No Preset";
            }

            return $"Focus using {_preset.name}";
        }

        public override Color GetButtonColor()
        {
            return new Color32(255, 204, 153, 255);
        }

        public override bool IsPropertyVisible(string propertyName)
        {
            if (propertyName == "_targetIndex")
            {
                return _target != null && _target.strategy == TargetResolutionStrategy.Tags;
            }
            if (propertyName == "_indicatorOffset" ||
                propertyName == "_indicatorAnchor" ||
                propertyName == "_aimToAnchor" ||
                propertyName == "_directionOffset")
            {
                return _overridePresetLayout;
            }
            if (propertyName == "_customIndicatorAnchor")
            {
                return _overridePresetLayout && _indicatorAnchor == GearEngine.GearEngine.Presentation.UI.Tags.Highlight.IndicatorAnchor.Custom;
            }
            return base.IsPropertyVisible(propertyName);
        }
    }
}
