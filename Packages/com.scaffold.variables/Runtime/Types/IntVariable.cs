using System;
using UnityEngine;

namespace Scaffold.Variables
{
    [CreateAssetMenu(menuName = "Scaffold/Variables/Int", fileName = "IntVariable")]
    public class IntVariable : VariableSO<int> { }

    [Serializable]
    public class IntReference : VariableReference<int, IntVariable> 
    { 
        public IntReference() { }
        public IntReference(int value) : base(value) { }
    }
}
