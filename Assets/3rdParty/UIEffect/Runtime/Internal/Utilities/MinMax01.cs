using System;
using UnityEngine;

namespace Coffee.UIEffectInternal
{
    [Serializable]
    public struct MinMax01
    {
        [SerializeField]
        private float m_Min;

        [SerializeField]
        private float m_Max;

        public MinMax01(float min, float max)
        {
            m_Min = Mathf.Clamp01(Mathf.Min(min, max));
            m_Max = Mathf.Clamp01(Mathf.Max(min, max));
        }

        public float min
        {
            get => m_Min;
            set
            {
                m_Min = Mathf.Clamp01(value);
                m_Max = Mathf.Max(value, m_Max);
            }
        }

        public float max
        {
            get => m_Max;
            set
            {
                m_Max = Mathf.Clamp01(value);
                m_Min = Mathf.Min(value, m_Min);
            }
        }

        public float average => (m_Max + m_Min) * 0.5f;

        public bool Approximately(MinMax01 other)
        {
            return Mathf.Approximately(m_Min, other.m_Min) && Mathf.Approximately(m_Max, other.m_Max);
        }
    }
}
