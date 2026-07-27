using System;
using System.Collections;
using System.Collections.Generic;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class UnityFrameScheduler : IFrameScheduler, IDisposable
    {
        public UnityFrameScheduler(UnityCoroutineRunner coroutineRunner, IBlackboardLogger logger)
        {
            this.coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly UnityCoroutineRunner coroutineRunner;
        private readonly IBlackboardLogger logger;
        private readonly List<ScheduledWork> scheduledWork = new List<ScheduledWork>();
        private readonly List<IDisposable> routineHandles = new List<IDisposable>();
        private long frame;
        private double elapsedSeconds;
        private bool disposed;

        public IDisposable ScheduleNextFrame(Action callback)
        {
            ThrowIfDisposed();
            return AddScheduledWork(new ScheduledWork(callback, frame + 1L, 0d, true));
        }

        public IDisposable Schedule(TimeSpan delay, Action callback)
        {
            ThrowIfDisposed();
            double dueTime = elapsedSeconds + Math.Max(delay.TotalSeconds, 0d);
            return AddScheduledWork(new ScheduledWork(callback, 0L, dueTime, false));
        }

        public IDisposable ScheduleRoutine(IEnumerator routine)
        {
            ThrowIfDisposed();
            IDisposable handle = coroutineRunner.Run(routine);
            TrackedHandle tracked = new TrackedHandle(handle, RemoveRoutine);
            routineHandles.Add(tracked);
            return tracked;
        }

        public void Tick(float deltaTime)
        {
            ThrowIfDisposed();
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
            }

            frame++;
            elapsedSeconds += deltaTime;
            ExecuteDueWork();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            DisposeScheduledWork();
            DisposeRoutines();
            disposed = true;
        }

        private IDisposable AddScheduledWork(ScheduledWork work)
        {
            scheduledWork.Add(work);
            return work;
        }

        private void ExecuteDueWork()
        {
            for (int index = scheduledWork.Count - 1; index >= 0; index--)
            {
                ExecuteWorkAt(index);
            }
        }

        private void ExecuteWorkAt(int index)
        {
            ScheduledWork work = scheduledWork[index];
            if (work.IsCancelled)
            {
                scheduledWork.RemoveAt(index);
                return;
            }

            if (work.IsDue(frame, elapsedSeconds))
            {
                scheduledWork.RemoveAt(index);
                Execute(work);
            }
        }

        private void Execute(ScheduledWork work)
        {
            try
            {
                work.Execute();
            }
            catch (Exception exception)
            {
                logger.Error("A scheduled Blackboard callback failed.", exception);
            }
        }

        private void RemoveRoutine(TrackedHandle handle)
        {
            routineHandles.Remove(handle);
        }

        private void DisposeScheduledWork()
        {
            foreach (ScheduledWork work in scheduledWork)
            {
                work.Dispose();
            }

            scheduledWork.Clear();
        }

        private void DisposeRoutines()
        {
            IDisposable[] handles = routineHandles.ToArray();
            foreach (IDisposable handle in handles)
            {
                handle.Dispose();
            }

            routineHandles.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(UnityFrameScheduler));
            }
        }

        private sealed class ScheduledWork : IDisposable
        {
            public ScheduledWork(Action callback, long dueFrame, double dueTime, bool frameBased)
            {
                this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
                this.dueFrame = dueFrame;
                this.dueTime = dueTime;
                this.frameBased = frameBased;
            }

            public bool IsCancelled => callback == null;

            private Action callback;
            private readonly long dueFrame;
            private readonly double dueTime;
            private readonly bool frameBased;

            public bool IsDue(long currentFrame, double currentTime)
            {
                return frameBased ? currentFrame >= dueFrame : currentTime >= dueTime;
            }

            public void Execute()
            {
                Action scheduledCallback = callback;
                callback = null;
                scheduledCallback?.Invoke();
            }

            public void Dispose()
            {
                callback = null;
            }
        }

        private sealed class TrackedHandle : IDisposable
        {
            public TrackedHandle(IDisposable inner, Action<TrackedHandle> onDisposed)
            {
                this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
                this.onDisposed = onDisposed ?? throw new ArgumentNullException(nameof(onDisposed));
            }

            private IDisposable inner;
            private Action<TrackedHandle> onDisposed;

            public void Dispose()
            {
                if (inner == null)
                {
                    return;
                }

                inner.Dispose();
                inner = null;
                Action<TrackedHandle> callback = onDisposed;
                onDisposed = null;
                callback?.Invoke(this);
            }
        }
    }
}
