using UnityEngine;

namespace GearEngine.GearEngine.Extensions
{
    public static class ObjectExtensions
    {
        public static void SafeDestroy(this Object obj)
        {
            if (obj == null) return;
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(obj);
                return;
            }
#endif
            Object.Destroy(obj);
        }
    }
}
