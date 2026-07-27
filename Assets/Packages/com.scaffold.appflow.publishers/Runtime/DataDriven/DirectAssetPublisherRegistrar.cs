using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    [Serializable]
    [UnityEngine.Scripting.Preserve]
    public sealed class DirectAssetPublisherRegistrar<T> : IPublisherRegistrar
        where T : UnityEngine.Object
    {
        [SerializeField]
        private T asset;

        public DirectAssetPublisherRegistrar()
        {
        }

        public DirectAssetPublisherRegistrar(T value)
        {
            asset = value;
        }

        public void Register(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"[{nameof(DirectAssetPublisherRegistrar<T>)}] asset is not set; rebake the publisher in the editor.");
            }

            builder.Register<DirectAssetPublisher<T>>(Lifetime.Singleton)
                .WithParameter(asset)
                .AsSelf()
                .As<IAsyncInitializable>();
        }
    }
}
