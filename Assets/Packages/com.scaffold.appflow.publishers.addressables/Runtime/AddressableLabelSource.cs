using System;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    [Serializable]
    public partial class AddressableLabelSource : IAssetPublisherSource
    {
        [SerializeField]
        private AssetLabelReference label = new();

        [SerializeField]
        [AddressableLabelTypeDropdown(nameof(label))]
        [Tooltip("Asset type used for typed PublishMany. Auto-resolved when the label points at a single type; otherwise pick one of the types found among the entries.")]
        private string typeAqn = string.Empty;

        public AssetLabelReference Label => label;

        public string TypeAqn => typeAqn;

        /// <inheritdoc />
        public IPublisherRegistrar TryCreateRuntimeBakedRegistrar()
        {
            if (label == null || string.IsNullOrEmpty(label.labelString) || string.IsNullOrEmpty(typeAqn))
            {
                return null;
            }

            Type t = Type.GetType(typeAqn, throwOnError: false);
            if (t == null || !typeof(UnityEngine.Object).IsAssignableFrom(t))
            {
                return null;
            }

            Type closed = typeof(AddressableLabelPublisherRegistrar<>).MakeGenericType(t);
            return (IPublisherRegistrar)Activator.CreateInstance(closed, label.labelString);
        }
    }
}
