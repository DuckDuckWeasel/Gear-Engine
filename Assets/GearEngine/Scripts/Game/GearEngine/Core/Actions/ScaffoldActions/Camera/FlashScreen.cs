using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    [CommandInfo("Camera", "Flash Screen", "Flashes the screen with a color and fades it out. Great for damage feedback.")]
    [Serializable]
    [AddComponentMenu("")]
    public class FlashScreen : ActionBase
    {
        [Tooltip("The color to flash")]
        [SerializeField] protected ColorData color = new ColorData(Color.white);

        [Tooltip("Duration of the fade out")]
        [SerializeField] protected FloatData duration = new FloatData(0.5f);

        [Tooltip("Wait until flash finishes?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (CanRunScheduledWork)
            {
                RunRoutine(FlashRoutine(), !waitUntilFinished);
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator FlashRoutine()
        {
            CreateFlash(out GameObject canvasObject, out Image image);
            CompleteDetachedAction();
            float elapsed = 0f;
            Color startColor = color.Value;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            while (elapsed < duration.Value)
            {
                elapsed += CurrentDeltaTime;
                image.color = Color.Lerp(startColor, endColor, elapsed / duration.Value);
                yield return null;
            }

            GameObject.Destroy(canvasObject);
            CompleteAwaitedAction();
        }

        private void CreateFlash(out GameObject canvasObject, out Image image)
        {
            canvasObject = new GameObject("FlashCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            GameObject imageObject = new GameObject("FlashImage");
            imageObject.transform.SetParent(canvasObject.transform, false);
            image = imageObject.AddComponent<Image>();
            image.color = color.Value;
            ConfigureRect(image.rectTransform);
        }

        private void ConfigureRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void CompleteDetachedAction()
        {
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        private void CompleteAwaitedAction()
        {
            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            return $"Flash screen {color.Value} for {duration.Value}s";
        }

        public override Color GetButtonColor() { return new Color32(216, 228, 240, 255); }
    }
}
