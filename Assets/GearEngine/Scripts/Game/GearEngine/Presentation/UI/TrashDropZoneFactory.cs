using GearEngine.GearEngine.Presentation.UI.Tags;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            GameObject rootObj = BuildRootObject(parentCanvas);
            RectTransform rootRect = rootObj.GetComponent<RectTransform>();
            CanvasPositionUtility.AnchorToWorldPosition(rootRect, parentCanvas, worldAnchor, offset, pivot);
            CanvasGroup canvasGroup = ConfigureCanvasGroup(rootObj);
            BuildBackground(rootObj);
            Image iconImage = BuildIcon(rootObj, trashIcon);
            TextMeshProUGUI label = BuildLabel(rootObj);
            TrashDropZoneView zone = BuildZone(rootObj, rootRect, iconImage, label, canvasGroup);
            AddTag(rootObj, trashZoneTag);
            rootObj.SetActive(false);
            return zone;
        }

        private static GameObject BuildRootObject(Canvas parentCanvas)
        {
            GameObject rootObj = new GameObject("TrashDropZone");
            rootObj.transform.SetParent(parentCanvas.transform, false);
            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(140f, 80f);
            return rootObj;
        }

        private static CanvasGroup ConfigureCanvasGroup(GameObject rootObj)
        {
            CanvasGroup canvasGroup = rootObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            return canvasGroup;
        }

        private static void BuildBackground(GameObject rootObj)
        {
            Image background = rootObj.AddComponent<Image>();
            background.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            background.raycastTarget = true;
        }

        private static Image BuildIcon(GameObject rootObj, Sprite trashIcon)
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
            return iconImage;
        }

        private static TextMeshProUGUI BuildLabel(GameObject rootObj)
        {
            GameObject labelObj = new GameObject("RewardLabel");
            labelObj.transform.SetParent(rootObj.transform, false);
            ConfigureLabelRect(labelObj.AddComponent<RectTransform>());
            return ConfigureLabel(labelObj.AddComponent<TextMeshProUGUI>());
        }

        private static void ConfigureLabelRect(RectTransform labelRect)
        {
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.offsetMin = new Vector2(10f, 0f);
            labelRect.offsetMax = new Vector2(-64f, 0f);
        }

        private static TextMeshProUGUI ConfigureLabel(TextMeshProUGUI label)
        {
            label.text = "+0";
            label.fontSize = 24f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.MidlineRight;
            label.color = new Color(0.3f, 1f, 0.5f, 1f);
            label.raycastTarget = false;
            return label;
        }

        private static TrashDropZoneView BuildZone(GameObject rootObj, RectTransform rootRect, Image iconImage, TextMeshProUGUI label, CanvasGroup canvasGroup)
        {
            TrashDropZoneView zone = rootObj.AddComponent<TrashDropZoneView>();
            zone.SetReferences(rootRect, iconImage, label, canvasGroup);
            return zone;
        }

        private static void AddTag(GameObject rootObj, TagSO trashZoneTag)
        {
            if (trashZoneTag == null)
            {
                return;
            }

            TagComponent tagComp = rootObj.AddComponent<TagComponent>();
            tagComp.AddTag(trashZoneTag);
        }
    }
}
