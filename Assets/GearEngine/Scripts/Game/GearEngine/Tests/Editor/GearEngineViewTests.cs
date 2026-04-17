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
        }

        private sealed class FakeDragService : IDragService
        {
            private readonly List<IDragTarget> targets = new List<IDragTarget>();

            public bool IsDragging { get; private set; }
            private object dragData;

            public T GetDragData<T>() where T : class => dragData as T;

            public void Register(IDragTarget target)
            {
                if (target != null && !targets.Contains(target))
                {
                    targets.Add(target);
                }
            }

            public void Unregister(IDragTarget target)
            {
                if (target != null)
                {
                    targets.Remove(target);
                }
            }

            public void StartDrag(object data)
            {
                dragData = data;
                IsDragging = true;
                var payload = new DragPayload(dragData, Vector3.zero, null);
                foreach (IDragTarget t in targets.ToArray())
                {
                    t.OnDragStarted(payload);
                }
            }

            public void EndDrag()
            {
                dragData = null;
                IsDragging = false;
                foreach (IDragTarget t in targets.ToArray())
                {
                    t.OnDragEnded();
                }
            }
        }
    }
}
