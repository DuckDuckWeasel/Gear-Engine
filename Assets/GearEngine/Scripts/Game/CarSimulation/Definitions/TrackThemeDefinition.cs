using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Tracks;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Track/Track Theme Definition", fileName = "TrackThemeDefinition")]
    public sealed class TrackThemeDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class SurfaceZone
        {
            public string Name => name;
            public float NormalizedPosition => Mathf.Repeat(normalizedPosition, 1f);
            public float NormalizedHalfLength => Mathf.Clamp(normalizedHalfLength, 0.001f, 0.1f);
            public float Width => Mathf.Max(0.1f, width);
            public float SpeedMultiplier => Mathf.Clamp(speedMultiplier, 0.6f, 1f);
            public float LateralPush => Mathf.Clamp(lateralPush, -1f, 1f);
            public Material Material => material;

            [SerializeField] private string name;
            [SerializeField, Range(0f, 1f)] private float normalizedPosition;
            [SerializeField, Range(0.001f, 0.1f)] private float normalizedHalfLength = 0.02f;
            [SerializeField, Min(0.1f)] private float width = 4f;
            [SerializeField, Range(0.6f, 1f)] private float speedMultiplier = 1f;
            [SerializeField, Range(-1f, 1f)] private float lateralPush;
            [SerializeField] private Material material;

            public bool Contains(float normalizedTrackPosition)
            {
                float wrappedDistance = Mathf.Repeat(normalizedTrackPosition - NormalizedPosition + 0.5f, 1f) - 0.5f;
                return Mathf.Abs(wrappedDistance) <= NormalizedHalfLength;
            }
        }

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public TrackBiome Biome => biome;
        public GameObject EnvironmentPrefab => environmentPrefab;
        public Vector3 EnvironmentLocalPosition => environmentLocalPosition;
        public Vector3 EnvironmentLocalEulerAngles => environmentLocalEulerAngles;
        public Vector3 EnvironmentLocalScale => environmentLocalScale;
        public Material RoadMaterial => roadMaterial;
        public Material GroundMaterial => groundMaterial;
        public bool HidesBaseGround => hideBaseGround;
        public bool UsesDefaultProps => useDefaultProps;
        public IReadOnlyList<SplinePropGenerator.PropRule> PropRules => propRules;
        public GameObject AmbientVfxPrefab => ambientVfxPrefab;
        public IReadOnlyList<SurfaceZone> SurfaceZones => surfaceZones;

        [SerializeField] private string displayName;
        [SerializeField] private TrackBiome biome;

        [Header("Environment")]
        [SerializeField] private GameObject environmentPrefab;
        [SerializeField] private Vector3 environmentLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 environmentLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 environmentLocalScale = Vector3.one;

        [Header("Surfaces")]
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Material groundMaterial;
        [SerializeField] private bool hideBaseGround;

        [Header("Props")]
        [SerializeField] private bool useDefaultProps = true;
        [SerializeReference] private List<SplinePropGenerator.PropRule> propRules = new List<SplinePropGenerator.PropRule>();

        [Header("Track Effects")]
        [SerializeField] private GameObject ambientVfxPrefab;
        [SerializeField] private List<SurfaceZone> surfaceZones = new List<SurfaceZone>();
    }
}
