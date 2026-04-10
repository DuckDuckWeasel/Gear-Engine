using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.CarSimulation
{
    public class CarSplineDriver : MonoBehaviour
    {
        [SerializeField] private SplineAnimate splineAnimate;
        [SerializeField] private AttributeSO speedAttribute;
        private CarEntity carEntity;

        public void Initialize(SplineContainer container)
        {
            splineAnimate.Container = container;
        }

        private void Start()
        {
            carEntity = GetComponent<CarEntity>();
            splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
            splineAnimate.Easing = SplineAnimate.EasingMode.None;
            carEntity.Subscribe(speedAttribute, OnSpeedChanged);
            splineAnimate.Play();
        }

        private void OnSpeedChanged(AttributeValue value)
        {
            if (value is FloatAttributeValue f)
            {
                splineAnimate.MaxSpeed = f.Value;
            }
        }

        private void OnDestroy()
        {
            carEntity?.Unsubscribe(speedAttribute, OnSpeedChanged);
        }
    }
}
