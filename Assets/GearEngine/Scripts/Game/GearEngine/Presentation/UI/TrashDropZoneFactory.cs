using GearEngine.GearEngine.Presentation.UI.Tags;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Factory responsible for building the TrashDropZone UI hierarchy programmatically.
    /// Separated from <see cref="TrashDropZoneView"/> to respect SRP — the view only
    /// manages runtime behavior (animation, hover, visibility).
    /// </summary>
    public static class TrashDropZoneFactory
    {
        /// <summary>
        /// Creates and returns an inactive TrashDropZoneView positioned relative to a world anchor
        /// projected into canvas space via <see cref="CanvasPositionUtility"/>.
        /// </summary>
        /// <param name="parentCanvas">Canvas to parent the trash zone to.</param>
        /// <param name="worldAnchor">World-space position to anchor the trash zone to.</param>
        /// <param name="offset">Pixel offset from the projected anchor in canvas local space.</param>
        /// <param name="pivot">Pivot for alignment control. Defaults to center (0.5, 0.5).</param>
        /// <param name="trashZoneTag">Optional tag for discovery by drag handlers via the tag system.</param>
        /// <param name="trashIcon">Sprite for the trash icon. If null, the icon image will be empty.</param>
        public static TrashDropZoneView Create(
            Canvas parentCanvas,
            Vector3 worldAnchor,
            Vector2 offset,
            Vector2 pivot,
            TagSO trashZoneTag = null,
            Sprite trashIcon = null)
        {
            if (parentCanvas == null)
            {
                throw new System.ArgumentNullException(nameof(parentCanvas));
            }

            // Root panel — no background, acts as hit area only
            GameObject rootObj = new GameObject("TrashDropZone");
            rootObj.transform.SetParent(parentCanvas.transform, false);

            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(140f, 80f);

            CanvasPositionUtility.AnchorToWorldPosition(
                rootRect, parentCanvas, worldAnchor, offset, pivot);

            CanvasGroup cg = rootObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = true;
            cg.interactable = false;

            // Background
            Image bg = rootObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            bg.raycastTarget = true;

            // Trash icon — right side, vertically centered, with padding from edge
            GameObject iconObj = new GameObject("TrashIcon");
            iconObj.transform.SetParent(rootObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(1f, 0.5f);
            iconRect.anchorMax = new Vector2(1f, 0.5f);
            iconRect.pivot = new Vector2(1f, 0.5f);
            iconRect.anchoredPosition = new Vector2(-10f, 0f);
            iconRect.sizeDelta = new Vector2(44f, 44f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.sprite = trashIcon;
            iconImage.preserveAspect = true;

            // Reward label — fills left area with padding from icon
            GameObject labelObj = new GameObject("RewardLabel");
            labelObj.transform.SetParent(rootObj.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            // Left inset 10px, right inset = icon (44) + icon padding (10) + gap (8) = 62px
            labelRect.offsetMin = new Vector2(10f, 0f);
            labelRect.offsetMax = new Vector2(-64f, 0f);

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "+0";
            label.fontSize = 24f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.MidlineRight;
            label.color = new Color(0.3f, 1f, 0.5f, 1f);
            label.raycastTarget = false;

            // Wire component
            TrashDropZoneView zone = rootObj.AddComponent<TrashDropZoneView>();
            zone.SetReferences(rootRect, iconImage, label, cg);

            // Tag for discovery by GearBoardDragHandler
            if (trashZoneTag != null)
            {
                var tagComp = rootObj.AddComponent<TagComponent>();
                tagComp.AddTag(trashZoneTag);
            }

            rootObj.SetActive(false);
            return zone;
        }
    }
}
