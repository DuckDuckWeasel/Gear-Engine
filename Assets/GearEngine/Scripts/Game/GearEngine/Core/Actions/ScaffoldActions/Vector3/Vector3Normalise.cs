using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    // <summary>
    /// Normalise a vector3, output can be the same as the input
    /// </summary>
    [CommandInfo("Vector3",
                 "Normalise",
                 "Normalise a Vector3")]
    [Serializable]
    public class Vector3Normalise : ActionBase
    {
        [SerializeField]
        protected Vector3Data vec3In, vec3Out;

        public override void OnEnter()
        {
            vec3Out.Value = vec3In.Value.normalized;

            Continue();
        }

        public override string GetSummary()
        {
            if (vec3Out.vector3Ref == null)
                return "";
            else
                return vec3Out.vector3Ref.Key;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            if (vec3In.vector3Ref == variable || vec3Out.vector3Ref == variable)
                return true;

            return false;
        }
    }
}