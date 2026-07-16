using System;
using UnityEngine;

namespace Scaffold.Variables
{
    [CreateAssetMenu(menuName = "Scaffold/Variables/Bool", fileName = "BoolVariable")]
    public class BoolVariable : VariableSO<bool> { }

    [Serializable]
    public class BoolReference : VariableReference<bool, BoolVariable> 
    { 
        public BoolReference() { }
        public BoolReference(bool value) : base(value) { }
    }
}
