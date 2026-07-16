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
    public class ShowUIFocusAction : IAction
    {
        [Group("Target")]
        [Tooltip("The target UI element to focus on. Use Tags or Runtime Anchors.")]
        [SerializeField] 
        private TargetReference _target = new TargetReference();

        [Group("Target")]
        [Tooltip("If multiple objects have this tag, pick the one at this index. Default is 0. (Only applies if using Tags strategy)")]
        [SerializeField] 
        private int _targetIndex = 0;

        [Group("Preset")]
        [Tooltip("The visual preset to use for the focus overlay and effects.")]
        [SerializeField] 
        private FocusPresetSO _preset;

        [Group("Overrides")]
        [Tooltip("If true, overrides the layout properties (anchor, offset, etc.) defined in the FocusPresetSO.")]
        [SerializeField]
        private bool _overridePresetLayout = false;

        [Group("Overrides")]
        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("Offset of the indicator relative to the anchor point on the target bounds.")]
        [SerializeField] 
        private Vector2 _indicatorOffset = Vector2.zero;

        [Group("Overrides")]
        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("Anchor point on the target UI bounds.")]
        [SerializeField] 
        private IndicatorAnchor _indicatorAnchor = IndicatorAnchor.TopCenter;

        [Group("Overrides")]
        [ShowIf(nameof(ShowCustomAnchor))]
        [Tooltip("Used only if IndicatorAnchor is set to Custom (e.g. 0.5, 1.0 is Top Center).")]
        [SerializeField]
        private Vector2 _customIndicatorAnchor = new Vector2(0.5f, 1f);

        [Group("Overrides")]
        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("If true, rotates the indicator to aim precisely at the anchor center.")]
        [SerializeField]
        private bool _aimToAnchor = true;

        [Group("Overrides")]
        [ShowIf(nameof(_overridePresetLayout))]
        [Tooltip("Pushes the indicator towards the anchor (if negative) or away from the anchor (if positive) along the normal direction of the anchor.")]
        [SerializeField]
        private float _directionOffset = 0f;

        private bool ShowCustomAnchor => _overridePresetLayout && _indicatorAnchor == IndicatorAnchor.Custom;

        public void Execute(System.Action onComplete)
        {
            if (_preset == null)
            {
                Debug.LogError("[ShowUIFocusAction] Preset is missing.");
                onComplete?.Invoke();
                return;
            }

            var focusService = TutorialFocusService.Instance;
            if (focusService == null)
            {
                Debug.LogError("[ShowUIFocusAction] TutorialFocusService.Instance is null.");
                onComplete?.Invoke();
                return;
            }

            List<TagComponent> matchingTags = new List<TagComponent>();
            RectTransform targetRect = null;

            if (_target.strategy == TargetResolutionStrategy.Tags && _target.tagFilter.soTags.Count > 0)
            {
                TagComponent[] allComponents = UnityEngine.Object.FindObjectsOfType<TagComponent>();
                
                foreach (TagComponent comp in allComponents)
                {
                    if (_target.IsMatch(comp.gameObject))
                    {
                        matchingTags.Add(comp);
                    }
                }

                if (matchingTags.Count == 0 || _targetIndex >= matchingTags.Count)
                {
                    Debug.LogWarning($"[ShowUIFocusAction] No valid target found for the specified TargetReference at index {_targetIndex}.");
                    onComplete?.Invoke();
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
                    Debug.LogWarning($"[ShowUIFocusAction] Could not resolve target using the specified TargetReference.");
                    onComplete?.Invoke();
                    return;
                }
            }

            if (targetRect == null)
            {
                Debug.LogError("[ShowUIFocusAction] Target does not have a RectTransform.");
                onComplete?.Invoke();
                return;
            }

            IndicatorAnchor anchor = _overridePresetLayout ? _indicatorAnchor : _preset.indicatorAnchor;
            Vector2 customAnchor = _overridePresetLayout ? _customIndicatorAnchor : _preset.customIndicatorAnchor;
            Vector2 offset = _overridePresetLayout ? _indicatorOffset : _preset.indicatorOffset;
            float dirOffset = _overridePresetLayout ? _directionOffset : _preset.directionOffset;
            bool aim = _overridePresetLayout ? _aimToAnchor : _preset.aimToAnchor;

            focusService.FocusOn(targetRect, _preset, anchor, customAnchor, offset, dirOffset, aim);

            onComplete?.Invoke();
        }
    }
}
