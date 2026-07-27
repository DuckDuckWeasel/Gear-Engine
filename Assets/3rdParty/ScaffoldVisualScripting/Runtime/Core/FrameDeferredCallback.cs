using System;

namespace Scaffold.VisualScripting
{
    internal sealed class FrameDeferredCallback : IDisposable
    {
        public FrameDeferredCallback(IFrameScheduler scheduler)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        private readonly IFrameScheduler scheduler;
        private IDisposable scheduledFrame;
        private Action callback;
        private int remainingFrames;
        private bool disposed;

        public void Schedule(int frameCount, Action scheduledCallback)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(FrameDeferredCallback));
            }

            Cancel();
            callback = scheduledCallback ?? throw new ArgumentNullException(nameof(scheduledCallback));
            remainingFrames = Math.Max(frameCount, 0);
            ScheduleNext();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Cancel();
            disposed = true;
        }

        public void Cancel()
        {
            scheduledFrame?.Dispose();
            scheduledFrame = null;
            callback = null;
            remainingFrames = 0;
        }

        private void OnNextFrame()
        {
            scheduledFrame = null;
            remainingFrames--;
            ScheduleNext();
        }

        private void ScheduleNext()
        {
            if (remainingFrames <= 0)
            {
                InvokeCallback();
                return;
            }

            scheduledFrame = scheduler.ScheduleNextFrame(OnNextFrame);
        }

        private void InvokeCallback()
        {
            Action scheduledCallback = callback;
            callback = null;
            scheduledCallback?.Invoke();
        }
    }
}
