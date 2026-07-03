using System;
using GearEngine.GearEngine.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace GearEngine.GearEngine.Visuals
{
    /// <summary>
    /// Board-agnostic gear visual. Board-side BoardGearAnimator pushes rotation, charge fill, and reparent;
    /// inventory only calls <see cref="ApplyConfig"/> and optional fill preview.
    /// </summary>
    public class GearView : ItemView, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event Action OnClicked;
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
        private MaterialPropertyBlock propertyBlock;

        private float animatedBaseRotationZ;
        private float visualRotationOffset;
        private float rapidSpinSpeed = 0f;
        private bool hasInitializedRotation = false;

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

        public void SetRotationTarget(float zDegrees, bool snap = false)
        {
            targetRotationZ = zDegrees;
            if (!hasInitializedRotation || snap)
            {
                animatedBaseRotationZ = zDegrees;
                hasInitializedRotation = true;
            }
        }

        /// <param name="normalized01">Fill amount 0..1.</param>
        /// <param name="snap">If true, applies immediately (no lerp).</param>
        public void SetChargeFillTarget(float normalized01, bool snap = false)
        {
            targetVisualFill = Mathf.Clamp01(normalized01);
            if (snap)
            {
                currentVisualFill = targetVisualFill;
                if (chargeFillRenderer != null)
                {
                    if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
                    chargeFillRenderer.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetFloat("_FillAmount", currentVisualFill);
                    chargeFillRenderer.SetPropertyBlock(propertyBlock);
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

        public void PlayChargeCompleteFeedback()
        {
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }

        public void SpinOnceVisual(float duration = 0.5f)
        {
            DOTween.To(() => visualRotationOffset, x => visualRotationOffset = x, visualRotationOffset - 360f, duration)
                .SetEase(Ease.OutQuad);
        }

        public void SetRapidSpin(bool enabled, float speed = 1500f)
        {
            rapidSpinSpeed = enabled ? speed : 0f;
        }

        private void Update()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * settleLerpSpeed);

            Transform rotateTarget = gearVisual != null ? gearVisual : transform;
            
            animatedBaseRotationZ = Mathf.LerpAngle(animatedBaseRotationZ, targetRotationZ, Time.deltaTime * rotationLerpSpeed);

            if (rapidSpinSpeed != 0f)
            {
                visualRotationOffset -= rapidSpinSpeed * Time.deltaTime;
            }

            rotateTarget.localRotation = Quaternion.Euler(0, 0, animatedBaseRotationZ + visualRotationOffset);

            if (targetVisualFill >= 0f && chargeFillRenderer != null)
            {
                currentVisualFill = Mathf.Lerp(currentVisualFill, targetVisualFill, Time.deltaTime * fillLerpSpeed);
                
                if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
                chargeFillRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_FillAmount", currentVisualFill);
                chargeFillRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[GearView] OnPointerClick fired! dragging={eventData.dragging}");
            if (!eventData.dragging)
            {
                OnClicked?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData) { }
        public void OnPointerUp(PointerEventData eventData) { }
    }
}
