using VContainer.Unity;

namespace Game.CarSimulation
{
    public class CarTrackBootstrap : IInitializable
    {
        private readonly CarFactory carFactory;
        private readonly CarDefinition carDefinition;
        private readonly TrackDefinition trackDefinition;
        private readonly Track track;

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
            carEntity.GetComponent<CarSplineDriver>().Initialize(track.SplineContainer);
        }
    }
}
