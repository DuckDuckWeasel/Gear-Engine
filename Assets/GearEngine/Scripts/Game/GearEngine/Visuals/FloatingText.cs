using UnityEngine;
using TMPro;
using DG.Tweening;

namespace GearEngine.GearEngine.Visuals
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private float floatDuration = 1.5f;
        [SerializeField] private float floatDistance = 1.5f; // 100 is too big for world space
        [SerializeField] private Color textColor = Color.white;
        
        private static int globalSpawnCounter = 0;
        private Vector3 baseScale = new Vector3(0.02f, 0.02f, 0.02f);

        public void SetBaseScale(float scale)
        {
            baseScale = new Vector3(scale, scale, scale);
            transform.localScale = baseScale;
        }

        private void Awake()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshProUGUI>();
                if (textMesh == null)
                {
                    textMesh = gameObject.AddComponent<TextMeshProUGUI>();
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.fontSize = 36f;
                }
            }

            // Ensure Canvas is setup for World Space rendering
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32767; // Force highest sorting order
            
            // Adjust scale to make UI text reasonable in world space
            transform.localScale = baseScale;
            
            // Force text material to ignore depth buffer (ZTest = Always)
            if (textMesh.fontSharedMaterial != null)
            {
                Material mat = new Material(textMesh.fontSharedMaterial);
                mat.SetInt("_ZTestMode", 8); // UnityEngine.Rendering.CompareFunction.Always
                textMesh.fontMaterial = mat;
            }
        }

        private void Update()
        {
            if (Camera.main != null)
            {
                // Make the text face the camera
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        public void Play(string text, float duration = 0f)
        {
            textMesh.text = text;
            textMesh.color = textColor;

            // Apply dynamic duration if provided and valid
            if (duration > 0.1f)
            {
                floatDuration = duration;
            }

            // Adjust Z position significantly towards camera to prevent clipping with gears
            if (Camera.main != null)
            {
                transform.position += Camera.main.transform.forward * -1.5f;
                
                // Deterministic scatter to prevent texts from perfectly overlapping
                globalSpawnCounter++;
                int index = globalSpawnCounter % 5;
                
                // 5 distinct offset positions
                float offsetX = 0f;
                float offsetY = 0f;
                switch (index)
                {
                    case 0: offsetX = 0f; offsetY = 0f; break;
                    case 1: offsetX = 0f; offsetY = 3.0f; break;
                    case 2: offsetX = 0f; offsetY = 6.0f; break;
                    case 3: offsetX = 0f; offsetY = 9.0f; break;
                    case 4: offsetX = 0f; offsetY = 12.0f; break;
                }

                Vector3 deterministicOffset = (Camera.main.transform.right * offsetX) +
                                              (Camera.main.transform.up * offsetY);
                transform.position += deterministicOffset;
            }

            float actualDistance = floatDistance > 10f ? 1.5f : floatDistance;
            
            // Calculate timings
            float fadeDuration = Mathf.Max(0.2f, floatDuration * 0.2f);
            float waitDuration = Mathf.Max(0.3f, floatDuration - fadeDuration);

            // Move up relative to the camera's upward direction ONLY during waitDuration
            Vector3 targetPosition = transform.position + (Camera.main != null ? Camera.main.transform.up : Vector3.up) * actualDistance;
            transform.DOMove(targetPosition, waitDuration).SetEase(Ease.OutQuad);

            // Scale animation: Punch on spawn, then shrink to size 20 equivalent (better readability)
            transform.localScale = Vector3.zero;
            Vector3 punchScale = baseScale * 1.5f; // Pop to 150% size
            Vector3 endScale = baseScale * (20f / 36f); // Equivalent to font size 20 (base is 36)

            Sequence scaleSeq = DOTween.Sequence();
            scaleSeq.Append(transform.DOScale(punchScale, 0.15f).SetEase(Ease.OutQuad));
            scaleSeq.Append(transform.DOScale(baseScale, 0.15f).SetEase(Ease.InOutSine));
            scaleSeq.AppendInterval(0.5f); // Hold at normal size before shrinking
            float shrinkDuration = Mathf.Max(0f, waitDuration - 0.8f); // 0.15 + 0.15 + 0.5 = 0.8s
            if (shrinkDuration > 0)
            {
                scaleSeq.Append(transform.DOScale(endScale, shrinkDuration).SetEase(Ease.InSine));
            }
            
            // Fade Sequence
            Sequence fadeSeq = DOTween.Sequence();
            fadeSeq.AppendInterval(waitDuration);
            fadeSeq.Append(DOTween.To(() => textMesh.color, x => textMesh.color = x, new Color(textColor.r, textColor.g, textColor.b, 0f), fadeDuration).SetEase(Ease.InOutQuad));
            fadeSeq.OnComplete(() => Destroy(gameObject));

            // Delayed Shake effect: Triggers exactly when shrinking ends and fading begins
            DOVirtual.DelayedCall(waitDuration, () => {
                if (this != null && gameObject != null)
                {
                    Vector3 shakeDir = Camera.main != null ? Camera.main.transform.right : Vector3.right;
                    // Shake only on X axis relative to camera
                    transform.DOShakePosition(fadeDuration, shakeDir * 0.2f, vibrato: 20, randomness: 0f, snapping: false, fadeOut: true);
                }
            });
        }
        
        public static FloatingText Spawn(Vector3 position, string text, float duration = 0f)
        {
            GameObject go = new GameObject("FloatingText");
            go.transform.position = position;
            FloatingText floatingText = go.AddComponent<FloatingText>();
            floatingText.Play(text, duration);
            return floatingText;
        }
    }
}
