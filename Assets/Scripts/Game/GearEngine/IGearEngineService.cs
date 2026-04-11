namespace Game.GearEngine
{
    public interface IGearEngineService
    {
        bool IsRunning { get; }
        void Play();
        void Stop();
    }
}
