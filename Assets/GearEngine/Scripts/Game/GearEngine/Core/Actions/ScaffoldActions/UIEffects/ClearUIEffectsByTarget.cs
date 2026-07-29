using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using UnityEngine;
using GearEngine.Core.Actions;
using GearEngine.Core.Architecture.References;
using TriInspector;
using GearEngine.GearEngine.Presentation.UI.Tags;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Clear UI Effects By Target", "Finds all UIEffect components on objects matching a TargetReference and clears them.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ClearUIEffectsByTarget : ActionBase
    {
        [Tooltip("The target UI element(s) whose effects should be cleared. Use Tags or Runtime Anchors.")]
        [SerializeField]
        private TargetReference target = new TargetReference();

        [Tooltip("If true, clears the effect from ALL objects that match the tag. If false, clears only from the first matching object.")]
        [SerializeField]
        private bool clearAllMatching = true;

        public override void OnEnter()
        {
            if (target.strategy == TargetResolutionStrategy.Tags && target.tagFilter.soTags.Count > 0)
            {
                foreach (TagComponent comp in TagComponent.Instances)
                {
                    if (IsTargetMatch(target, comp.gameObject))
                    {
                        ClearEffectOnGameObject(comp.gameObject);

                        if (!clearAllMatching)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                GameObject resolvedTarget = ResolveTarget(target);
                if (resolvedTarget != null)
                {
                    ClearEffectOnGameObject(resolvedTarget);
                }
                else
                {
                    Debug.LogWarning($"[ClearUIEffectsByTarget] Could not resolve target using the specified TargetReference.");
                }
            }

            Continue();
        }

        private void ClearEffectOnGameObject(GameObject go)
        {
            UIEffect effect = go.GetComponent<UIEffect>();
            if (effect != null)
            {
                effect.Clear();
            }
        }

        public override string GetSummary()
        {
            return "Clear UI Effects on Target";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool IsPropertyVisible(string propertyName)
        {
            if (propertyName == nameof(clearAllMatching))
            {
                return target != null && target.strategy == TargetResolutionStrategy.Tags;
            }
            return base.IsPropertyVisible(propertyName);
        }
    }
}
