using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Makes a sprite visible / invisible by setting the color alpha.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Show Sprite", 
                 "Makes a sprite visible / invisible by setting the color alpha.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class ShowSprite : ActionBase
    {
        [Tooltip("Sprite object to be made visible / invisible")]
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Tooltip("Make the sprite visible or invisible")]
        [SerializeField] protected BooleanData visible = new BooleanData(false);

        [Tooltip("Affect the visibility of child sprites")]
        [SerializeField] protected bool affectChildren = true;

        protected virtual void SetSpriteAlpha(SpriteRenderer renderer, bool visible)
        {
            Color spriteColor = renderer.color;
            spriteColor.a = visible ? 1f : 0f;
            renderer.color = spriteColor;
        }

        #region Public members

        public override void OnEnter()
        {
            if (spriteRenderer != null)
            {
                if (affectChildren)
                {
                    var spriteRenderers = spriteRenderer.gameObject.gameObject.GetComponentsInChildren<SpriteRenderer>();
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        var sr = spriteRenderers[i];
                        SetSpriteAlpha(sr, visible.Value);
                    }
                }
                else
                {
                    SetSpriteAlpha(spriteRenderer, visible.Value);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (spriteRenderer == null)
            {
                return "Error: No sprite renderer selected";
            }

            return spriteRenderer.name + " to " + (visible.Value ? "visible" : "invisible");
        }

        public override Color GetButtonColor()
        {
            return new Color32(221, 184, 169, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return visible.booleanRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("visible")] public bool visibleOLD;

        protected virtual void OnEnable()
        {
            if (visibleOLD != default(bool))
            {
                visible.Value = visibleOLD;
                visibleOLD = default(bool);
            }
        }

        #endregion
    }
}