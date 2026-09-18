using System.Collections.Generic;
using GearEngine.CarSimulation.Tracks;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Track/Track Theme Definition", fileName = "TrackThemeDefinition")]
    public sealed class TrackThemeDefinition : ScriptableObject
    {
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public GameObject EnvironmentPrefab => environmentPrefab;
        public Vector3 EnvironmentLocalPosition => environmentLocalPosition;
        public Vector3 EnvironmentLocalEulerAngles => environmentLocalEulerAngles;
        public Vector3 EnvironmentLocalScale => environmentLocalScale;
        public Material RoadMaterial => roadMaterial;
        public Material GroundMaterial => groundMaterial;
        public bool HidesBaseGround => hideBaseGround;
        public bool UsesDefaultProps => useDefaultProps;
        public IReadOnlyList<SplinePropGenerator.PropRule> PropRules => propRules;

        [SerializeField] private string displayName;

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
    }
}
