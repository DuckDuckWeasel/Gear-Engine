using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.Entities;
using Scaffold.MVVM;

namespace Game.CarSimulation
{
    public partial class TrackViewModel : ViewModel
    {
        private readonly List<IDisposable> attributeMirrorSubscriptions = new List<IDisposable>();

        public TrackDefinition Track { get; }

        [ObservableProperty]
        private CarEntity car;

        [ObservableProperty]
        private bool isRunning;

        [ObservableProperty]
        private float currentSpeed;

        [ObservableProperty]
        private float trackProgress01;

        public TrackViewModel(TrackDefinition track, CarEntity car = null)
        {
            Track = track ?? throw new ArgumentNullException(nameof(track));
            Car = car;
            RegisterAttributeMirrors();
        }

        internal void SetRunning(bool running)
        {
            IsRunning = running;
        }

        internal void TearDown()
        {
            ClearAttributeMirrors();
        }

        private void RegisterAttributeMirrors()
        {
            ClearAttributeMirrors();
            if (Car?.Instance == null)
            {
                return;
            }

            TrySubscribeFirstFloatMirror();
        }

        private void TrySubscribeFirstFloatMirror()
        {
            foreach (AttributeEntry entry in Car.Definition.Entries)
            {
                if (entry == null || entry.Attribute == null || entry.Attribute.ValueType != AttributeValueType.Float)
                {
                    continue;
                }

                AttributeSO attributeSo = entry.Attribute;
                IDisposable subscription = Car.Instance.SubscribeToAttribute<FloatAttributeValue>(attributeSo, OnFloatMirrorChanged);
                attributeMirrorSubscriptions.Add(subscription);
                return;
            }
        }

        private void OnFloatMirrorChanged(FloatAttributeValue value)
        {
            CurrentSpeed = value.Value;
        }

        private void ClearAttributeMirrors()
        {
            for (int i = 0; i < attributeMirrorSubscriptions.Count; i++)
            {
                attributeMirrorSubscriptions[i]?.Dispose();
            }

            attributeMirrorSubscriptions.Clear();
        }
    }
}
