using System;
using Coffee.UIEffectInternal;
using UnityEngine;

namespace Coffee.UIEffects
{
    /// <summary>
    /// Describes one independently animated layer used by Transition.Pattern.
    /// </summary>
    [Serializable]
    public sealed class PatternLayer
    {
        public const int MaxCount = 4;

        public bool m_Enabled;
        public Texture m_Texture;
        [Range(0, 1)] public float m_Opacity = 1;
        public Vector2 m_TextureScale = Vector2.one;
        public Vector2 m_TextureOffset = Vector2.zero;
        public Vector2 m_TextureSpeed = Vector2.zero;
        [Range(0, 360)] public float m_Rotation;
        public bool m_KeepAspectRatio = true;
        [Range(0, 1)] public float m_Rate = 0.5f;
        public bool m_TextureReverse;
        [Range(0, 1)] public float m_Width = 0.2f;
        public MinMax01 m_Range = new MinMax01(0, 1);
        public bool m_PatternReverse;
        [Range(-5, 5)] public float m_AutoPlaySpeed;
        public ColorFilter m_ColorFilter = ColorFilter.MultiplyAdditive;
        public Color m_Color = new Color(0f, 0.5f, 1f, 1f);
        public bool m_ColorGlow;
        public PatternArea m_Area = PatternArea.Inner;

        public PatternLayer Clone()
        {
            PatternLayer clone = new PatternLayer();
            clone.CopyFrom(this);
            return clone;
        }

        public void CopyFrom(PatternLayer source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            m_Enabled = source.m_Enabled;
            m_Texture = source.m_Texture;
            m_Opacity = source.m_Opacity;
            m_TextureScale = source.m_TextureScale;
            m_TextureOffset = source.m_TextureOffset;
            m_TextureSpeed = source.m_TextureSpeed;
            m_Rotation = source.m_Rotation;
            m_KeepAspectRatio = source.m_KeepAspectRatio;
            m_Rate = source.m_Rate;
            m_TextureReverse = source.m_TextureReverse;
            m_Width = source.m_Width;
            m_Range = source.m_Range;
            m_PatternReverse = source.m_PatternReverse;
            m_AutoPlaySpeed = source.m_AutoPlaySpeed;
            m_ColorFilter = source.m_ColorFilter;
            m_Color = source.m_Color;
            m_ColorGlow = source.m_ColorGlow;
            m_Area = source.m_Area;
        }

        internal static PatternLayer[] CloneFixed(PatternLayer[] source)
        {
            PatternLayer[] layers = new PatternLayer[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                layers[i] = source != null && i < source.Length && source[i] != null
                    ? source[i].Clone()
                    : new PatternLayer { m_Enabled = false };
            }

            return layers;
        }
    }
}
