using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
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
        }

        private sealed class FakeDragService : IDragService
        {
            public bool IsDragging { get; private set; }
            private object dragData;

            public event Action<object> OnDragStarted;
            public event Action OnDragEnded;

            public T GetDragData<T>() where T : class => dragData as T;

            public void StartDrag(object data)
            {
                dragData = data;
                IsDragging = true;
                OnDragStarted?.Invoke(data);
            }

            public void EndDrag()
            {
                dragData = null;
                IsDragging = false;
                OnDragEnded?.Invoke();
            }
        }
    }
}
