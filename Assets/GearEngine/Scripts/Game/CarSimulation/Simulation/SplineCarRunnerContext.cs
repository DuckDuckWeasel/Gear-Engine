using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Simulation
{
    public class SplineCarRunnerContext
    {
        public GearEngine.CarSimulation.Entity.CarEntity entity;
        public SplineContainer track;
        public PrometeoCarController targetCar;
        public Rigidbody targetCarRb;
        public float splineLength;
        public float previousProgressPercent;
        public bool isPaused = true;

        public float currentSpeed;
        public Vector3[] upcomingWaypoints;
        public bool isBrakingForCurve;
        public bool requiresHandbrake;
        public bool isHairpinPowerDrift;
        public float currentDeviation;

        public bool mustRecalculateWaypoints = true;
        public List<CarAreaModifier> activeAreaModifiers = new List<CarAreaModifier>();

        public PrometeoTouchInput aiThrottle;
        public PrometeoTouchInput aiReverse;
        public PrometeoTouchInput aiLeft;
        public PrometeoTouchInput aiRight;
        public PrometeoTouchInput aiBrake;

        public Definitions.CarVariableSet Variables;
        public Definitions.RoguelikeCarStats sourceStats;
        
        public float maxSimulationSpeed;
        public float safeCornerSpeed;
        public float arcadeSteerAssist;
        public int calculatedAcceleration;
        public int calculatedBrakeForce;
        public int calculatedDriftGrip;
        public float currentSimulationMultiplier;

        public float driftAccelerationMultiplier;
        public float driftSteerAssistMultiplier;
        public float hairpinAccelerationBoost;
        public float hairpinSteerAssistBoost;
        public float baseWaypointDistance;
        public float distanceSpeedMultiplier;
        public float waypointArrivalRangeBase;
        public float waypointArrivalSpeedMultiplier;
        public float preCurveWideOffset;
        public float postCurveWideOffset;
        public float steerDeadzone;
        public float curveAngleThreshold;
        public float handbrakeAngleThreshold;
        public float hairpinAngleThreshold;
        public float macroCurveAngleThreshold;
    }
}
