using UnityEngine;
using TriInspector;

[RequireComponent(typeof(Collider))]
public class CarAreaModifier : MonoBehaviour
{
    [Title("Temporary Multipliers")]
    [InfoBox("Add this to a volume trigger (e.g. Water Puddle, Oil, Speed Bump) to dynamically modify the SplineCarRunner stats when the car passes through it.")]
    [Tooltip("Multiplies the AI's max speed and safe corner speed. (e.g. 0.5 cuts speed in half like a water puddle).")]
    public float speedMultiplier = 1f;

    [Tooltip("Multiplies the Prometeo Car acceleration output.")]
    public float accelerationMultiplier = 1f;

    [Tooltip("Multiplies the physical force applied by the engine to keep the car glued to the spline line. (e.g. 0 makes it slide out into grass like Oil).")]
    public float splineGripMultiplier = 1f;
}
