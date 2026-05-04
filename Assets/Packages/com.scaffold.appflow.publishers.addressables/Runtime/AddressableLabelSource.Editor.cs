#if UNITY_EDITOR
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine.AddressableAssets;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    public partial class AddressableLabelSource
    {
        /// <summary>Editor: sets label + <see cref="TypeAqn"/> for bootstrap / migration seeding.</summary>
        public void SetLabelAndTypeForEditor(string labelString, string assemblyQualifiedTypeName)
        {
            if (label == null)
            {
                label = new AssetLabelReference();
            }

            label.labelString = labelString;
            typeAqn = assemblyQualifiedTypeName;
        }

        public bool IsConfigured
        {
            get
            {
                if (Label == null || string.IsNullOrEmpty(Label.labelString))
                {
                    return false;
                }

                return AddressableBakeUtility.ResolveLabelBakeType(Label.labelString, TypeAqn) != null;
            }
        }

        public IPublisherRegistrar Bake() => AddressableBakeUtility.BakeAddressableLabel(this);
    }
}
#endif
