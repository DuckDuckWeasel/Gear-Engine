using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Blackboard variable that stores a dialogue character.
    /// </summary>
    [VariableInfo("Narrative", "Character")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class CharacterVariable : VariableBase<Character>
    {
    }

    /// <summary>
    /// Selects a character from a direct reference, Blackboard variable, or ScriptableObject value.
    /// </summary>
    [System.Serializable]
    public struct CharacterData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(CharacterVariable))]
        public CharacterVariable characterRef;

        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public CharacterValueSO characterSO;

        [SerializeField]
        public Character characterVal;

        public CharacterData(Character value)
        {
            characterRef = null;
            source = VariableDataSource.Unspecified;
            characterSO = null;
            characterVal = value;
        }

        public Character Value
        {
            get { return VariableValueReference.Resolve(characterRef, characterVal, characterSO, source); }
            set { VariableValueReference.Assign(characterRef, ref characterVal, characterSO, source, value); }
        }

        public bool IsConfigured
        {
            get
            {
                return source != VariableDataSource.Unspecified ||
                       characterRef != null ||
                       characterSO != null ||
                       characterVal != null;
            }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(characterRef, characterVal, characterSO, source);
        }
    }
}
