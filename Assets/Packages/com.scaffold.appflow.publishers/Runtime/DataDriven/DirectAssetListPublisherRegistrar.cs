using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    [Serializable]
    [UnityEngine.Scripting.Preserve]
    public sealed class DirectAssetListPublisherRegistrar<T> : IPublisherRegistrar
        where T : UnityEngine.Object
    {
        [SerializeField]
        private List<T> assets = new();

        public DirectAssetListPublisherRegistrar()
        {
        }

        public DirectAssetListPublisherRegistrar(IReadOnlyList<T> source)
        {
            if (source != null)
            {
                assets = new List<T>(source);
            }
        }

        public void Register(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (assets == null || assets.Count == 0)
            {
                throw new InvalidOperationException(
                    $"[{nameof(DirectAssetListPublisherRegistrar<T>)}] assets is empty; rebake the publisher in the editor.");
            }

            IReadOnlyList<T> asReadonly = assets;
            builder.Register<DirectAssetListPublisher<T>>(Lifetime.Singleton)
                .WithParameter(asReadonly)
                .AsSelf()
                .As<IAsyncInitializable>();
        }
    }
}
