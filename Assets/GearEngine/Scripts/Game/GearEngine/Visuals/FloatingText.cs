using UnityEngine;
using TMPro;
using DG.Tweening;

namespace GearEngine.GearEngine.Visuals
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private float floatDuration = 1.5f;
        [SerializeField] private float floatDistance = 1.0f;
        [SerializeField] private Color textColor = Color.white;

        private void Awake()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshPro>();
                if (textMesh == null)
                {
                    textMesh = gameObject.AddComponent<TextMeshPro>();
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.fontSize = 4f;
                    textMesh.outlineWidth = 0.2f;
                }
            }
        }

        public void Play(string text)
        {
            textMesh.text = text;
            textMesh.color = textColor;

            transform.DOMoveY(transform.position.y + floatDistance, floatDuration).SetEase(Ease.OutQuad);
            DOTween.To(() => textMesh.color, x => textMesh.color = x, new Color(textColor.r, textColor.g, textColor.b, 0f), floatDuration).SetEase(Ease.InQuart).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
        
        public static FloatingText Spawn(Vector3 position, string text)
        {
            GameObject go = new GameObject("FloatingText");
            go.transform.position = position;
            FloatingText floatingText = go.AddComponent<FloatingText>();
            floatingText.Play(text);
            return floatingText;
        }
    }
}
