using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[InfoBox("Generates props (e.g., pole cones) along the edges of the track's curves based on probability rules and min/max ranges.\n\nAllows for 'lists of lists of things' as requested, instantiating random items from the defined groups.")]
public class SplinePropGenerator : MonoBehaviour
{
    [Required]
    public SplineContainer track;

    public enum PlacementSide
    {
        Left,
        Right,
        BothEdges
    }

    [System.Serializable]
    public class PropRule
    {
        [Title("Instantiation")]
        [Tooltip("List of possible objects to spawn. A random one will be chosen for each placement (satisfying the 'list of things').")]
        public List<GameObject> prefabs;

        [Title("Positioning")]
        [Tooltip("Which side of the track to place the objects.")]
        public PlacementSide side = PlacementSide.BothEdges;

        [Tooltip("Distance from the center of the spline to the edge of the track.")]
        public float lateralDistance = 4.5f;

        [Title("Distribution per Curve")]
        [Range(0f, 1f), Tooltip("Chance (0% to 100%) to spawn this prop group in a given curve segment.")]
        public float chancePerCurve = 0.8f;

        [Tooltip("Minimum amount of objects to instantiate in the curve, if selected.")]
        public int minAmount = 3;

        [Tooltip("Maximum amount of objects to instantiate in the curve, if selected.")]
        public int maxAmount = 10;
    }

    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<PropRule> propRules = new List<PropRule>();

    [Tooltip("If true, automatically generates the props when the scene starts.")]
    public bool generateOnStart = true;

    [FoldoutGroup("Generated Objects"), ReadOnly]
    public List<GameObject> generatedProps = new List<GameObject>();

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    [Button("Generate / Reset Props", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void Generate()
    {
        ClearProps();

        if (track == null || propRules.Count == 0) return;

        var spline = track.Spline;
        if (spline == null) return;

        Transform container = new GameObject("Generated Props").transform;
        container.SetParent(this.transform);
        container.localPosition = Vector3.zero;

        // Iterate through every curve segment defined by the Spline's knots
        int curveCount = spline.GetCurveCount();
        
        for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
        {
            foreach (var rule in propRules)
            {
                if (rule.prefabs == null || rule.prefabs.Count == 0) continue;

                // Evaluate Chance specifically for this curve
                if (UnityEngine.Random.value <= rule.chancePerCurve)
                {
                    int amount = UnityEngine.Random.Range(rule.minAmount, rule.maxAmount + 1);

                    for (int i = 0; i < amount; i++)
                    {
                        // Fraction of distance within this specific curve (from 0 to 1)
                        float localT = amount == 1 ? 0.5f : (i / (float)(amount - 1));
                        
                        // Map local curve T to overall Spline T
                        float globalT = (curveIndex + localT) / curveCount;

                        Vector3 position = track.transform.TransformPoint(SplineUtility.EvaluatePosition(spline, globalT));
                        Vector3 tangent = track.transform.TransformDirection(SplineUtility.EvaluateTangent(spline, globalT));
                        Vector3 up = track.transform.TransformDirection(SplineUtility.EvaluateUpVector(spline, globalT));
                        Vector3 right = Vector3.Cross(up, tangent).normalized;

                        // Determine actual side for this specific prop instance
                        float currentLateralMultiplier = 0f;
                        switch (rule.side)
                        {
                            case PlacementSide.Left:
                                currentLateralMultiplier = -1f;
                                break;
                            case PlacementSide.Right:
                                currentLateralMultiplier = 1f;
                                break;
                            case PlacementSide.BothEdges:
                                // Alternate sides for an even distribution
                                currentLateralMultiplier = (i % 2 == 0) ? 1f : -1f;
                                break;
                        }

                        Vector3 finalPos = position + (right * rule.lateralDistance * currentLateralMultiplier);

                        GameObject prefabToSpawn = rule.prefabs[UnityEngine.Random.Range(0, rule.prefabs.Count)];
                        if (prefabToSpawn != null)
                        {
                            GameObject instance = Instantiate(prefabToSpawn, finalPos, Quaternion.LookRotation(tangent, up));
                            instance.transform.SetParent(container);
                            generatedProps.Add(instance);
                        }
                    }
                }
            }
        }
    }

    [Button("Clear Props")]
    public void ClearProps()
    {
        foreach (var obj in generatedProps)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        generatedProps.Clear();

        Transform container = transform.Find("Generated Props");
        if (container != null)
        {
            DestroyImmediate(container.gameObject);
        }
    }
}
