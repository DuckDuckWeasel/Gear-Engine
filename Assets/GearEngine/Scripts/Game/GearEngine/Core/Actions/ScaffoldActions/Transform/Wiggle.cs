using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Wiggle", "Adds a continuous wiggle/shake effect over time.")]
    [Serializable]
    [AddComponentMenu("")]
    public class Wiggle : ActionBase
    {
        [Tooltip("The Transform to wiggle")]
        [SerializeField] protected GameObjectData targetGameObject;

        [Tooltip("Wiggle position?")]
        [SerializeField] protected bool wigglePosition = true;
        [SerializeField] protected Vector3Data positionAmplitude = new Vector3Data(new Vector3(0.5f, 0.5f, 0.5f));

        [Tooltip("Wiggle rotation?")]
        [SerializeField] protected bool wiggleRotation = true;
        [SerializeField] protected Vector3Data rotationAmplitude = new Vector3Data(new Vector3(10f, 10f, 10f));

        [Tooltip("Wiggle duration")]
        [SerializeField] protected FloatData duration = new FloatData(1f);

        [Tooltip("Wiggle speed frequency")]
        [SerializeField] protected FloatData speed = new FloatData(10f);

        [Tooltip("Wait until finished?")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetGameObject.Value != null && CanRunScheduledWork)
            {
                RunRoutine(WiggleRoutine(), !waitUntilFinished);
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator WiggleRoutine()
        {
            Transform target = targetGameObject.Value.transform;
            Vector3 startPosition = target.localPosition;
            Vector3 startRotation = target.localEulerAngles;
            float elapsed = 0f;
            CompleteDetachedAction();
            while (elapsed < duration.Value)
            {
                elapsed += CurrentDeltaTime;
                float currentTime = (float)CurrentElapsedSeconds;
                ApplyWiggle(target, startPosition, startRotation, currentTime);
                yield return null;
            }

            ResetTransform(target, startPosition, startRotation);
            CompleteAwaitedAction();
        }

        private void CompleteDetachedAction()
        {
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        private void ApplyWiggle(Transform target, Vector3 startPosition, Vector3 startRotation, float currentTime)
        {
            if (wigglePosition)
            {
                ApplyPositionWiggle(target, startPosition, currentTime);
            }
            if (wiggleRotation)
            {
                ApplyRotationWiggle(target, startRotation, currentTime);
            }
        }

        private void ApplyPositionWiggle(Transform target, Vector3 startPosition, float currentTime)
        {
            float noiseX = (Mathf.PerlinNoise(currentTime * speed.Value, 0) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0, currentTime * speed.Value) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(currentTime * speed.Value, currentTime * speed.Value) - 0.5f) * 2f;
            Vector3 noise = new Vector3(noiseX, noiseY, noiseZ);
            target.localPosition = startPosition + Vector3.Scale(noise, positionAmplitude.Value);
        }

        private void ApplyRotationWiggle(Transform target, Vector3 startRotation, float currentTime)
        {
            float sample = currentTime * speed.Value + 10f;
            float noiseX = (Mathf.PerlinNoise(sample, 0) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0, sample) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(sample, sample) - 0.5f) * 2f;
            Vector3 noise = new Vector3(noiseX, noiseY, noiseZ);
            target.localEulerAngles = startRotation + Vector3.Scale(noise, rotationAmplitude.Value);
        }

        private void ResetTransform(Transform target, Vector3 startPosition, Vector3 startRotation)
        {
            target.localPosition = startPosition;
            target.localEulerAngles = startRotation;
        }

        private void CompleteAwaitedAction()
        {
            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null)
            {
                return "Error: No target";
            }

            return $"Wiggle {targetGameObject.Value.name}";
        }

        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
