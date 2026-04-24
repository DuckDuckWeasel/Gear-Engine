using System;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    /// <summary>Closed-generic registrar baked into <see cref="AddressableScriptableObjectPublisherSO"/> at edit time.</summary>
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
