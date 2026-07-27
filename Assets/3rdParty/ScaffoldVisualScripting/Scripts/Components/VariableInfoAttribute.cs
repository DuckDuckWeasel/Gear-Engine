using System;

namespace Scaffold
{
    /// <summary>
    /// Describes a legacy compatibility variable type.
    /// </summary>
    public sealed class VariableInfoAttribute : Attribute
    {
        public VariableInfoAttribute(
            string category,
            string variableType,
            int order = 0,
            bool isPreviewedOnly = false)
        {
            Category = category;
            VariableType = variableType;
            Order = order;
            IsPreviewedOnly = isPreviewedOnly;
        }

        public string Category { get; set; }

        public string VariableType { get; set; }

        public int Order { get; set; }

        public bool IsPreviewedOnly { get; set; }
    }
}
