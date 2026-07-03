using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class RaceDriftScoreViewModel : ViewModel
    {
        private const float DefaultBaseRate = 100f;
        private const float DefaultGraceTime = 1.5f;

        private readonly RaceState session;
        private readonly CarViewModel carViewModel;
        private readonly float baseRate;
        private readonly float graceTimeLimit;

        [ObservableProperty] private float currentPoints;
        [ObservableProperty] private int displayPoints;
        [ObservableProperty] private int currentMultiplier = 1;
        [ObservableProperty] private bool isDisplayingScore;

        public int TotalDriftScore => session.TotalDriftScore;

        public event Action MultiplierIncreased;
        public event Action ScoreBanked;

        private float graceTimer = 0f;
        private bool wasDriftingLastFrame = false;

        public RaceDriftScoreViewModel(RaceState session, CarViewModel carViewModel, float baseRate = DefaultBaseRate, float graceTimeLimit = DefaultGraceTime)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.carViewModel = carViewModel ?? throw new ArgumentNullException(nameof(carViewModel));
            this.baseRate = baseRate;
            this.graceTimeLimit = graceTimeLimit;
        }

        public void Tick(float deltaTime)
        {
            if (session.Phase != SimulationLifecycleState.Running)
            {
                return;
            }

            bool isDrifting = carViewModel.IsDrifting;

            if (isDrifting)
            {
                if (!wasDriftingLastFrame)
                {
                    // Started drifting
                    if (graceTimer > 0f && graceTimer <= graceTimeLimit && IsDisplayingScore)
                    {
                        // Resumed within grace period
                        CurrentMultiplier++;
                        MultiplierIncreased?.Invoke();
                    }
                    else if (!IsDisplayingScore)
                    {
                        // Fresh start
                        IsDisplayingScore = true;
                        CurrentPoints = 0f;
                        CurrentMultiplier = 1;
                        DisplayPoints = 0;
                    }
                }

                CurrentPoints += baseRate * deltaTime;
                DisplayPoints = (int)CurrentPoints;
                graceTimer = 0f; // Reset grace timer while drifting
            }
            else
            {
                if (wasDriftingLastFrame)
                {
                    // Stopped drifting
                    graceTimer = 0f;
                }
                
                if (IsDisplayingScore)
                {
                    graceTimer += deltaTime;
                    
                    if (graceTimer > graceTimeLimit)
                    {
                        BankScore();
                    }
                }
            }

            wasDriftingLastFrame = isDrifting;
        }

        private void BankScore()
        {
            if (CurrentPoints > 0)
            {
                session.TotalDriftScore += (int)CurrentPoints * CurrentMultiplier;
                ScoreBanked?.Invoke();
            }
            
            IsDisplayingScore = false;
            CurrentPoints = 0f;
            CurrentMultiplier = 1;
            DisplayPoints = 0;
        }
    }
}
