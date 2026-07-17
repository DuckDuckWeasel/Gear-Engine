using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Spawns a new object based on a reference to a scene or prefab game object.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Spawn Object", 
                 "Spawns a new object based on a reference to a scene or prefab game object.", 
        Priority = 10)]
    [CommandInfo("GameObject",
                 "Instantiate",
                 "Instantiate a game object")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class SpawnObject : ActionBase
    {
        [Tooltip("Game object to copy when spawning. Can be a scene object or a prefab.")]
        [SerializeField] protected GameObjectData sourceObject;

        [Tooltip("Transform to use as parent during instantiate.")]
        [SerializeField] protected TransformData parentTransform;

        [Tooltip("If true, will use the Transfrom of this Flowchart for the position and rotation.")]
        [SerializeField] protected BooleanData spawnAtSelf = new BooleanData(false);

        [Tooltip("Local position of newly spawned object.")]
        [SerializeField] protected Vector3Data spawnPosition;

        [Tooltip("Local rotation of newly spawned object.")]
        [SerializeField] protected Vector3Data spawnRotation;



        [Tooltip("Optional variable to store the GameObject that was just created.")]
        [SerializeField]
        protected GameObjectData newlySpawnedObject;

        #region Public members

        public override void OnEnter()
        {
            if (sourceObject.Value == null)
            {
                Continue();
                return;
            }

            GameObject newObject = null;

            if (parentTransform.Value != null)
            {
                newObject = GameObject.Instantiate(sourceObject.Value,parentTransform.Value);
            }
            else
            {
                newObject = GameObject.Instantiate(sourceObject.Value);
            }

            if (!spawnAtSelf.Value)
            {
                newObject.gameObject.transform.localPosition = spawnPosition.Value;
                newObject.gameObject.transform.localRotation = Quaternion.Euler(spawnRotation.Value);
            }
            else
            {
                newObject.gameObject.transform.SetPositionAndRotation(host.transform.position, host.transform.rotation);
            }

            newlySpawnedObject.Value = newObject;

            Continue();
        }

        public override string GetSummary()
        {
            if (sourceObject.Value == null)
            {
                return "Error: No source GameObject specified";
            }

            return sourceObject.Value.name;
        }
        
        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            if (sourceObject.gameObjectRef == variable || parentTransform.transformRef == variable ||
                spawnAtSelf.booleanRef == variable || spawnPosition.vector3Ref == variable ||
                spawnRotation.vector3Ref == variable)
                return true;

            return false;
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("sourceObject")] public GameObject sourceObjectOLD;
        [HideInInspector] [FormerlySerializedAs("parentTransform")] public Transform parentTransformOLD;
        [HideInInspector] [FormerlySerializedAs("spawnPosition")] public Vector3 spawnPositionOLD;
        [HideInInspector] [FormerlySerializedAs("spawnRotation")] public Vector3 spawnRotationOLD;

        protected virtual void OnEnable()
        {
            if (sourceObjectOLD != null)
            {
                sourceObject.Value = sourceObjectOLD;
                sourceObjectOLD = null;
            }
            if (parentTransformOLD != null)
            {
                parentTransform.Value = parentTransformOLD;
                parentTransformOLD = null;
            }
            if (spawnPositionOLD != default(Vector3))
            {
                spawnPosition.Value = spawnPositionOLD;
                spawnPositionOLD = default(Vector3);
            }
            if (spawnRotationOLD != default(Vector3))
            {
                spawnRotation.Value = spawnRotationOLD;
                spawnRotationOLD = default(Vector3);
            }
        }

        #endregion
    }
}