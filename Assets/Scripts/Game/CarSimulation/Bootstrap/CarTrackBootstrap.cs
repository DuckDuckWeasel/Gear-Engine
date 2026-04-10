using UnityEngine.Splines;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public class CarTrackBootstrap : IInitializable
    {
        private readonly CarFactory carFactory;
        private readonly CarDefinition carDefinition;
        private readonly SplineContainer splineContainer;

        public CarTrackBootstrap(CarFactory carFactory, CarDefinition carDefinition, SplineContainer splineContainer)
        {
            this.carFactory = carFactory;
            this.carDefinition = carDefinition;
            this.splineContainer = splineContainer;
        }

        public void Initialize()
        {
            var carEntity = carFactory.Create(carDefinition);
            carEntity.GetComponent<CarSplineDriver>().Initialize(splineContainer);
        }
    }
}
