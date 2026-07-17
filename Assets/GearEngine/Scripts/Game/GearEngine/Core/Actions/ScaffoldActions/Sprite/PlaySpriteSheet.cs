using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Sprite", "Play Sprite Sheet", "Plays an array of sprites sequentially on a SpriteRenderer.")]
    [Serializable]
    [AddComponentMenu("")]
    public class PlaySpriteSheet : ActionBase
    {
        [Tooltip("The SpriteRenderer to modify")]
        [SerializeField] protected SpriteRenderer targetRenderer;
        
        [Tooltip("The sprites to play")]
        [SerializeField] protected Sprite[] frames;

        [Tooltip("Frames per second")]
        [SerializeField] protected FloatData fps = new FloatData(12f);
        
        [Tooltip("Wait until animation finishes?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetRenderer != null && frames != null && frames.Length > 0 && flowchart != null)
            {
                flowchart.StartCoroutine(PlayRoutine());
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator PlayRoutine()
        {
            if (!waitUntilFinished) Continue();

            float delay = 1f / fps.Value;
            
            for (int i = 0; i < frames.Length; i++)
            {
                targetRenderer.sprite = frames[i];
                yield return new WaitForSeconds(delay);
            }

            if (waitUntilFinished) Continue();
        }

        public override string GetSummary()
        {
            if (targetRenderer == null) return "Error: No Target Renderer";
            return $"Play {frames.Length} frames at {fps.Value} FPS";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
