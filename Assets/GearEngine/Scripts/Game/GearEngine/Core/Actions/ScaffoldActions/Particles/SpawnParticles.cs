using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Particles", "Spawn Particles", "Instantiates a particle prefab at a location and plays it, optionally destroying it when done.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SpawnParticles : ActionBase
    {
        [Tooltip("The ParticleSystem prefab to spawn")]
        [SerializeField] protected GameObjectData particlePrefab;
        
        [Tooltip("Where to spawn it")]
        [SerializeField] protected Vector3Data spawnPosition;

        [Tooltip("Auto destroy the GameObject after the particles finish playing?")]
        [SerializeField] protected bool autoDestroy = true;

        public override void OnEnter()
        {
            if (particlePrefab.Value != null)
            {
                GameObject instance = GameObject.Instantiate(particlePrefab.Value, spawnPosition.Value, Quaternion.identity);
                ParticleSystem ps = instance.GetComponent<ParticleSystem>();
                
                if (ps != null)
                {
                    ps.Play();
                    if (autoDestroy)
                    {
                        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                        GameObject.Destroy(instance, duration);
                    }
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (particlePrefab.Value == null) return "Error: No Prefab";
            return $"Spawn {particlePrefab.Value.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(206, 206, 206, 255); }
    }
}
