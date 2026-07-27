
using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("Other", "Object")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ObjectVariable : VariableBase<Object>
    {
    }

    /// <summary>
    /// Container for an Object variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct ObjectData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public ObjectVariable objectRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public ObjectValueSO objectSO;
        [SerializeField]
        public Object objectVal;

        public ObjectData(Object v)
        {
            objectVal = v;
            objectRef = null;
            source = VariableDataSource.Unspecified;
            objectSO = null;
        }

        public static implicit operator Object(ObjectData objectData)
        {
            return objectData.Value;
        }

        public Object Value
        {
            get { return VariableValueReference.Resolve(objectRef, objectVal, objectSO, source); }
            set { VariableValueReference.Assign(objectRef, ref objectVal, objectSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(objectRef, objectVal, objectSO, source);
        }
    }
}