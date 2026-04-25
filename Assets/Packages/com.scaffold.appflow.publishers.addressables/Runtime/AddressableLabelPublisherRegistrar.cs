using System;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    [Serializable]
    [UnityEngine.Scripting.Preserve]
    public sealed class AddressableLabelPublisherRegistrar<T> : IPublisherRegistrar
        where T : UnityEngine.Object
    {
        public AddressableLabelPublisherRegistrar()
        {
        }

        public AddressableLabelPublisherRegistrar(string label)
        {
            labelString = label ?? throw new ArgumentNullException(nameof(label));
        }

        [SerializeField]
        private string labelString = string.Empty;

        public void Register(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(labelString))
            {
                throw new InvalidOperationException(
                    $"[{nameof(AddressableLabelPublisherRegistrar<T>)}] label is empty; rebake the asset publisher in the editor.");
            }

            builder.Register<AddressableLabelPublisher<T>>(Lifetime.Singleton)
                .WithParameter(labelString)
                .AsSelf()
                .As<IAsyncInitializable>();
        }
    }
}
