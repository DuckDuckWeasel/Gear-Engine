using System;
using System.Collections;

namespace Scaffold.VisualScripting
{
    public interface IFrameScheduler
    {
        IDisposable ScheduleNextFrame(Action callback);

        IDisposable Schedule(TimeSpan delay, Action callback);

        IDisposable ScheduleRoutine(IEnumerator routine);

        void Tick(float deltaTime);
    }
}
