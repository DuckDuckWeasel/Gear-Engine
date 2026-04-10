using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.CarSimulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    public sealed class Track : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private SplineExtrude splineExtrude;

        public SplineContainer SplineContainer => splineContainer;

        private void Awake()
        {
            EnsureSplineContainerReference();
        EnsureSplineExtrudeReference();
        }

        public void Initialize(TrackDefinition data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            ExecuteInitialize(data);
        }

        private void ExecuteInitialize(TrackDefinition data)
        {
            EnsureSplineContainerReference();
        EnsureSplineExtrudeReference();
            if (!HasSplineContainerOrLog() || !HasSplineDataOrLog(data))
            {
                return;
            }

            CopySplineIntoContainer(data.Spline, splineContainer);
            RebuildVisualSplineExtrude(data.Spline);
        }

        private bool HasSplineContainerOrLog()
        {
            if (splineContainer != null)
            {
                return true;
            }

            LogSplineContainerMissing();
            return false;
        }

        private bool HasSplineDataOrLog(TrackDefinition data)
        {
            if (data.Spline.Count > 0)
            {
                return true;
            }

            LogEmptyTrackDefinition(data.name);
            return false;
        }

        private void EnsureSplineContainerReference()
        {
            if (splineContainer == null)
            {
                splineContainer = GetComponent<SplineContainer>();
            }
        }

    private void EnsureSplineExtrudeReference()
    {
        if (splineExtrude != null)
        {
            return;
        }

        splineExtrude = GetComponent<SplineExtrude>();
        if (splineExtrude == null)
        {
            splineExtrude = GetComponentInParent<SplineExtrude>();
        }
    }

        private void LogSplineContainerMissing()
        {
            Debug.LogError("[Track] SplineContainer is missing; cannot Initialize.");
        }

        private void LogEmptyTrackDefinition(string definitionName)
        {
            Debug.LogError($"[Track] TrackDefinition '{definitionName}' has no spline knots.");
        }

        private void RebuildVisualSplineExtrude(Spline source)
        {
            if (splineExtrude == null)
            {
                return;
            }

            SyncVisualContainer(source);
            splineExtrude.Rebuild();
        }

        private void SyncVisualContainer(Spline source)
        {
            var visualContainer = splineExtrude.Container;
            if (visualContainer == null)
            {
                splineExtrude.Container = splineContainer;
                return;
            }

            if (visualContainer != splineContainer)
            {
                CopySplineIntoContainer(source, visualContainer);
            }
        }

        private void CopySplineIntoContainer(Spline source, SplineContainer targetContainer)
        {
            var target = targetContainer.Spline;
            target.Knots = source.Knots;
            target.Closed = source.Closed;
        }
    }
}
