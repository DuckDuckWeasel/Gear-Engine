using System;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public class CarTrackBootstrap : IInitializable, IRaceDriver
    {
        private readonly CarFactory carFactory;
        private readonly CarDefinition carDefinition;
        private readonly TrackDefinition trackDefinition;
        private readonly Track track;
        private CarSplineDriver carDriver;

        public CarTrackBootstrap(CarFactory carFactory, CarDefinition carDefinition, TrackDefinition trackDefinition, Track track)
        {
            this.carFactory = carFactory;
            this.carDefinition = carDefinition;
            this.trackDefinition = trackDefinition;
            this.track = track;
        }

        public void Initialize()
        {
            track.Initialize(trackDefinition);
            var carEntity = carFactory.Create(carDefinition);
            carDriver = carEntity.GetComponent<CarSplineDriver>();
            carDriver.Initialize(track.SplineContainer);
        }

        public void StartDriving()
        {
            if (carDriver == null)
            {
                throw new InvalidOperationException(
                    "[CarTrackBootstrap] StartDriving called before Initialize.");
            }

            carDriver.StartDriving();
        }
    }
}
