using System;
using UnityEngine;

namespace Scaffold
{
    [Serializable]
    public sealed class ObjectValue
    {
        [Tooltip("The Type assemblyname")]
        public string TypeAssemblyName;

        [Tooltip("The Type fullname")]
        public string TypeFullName;

        [Tooltip("The Int value")]
        public int IntValue;

        [Tooltip("The Bool value")]
        public bool BoolValue;

        [Tooltip("The Float value")]
        public float FloatValue;

        [Tooltip("The String value")]
        public string StringValue;

        [Tooltip("The Color value")]
        public Color ColorValue;

        [Tooltip("The Game object value")]
        public GameObject GameObjectValue;

        [Tooltip("The Material value")]
        public Material MaterialValue;

        public UnityEngine.Object UnityObjectValue;

        [Tooltip("The Sprite value")]
        public Sprite SpriteValue;

        [Tooltip("The Texture value")]
        public Texture TextureValue;

        [Tooltip("The Vector2 value")]
        public Vector2 Vector2Value;

        [Tooltip("The Vector3 value")]
        public Vector3 Vector3Value;

        public object GetValue()
        {
            if (TypeFullName != null && TypeFullName.StartsWith("System.", StringComparison.Ordinal))
            {
                return GetSystemValue();
            }

            return GetUnityValue();
        }

        private object GetSystemValue()
        {
            switch (TypeFullName)
            {
                case "System.Int32": return IntValue;
                case "System.Boolean": return BoolValue;
                case "System.Single": return FloatValue;
                case "System.String": return StringValue;
                default: return GetReflectedValue();
            }
        }

        private object GetUnityValue()
        {
            switch (TypeFullName)
            {
                case "UnityEngine.Color": return ColorValue;
                case "UnityEngine.GameObject": return GameObjectValue;
                case "UnityEngine.Material": return MaterialValue;
                case "UnityEngine.Sprite": return SpriteValue;
                case "UnityEngine.Texture": return TextureValue;
                case "UnityEngine.Vector2": return Vector2Value;
                case "UnityEngine.Vector3": return Vector3Value;
                default: return GetReflectedValue();
            }
        }

        private object GetReflectedValue()
        {
            Type objectType = ReflectionHelper.GetType(TypeAssemblyName);
            if (objectType != null &&
                objectType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return UnityObjectValue;
            }

            return objectType != null && objectType.IsEnum
                ? Enum.ToObject(objectType, IntValue)
                : null;
        }
    }
}
