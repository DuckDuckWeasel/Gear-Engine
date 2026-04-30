using Sirenix.OdinInspector;
using UnityEngine;

namespace GearEngine.CarSimulation.SplineSimulation
{
    /// <summary>
    /// Per-track set of <see cref="AnimationCurve"/>s that map normalized spline
    /// progress (t: 0–1) to lateral offset in meters for each personality stat axis.
    /// <para>
    /// Each curve is evaluated and scaled by the corresponding
    /// <see cref="DriverPersonality"/> stat (0–10). The final lateral offset is the
    /// sum of all 5 contributions, clamped to the track half-width.
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "GearEngine/Spline Evaluate/Lane Profile", fileName = "LaneProfile")]
    public sealed class LaneProfile : ScriptableObject
    {
        [FoldoutGroup("Aggression", expanded: true)]
        [InfoBox("Positive = outside (right of centerline), Negative = inside (left). Evaluated at curve apexes to simulate aggressive inside cutting.")]
        [SerializeField] private AnimationCurve aggressionCurve = AnimationCurve.Constant(0f, 1f, 0f);

        [FoldoutGroup("Drift")]
        [InfoBox("Post-apex exit line width. Positive values widen the exit arc to simulate controlled oversteer.")]
        [SerializeField] private AnimationCurve driftCurve = AnimationCurve.Constant(0f, 1f, 0f);

        [FoldoutGroup("Line Width")]
        [InfoBox("General lane wandering amplitude. This is a base shape; Perlin noise is added on top scaled by the Consistency stat.")]
        [SerializeField] private AnimationCurve widthCurve = AnimationCurve.Constant(0f, 1f, 0f);

        [FoldoutGroup("Risk Entry")]
        [InfoBox("Lateral entry offset for late-braking entries. Negative values pull the car inside for tighter turn-in.")]
        [SerializeField] private AnimationCurve riskEntryCurve = AnimationCurve.Constant(0f, 1f, 0f);

        [FoldoutGroup("Noise Settings")]
        [Tooltip("Frequency multiplier for the Perlin noise applied to the Consistency axis.")]
        [SerializeField] private float noiseFrequency = 3f;

        public AnimationCurve AggressionCurve => aggressionCurve;
        public AnimationCurve DriftCurve => driftCurve;
        public AnimationCurve WidthCurve => widthCurve;
        public AnimationCurve RiskEntryCurve => riskEntryCurve;
        public float NoiseFrequency => noiseFrequency;
    }
}
