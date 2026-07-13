using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using System.Collections.Generic;
using Coffee.UIEffects;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    public class TutorialFocusService : IInitializable
    {
        private GameObject _focusCanvasGo;
        private Canvas _focusCanvas;
        private Image _darkOverlay;
        
        private List<Component> _addedOverrideComponents = new List<Component>();
        private GameObject _currentIndicator;
        private UIEffect _currentUIEffect;

        public void Initialize()
        {
            _focusCanvasGo = new GameObject("TutorialFocusCanvas");
            UnityEngine.Object.DontDestroyOnLoad(_focusCanvasGo);
            
            _focusCanvas = _focusCanvasGo.AddComponent<Canvas>();
            _focusCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _focusCanvas.sortingOrder = 30000;
            
            _focusCanvasGo.AddComponent<GraphicRaycaster>();
            
            GameObject overlayGo = new GameObject("DarkOverlay");
            overlayGo.transform.SetParent(_focusCanvasGo.transform, false);
            _darkOverlay = overlayGo.AddComponent<Image>();
            
            RectTransform rect = _darkOverlay.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            _focusCanvasGo.SetActive(false);
        }

        public void FocusOn(RectTransform target, FocusPresetSO preset, IndicatorAnchor anchor, Vector2 customAnchor, Vector2 positionOffset, float directionOffset, bool aimToAnchor)
        {
            ClearFocus();

            if (target == null)
            {
                Debug.LogError("[TutorialFocusService] Target RectTransform is null.");
                return;
            }

            // Setup Dark Overlay
            if (preset.useDarkOverlay)
            {
                _darkOverlay.color = preset.overlayColor;
                _darkOverlay.raycastTarget = preset.blockClicksOutside;
                _focusCanvasGo.SetActive(true);
            }

            // Override Sorting on Target
            Canvas targetCanvas = target.GetComponent<Canvas>();
            if (targetCanvas == null)
            {
                targetCanvas = target.gameObject.AddComponent<Canvas>();
                _addedOverrideComponents.Add(targetCanvas);
            }
            
            targetCanvas.overrideSorting = true;
            targetCanvas.sortingOrder = _focusCanvas.sortingOrder + 1;

            GraphicRaycaster targetRaycaster = target.GetComponent<GraphicRaycaster>();
            if (targetRaycaster == null)
            {
                targetRaycaster = target.gameObject.AddComponent<GraphicRaycaster>();
                _addedOverrideComponents.Add(targetRaycaster);
            }

            // Spawn Indicator
            if (preset.indicatorPrefab != null)
            {
                _currentIndicator = UnityEngine.Object.Instantiate(preset.indicatorPrefab, _focusCanvasGo.transform);
                RectTransform indRect = _currentIndicator.GetComponent<RectTransform>();
                if (indRect != null)
                {
                    Vector3[] corners = new Vector3[4];
                    target.GetWorldCorners(corners);
                    // corners: 0 = BottomLeft, 1 = TopLeft, 2 = TopRight, 3 = BottomRight
                    Vector3 bl = corners[0];
                    Vector3 tr = corners[2];
                    
                    Vector2 anchorVec = GetAnchorVector(anchor, customAnchor);
                    
                    Vector3 targetPos = new Vector3(
                        Mathf.Lerp(bl.x, tr.x, anchorVec.x),
                        Mathf.Lerp(bl.y, tr.y, anchorVec.y),
                        bl.z
                    );
                    
                    Vector3 centerPos = new Vector3(
                        Mathf.Lerp(bl.x, tr.x, 0.5f),
                        Mathf.Lerp(bl.y, tr.y, 0.5f),
                        bl.z
                    );

                    // Calculate normal direction (outward from center to anchor)
                    Vector3 directionOutward = (targetPos - centerPos).normalized;
                    if (directionOutward.sqrMagnitude < 0.01f)
                    {
                        directionOutward = Vector3.up; // Fallback if anchor is exactly Center
                    }

                    // Apply position offset and direction offset
                    indRect.position = targetPos 
                                     + new Vector3(positionOffset.x, positionOffset.y, 0)
                                     + (directionOutward * directionOffset);

                    // Aim to anchor
                    if (aimToAnchor)
                    {
                        // Assume the indicator's "forward" or "up" needs to point to the target center.
                        // For 2D UI arrows, usually transform.up points to the target.
                        // We rotate it to point inward (opposite of outward direction).
                        float angle = Mathf.Atan2(-directionOutward.y, -directionOutward.x) * Mathf.Rad2Deg;
                        // Subtract 90 because default Atan2 0 is right, but transform.up is up (90 deg).
                        indRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
                    }
                }
            }

            // Apply UIEffect
            if (preset.useUIEffect && preset.uiEffectPreset != null)
            {
                _currentUIEffect = target.gameObject.AddComponent<UIEffect>();
                _currentUIEffect.LoadPreset(preset.uiEffectPreset, false);
            }
        }

        public void ClearFocus()
        {
            foreach (Component comp in _addedOverrideComponents)
            {
                if (comp != null)
                {
                    UnityEngine.Object.Destroy(comp);
                }
            }
            _addedOverrideComponents.Clear();

            if (_currentIndicator != null) UnityEngine.Object.Destroy(_currentIndicator);
            if (_currentUIEffect != null) UnityEngine.Object.Destroy(_currentUIEffect);

            if (_focusCanvasGo != null)
            {
                _focusCanvasGo.SetActive(false);
            }
        }

        private Vector2 GetAnchorVector(IndicatorAnchor anchor, Vector2 custom)
        {
            switch (anchor)
            {
                case IndicatorAnchor.TopLeft: return new Vector2(0f, 1f);
                case IndicatorAnchor.TopCenter: return new Vector2(0.5f, 1f);
                case IndicatorAnchor.TopRight: return new Vector2(1f, 1f);
                case IndicatorAnchor.MiddleLeft: return new Vector2(0f, 0.5f);
                case IndicatorAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
                case IndicatorAnchor.MiddleRight: return new Vector2(1f, 0.5f);
                case IndicatorAnchor.BottomLeft: return new Vector2(0f, 0f);
                case IndicatorAnchor.BottomCenter: return new Vector2(0.5f, 0f);
                case IndicatorAnchor.BottomRight: return new Vector2(1f, 0f);
                case IndicatorAnchor.Custom: return custom;
                default: return new Vector2(0.5f, 1f);
            }
        }
    }
}
