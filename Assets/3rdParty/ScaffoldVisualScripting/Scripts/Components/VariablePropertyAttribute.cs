using System;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Declares the compatibility variable types accepted by a serialized field.
    /// </summary>
    public sealed class VariablePropertyAttribute : PropertyAttribute
    {
        public VariablePropertyAttribute(params Type[] variableTypes)
        {
            VariableTypes = variableTypes;
        }

        public VariablePropertyAttribute(AllVariableTypes.VariableAny any)
        {
            VariableTypes = AllVariableTypes.AllScaffoldVarTypes;
        }

        public VariablePropertyAttribute(
            string defaultText,
            params Type[] variableTypes)
        {
            this.defaultText = defaultText;
            VariableTypes = variableTypes;
        }

        public string defaultText = "<None>";

        public string compatibleVariableName = string.Empty;

        public Type[] VariableTypes { get; set; }
    }
}
