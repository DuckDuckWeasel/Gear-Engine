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
        private Vector3 baseScale = new Vector3(0.022f, 0.022f, 0.022f);
        
        private Transform carTransform;
        private bool isExploding = false;
        private float aliveTime = 0f;
        private System.Action onExplodeAction;

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
            aliveTime += Time.deltaTime;

            if (Camera.main != null)
            {
                // Make the text face the camera
                transform.rotation = Camera.main.transform.rotation;
            }

            // Explosion logic when car passes by
            if (!isExploding && carTransform != null && aliveTime > 0.6f)
            {
                // 8.0f threshold accounts for the height offset of the text and makes collision easier
                if (Vector3.Distance(transform.position, carTransform.position) < 8.0f)
                {
                    ExplodeAndFade();
                }
            }
        }

        private void ExplodeAndFade()
        {
            isExploding = true;
            transform.DOKill(); // Stop all existing animations

            onExplodeAction?.Invoke();

            Sequence explodeSeq = DOTween.Sequence();
            // Explosion punch scale (less big, slower)
            explodeSeq.Append(transform.DOScale(baseScale * 2.2f, 0.4f).SetEase(Ease.OutExpo));
            // Fade out (slower)
            explodeSeq.Join(DOTween.To(() => textMesh.color, x => textMesh.color = x, new Color(textColor.r, textColor.g, textColor.b, 0f), 0.4f));
            explodeSeq.OnComplete(() => Destroy(gameObject));
        }

        public void Play(string text, float duration = 0f, Vector3? endMoveDirection = null, Transform carRef = null, System.Action onExplode = null)
        {
            this.onExplodeAction = onExplode;
            if (carRef != null)
            {
                carTransform = carRef;
            }
            transform.SetParent(null);
            textMesh.text = text;
            textMesh.color = textColor;

            float speedMultiplier = 1f;
            if (endMoveDirection.HasValue)
            {
                Vector3 dir = endMoveDirection.Value;
                dir.y = 0f;
                speedMultiplier = Mathf.Max(1f, dir.magnitude);
                endMoveDirection = dir.normalized;
            }

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
            Vector3 upMovement = (Camera.main != null ? Camera.main.transform.up : Vector3.up) * actualDistance;
            transform.DOBlendableMoveBy(upMovement, waitDuration).SetEase(Ease.OutQuad);

            // Initial movement backwards (relative to the car's direction) for 0.5 seconds
            if (endMoveDirection.HasValue)
            {
                // Multiplier scales with the car's speed (encoded in the magnitude of endMoveDirection)
                transform.DOBlendableMoveBy(endMoveDirection.Value * 10f * speedMultiplier, 0.5f).SetEase(Ease.OutQuad);
            }

            // Scale animation: Punch on spawn, then shrink to -20%
            transform.localScale = Vector3.zero;
            Vector3 punchScale = baseScale * 1.8f; // Pop to 180% size
            Vector3 endScale = baseScale * 0.8f; // -20% of base size

            Sequence scaleSeq = DOTween.Sequence();
            scaleSeq.Append(transform.DOScale(punchScale, 0.15f).SetEase(Ease.OutQuad));
            scaleSeq.Append(transform.DOScale(baseScale, 0.15f).SetEase(Ease.InOutSine));
            scaleSeq.AppendInterval(0.5f); // Hold at normal size before shrinking
            float shrinkDuration = Mathf.Max(0f, waitDuration - 0.8f); // 0.15 + 0.15 + 0.5 = 0.8s
            if (shrinkDuration > 0)
            {
                scaleSeq.Append(transform.DOScale(endScale, shrinkDuration).SetEase(Ease.InSine));
            }
            
            // Delayed Shake effect: Triggers exactly when shrinking ends
            DOVirtual.DelayedCall(waitDuration, () => {
                if (this != null && gameObject != null)
                {
                    Vector3 shakeDir = Camera.main != null ? Camera.main.transform.right : Vector3.right;
                    // Shake only on X axis relative to camera
                    transform.DOShakePosition(fadeDuration, shakeDir * 0.2f, vibrato: 20, randomness: 0f, snapping: false, fadeOut: true);
                }
            });

            // Fade Sequence: Starts after the shake finishes
            Sequence fadeSeq = DOTween.Sequence();
            fadeSeq.AppendInterval(waitDuration + fadeDuration);
            fadeSeq.AppendCallback(() => {
                if (this != null && gameObject != null)
                {
                    // Scale down to 0 and move up a bit to "poof" away
                    transform.DOScale(Vector3.zero, fadeDuration).SetEase(Ease.InBack);
                    Vector3 upPoof = (Camera.main != null ? Camera.main.transform.up : Vector3.up);
                    transform.DOBlendableMoveBy(upPoof * 1.5f, fadeDuration).SetEase(Ease.InQuad);
                }
            });
            fadeSeq.Append(DOTween.To(() => textMesh.color, x => textMesh.color = x, new Color(textColor.r, textColor.g, textColor.b, 0f), fadeDuration).SetEase(Ease.InQuad));
            fadeSeq.OnComplete(() => Destroy(gameObject));
        }
        
        public static FloatingText Spawn(Vector3 position, string text, float duration = 0f, Vector3? endMoveDirection = null, Transform carRef = null, System.Action onExplode = null)
        {
            GameObject go = new GameObject("FloatingText");
            go.transform.position = position;
            FloatingText floatingText = go.AddComponent<FloatingText>();
            floatingText.Play(text, duration, endMoveDirection, carRef, onExplode);
            return floatingText;
        }
    }
}
