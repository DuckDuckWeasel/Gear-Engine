using UnityEngine;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    /// <summary>String field (assembly-qualified name) edited as a popup whose options are the asset types found among the entries of a sibling <see cref="UnityEngine.AddressableAssets.AssetLabelReference"/> field.</summary>
    public sealed class AddressableLabelTypeDropdownAttribute : PropertyAttribute
    {
        public AddressableLabelTypeDropdownAttribute(string siblingLabelFieldName)
        {
            SiblingLabelFieldName = siblingLabelFieldName;
        }

        public readonly string SiblingLabelFieldName;
    }
}
