using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using System.Collections.Generic;
using Coffee.UIEffects;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    public class TutorialFocusService : IInitializable
    {
        private static TutorialFocusService _instance;
        public static TutorialFocusService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TutorialFocusService();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

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

        private TargetCanvasState _currentTargetCanvasState;

        public void FocusOn(RectTransform target, FocusPresetSO preset, IndicatorAnchor anchor, Vector2 customAnchor, Vector2 positionOffset, float directionOffset, bool aimToAnchor)
        {
            if (target == null || preset == null) return;

            if (_focusCanvasGo == null)
            {
                Initialize();
            }

            // Setup Dark Overlay
            if (preset.useDarkOverlay)
            {
                _darkOverlay.color = preset.overlayColor;
                _darkOverlay.raycastTarget = preset.blockClicksOutside;
                _focusCanvasGo.SetActive(true);
            }

            Canvas rootCanvas = target.GetComponentInParent<Canvas>();
            if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;
            
            bool is3DTarget = false;
            
            if (rootCanvas != null)
            {
                _focusCanvas.renderMode = rootCanvas.renderMode;
                if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera || rootCanvas.renderMode == RenderMode.WorldSpace)
                {
                    _focusCanvas.worldCamera = rootCanvas.worldCamera;
                    _focusCanvas.planeDistance = rootCanvas.planeDistance - 1f;
                }
                _focusCanvas.sortingLayerID = rootCanvas.sortingLayerID;

                // Copy CanvasScaler settings so offset units scale with resolution
                CanvasScaler rootScaler = rootCanvas.GetComponent<CanvasScaler>();
                if (rootScaler != null)
                {
                    CanvasScaler focusScaler = _focusCanvasGo.GetComponent<CanvasScaler>();
                    if (focusScaler == null) focusScaler = _focusCanvasGo.AddComponent<CanvasScaler>();
                    focusScaler.uiScaleMode = rootScaler.uiScaleMode;
                    focusScaler.referenceResolution = rootScaler.referenceResolution;
                    focusScaler.screenMatchMode = rootScaler.screenMatchMode;
                    focusScaler.matchWidthOrHeight = rootScaler.matchWidthOrHeight;
                    focusScaler.referencePixelsPerUnit = rootScaler.referencePixelsPerUnit;
                }
            }
            else
            {
                // Target is a 3D Object or Sprite outside a Canvas!
                is3DTarget = true;
                _focusCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                _focusCanvas.worldCamera = Camera.main;
                _focusCanvas.planeDistance = 5f; // Place it close to the camera
                _focusCanvas.sortingLayerID = UnityEngine.SortingLayer.NameToID("Default");
            }

            _currentTargetCanvasState = new TargetCanvasState();

            if (!is3DTarget)
            {
                // Override Sorting on UI Target using Canvas
                Canvas targetCanvas = target.GetComponent<Canvas>();
                if (targetCanvas == null)
                {
                    targetCanvas = target.gameObject.AddComponent<Canvas>();
                    _currentTargetCanvasState.wasAdded = true;
                }
                else
                {
                    _currentTargetCanvasState.wasAdded = false;
                    _currentTargetCanvasState.originalOverride = targetCanvas.overrideSorting;
                    _currentTargetCanvasState.originalOrder = targetCanvas.sortingOrder;
                }
                _currentTargetCanvasState.canvas = targetCanvas;
                
                targetCanvas.overrideSorting = true;
                targetCanvas.sortingOrder = _focusCanvas.sortingOrder + 1;

                GraphicRaycaster targetRaycaster = target.GetComponent<GraphicRaycaster>();
                if (targetRaycaster == null)
                {
                    targetRaycaster = target.gameObject.AddComponent<GraphicRaycaster>();
                    _currentTargetCanvasState.raycasterWasAdded = true;
                }
                else
                {
                    _currentTargetCanvasState.raycasterWasAdded = false;
                }
                _currentTargetCanvasState.raycaster = targetRaycaster;
            }
            else
            {
                // Override Sorting on 3D Target using SortingGroup
                UnityEngine.Rendering.SortingGroup sortingGroup = target.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (sortingGroup == null)
                {
                    sortingGroup = target.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                    _currentTargetCanvasState.wasSortingGroupAdded = true;
                }
                else
                {
                    _currentTargetCanvasState.wasSortingGroupAdded = false;
                    _currentTargetCanvasState.originalSortingLayer = sortingGroup.sortingLayerID;
                    _currentTargetCanvasState.originalSortingOrder = sortingGroup.sortingOrder;
                }
                _currentTargetCanvasState.sortingGroup = sortingGroup;

                sortingGroup.sortingLayerID = _focusCanvas.sortingLayerID;
                sortingGroup.sortingOrder = _focusCanvas.sortingOrder + 1;
            }

            // Spawn Indicator
            if (preset.indicatorPrefab != null)
            {
                _currentIndicator = UnityEngine.Object.Instantiate(preset.indicatorPrefab, _focusCanvasGo.transform);
                
                Canvas indCanvas = _currentIndicator.GetComponent<Canvas>();
                if (indCanvas == null) indCanvas = _currentIndicator.AddComponent<Canvas>();
                indCanvas.overrideSorting = true;
                indCanvas.sortingOrder = _focusCanvas.sortingOrder + 2;

                RectTransform indRect = _currentIndicator.GetComponent<RectTransform>();
                if (indRect != null)
                {
                    // Copy target's anchors and pivot as requested, but keep its original sizeDelta
                    indRect.anchorMin = target.anchorMin;
                    indRect.anchorMax = target.anchorMax;
                    indRect.pivot = target.pivot;

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

                    Camera cam = rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : (rootCanvas != null ? rootCanvas.worldCamera : null);
                    Vector2 screenTargetPos = RectTransformUtility.WorldToScreenPoint(cam, targetPos);
                    Vector2 screenCenterPos = RectTransformUtility.WorldToScreenPoint(cam, centerPos);

                    // Calculate normal direction (outward from center to anchor) in screen space
                    Vector2 directionOutward = (screenTargetPos - screenCenterPos).normalized;
                    if (directionOutward.sqrMagnitude < 0.01f)
                    {
                        directionOutward = Vector2.up; // Fallback if anchor is exactly Center
                    }

                    // Scale offsets by canvas scaleFactor for resolution independence
                    // 20f is the base unit: 1 direction/offset unit = 20 canvas units
                    float scaleFactor = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
                    float pixelDirOffset = directionOffset * 20f * scaleFactor;
                    Vector2 pixelPosOffset = positionOffset * 20f * scaleFactor;
                    
                    Vector2 finalScreenPos = screenTargetPos + pixelPosOffset + (directionOutward * pixelDirOffset);
                    
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(_focusCanvas.GetComponent<RectTransform>(), finalScreenPos, cam, out Vector3 finalWorldPos);
                    indRect.position = finalWorldPos;

                    // Aim to anchor: rotate so the indicator points from its position toward the target center
                    if (aimToAnchor)
                    {
                        Vector2 aimDirection = (screenCenterPos - finalScreenPos).normalized;
                        float angle = Vector2.SignedAngle(Vector2.up, aimDirection);
                        indRect.rotation = Quaternion.Euler(0, 0, angle);
                    }
                    
                    // Disable raycasts on the indicator so it never blocks clicks/hovers
                    Graphic[] graphics = _currentIndicator.GetComponentsInChildren<Graphic>(true);
                    foreach (Graphic g in graphics)
                    {
                        g.raycastTarget = false;
                    }
                }
            }

            // Apply UIEffect
            if (preset.useUIEffect && preset.uiEffectPreset != null)
            {
                _currentUIEffect = target.gameObject.AddComponent<UIEffect>();
                _currentUIEffect.ExecutePreset(preset.uiEffectPreset, false);
            }
        }

        public void ClearFocus()
        {
            if (_currentTargetCanvasState != null)
            {
                if (_currentTargetCanvasState.raycaster != null)
                {
                    if (_currentTargetCanvasState.raycasterWasAdded)
                    {
                        UnityEngine.Object.Destroy(_currentTargetCanvasState.raycaster);
                    }
                }

                if (_currentTargetCanvasState.canvas != null)
                {
                    if (_currentTargetCanvasState.wasAdded)
                    {
                        Canvas canvasToDestroy = _currentTargetCanvasState.canvas;
                        canvasToDestroy.enabled = false;
                        UnityEngine.Object.Destroy(canvasToDestroy);
                    }
                    else
                    {
                        _currentTargetCanvasState.canvas.overrideSorting = _currentTargetCanvasState.originalOverride;
                        _currentTargetCanvasState.canvas.sortingOrder = _currentTargetCanvasState.originalOrder;
                    }
                }
                
                if (_currentTargetCanvasState.sortingGroup != null)
                {
                    if (_currentTargetCanvasState.wasSortingGroupAdded)
                    {
                        UnityEngine.Object.Destroy(_currentTargetCanvasState.sortingGroup);
                    }
                    else
                    {
                        _currentTargetCanvasState.sortingGroup.sortingLayerID = _currentTargetCanvasState.originalSortingLayer;
                        _currentTargetCanvasState.sortingGroup.sortingOrder = _currentTargetCanvasState.originalSortingOrder;
                    }
                }

                _currentTargetCanvasState = null;
            }

            if (_currentIndicator != null) UnityEngine.Object.Destroy(_currentIndicator);
            if (_currentUIEffect != null) UnityEngine.Object.Destroy(_currentUIEffect);

            if (_focusCanvasGo != null)
            {
                _focusCanvasGo.SetActive(false);
            }
        }

        private void RunDestructionWorkaround(GameObject targetGo)
        {
            if (targetGo == null) return;
            // A simple trick to rebuild the graphic without triggering OnEnable/OnDisable 
            // is to modify the hierarchy slightly or mark layouts dirty.
            // But the most robust way that avoids OnEnable logic is toggling the Graphic component.
            Graphic[] graphics = targetGo.GetComponentsInChildren<Graphic>(true);
            foreach (Graphic g in graphics)
            {
                g.SetAllDirty();
                g.Rebuild(CanvasUpdate.PreRender);
            }
            // If SetAllDirty isn't enough, we wait for end of frame and toggle canvas renderer
            // Since TutorialFocusService is not a MonoBehaviour, we use the _focusCanvasGo to run the coroutine
            if (_focusCanvasGo != null)
            {
                MonoBehaviour runner = _focusCanvasGo.GetComponent<MonoBehaviour>();
                if (runner == null)
                {
                    // Add a dummy MonoBehaviour just to run the coroutine
                    runner = _focusCanvasGo.AddComponent<CoroutineRunner>();
                }
                runner.StartCoroutine(DestructionWorkaroundCoroutine(targetGo));
            }
        }

        private class CoroutineRunner : MonoBehaviour { }

        private System.Collections.IEnumerator DestructionWorkaroundCoroutine(GameObject targetGo)
        {
            yield return new WaitForEndOfFrame();
            if (targetGo != null)
            {
                CanvasRenderer[] renderers = targetGo.GetComponentsInChildren<CanvasRenderer>(true);
                foreach (CanvasRenderer cr in renderers)
                {
                    cr.cull = true;
                    cr.cull = false;
                }
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
