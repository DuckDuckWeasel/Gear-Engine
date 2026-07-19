using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Scripting",
                 "Call String Method",
                 "Calls a named method on a GameObject using the GameObject.SendMessage() system.")]
    [Serializable]
    public class CallStringMethod : ActionBase, ISerializationCallbackReceiver
    {
        [Tooltip("Target monobehavior which contains the method we want to call")]
        [SerializeField][InspectorName("Target Object")] protected GameObjectData targetObjectData;

        [Tooltip("Name of the method to call")]
        [SerializeField][InspectorName("Method Name")] protected StringData methodNameData = new StringData("");

        [Tooltip("Delay (in seconds) before the method will be called")]
        [SerializeField][InspectorName("Delay")] protected FloatData delayData;

        [HideInInspector][SerializeField] private GameObject targetObject;
        [HideInInspector][SerializeField] private string methodName = "";
        [HideInInspector][SerializeField] private float delay;

        #region Public members

        public override void OnEnter()
        {
            if (targetObjectData.Value == null ||
                string.IsNullOrEmpty(methodNameData.Value))
            {
                Continue();
                return;
            }

            if (Mathf.Approximately(delayData.Value, 0f))
            {
                CallTheMethod();
            }
            else
            {
                Invoke("CallTheMethod", delayData.Value);
            }

            Continue();
        }

        protected virtual void CallTheMethod()
        {
            targetObjectData.Value.SendMessage(methodNameData.Value, SendMessageOptions.DontRequireReceiver);
        }

        public override string GetSummary()
        {
            if (targetObjectData.Value == null)
            {
                return "Error: No target GameObject specified";
            }

            if (string.IsNullOrEmpty(methodNameData.Value))
            {
                return "Error: No named method specified";
            }

            return targetObjectData.Value.name + " : " + methodNameData.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetObjectData.gameObjectRef == variable ||
                   methodNameData.stringRef == variable ||
                   delayData.floatRef == variable ||
                   base.HasReference(variable);
        }

        #region Backwards compatibility

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            MigrateTargetObject();
            MigrateMethodName();
            MigrateDelay();
        }

        private void MigrateTargetObject()
        {
            if (targetObjectData.gameObjectVal == null && targetObject != null)
            {
                targetObjectData.gameObjectVal = targetObject;
                targetObjectData.source = VariableDataSource.Direct;
                targetObject = null;
            }
        }

        private void MigrateMethodName()
        {
            if (string.IsNullOrEmpty(methodNameData.stringVal) && !string.IsNullOrEmpty(methodName))
            {
                methodNameData.stringVal = methodName;
                methodNameData.source = VariableDataSource.Direct;
                methodName = "";
            }
        }

        private void MigrateDelay()
        {
            if (Mathf.Approximately(delayData.floatVal, 0f) && !Mathf.Approximately(delay, 0f))
            {
                delayData.floatVal = delay;
                delayData.source = VariableDataSource.Direct;
                delay = 0f;
            }
        }

        #endregion

        #endregion
    }
}
