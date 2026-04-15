using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed partial class RaceRuntimeState : Model
    {
        [ObservableProperty] private int currentLap;
        [ObservableProperty] private float currentTime;
        [ObservableProperty] private float progress01;
        [ObservableProperty] private int currentSegmentIndex;
        [ObservableProperty] private float currentSpeed;
        [ObservableProperty] private bool isDrifting;
        [ObservableProperty] private bool isOvershot;
        [ObservableProperty] private float distanceTravelled;

        public void Reset()
        {
            CurrentLap = 0;
            CurrentTime = 0f;
            Progress01 = 0f;
            CurrentSegmentIndex = 0;
            CurrentSpeed = 0f;
            IsDrifting = false;
            IsOvershot = false;
            DistanceTravelled = 0f;
        }
    }
}
