using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    [Serializable]
    public sealed class DirectAssetSource : IAssetPublisherSource
    {
        [SerializeField]
        private UnityEngine.Object asset;

        public UnityEngine.Object Asset => asset;

#if UNITY_EDITOR
        public bool IsConfigured => asset != null;

        public IPublisherRegistrar Bake()
        {
            if (asset == null)
            {
                return null;
            }

            Type closed = typeof(DirectAssetPublisherRegistrar<>).MakeGenericType(asset.GetType());
            return (IPublisherRegistrar)Activator.CreateInstance(closed, asset);
        }
#endif
    }
}
