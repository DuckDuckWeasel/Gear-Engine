using System.Collections.Generic;
using Coffee.UIEffects;

namespace Scaffold
{
    public static class ScaffoldUIEffectRegistry
    {
        private static readonly HashSet<UIEffect> trackedEffects = new HashSet<UIEffect>();

        public static void Track(UIEffect effect)
        {
            if (effect != null)
            {
                trackedEffects.Add(effect);
            }
        }

        public static void ClearAll()
        {
            foreach (UIEffect effect in trackedEffects)
            {
                if (effect != null)
                {
                    effect.Clear();
                }
            }
            trackedEffects.Clear();
        }
    }
}
