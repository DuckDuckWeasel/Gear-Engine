using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    // <summary>
    /// Store UnityEngine.Input.mousePosition and mouse screen conversions in a variable(s)
    /// </summary>
    [CommandInfo("Input",
                 "Get Mouse Position",
                 "Store various interpretations of UnityEngine.Input.mousePosition")]
    [Serializable]
    public class GetMousePosition : ActionBase
    {
        [VariableProperty(typeof(Vector2Variable))]
        [SerializeField]
        [Tooltip("The Screen position")]
        protected Vector2Variable screenPosition;

        [Tooltip("If null, Camera.main is used")]
        protected Camera castCamera;

        [VariableProperty(typeof(Vector2Variable))]
        [SerializeField]
        [Tooltip("The View position")]
        protected Vector2Variable viewPosition;

        [VariableProperty(typeof(Vector3Variable))]
        [SerializeField]
        [Tooltip("The World position")]
        protected Vector3Variable worldPosition;

        [VariableProperty(typeof(Vector3Variable))]
        [SerializeField]
        [Tooltip("The World direction")]
        protected Vector3Variable worldDirection;

        public override void OnEnter()
        {
            if (castCamera == null)
            {
                castCamera = Camera.main;
            }

            if (screenPosition != null)
            {
                screenPosition.Value = UnityEngine.Input.mousePosition;
            }

            if (viewPosition != null)
            {
                viewPosition.Value = castCamera.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
            }

            if (worldPosition != null)
            {
                Vector3 screenWithZ = UnityEngine.Input.mousePosition;
                screenWithZ.z = castCamera.nearClipPlane;
                worldPosition.Value = castCamera.ScreenToWorldPoint(screenWithZ);
            }

            if (worldDirection != null)
            {
                Vector3 screenWithZ = UnityEngine.Input.mousePosition;
                screenWithZ.z = castCamera.nearClipPlane;
                worldDirection.Value = castCamera.ScreenPointToRay(screenWithZ).direction;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return (screenPosition != null ? screenPosition.Key + " " : "") +
                    (castCamera != null ? castCamera.name + " " : "MainCam") +
                    (viewPosition != null ? viewPosition.Key + " " : "") +
                    (worldPosition != null ? worldPosition.Key + " " : "") +
                    (worldDirection != null ? worldDirection.Key + " " : "");
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return (screenPosition == variable ||
                viewPosition == variable ||
                worldPosition == variable ||
                worldDirection == variable);
        }
    }
}