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
            if (targetGameObject.Value != null && blackboard != null)
            {
                blackboard.StartCoroutine(WiggleRoutine());
            }
            else
            {
                Continue();
            }
        }

        private IEnumerator WiggleRoutine()
        {
            Transform t = targetGameObject.Value.transform;
            Vector3 startPos = t.localPosition;
            Vector3 startRot = t.localEulerAngles;
            float elapsed = 0f;

            if (!waitUntilFinished) Continue();

            while (elapsed < duration.Value)
            {
                elapsed += Time.deltaTime;
                
                if (wigglePosition)
                {
                    float noiseX = (Mathf.PerlinNoise(Time.time * speed.Value, 0) - 0.5f) * 2f;
                    float noiseY = (Mathf.PerlinNoise(0, Time.time * speed.Value) - 0.5f) * 2f;
                    float noiseZ = (Mathf.PerlinNoise(Time.time * speed.Value, Time.time * speed.Value) - 0.5f) * 2f;
                    t.localPosition = startPos + new Vector3(noiseX * positionAmplitude.Value.x, noiseY * positionAmplitude.Value.y, noiseZ * positionAmplitude.Value.z);
                }
                
                if (wiggleRotation)
                {
                    float noiseX = (Mathf.PerlinNoise(Time.time * speed.Value + 10, 0) - 0.5f) * 2f;
                    float noiseY = (Mathf.PerlinNoise(0, Time.time * speed.Value + 10) - 0.5f) * 2f;
                    float noiseZ = (Mathf.PerlinNoise(Time.time * speed.Value + 10, Time.time * speed.Value + 10) - 0.5f) * 2f;
                    t.localEulerAngles = startRot + new Vector3(noiseX * rotationAmplitude.Value.x, noiseY * rotationAmplitude.Value.y, noiseZ * rotationAmplitude.Value.z);
                }

                yield return null;
            }

            t.localPosition = startPos;
            t.localEulerAngles = startRot;

            if (waitUntilFinished) Continue();
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null) return "Error: No target";
            return $"Wiggle {targetGameObject.Value.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
