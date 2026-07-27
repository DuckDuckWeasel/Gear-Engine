
using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Material variable type.
    /// </summary>
    [VariableInfo("Other", "Material")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class MaterialVariable : VariableBase<Material>
    {
    }

    /// <summary>
    /// Container for a Material variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct MaterialData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(MaterialVariable))]
        public MaterialVariable materialRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public MaterialValueSO materialSO;
        [SerializeField]
        public Material materialVal;

        public MaterialData(Material v)
        {
            materialVal = v;
            materialRef = null;
            source = VariableDataSource.Unspecified;
            materialSO = null;
        }

        public static implicit operator Material(MaterialData materialData)
        {
            return materialData.Value;
        }

        public Material Value
        {
            get { return VariableValueReference.Resolve(materialRef, materialVal, materialSO, source); }
            set { VariableValueReference.Assign(materialRef, ref materialVal, materialSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(materialRef, materialVal, materialSO, source);
        }
    }
}