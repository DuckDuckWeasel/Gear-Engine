using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.GearEngine.Visuals
{
    /// <summary>Display-only binding (inventory slot, drag ghost): scale, sprites, sorting, optional <see cref="CanvasGroup"/>.</summary>
    public readonly struct DisplayOptions
    {
        public DisplayOptions(int sortingOrder, float scaleMultiplier, bool blocksRaycasts, float canvasGroupAlpha)
        {
            SortingOrder = sortingOrder;
            ScaleMultiplier = scaleMultiplier;
            BlocksRaycasts = blocksRaycasts;
            CanvasGroupAlpha = canvasGroupAlpha;
        }

        public int SortingOrder { get; }

        public float ScaleMultiplier { get; }

        public bool BlocksRaycasts { get; }

        public float CanvasGroupAlpha { get; }

        public static DisplayOptions Inventory(int sortingOrder, float scaleMultiplier) =>
            new DisplayOptions(sortingOrder, scaleMultiplier, blocksRaycasts: true, canvasGroupAlpha: 1f);

        public static DisplayOptions Ghost(float alpha) =>
            new DisplayOptions(sortingOrder: 0, scaleMultiplier: 1f, blocksRaycasts: false, canvasGroupAlpha: alpha);
    }

    public class GearView : MonoBehaviour
    {
        private IGridNode targetNode;

        [SerializeField]
        private Transform gearVisual;

        [SerializeField]
        private SpriteRenderer chargeFillRenderer;

        private BoardLayoutSO boardLayout;

        private BoardRulesSO boardRules;

        private Func<Vector2Int, Transform> getSlotTransform;
        private Vector2Int lastKnownGridPosition = new Vector2Int(int.MinValue, int.MinValue);

        private float baseRotationOffset;
        private float currentVisualFill;

        public IGridNode TargetNode => targetNode;

        /// <summary>
        /// Binds logical state to this view instance. Prefab must reference <see cref="gearVisual"/> and optional <see cref="chargeFillRenderer"/>.
        /// </summary>
        public void Bind(
            IGridNode node,
            BoardLayoutSO layout,
            BoardRulesSO rules,
            Func<Vector2Int, Transform> getSlot,
            GearConfigData configData)
        {
            targetNode = node;
            boardLayout = layout;
            boardRules = rules;
            getSlotTransform = getSlot;
            lastKnownGridPosition = node.Position;

            RecalculateRotationOffset();

            if (configData != null && gearVisual != null)
            {
                float s = configData.RelativeScaleMultiplier;
                gearVisual.localScale = new Vector3(s, s, s);
            }

            if (configData?.UIIcon != null && chargeFillRenderer != null)
            {
                chargeFillRenderer.sprite = configData.UIIcon;
            }

            if (chargeFillRenderer != null && node is BaseGearNode baseGear && baseGear.ConfigData != null && baseGear.ConfigData.MaxCharge > 0)
            {
                currentVisualFill = 0f;
            }

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>Editor tests: assigns serialized references without a prefab asset.</summary>
        internal void WireTestReferences(Transform gearVisualRef, SpriteRenderer chargeRef = null)
        {
            gearVisual = gearVisualRef;
            chargeFillRenderer = chargeRef;
        }

        /// <summary>
        /// Binds config for display only (no board node). Does not require <see cref="BoardLayoutSO"/> or slot transforms.
        /// </summary>
        public void BindForDisplay(GearConfigData configData, DisplayOptions options)
        {
            ClearDisplayBoardState();
            ApplyDisplayScale(configData, options);
            ApplyDisplayIconAndFill(configData);
            ApplyDisplaySorting(options);
            ApplyDisplayCanvasGroupIfNeeded(options);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void ClearDisplayBoardState()
        {
            targetNode = null;
            boardLayout = null;
            boardRules = null;
            getSlotTransform = null;
            lastKnownGridPosition = new Vector2Int(int.MinValue, int.MinValue);
        }

        private void ApplyDisplayScale(GearConfigData configData, DisplayOptions options)
        {
            if (configData == null || gearVisual == null)
            {
                return;
            }

            float uniform = configData.RelativeScaleMultiplier * options.ScaleMultiplier;
            gearVisual.localScale = new Vector3(uniform, uniform, uniform);
        }

        private void ApplyDisplayIconAndFill(GearConfigData configData)
        {
            if (configData?.UIIcon != null && chargeFillRenderer != null)
            {
                chargeFillRenderer.sprite = configData.UIIcon;
            }

            if (chargeFillRenderer == null || chargeFillRenderer.material == null)
            {
                return;
            }

            currentVisualFill = 1f;
            chargeFillRenderer.material.SetFloat("_FillAmount", 1f);
        }

        private void ApplyDisplaySorting(DisplayOptions options)
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in renderers)
            {
                int prefabOrder = sr.sortingOrder;
                sr.sortingOrder = options.SortingOrder + prefabOrder;
            }
        }

        private void ApplyDisplayCanvasGroupIfNeeded(DisplayOptions options)
        {
            if (options.BlocksRaycasts && options.CanvasGroupAlpha >= 0.999f)
            {
                return;
            }

            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = options.CanvasGroupAlpha;
            canvasGroup.blocksRaycasts = options.BlocksRaycasts;
        }

        private void RecalculateRotationOffset()
        {
            baseRotationOffset = 0f;
            if (targetNode != null && boardLayout != null && boardRules != null && (targetNode.Position.x + targetNode.Position.y) % 2 == 0)
            {
                baseRotationOffset = boardLayout.StaggeredRotationOffset;
            }
        }

        private void Update()
        {
            if (targetNode == null || boardLayout == null || getSlotTransform == null)
            {
                return;
            }

            if (targetNode.Position != lastKnownGridPosition)
            {
                lastKnownGridPosition = targetNode.Position;
                RecalculateRotationOffset();
                Transform newParent = getSlotTransform(targetNode.Position);
                if (newParent != null && newParent != transform.parent)
                {
                    transform.SetParent(newParent, true);
                }
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * 20f);

            Transform rotateTarget = gearVisual != null ? gearVisual : transform;
            Quaternion targetRot = Quaternion.Euler(0, 0, (-targetNode.CurrentRotation) + baseRotationOffset);
            rotateTarget.localRotation = Quaternion.Lerp(rotateTarget.localRotation, targetRot, Time.deltaTime * 15f);

            if (chargeFillRenderer != null && targetNode is BaseGearNode baseGear && baseGear.ConfigData != null && baseGear.ConfigData.MaxCharge > 0)
            {
                float targetFill = baseGear.CurrentCharge / baseGear.ConfigData.MaxCharge;
                currentVisualFill = Mathf.Lerp(currentVisualFill, targetFill, Time.deltaTime * 10f);

                if (chargeFillRenderer.material != null)
                {
                    chargeFillRenderer.material.SetFloat("_FillAmount", currentVisualFill);
                }
            }
        }
    }
}
