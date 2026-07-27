using System;
using UnityEngine;

namespace Scaffold.Variables
{
    [CreateAssetMenu(menuName = "Scaffold/Variables/String", fileName = "StringVariable")]
    public class StringVariable : VariableSO<string> { }

    [Serializable]
    public class StringReference : VariableReference<string, StringVariable> 
    { 
        public StringReference() { }
        public StringReference(string value) : base(value) { }
    }
}
