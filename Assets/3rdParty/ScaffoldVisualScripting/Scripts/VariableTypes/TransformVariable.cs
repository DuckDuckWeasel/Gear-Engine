
using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Transform variable type.
    /// </summary>
    [VariableInfo("Other", "Transform")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class TransformVariable : VariableBase<Transform>
    {
    }

    /// <summary>
    /// Container for a Transform variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct TransformData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TransformVariable))]
        public TransformVariable transformRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public TransformValueSO transformSO;
        [SerializeField]
        public Transform transformVal;

        public TransformData(Transform v)
        {
            transformVal = v;
            transformRef = null;
            source = VariableDataSource.Unspecified;
            transformSO = null;
        }

        public static implicit operator Transform(TransformData vector3Data)
        {
            return vector3Data.Value;
        }

        public Transform Value
        {
            get { return VariableValueReference.Resolve(transformRef, transformVal, transformSO, source); }
            set { VariableValueReference.Assign(transformRef, ref transformVal, transformSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(transformRef, transformVal, transformSO, source);
        }
    }
}