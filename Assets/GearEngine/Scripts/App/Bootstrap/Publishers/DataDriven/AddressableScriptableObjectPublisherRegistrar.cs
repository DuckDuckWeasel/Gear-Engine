using System;
using Scaffold.AppFlow;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap.Publishers.DataDriven
{
    // todo: Closed-generic registrar baked into AddressableScriptableObjectPublisherSO at edit time.
    [Serializable]
    [UnityEngine.Scripting.Preserve]
    public sealed class AddressableScriptableObjectPublisherRegistrar<T> : IPublisherRegistrar where T : ScriptableObject
    {
        public AddressableScriptableObjectPublisherRegistrar()
        {
        }

        public AddressableScriptableObjectPublisherRegistrar(string addressableKey)
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
                    $"[{nameof(AddressableScriptableObjectPublisherRegistrar<T>)}] addressableKey is empty; rebake the publisher asset.");
            }

            builder.Register<DataDrivenAddressableScriptableObjectPublisher<T>>(Lifetime.Singleton).WithParameter(addressableKey).AsSelf().As<IAsyncInitializable>();
        }
    }
}
