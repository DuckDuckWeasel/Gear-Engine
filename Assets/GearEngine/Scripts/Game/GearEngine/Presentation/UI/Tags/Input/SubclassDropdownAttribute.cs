using UnityEngine;
using System;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    public class SubclassDropdownAttribute : PropertyAttribute
    {
        public string BaseTypeName { get; private set; }

        public SubclassDropdownAttribute(string baseTypeName)
        {
            BaseTypeName = baseTypeName;
        }
    }
}
