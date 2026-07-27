using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Physics",
                 "Raycast 2D",
                 "Casts a ray against colliders in the 2D scene. Stores true if it hits something, and outputs the hit point.")]
    [Serializable]
    public class Raycast2D : ActionBase
    {
        [Tooltip("The starting point of the ray in world coordinates.")]
        [SerializeField] protected Vector2Data origin;

        [Tooltip("The direction of the ray.")]
        [SerializeField] protected Vector2Data direction;

        [Tooltip("The maximum distance the ray should check for collisions.")]
        [SerializeField] protected FloatData distance = new FloatData(Mathf.Infinity);

        [Tooltip("Layer mask to filter which objects can be hit.")]
        [SerializeField] protected LayerMask layerMask = -1; // Default to Everything

        [Tooltip("Variable to store whether a hit occurred.")]
        [VariableProperty(typeof(BooleanVariable))]
        [SerializeField] protected BooleanVariable outDidHit;

        [Tooltip("Variable to store the hit point position in world space.")]
        [VariableProperty(typeof(Vector2Variable))]
        [SerializeField] protected Vector2Variable outHitPosition;

        [Tooltip("Variable to store the GameObject that was hit.")]
        [VariableProperty(typeof(GameObjectVariable))]
        [SerializeField] protected GameObjectVariable outHitGameObject;

        public override void OnEnter()
        {
            RaycastHit2D hitInfo = UnityEngine.Physics2D.Raycast(origin.Value, direction.Value, distance.Value, layerMask);

            bool hit = hitInfo.collider != null;
            if (outDidHit != null) outDidHit.Value = hit;

            if (hit)
            {
                if (outHitPosition != null) outHitPosition.Value = hitInfo.point;
                if (outHitGameObject != null) outHitGameObject.Value = hitInfo.collider.gameObject;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return "From " + origin.Value.ToString() + " Dir: " + direction.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return origin.vector2Ref == variable || direction.vector2Ref == variable || distance.floatRef == variable ||
                   outDidHit == variable || outHitPosition == variable || outHitGameObject == variable;
        }
    }
}
