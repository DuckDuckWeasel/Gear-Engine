using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Physics",
                 "Raycast",
                 "Casts a ray against all colliders in the scene. Stores true if it hits something, and outputs the hit point.")]
    [Serializable]
    public class Raycast : ActionBase
    {
        [Tooltip("The starting point of the ray in world coordinates.")]
        [SerializeField] protected Vector3Data origin;

        [Tooltip("The direction of the ray.")]
        [SerializeField] protected Vector3Data direction;

        [Tooltip("The maximum distance the ray should check for collisions.")]
        [SerializeField] protected FloatData distance = new FloatData(Mathf.Infinity);

        [Tooltip("Layer mask to filter which objects can be hit.")]
        [SerializeField] protected LayerMask layerMask = -1; // Default to Everything

        [Tooltip("Variable to store whether a hit occurred.")]
        [VariableProperty(typeof(BooleanVariable))]
        [SerializeField] protected BooleanVariable outDidHit;

        [Tooltip("Variable to store the hit point position in world space.")]
        [VariableProperty(typeof(Vector3Variable))]
        [SerializeField] protected Vector3Variable outHitPosition;

        [Tooltip("Variable to store the GameObject that was hit.")]
        [VariableProperty(typeof(GameObjectVariable))]
        [SerializeField] protected GameObjectVariable outHitGameObject;

        public override void OnEnter()
        {
            RaycastHit hitInfo;
            bool hit = UnityEngine.Physics.Raycast(origin.Value, direction.Value, out hitInfo, distance.Value, layerMask);

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
            return origin.vector3Ref == variable || direction.vector3Ref == variable || distance.floatRef == variable ||
                   outDidHit == variable || outHitPosition == variable || outHitGameObject == variable;
        }
    }
}
