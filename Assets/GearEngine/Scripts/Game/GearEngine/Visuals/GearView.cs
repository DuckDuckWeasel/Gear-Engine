using System;
using GearEngine.GearEngine.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace GearEngine.GearEngine.Visuals
{
    public class GearView : ItemView, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Transform gearVisual;

        [SerializeField]
        private Image gearImage;

        [SerializeField]
        private Image chargeFillImage;

        [SerializeField]
        private float rotationLerpSpeed = 15f;

        [SerializeField]
        private float fillLerpSpeed = 10f;

        [SerializeField]
        private float settleLerpSpeed = 20f;

        private float targetRotationZ;
        private float currentVisualFill;
        private float targetVisualFill = -1f;
        private Material chargeMaterialInstance;

        private float animatedBaseRotationZ;
        private float visualRotationOffset;
        private float rapidSpinSpeed = 0f;
        private bool hasInitializedRotation = false;
        private float baseScale = 1f;
        private float rapidSpinScalePhase = 0f;
        private float rapidSpinOffset = 0f;

        public event Action OnClicked;

        internal void WireTestReferences(Transform gearVisualRef, Image gearImageRef = null, Image chargeImageRef = null)
        {
            gearVisual = gearVisualRef;
            gearImage = gearImageRef;
            chargeFillImage = chargeImageRef;
        }

        public void ApplyConfig(GearItemData config)
        {
            if (config == null)
            {
                return;
            }

            ApplyScale(config.RelativeScaleMultiplier);
            ApplyIcon(config.UIIcon);
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

        public void SetChargeFillTarget(float normalized01, bool snap = false)
        {
            targetVisualFill = Mathf.Clamp01(normalized01);
            if (snap)
            {
                currentVisualFill = targetVisualFill;
                ApplyChargeFill();
            }
        }

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
            if (transform is RectTransform rect)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
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
            SettleToOrigin();
            animatedBaseRotationZ = Mathf.LerpAngle(animatedBaseRotationZ, targetRotationZ, Time.deltaTime * rotationLerpSpeed);
            UpdateRapidSpin();
            ApplyRotation();
            UpdateChargeFill();
        }

        private void ApplyScale(float scale)
        {
            if (gearVisual == null)
            {
                return;
            }
            baseScale = scale;
            gearVisual.localScale = new Vector3(baseScale, baseScale, baseScale);
        }

        private void ApplyIcon(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }
            if (chargeFillImage != null)
            {
                chargeFillImage.sprite = sprite;
            }
        }

        private void SettleToOrigin()
        {
            if (transform is RectTransform rect)
            {
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, Vector2.zero, Time.deltaTime * settleLerpSpeed);
            }
        }

        private void UpdateRapidSpin()
        {
            if (rapidSpinSpeed != 0f)
            {
                ApplyRapidSpin();
                return;
            }
            RecoverRapidSpinOffset();
            RecoverRapidSpinScale();
        }

        private void ApplyRapidSpin()
        {
            rapidSpinOffset -= rapidSpinSpeed * Time.deltaTime;
            rapidSpinScalePhase += Time.deltaTime * 25f;
            float scaleMod = 1f + (Mathf.Sin(rapidSpinScalePhase) * 0.05f);
            if (gearVisual != null)
            {
                gearVisual.localScale = Vector3.one * (baseScale * scaleMod);
            }
        }

        private void RecoverRapidSpinOffset()
        {
            if (rapidSpinOffset == 0f)
            {
                return;
            }
            float currentMod = rapidSpinOffset % 360f;
            currentMod = currentMod > 0f ? currentMod - 360f : currentMod;
            bool isSettled = Mathf.Abs(currentMod) < 0.5f || Mathf.Abs(currentMod + 360f) < 0.5f;
            rapidSpinOffset = isSettled ? 0f : Mathf.Lerp(currentMod, -360f, Time.deltaTime * 15f);
        }

        private void RecoverRapidSpinScale()
        {
            if (gearVisual == null || Mathf.Abs(gearVisual.localScale.x - baseScale) <= 0.001f)
            {
                return;
            }
            float currentScale = Mathf.Lerp(gearVisual.localScale.x, baseScale, Time.deltaTime * 15f);
            gearVisual.localScale = Vector3.one * currentScale;
        }

        private void ApplyRotation()
        {
            Transform rotateTarget = gearVisual != null ? gearVisual : transform;
            float rotation = animatedBaseRotationZ + visualRotationOffset + rapidSpinOffset;
            rotateTarget.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private void UpdateChargeFill()
        {
            if (targetVisualFill < 0f || chargeFillImage == null)
            {
                return;
            }
            currentVisualFill = Mathf.Lerp(currentVisualFill, targetVisualFill, Time.deltaTime * fillLerpSpeed);
            ApplyChargeFill();
        }

        private void ApplyChargeFill()
        {
            if (chargeFillImage == null)
            {
                return;
            }
            EnsureChargeMaterial();
            if (chargeMaterialInstance == null)
            {
                return;
            }
            chargeMaterialInstance.SetFloat("_FillAmount", currentVisualFill);
            chargeFillImage.SetMaterialDirty();
        }

        private void EnsureChargeMaterial()
        {
            if (chargeMaterialInstance != null)
            {
                return;
            }
            Material sourceMaterial = chargeFillImage.material;
            if (sourceMaterial == null)
            {
                return;
            }
            chargeMaterialInstance = new Material(sourceMaterial) { name = $"{sourceMaterial.name}_Instance" };
            chargeFillImage.material = chargeMaterialInstance;
        }

        private void OnDestroy()
        {
            if (chargeMaterialInstance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(chargeMaterialInstance);
            }
            else
            {
                DestroyImmediate(chargeMaterialInstance);
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
