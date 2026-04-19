using System;
using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public class GearEngineViewTests
    {
        private sealed class FakeEngine : IGearEngineService
        {
            public bool IsRunning => false;
            public void Play()
            {
            }

            public void Stop()
            {
            }

            public void ResetGridSimulationState()
            {
            }
        }

        private sealed class FakeDragService : IDragService
        {
            private readonly List<IDragLifecycleListener> listeners = new List<IDragLifecycleListener>();

            public bool IsDragging { get; private set; }

            public void Register(IDragLifecycleListener listener)
            {
                if (listener != null && !listeners.Contains(listener))
                {
                    listeners.Add(listener);
                }
            }

            public void Unregister(IDragLifecycleListener listener)
            {
                if (listener != null)
                {
                    listeners.Remove(listener);
                }
            }

            public void StartDrag(DragPayload payload)
            {
                IsDragging = true;
                foreach (IDragLifecycleListener l in listeners.ToArray())
                {
                    l.OnDragStarted(payload);
                }
            }

            public void EndDrag()
            {
                IsDragging = false;
                foreach (IDragLifecycleListener l in listeners.ToArray())
                {
                    l.OnDragEnded();
                }
            }
        }
    }
}
