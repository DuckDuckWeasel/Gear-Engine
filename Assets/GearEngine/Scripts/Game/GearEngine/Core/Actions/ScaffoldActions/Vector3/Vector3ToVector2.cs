using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    // <summary>
    /// Convert scaffold vec3 to vec2
    /// </summary>
    [CommandInfo("Vector3",
                 "ToVector2",
                 "Convert Scaffold Vector3 to Scaffold Vector2")]
    [Serializable]
    public class Vector3ToVector2 : ActionBase
    {
        [SerializeField]
        protected Vector3Data vec3;


        [SerializeField]
        protected Vector2Data vec2;

        public override void OnEnter()
        {
            vec2.Value = vec3.Value;

            Continue();
        }

        public override string GetSummary()
        {
            if (vec3.vector3Ref != null && vec2.vector2Ref != null)
            {
                return "Converting " + vec3.vector3Ref.Key + " to " + vec2.vector2Ref.Key;
            }

            return "Error: variables not set";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }


        public override bool HasReference(Variable variable)
        {
            if (variable == vec3.vector3Ref || variable == vec2.vector2Ref)
                return true;

            return false;
        }
    }
}