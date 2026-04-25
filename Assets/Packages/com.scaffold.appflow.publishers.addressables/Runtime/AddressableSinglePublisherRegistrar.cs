using System;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    [Serializable]
    [UnityEngine.Scripting.Preserve]
    public sealed class AddressableSinglePublisherRegistrar<T> : IPublisherRegistrar
        where T : UnityEngine.Object
    {
        public AddressableSinglePublisherRegistrar()
        {
        }

        public AddressableSinglePublisherRegistrar(string addressableKey)
        {
            this.addressableKey = addressableKey ?? throw new ArgumentNullException(nameof(addressableKey));
        }

        [SerializeField]
        private string addressableKey = string.Empty;

        public void Register(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(addressableKey))
            {
                throw new InvalidOperationException(
                    $"[{nameof(AddressableSinglePublisherRegistrar<T>)}] addressableKey is empty; rebake the asset publisher in the editor.");
            }

            builder.Register<AddressableSinglePublisher<T>>(Lifetime.Singleton)
                .WithParameter(addressableKey)
                .AsSelf()
                .As<IAsyncInitializable>();
        }
    }
}
