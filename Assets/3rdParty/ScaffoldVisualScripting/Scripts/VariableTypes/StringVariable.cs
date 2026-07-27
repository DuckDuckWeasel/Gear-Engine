
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// String variable type.
    /// </summary>
    [VariableInfo("", "String")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class StringVariable : VariableBase<string>
    {
    }

    /// <summary>
    /// Container for a string variable reference or constant value.
    /// Appears as a single line property in the inspector.
    /// For a multi-line property, use StringDataMulti.
    /// </summary>
    [System.Serializable]
    public struct StringData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(StringVariable))]
        public StringVariable stringRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public StringValueSO stringSO;
        [SerializeField]
        public string stringVal;

        public StringData(string v)
        {
            stringVal = v;
            stringRef = null;
            source = VariableDataSource.Unspecified;
            stringSO = null;
        }

        public static implicit operator string(StringData spriteData)
        {
            return spriteData.Value;
        }

        public string Value
        {
            get
            {
                if (stringVal == null) stringVal = "";
                return VariableValueReference.Resolve(stringRef, stringVal, stringSO, source); }
            set { VariableValueReference.Assign(stringRef, ref stringVal, stringSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(stringRef, stringVal, stringSO, source);
        }
    }

    /// <summary>
    /// Container for a string variable reference or constant value.
    /// Appears as a multi-line property in the inspector.
    /// For a single-line property, use StringData.
    /// </summary>
    [System.Serializable]
    public struct StringDataMulti
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(StringVariable))]
        public StringVariable stringRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public StringValueSO stringSO;
        [TextArea(1,15)]
        [SerializeField]
        public string stringVal;

        public StringDataMulti(string v)
        {
            stringVal = v;
            stringRef = null;
            source = VariableDataSource.Unspecified;
            stringSO = null;
        }

        public static implicit operator string(StringDataMulti spriteData)
        {
            return spriteData.Value;
        }

        public string Value
        {
            get
            {
                if (stringVal == null) stringVal = "";
                return VariableValueReference.Resolve(stringRef, stringVal, stringSO, source); }
            set { VariableValueReference.Assign(stringRef, ref stringVal, stringSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(stringRef, stringVal, stringSO, source);
        }
    }

}