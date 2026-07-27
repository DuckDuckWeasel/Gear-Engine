using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Follow Target", "Makes a GameObject continuously follow another target.")]
    [Serializable]
    [AddComponentMenu("")]
    public class FollowTarget : ActionBase
    {
        [Tooltip("The GameObject that will follow")]
        [SerializeField] protected GameObjectData follower;

        [Tooltip("The target to follow")]
        [SerializeField] protected GameObjectData target;

        [Tooltip("Follow speed")]
        [SerializeField] protected FloatData speed = new FloatData(5f);

        [Tooltip("Offset from target")]
        [SerializeField] protected Vector3Data offset;

        public override void OnEnter()
        {
            if (follower.Value != null && target.Value != null && CanRunScheduledWork)
            {
                RunRoutine(FollowRoutine(), true);
            }
            Continue();
        }

        private IEnumerator FollowRoutine()
        {
            while (follower.Value != null && target.Value != null)
            {
                Vector3 targetPos = target.Value.transform.position + offset.Value;
                Vector3 currentPosition = follower.Value.transform.position;
                follower.Value.transform.position = Vector3.Lerp(currentPosition, targetPos, CurrentDeltaTime * speed.Value);
                yield return null;
            }
        }

        public override string GetSummary()
        {
            if (follower.Value == null || target.Value == null)
            {
                return "Error: Missing objects";
            }

            return $"Follow {target.Value.name}";
        }

        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
