using UnityEngine;

namespace GearEngine.CarSimulation.Simulation
{
    public class CarAreaSensor : MonoBehaviour
    {
        public SplineCarRunnerService service;
        public PrometeoCarController car;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CarAreaModifier modifier))
            {
                if (service != null && car != null)
                {
                    service.AddAreaModifier(car, modifier);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out CarAreaModifier modifier))
            {
                if (service != null && car != null)
                {
                    service.RemoveAreaModifier(car, modifier);
                }
            }
        }
    }
}
