
using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// GameObject variable type.
    /// </summary>
    [VariableInfo("Other", "GameObject")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class GameObjectVariable : VariableBase<GameObject>
    {
    }

    /// <summary>
    /// Container for a GameObject variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct GameObjectData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(GameObjectVariable))]
        public GameObjectVariable gameObjectRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public GameObjectValueSO gameObjectSO;
        [SerializeField]
        public GameObject gameObjectVal;

        public GameObjectData(GameObject v)
        {
            gameObjectVal = v;
            gameObjectRef = null;
            source = VariableDataSource.Unspecified;
            gameObjectSO = null;
        }

        public static implicit operator GameObject(GameObjectData gameObjectData)
        {
            return gameObjectData.Value;
        }

        public GameObject Value
        {
            get { return VariableValueReference.Resolve(gameObjectRef, gameObjectVal, gameObjectSO, source); }
            set { VariableValueReference.Assign(gameObjectRef, ref gameObjectVal, gameObjectSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(gameObjectRef, gameObjectVal, gameObjectSO, source);
        }
    }
}