using System;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Selects a direct GameObject, a plain Blackboard variable, or a value asset.
    /// </summary>
    [Serializable]
    public struct GameObjectData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(GameObjectVariable))]
        public GameObjectVariable gameObjectRef;

        [SerializeField] public VariableDataSource source;
        [SerializeField] public GameObjectValueSO gameObjectSO;
        [SerializeField] public GameObject gameObjectVal;

        public GameObjectData(GameObject value)
        {
            gameObjectVal = value;
            gameObjectRef = null;
            source = VariableDataSource.Direct;
            gameObjectSO = null;
        }

        public GameObject Value
        {
            get
            {
                return VariableValueReference.Resolve(
                    gameObjectRef,
                    gameObjectVal,
                    gameObjectSO,
                    source);
            }
            set
            {
                VariableValueReference.Assign(
                    gameObjectRef,
                    ref gameObjectVal,
                    gameObjectSO,
                    source,
                    value);
            }
        }

        public static implicit operator GameObject(
            GameObjectData gameObjectData)
        {
            return gameObjectData.Value;
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(
                gameObjectRef,
                gameObjectVal,
                gameObjectSO,
                source);
        }
    }
}
