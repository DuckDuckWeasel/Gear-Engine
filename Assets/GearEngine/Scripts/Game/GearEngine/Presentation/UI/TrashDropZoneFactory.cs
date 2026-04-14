using GearEngine.GearEngine.Presentation.UI.Tags;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GearEngine.GearEngine.Presentation.UI
{
    public static class TrashDropZoneFactory
    {
        public static TrashDropZoneView Create(Canvas parentCanvas, Vector3 worldAnchor, Vector2 offset, Vector2 pivot, TagSO trashZoneTag = null, Sprite trashIcon = null)
        {
            if (parentCanvas == null)
            {
                throw new System.ArgumentNullException(nameof(parentCanvas));
            }

            GameObject rootObj = CreateRootObject(parentCanvas);
            RectTransform rootRect = BuildRootRect(rootObj, parentCanvas, worldAnchor, offset, pivot);
            CanvasGroup cg = AddCanvasGroup(rootObj);
            AddBackgroundImage(rootObj);
            AddTrashIcon(rootObj, trashIcon);
            TextMeshProUGUI label = AddRewardLabel(rootObj);
            TrashDropZoneView zone = WireTrashZone(rootObj, rootRect, label, cg);
            TryAddTrashTag(rootObj, trashZoneTag);
            rootObj.SetActive(false);
            return zone;
        }

        private static RectTransform BuildRootRect(GameObject rootObj, Canvas parentCanvas, Vector3 worldAnchor, Vector2 offset, Vector2 pivot)
        {
            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(140f, 80f);
            CanvasPositionUtility.AnchorToWorldPosition(rootRect, parentCanvas, worldAnchor, offset, pivot);
            return rootRect;
        }

        private static GameObject CreateRootObject(Canvas parentCanvas)
        {
            GameObject rootObj = new GameObject("TrashDropZone");
            rootObj.transform.SetParent(parentCanvas.transform, false);
            return rootObj;
        }

        private static CanvasGroup AddCanvasGroup(GameObject rootObj)
        {
            CanvasGroup cg = rootObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = true;
            cg.interactable = false;
            return cg;
        }

        private static void AddBackgroundImage(GameObject rootObj)
        {
            Image bg = rootObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            bg.raycastTarget = true;
        }

        private static void AddTrashIcon(GameObject rootObj, Sprite trashIcon)
        {
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
        }

        private static TextMeshProUGUI AddRewardLabel(GameObject rootObj)
        {
            GameObject labelObj = new GameObject("RewardLabel");
            labelObj.transform.SetParent(rootObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.offsetMin = new Vector2(10f, 0f);
            labelRect.offsetMax = new Vector2(-64f, 0f);
            return ConfigureRewardText(labelObj.AddComponent<TextMeshProUGUI>());
        }

        private static TextMeshProUGUI ConfigureRewardText(TextMeshProUGUI label)
        {
            label.text = "+0";
            label.fontSize = 24f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.MidlineRight;
            label.color = new Color(0.3f, 1f, 0.5f, 1f);
            label.raycastTarget = false;
            return label;
        }

        private static TrashDropZoneView WireTrashZone(GameObject rootObj, RectTransform rootRect, TextMeshProUGUI label, CanvasGroup cg)
        {
            Image iconImage = rootObj.transform.Find("TrashIcon").GetComponent<Image>();
            TrashDropZoneView zone = rootObj.AddComponent<TrashDropZoneView>();
            zone.SetReferences(rootRect, iconImage, label, cg);
            return zone;
        }

        private static void TryAddTrashTag(GameObject rootObj, TagSO trashZoneTag)
        {
            if (trashZoneTag == null)
            {
                return;
            }

            var tagComp = rootObj.AddComponent<TagComponent>();
            tagComp.AddTag(trashZoneTag);
        }
    }
}
