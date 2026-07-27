using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Camera", "Camera Zoom", "Instantly changes the Camera's Orthographic Size or Field of View.")]
    [Serializable]
    [AddComponentMenu("")]
    public class CameraZoom : ActionBase
    {
        [Tooltip("The camera to zoom. If empty, uses Camera.main")]
        [SerializeField] protected Camera targetCamera;
        
        [Tooltip("The new Field of View (3D) or Ortho Size (2D)")]
        [SerializeField] protected FloatData zoomValue = new FloatData(60f);

        public override void OnEnter()
        {
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            
            if (cam != null)
            {
                if (cam.orthographic)
                {
                    cam.orthographicSize = zoomValue.Value;
                }
                else
                {
                    cam.fieldOfView = zoomValue.Value;
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            return $"Zoom to {zoomValue.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(216, 228, 240, 255); }
    }
}
