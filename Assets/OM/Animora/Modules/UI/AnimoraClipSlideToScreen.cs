using OM.Animora.Runtime;
using UnityEngine;

namespace OM.Animora.Modules
{
    [System.Serializable]
    [AnimoraCreate("Slide To Screen", "UI/Slide To Screen")]
    [AnimoraIcon("AnimationClip Icon")]
    [AnimoraKeywords("slide", "to", "screen", "action")]
    [AnimoraDescription("This is a slide to screen action")]
    public class AnimoraClipSlideToScreen : AnimoraClipWithTarget<RectTransform>
    {
        public enum ScreenSlideType
        {
            FromLeft,
            FromRight,
            FromTop,
            FromBottom,
        }
        
        [OM_StartGroup("Screen Slide Settings", "Settings")]
        [SerializeField] private ScreenSlideType slideType;
        [SerializeField] private EaseData ease = new EaseData(EasingFunction.OutBack);
        
        private Vector2[] restPositions;

        public override void ResetBeforePlay(bool isPreviewing, OM_PlayDirection playDirection)
        {
            base.ResetBeforePlay(isPreviewing, playDirection);
            restPositions = null;
        }

        public override void Enter()
        {
            base.Enter();

            var list = targets.GetTargets();
            if (restPositions == null || restPositions.Length != list.Count)
            {
                restPositions = new Vector2[list.Count];
                for (var index = 0; index < list.Count; index++)
                {
                    var target = list[index];
                    if (target != null)
                    {
                        restPositions[index] = target.anchoredPosition;
                    }
                }
            }
        }


        private Vector2 GetStartPosition(int index)
        {
            var rectTransform = targets.GetTargetAt(index);
            if (rectTransform == null) return Vector2.zero;
            
            var parent = rectTransform.parent as RectTransform;
            var offset = Vector2.zero;

            if (parent != null)
            {
                var parentRect = parent.rect;
                switch (slideType)
                {
                    case ScreenSlideType.FromLeft:
                        offset = new Vector2(-parentRect.width, 0);
                        break;
                    case ScreenSlideType.FromRight:
                        offset = new Vector2(parentRect.width, 0);
                        break;
                    case ScreenSlideType.FromTop:
                        offset = new Vector2(0, parentRect.height);
                        break;
                    case ScreenSlideType.FromBottom:
                        offset = new Vector2(0, -parentRect.height);
                        break;
                }
            }

            return restPositions != null && index < restPositions.Length ? restPositions[index] + offset : offset;
        }
        
        public override void OnEvaluate(float time, float clipTime, float normalizedTime, bool isPreviewing)
        {
            base.OnEvaluate(time, clipTime, normalizedTime, isPreviewing);

            var list = targets.GetTargets();
            if (restPositions == null || restPositions.Length != list.Count)
            {
                // Safety fallback for preview mode if Enter wasn't called
                restPositions = new Vector2[list.Count];
                for (var i = 0; i < list.Count; i++)
                {
                    var target = list[i];
                    if (target != null)
                    {
                        restPositions[i] = target.anchoredPosition;
                    }
                }
            }

            for (var i = 0; i < list.Count; i++)
            {
                var rectTransform = targets.GetTargetAt(i);
                if (rectTransform == null) continue;
                
                rectTransform.anchoredPosition =
                    Vector2.LerpUnclamped(GetStartPosition(i), restPositions[i], ease.Evaluate(normalizedTime));
            }
        }
        
        public override void OnPreviewChanged(AnimoraPlayer animoraPlayer, bool isOn)
        {
            base.OnPreviewChanged(animoraPlayer, isOn);
            
            var list = targets.GetTargets();
            for (var i = 0; i < list.Count; i++)
            {
                var rectTransform = targets.GetTargetAt(i);
                if (rectTransform != null)
                {
                    AnimoraPreviewManager.RecordOrUndoObject(isOn,this,rectTransform.anchoredPosition, (e)=> rectTransform.anchoredPosition = e);
                }
            }
        }
    }
}