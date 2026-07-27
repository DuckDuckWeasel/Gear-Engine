using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using System.Collections.Generic;
using Coffee.UIEffects;
using GearEngine.GearEngine.Extensions;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    public class TutorialFocusService : IInitializable
    {
        internal const float k_offsetPixelsPerUnit = 20f;

        private static TutorialFocusService s_instance;
        public static TutorialFocusService Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new TutorialFocusService();
                    s_instance.Initialize();
                }
                return s_instance;
            }
        }

        public static bool TryGetInstance(out TutorialFocusService instance)
        {
            instance = s_instance;
            return instance != null;
        }

        private GameObject focusCanvasGameObject;
        private Canvas focusCanvas;
        private Image darkOverlay;

        private List<Component> addedOverrideComponents = new List<Component>();
        private GameObject currentIndicator;
        private UIEffect currentUiEffect;

        public void Initialize()
        {
            focusCanvasGameObject = new GameObject("TutorialFocusCanvas");
            if (Application.isPlaying)
            {
                UnityEngine.Object.DontDestroyOnLoad(
                    focusCanvasGameObject);
            }

            focusCanvas = focusCanvasGameObject.AddComponent<Canvas>();
            focusCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            focusCanvas.sortingOrder = 30000;

            focusCanvasGameObject.AddComponent<GraphicRaycaster>();

            GameObject overlayGo = new GameObject("DarkOverlay");
            overlayGo.transform.SetParent(focusCanvasGameObject.transform, false);
            darkOverlay = overlayGo.AddComponent<Image>();

            RectTransform rect = darkOverlay.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            focusCanvasGameObject.SetActive(false);
        }

        private TargetCanvasState currentTargetCanvasState;

        public void FocusOn(RectTransform target, FocusPresetSO preset, IndicatorAnchor anchor, Vector2 customAnchor, Vector2 positionOffset, float directionOffset, bool aimToAnchor)
        {
            if (target == null || preset == null)
            {
                return;
            }

            if (focusCanvasGameObject == null)
            {
                Initialize();
            }

            // Setup Dark Overlay
            if (preset.useDarkOverlay)
            {
                darkOverlay.color = preset.overlayColor;
                darkOverlay.raycastTarget = preset.blockClicksOutside;
                focusCanvasGameObject.SetActive(true);
            }

            Canvas rootCanvas = target.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                rootCanvas = rootCanvas.rootCanvas;
            }

            bool is3DTarget = false;

            if (rootCanvas != null)
            {
                focusCanvas.renderMode = rootCanvas.renderMode;
                if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera || rootCanvas.renderMode == RenderMode.WorldSpace)
                {
                    focusCanvas.worldCamera = rootCanvas.worldCamera;
                    focusCanvas.planeDistance = rootCanvas.planeDistance - 1f;
                }
                focusCanvas.sortingLayerID = rootCanvas.sortingLayerID;

                // Copy CanvasScaler settings so offset units scale with resolution
                CanvasScaler rootScaler = rootCanvas.GetComponent<CanvasScaler>();
                if (rootScaler != null)
                {
                    CanvasScaler focusScaler = focusCanvasGameObject.GetComponent<CanvasScaler>();
                    if (focusScaler == null)
                    {
                        focusScaler = focusCanvasGameObject.AddComponent<CanvasScaler>();
                    }

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
                focusCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                focusCanvas.worldCamera = Camera.main;
                focusCanvas.planeDistance = 5f; // Place it close to the camera
                focusCanvas.sortingLayerID = UnityEngine.SortingLayer.NameToID("Default");
            }

            currentTargetCanvasState = new TargetCanvasState();

            if (!is3DTarget)
            {
                // Override Sorting on UI Target using Canvas
                Canvas targetCanvas = target.GetComponent<Canvas>();
                if (targetCanvas == null)
                {
                    targetCanvas = target.gameObject.AddComponent<Canvas>();
                    currentTargetCanvasState.wasAdded = true;
                }
                else
                {
                    currentTargetCanvasState.wasAdded = false;
                    currentTargetCanvasState.originalOverride = targetCanvas.overrideSorting;
                    currentTargetCanvasState.originalOrder = targetCanvas.sortingOrder;
                }
                currentTargetCanvasState.canvas = targetCanvas;

                targetCanvas.overrideSorting = true;
                targetCanvas.sortingOrder = focusCanvas.sortingOrder + 1;

                GraphicRaycaster targetRaycaster = target.GetComponent<GraphicRaycaster>();
                if (targetRaycaster == null)
                {
                    targetRaycaster = target.gameObject.AddComponent<Scaffold.Input.FilteredGraphicRaycaster>();
                    targetRaycaster.TryInject();
                    currentTargetCanvasState.raycasterWasAdded = true;
                }
                else
                {
                    if (!(targetRaycaster is Scaffold.Input.FilteredGraphicRaycaster))
                    {
                        Debug.LogWarning("[TutorialFocusService] Target has a GraphicRaycaster that is NOT a FilteredGraphicRaycaster. Pointer Enter/Click events might not fire for this target.");
                    }
                    currentTargetCanvasState.raycasterWasAdded = false;
                }
                currentTargetCanvasState.raycaster = targetRaycaster;
            }
            else
            {
                // Override Sorting on 3D Target using SortingGroup
                UnityEngine.Rendering.SortingGroup sortingGroup = target.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (sortingGroup == null)
                {
                    sortingGroup = target.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                    currentTargetCanvasState.wasSortingGroupAdded = true;
                }
                else
                {
                    currentTargetCanvasState.wasSortingGroupAdded = false;
                    currentTargetCanvasState.originalSortingLayer = sortingGroup.sortingLayerID;
                    currentTargetCanvasState.originalSortingOrder = sortingGroup.sortingOrder;
                }
                currentTargetCanvasState.sortingGroup = sortingGroup;

                sortingGroup.sortingLayerID = focusCanvas.sortingLayerID;
                sortingGroup.sortingOrder = focusCanvas.sortingOrder + 1;
            }

            // Spawn Indicator
            if (preset.indicatorPrefab != null)
            {
                currentIndicator = UnityEngine.Object.Instantiate(preset.indicatorPrefab, focusCanvasGameObject.transform);

                Canvas indCanvas = currentIndicator.GetComponent<Canvas>();
                if (indCanvas == null)
                {
                    indCanvas = currentIndicator.AddComponent<Canvas>();
                }

                indCanvas.overrideSorting = true;
                indCanvas.sortingOrder = focusCanvas.sortingOrder + 2;

                RectTransform indRect = currentIndicator.GetComponent<RectTransform>();
                if (indRect != null)
                {
                    indRect.anchorMin = new Vector2(0.5f, 0.5f);
                    indRect.anchorMax = new Vector2(0.5f, 0.5f);
                    indRect.pivot = new Vector2(0.5f, 0.5f);

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

                    Vector2 finalScreenPos = CalculateIndicatorScreenPosition(
                        screenTargetPos,
                        screenCenterPos,
                        positionOffset,
                        directionOffset);

                    Canvas.ForceUpdateCanvases();
                    RectTransform focusCanvasRect = focusCanvas.GetComponent<RectTransform>();
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            focusCanvasRect,
                            finalScreenPos,
                            cam,
                            out Vector2 finalCanvasPosition))
                    {
                        indRect.anchoredPosition = finalCanvasPosition;
                    }
                    else
                    {
                        Debug.LogError("[TutorialFocusService] Failed to convert the indicator screen position into focus canvas coordinates.");
                    }

                    // Aim to anchor: rotate so the indicator points from its position toward the target center
                    if (aimToAnchor)
                    {
                        Vector2 aimDirection = (screenCenterPos - finalScreenPos).normalized;
                        float angle = Vector2.SignedAngle(Vector2.up, aimDirection);
                        indRect.rotation = Quaternion.Euler(0, 0, angle);
                    }

                    // Disable raycasts on the indicator so it never blocks clicks/hovers
                    Graphic[] graphics = currentIndicator.GetComponentsInChildren<Graphic>(true);
                    foreach (Graphic g in graphics)
                    {
                        g.raycastTarget = false;
                    }
                }
            }

            // Apply UIEffect
            if (preset.useUIEffect && preset.uiEffectPreset != null)
            {
                currentUiEffect = target.gameObject.AddComponent<UIEffect>();
                currentUiEffect.ExecutePreset(preset.uiEffectPreset, false);
            }
        }

        internal static Vector2 CalculateIndicatorScreenPosition(
            Vector2 anchorScreenPosition,
            Vector2 centerScreenPosition,
            Vector2 positionOffset,
            float directionOffset)
        {
            Vector2 directionOutward = (anchorScreenPosition - centerScreenPosition).normalized;
            if (directionOutward.sqrMagnitude < 0.01f)
            {
                directionOutward = Vector2.up;
            }

            return anchorScreenPosition +
                   (positionOffset * k_offsetPixelsPerUnit) +
                   (directionOutward * directionOffset * k_offsetPixelsPerUnit);
        }

        public void ClearFocus()
        {
            if (currentTargetCanvasState != null)
            {
                if (currentTargetCanvasState.raycaster != null)
                {
                    if (currentTargetCanvasState.raycasterWasAdded)
                    {
                        UnityEngine.Object.Destroy(currentTargetCanvasState.raycaster);
                    }
                }

                if (currentTargetCanvasState.canvas != null)
                {
                    if (currentTargetCanvasState.wasAdded)
                    {
                        Canvas canvasToDestroy = currentTargetCanvasState.canvas;
                        canvasToDestroy.enabled = false;
                        UnityEngine.Object.Destroy(canvasToDestroy);
                    }
                    else
                    {
                        currentTargetCanvasState.canvas.overrideSorting = currentTargetCanvasState.originalOverride;
                        currentTargetCanvasState.canvas.sortingOrder = currentTargetCanvasState.originalOrder;
                    }
                }

                if (currentTargetCanvasState.sortingGroup != null)
                {
                    if (currentTargetCanvasState.wasSortingGroupAdded)
                    {
                        UnityEngine.Object.Destroy(currentTargetCanvasState.sortingGroup);
                    }
                    else
                    {
                        currentTargetCanvasState.sortingGroup.sortingLayerID = currentTargetCanvasState.originalSortingLayer;
                        currentTargetCanvasState.sortingGroup.sortingOrder = currentTargetCanvasState.originalSortingOrder;
                    }
                }

                currentTargetCanvasState = null;
            }

            if (currentIndicator != null)
            {
                UnityEngine.Object.Destroy(currentIndicator);
            }

            if (currentUiEffect != null)
            {
                UnityEngine.Object.Destroy(currentUiEffect);
            }

            if (focusCanvasGameObject != null)
            {
                focusCanvasGameObject.SetActive(false);
            }
        }

        private void RunDestructionWorkaround(GameObject targetGo)
        {
            if (targetGo == null)
            {
                return;
            }
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
            // Since TutorialFocusService is not a MonoBehaviour, we use the focusCanvasGameObject to run the coroutine
            if (focusCanvasGameObject != null)
            {
                MonoBehaviour runner = focusCanvasGameObject.GetComponent<MonoBehaviour>();
                if (runner == null)
                {
                    // Add a dummy MonoBehaviour just to run the coroutine
                    runner = focusCanvasGameObject.AddComponent<CoroutineRunner>();
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
