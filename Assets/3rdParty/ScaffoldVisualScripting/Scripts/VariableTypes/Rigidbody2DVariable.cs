
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Rigidbody2D variable type.
    /// </summary>
    [VariableInfo("Other", "Rigidbody2D")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Rigidbody2DVariable : VariableBase<Rigidbody2D>
    {
    }

    /// <summary>
    /// Container for a Rigidbody2D variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct Rigidbody2DData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Rigidbody2DVariable))]
        public Rigidbody2DVariable rigidbody2DRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public Rigidbody2DValueSO rigidbody2DSO;
        [SerializeField]
        public Rigidbody2D rigidbody2DVal;

        public static implicit operator Rigidbody2D(Rigidbody2DData rigidbody2DData)
        {
            return rigidbody2DData.Value;
        }

        public Rigidbody2DData(Rigidbody2D v)
        {
            rigidbody2DVal = v;
            rigidbody2DRef = null;
            source = VariableDataSource.Unspecified;
            rigidbody2DSO = null;
        }

        public Rigidbody2D Value
        {
            get { return VariableValueReference.Resolve(rigidbody2DRef, rigidbody2DVal, rigidbody2DSO, source); }
            set { VariableValueReference.Assign(rigidbody2DRef, ref rigidbody2DVal, rigidbody2DSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(rigidbody2DRef, rigidbody2DVal, rigidbody2DSO, source);
        }
    }
}