using System;

namespace Scaffold.VisualScripting
{
    public interface IFrameScheduler
    {
        IDisposable ScheduleNextFrame(Action callback);

        IDisposable Schedule(TimeSpan delay, Action callback);

        void Tick(float deltaTime);
    }
}
