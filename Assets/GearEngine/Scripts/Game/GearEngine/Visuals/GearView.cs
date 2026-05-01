using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.GearEngine.Visuals
{
    /// <summary>
    /// Board-agnostic gear visual. Board-side BoardGearAnimator pushes rotation, charge fill, and reparent;
    /// inventory only calls <see cref="ApplyConfig"/> and optional fill preview.
    /// </summary>
    public class GearView : ItemView
    {
        [SerializeField]
        private Transform gearVisual;

        [SerializeField]
        private SpriteRenderer chargeFillRenderer;

        [SerializeField]
        private float rotationLerpSpeed = 15f;

        [SerializeField]
        private float fillLerpSpeed = 10f;

        [SerializeField]
        private float settleLerpSpeed = 20f;

        private float targetRotationZ;
        private float currentVisualFill;
        private float targetVisualFill = -1f;

        /// <summary>Editor tests: assigns serialized references without a prefab asset.</summary>
        internal void WireTestReferences(Transform gearVisualRef, SpriteRenderer chargeRef = null)
        {
            gearVisual = gearVisualRef;
            chargeFillRenderer = chargeRef;
        }

        public void ApplyConfig(GearItemData config)
        {
            if (config == null)
            {
                return;
            }

            if (gearVisual != null)
            {
                float s = config.RelativeScaleMultiplier;
                gearVisual.localScale = new Vector3(s, s, s);
            }

            if (config.UIIcon != null && chargeFillRenderer != null)
            {
                chargeFillRenderer.sprite = config.UIIcon;
            }
        }

        public void SetRotationTarget(float zDegrees)
        {
            targetRotationZ = zDegrees;
        }

        /// <param name="normalized01">Fill amount 0..1.</param>
        /// <param name="snap">If true, applies immediately (no lerp).</param>
        public void SetChargeFillTarget(float normalized01, bool snap = false)
        {
            targetVisualFill = Mathf.Clamp01(normalized01);
            if (snap)
            {
                currentVisualFill = targetVisualFill;
                if (chargeFillRenderer != null && chargeFillRenderer.material != null)
                {
                    chargeFillRenderer.material.SetFloat("_FillAmount", currentVisualFill);
                }
            }
        }

        /// <summary>Clears charge fill driving so <see cref="Update"/> does not write the material.</summary>
        public void ClearChargeFillTarget()
        {
            targetVisualFill = -1f;
        }

        public void SetReparent(Transform parent)
        {
            if (parent != null && parent != transform.parent)
            {
                transform.SetParent(parent, true);
            }
        }

        public void SettleNow()
        {
            transform.localPosition = Vector3.zero;
        }

        private void Update()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * settleLerpSpeed);

            Transform rotateTarget = gearVisual != null ? gearVisual : transform;
            Quaternion target = Quaternion.Euler(0, 0, targetRotationZ);
            rotateTarget.localRotation = Quaternion.Lerp(rotateTarget.localRotation, target, Time.deltaTime * rotationLerpSpeed);

            if (targetVisualFill >= 0f && chargeFillRenderer != null && chargeFillRenderer.material != null)
            {
                currentVisualFill = Mathf.Lerp(currentVisualFill, targetVisualFill, Time.deltaTime * fillLerpSpeed);
                chargeFillRenderer.material.SetFloat("_FillAmount", currentVisualFill);
            }
        }
    }
}
