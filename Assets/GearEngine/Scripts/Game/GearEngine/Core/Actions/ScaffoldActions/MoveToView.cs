using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Moves the camera to a location specified by a View object.
    /// </summary>
    [CommandInfo("Camera",
                 "Move To View",
                 "Moves the camera to a location specified by a View object.")]
    [Serializable]
    public class MoveToView : ActionBase
    {
        [SerializeField] private CameraManager cameraManager;

        [Tooltip("Time for move effect to complete")]
        [SerializeField] protected float duration = 1;

        [Tooltip("View to transition to when move is complete")]
        [SerializeField] protected View targetView;
        public virtual View TargetView { get { return targetView; } }

        [Tooltip("Wait until the fade has finished before executing next command")]
        [SerializeField] protected bool waitUntilFinished = true;

        [Tooltip("Camera to use for the pan. Will use main camera if set to none.")]
        [SerializeField] protected Camera targetCamera;

        [SerializeField] protected LeanTweenType orthoSizeTweenType = LeanTweenType.easeInOutQuad;
        [SerializeField] protected LeanTweenType posTweenType = LeanTweenType.easeInOutQuad;
        [SerializeField] protected LeanTweenType rotTweenType = LeanTweenType.easeInOutQuad;

        protected virtual void AcquireCamera()
        {
            if (targetCamera != null)
            {
                return;
            }

            targetCamera = Camera.main;
        }

        public virtual void Start()
        {
            AcquireCamera();
        }

        #region Public members

        public override void OnEnter()
        {
            AcquireCamera();
            if (cameraManager == null)
            {
                Debug.LogError(
                    "[MoveToView] A CameraManager reference is required.");
                Fail();
                return;
            }

            if (targetCamera == null ||
                targetView == null)
            {
                Continue();
                return;
            }

            Vector3 targetPosition = targetView.gameObject.transform.position;
            Quaternion targetRotation = targetView.gameObject.transform.rotation;
            float targetSize = targetView.ViewSize;

            cameraManager.PanToPosition(targetCamera, targetPosition, targetRotation, targetSize, duration, delegate
            {
                if (waitUntilFinished)
                {
                    Continue();
                }
            }, orthoSizeTweenType, posTweenType, rotTweenType);

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override void OnStopExecuting()
        {
            cameraManager?.Stop();
        }

        public override string GetSummary()
        {
            if (targetView == null)
            {
                return "Error: No view selected";
            }
            else
            {
                return targetView.name;
            }
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion
    }
}
