using GearEngine.Core.Actions;
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Transform", "Billboard", "Makes a GameObject continuously face the main camera.")]
    [Serializable]
    [AddComponentMenu("")]
    public class Billboard : ActionBase
    {
        [Tooltip("The GameObject to billboard")]
        [SerializeField] protected GameObjectData targetGameObject;

        public override void OnEnter()
        {
            if (targetGameObject.Value != null && CanRunScheduledWork)
            {
                RunRoutine(BillboardRoutine(), true);
            }
            Continue();
        }

        private IEnumerator BillboardRoutine()
        {
            Camera cam = Camera.main;
            while (targetGameObject.Value != null && cam != null)
            {
                targetGameObject.Value.transform.LookAt(cam.transform);
                yield return null;
            }
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null)
            {
                return "Error: No target";
            }

            return $"Billboard {targetGameObject.Value.name}";
        }

        public override Color GetButtonColor() { return new Color32(228, 237, 204, 255); }
    }
}
