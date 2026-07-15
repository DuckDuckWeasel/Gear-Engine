using Fungus;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Command = Fungus.Command;
using System.Linq;
using GearEngine.GearEngine.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    [CommandInfo("Tutorial", "Show UI Focus", "Highlights a UI object by bringing it in front of a dark overlay and showing indicators.")]
    public class ShowUIFocusCommand : Command
    {
        [Inject] 
        private TutorialFocusService _focusService;

        [Tooltip("The tag of the UI element to focus on.")]
        [SerializeField] 
        private TagSO _targetTag;

        [Tooltip("The visual preset to use for the focus overlay and effects.")]
        [SerializeField] 
        private FocusPresetSO _preset;

        [Tooltip("If true, overrides the layout properties (anchor, offset, etc.) defined in the FocusPresetSO.")]
        [SerializeField]
        private bool _overridePresetLayout = false;

        [Tooltip("Offset of the indicator relative to the anchor point on the target bounds.")]
        [SerializeField] 
        private Vector2 _indicatorOffset = Vector2.zero;

        [Tooltip("Anchor point on the target UI bounds.")]
        [SerializeField] 
        private IndicatorAnchor _indicatorAnchor = IndicatorAnchor.TopCenter;

        [Tooltip("Used only if IndicatorAnchor is set to Custom (e.g. 0.5, 1.0 is Top Center).")]
        [SerializeField]
        private Vector2 _customIndicatorAnchor = new Vector2(0.5f, 1f);

        [Tooltip("If true, rotates the indicator to aim precisely at the anchor center.")]
        [SerializeField]
        private bool _aimToAnchor = true;

        [Tooltip("Pushes the indicator towards the anchor (if negative) or away from the anchor (if positive) along the normal direction of the anchor.")]
        [SerializeField]
        private float _directionOffset = 0f;

        [Tooltip("If multiple objects have this tag, pick the one at this index. Default is 0.")]
        [SerializeField] 
        private int _targetIndex = 0;

#if UNITY_EDITOR
        /// <summary>
        /// Hides fields that only apply to specific configurations
        /// (drawn by Fungus's CommandEditor, which honors this hook).
        /// </summary>
        public override bool IsPropertyVisible(string propertyName)
        {
            if (propertyName == "_indicatorOffset" || 
                propertyName == "_indicatorAnchor" || 
                propertyName == "_customIndicatorAnchor" || 
                propertyName == "_aimToAnchor" || 
                propertyName == "_directionOffset")
            {
                if (!_overridePresetLayout) return false;
            }

            if (propertyName == "_customIndicatorAnchor")
            {
                return _indicatorAnchor == IndicatorAnchor.Custom;
            }

            return base.IsPropertyVisible(propertyName);
        }
#endif
        public override void OnEnter()
        {
            if (_targetTag == null || _preset == null)
            {
                Debug.LogError("[ShowUIFocusCommand] TargetTag or Preset is missing.");
                Continue();
                return;
            }

            // Using Singleton to guarantee a single Canvas across all commands
            if (_focusService == null)
            {
                _focusService = TutorialFocusService.Instance;
            }

            TagComponent[] allComponents = FindObjectsOfType<TagComponent>();
            List<TagComponent> matchingTags = new List<TagComponent>();
            
            foreach (TagComponent comp in allComponents)
            {
                if (comp.HasTag(_targetTag))
                {
                    matchingTags.Add(comp);
                }
            }

            if (matchingTags.Count == 0 || _targetIndex >= matchingTags.Count)
            {
                Debug.LogWarning($"[ShowUIFocusCommand] No valid target found for tag {_targetTag.name} at index {_targetIndex}.");
                Continue();
                return;
            }

            // Sort by hierarchy to be deterministic
            matchingTags.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            RectTransform targetRect = matchingTags[_targetIndex].GetComponent<RectTransform>();
            if (targetRect == null)
            {
                Debug.LogError("[ShowUIFocusCommand] Target does not have a RectTransform.");
                Continue();
                return;
            }

            IndicatorAnchor anchor = _overridePresetLayout ? _indicatorAnchor : _preset.indicatorAnchor;
            Vector2 customAnchor = _overridePresetLayout ? _customIndicatorAnchor : _preset.customIndicatorAnchor;
            Vector2 offset = _overridePresetLayout ? _indicatorOffset : _preset.indicatorOffset;
            float dirOffset = _overridePresetLayout ? _directionOffset : _preset.directionOffset;
            bool aim = _overridePresetLayout ? _aimToAnchor : _preset.aimToAnchor;

            _focusService.FocusOn(targetRect, _preset, anchor, customAnchor, offset, dirOffset, aim);

            Continue();
        }

        public override string GetSummary()
        {
            if (_targetTag == null)
                return "Error: No Tag";
            return _targetTag.name;
        }
    }
}
