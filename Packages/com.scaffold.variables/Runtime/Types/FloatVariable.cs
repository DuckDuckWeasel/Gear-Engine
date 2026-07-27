using System;
using UnityEngine;

namespace Scaffold.Variables
{
    [CreateAssetMenu(menuName = "Scaffold/Variables/Float", fileName = "FloatVariable")]
    public class FloatVariable : VariableSO<float> { }

    [Serializable]
    public class FloatReference : VariableReference<float, FloatVariable> 
    { 
        public FloatReference() { }
        public FloatReference(float value) : base(value) { }
    }
}
