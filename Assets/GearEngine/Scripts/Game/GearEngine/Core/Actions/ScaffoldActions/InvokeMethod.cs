using GearEngine.Core.Actions;

using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using MarkerMetro.Unity.WinLegacy.Reflection;
using System.Linq;

namespace Scaffold
{
    [CommandInfo("Scripting",
                 "Invoke Method",
                 "Invokes a method of a component via reflection. Supports passing multiple parameters and storing returned values in a Scaffold variable.")]
    [Serializable]
    public class InvokeMethod : ActionBase
    {
        public virtual GameObject TargetObject { get { return targetObject; } }

        [Tooltip("A description of what this command does. Appears in the command summary.")]
        [SerializeField] protected string description = "";

        [Tooltip("GameObject containing the component method to be invoked")]
        [SerializeField] protected GameObject targetObject;

        [HideInInspector]
        [Tooltip("Name of assembly containing the target component")]
        [SerializeField] protected string targetComponentAssemblyName;

        [HideInInspector]
        [Tooltip("Full name of the target component")]
        [SerializeField] protected string targetComponentFullname;

        [HideInInspector]
        [Tooltip("Display name of the target component")]
        [SerializeField] protected string targetComponentText;

        [HideInInspector]
        [Tooltip("Name of target method to invoke on the target component")]
        [SerializeField] protected string targetMethod;

        [HideInInspector]
        [Tooltip("Display name of target method to invoke on the target component")]
        [SerializeField] protected string targetMethodText;

        [HideInInspector]
        [Tooltip("List of parameters to pass to the invoked method")]
        [SerializeField] protected InvokeMethodParameter[] methodParameters;

        [HideInInspector]
        [Tooltip("If true, store the return value in a Blackboard Variable of the same type.")]
        [SerializeField] protected bool saveReturnValue;

        [HideInInspector]
        [Tooltip("Name of Scaffold variable to store the return value in")]
        [SerializeField] protected string returnValueVariableKey;

        [HideInInspector]
        [Tooltip("The type of the return value")]
        [SerializeField] protected string returnValueType;

        [HideInInspector]
        [Tooltip("If true, list all inherited methods for the component")]
        [SerializeField] protected bool showInherited;

        [HideInInspector]
        [Tooltip("The coroutine call behavior for methods that return IEnumerator")]
        [SerializeField] protected CallMode callMode;

        protected Type componentType;
        [Tooltip("The Obj component")]
        protected Component objComponent;
        [Tooltip("The Parameter types")]
        protected Type[] parameterTypes = null;
        [Tooltip("The Obj method")]
        protected MethodInfo objMethod;

        #region Public members

        public override void OnEnter()
        {
            try
            {
                ExecuteInvocation();
            }
            catch (Exception exception)
            {
                LogInvocationError(exception);
                Fail();
            }
        }

        private void ExecuteInvocation()
        {
            if (!HasInvocationTarget())
            {
                Continue();
                return;
            }

            InitializeInvocation();
            if (returnValueType == "System.Collections.IEnumerator")
            {
                ExecuteEnumerator();
                return;
            }

            ExecuteMethod();
        }

        private bool HasInvocationTarget()
        {
            bool hasComponent = !string.IsNullOrEmpty(targetComponentAssemblyName);
            bool hasMethod = !string.IsNullOrEmpty(targetMethod);
            return targetObject != null && hasComponent && hasMethod;
        }

        protected virtual void InitializeInvocation()
        {
            ResolveComponent();
            ResolveMethod();
        }

        private void ResolveComponent()
        {
            if (componentType == null)
            {
                componentType = ReflectionHelper.GetType(targetComponentAssemblyName);
            }
            if (objComponent == null)
            {
                objComponent = targetObject.GetComponent(componentType);
            }
        }

        private void ResolveMethod()
        {
            if (parameterTypes == null)
            {
                parameterTypes = GetParameterTypes();
            }
            if (objMethod == null)
            {
                objMethod = UnityEvent.GetValidMethodInfo(objComponent, targetMethod, parameterTypes);
            }
        }

        protected virtual Type[] GetParameterTypes()
        {
            Type[] types = new Type[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; i++)
            {
                InvokeMethodParameter item = methodParameters[i];
                types[i] = ReflectionHelper.GetType(item.ObjectValue.TypeAssemblyName);
            }

            return types;
        }

        private void ExecuteEnumerator()
        {
            bool detached = callMode != CallMode.WaitUntilFinished;
            RunRoutine(ExecuteCoroutine(), detached);
            if (callMode == CallMode.Continue)
            {
                Continue();
                return;
            }
            if (callMode == CallMode.Stop)
            {
                StopParentBlock();
            }
        }

        protected virtual IEnumerator ExecuteCoroutine()
        {
            object result = objMethod.Invoke(objComponent, GetParameterValues());
            yield return (IEnumerator)result;
            if (callMode == CallMode.WaitUntilFinished)
            {
                Continue();
            }
        }

        private void ExecuteMethod()
        {
            object returnValue = objMethod.Invoke(objComponent, GetParameterValues());
            if (saveReturnValue)
            {
                SetVariable(returnValueVariableKey, returnValue);
            }
            Continue();
        }

        protected virtual object[] GetParameterValues()
        {
            object[] values = new object[methodParameters.Length];
            Blackboard currentBlackboard = GetBlackboard();
            for (int i = 0; i < methodParameters.Length; i++)
            {
                values[i] = ResolveParameterValue(methodParameters[i], currentBlackboard);
            }

            return values;
        }

        private object ResolveParameterValue(InvokeMethodParameter parameter, Blackboard currentBlackboard)
        {
            if (string.IsNullOrEmpty(parameter.VariableKey))
            {
                return parameter.ObjectValue.GetValue();
            }

            Variable variable = currentBlackboard.GetVariable(parameter.VariableKey);
            return GetVariableValue(variable);
        }

        private object GetVariableValue(Variable variable)
        {
            object scalarValue = GetScalarVariableValue(variable);
            return scalarValue ?? GetUnityVariableValue(variable);
        }

        private object GetScalarVariableValue(Variable variable)
        {
            switch (variable)
            {
                case IntegerVariable item: return item.Value;
                case BooleanVariable item: return item.Value;
                case FloatVariable item: return item.Value;
                case StringVariable item: return item.Value;
                default: return null;
            }
        }

        private object GetUnityVariableValue(Variable variable)
        {
            switch (variable)
            {
                case ColorVariable item: return item.Value;
                case GameObjectVariable item: return item.Value;
                case MaterialVariable item: return item.Value;
                case SpriteVariable item: return item.Value;
                case TextureVariable item: return item.Value;
                case Vector2Variable item: return item.Value;
                case Vector3Variable item: return item.Value;
                case ObjectVariable item: return item.Value;
                default: return null;
            }
        }

        protected virtual void SetVariable(string key, object value)
        {
            Variable variable = GetBlackboard().GetVariable(key);
            if (variable == null || SetScalarVariable(variable, value))
            {
                return;
            }

            SetUnityVariable(variable, value);
        }

        private bool SetScalarVariable(Variable variable, object value)
        {
            switch (variable)
            {
                case IntegerVariable item: item.Value = (int)value; return true;
                case BooleanVariable item: item.Value = (bool)value; return true;
                case FloatVariable item: item.Value = (float)value; return true;
                case StringVariable item: item.Value = (string)value; return true;
                default: return false;
            }
        }

        private void SetUnityVariable(Variable variable, object value)
        {
            switch (variable)
            {
                case ColorVariable item: item.Value = (Color)value; break;
                case GameObjectVariable item: item.Value = (GameObject)value; break;
                case MaterialVariable item: item.Value = (Material)value; break;
                case SpriteVariable item: item.Value = (Sprite)value; break;
                case TextureVariable item: item.Value = (Texture)value; break;
                case Vector2Variable item: item.Value = (Vector2)value; break;
                case Vector3Variable item: item.Value = (Vector3)value; break;
                case ObjectVariable item: item.Value = (UnityEngine.Object)value; break;
            }
        }

        private void LogInvocationError(Exception exception)
        {
            Debug.LogError($"[InvokeMethod] Error invoking '{targetMethod}' on {targetComponentText}: {exception.Message}\n{exception.StackTrace}");
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override string GetSummary()
        {
            if (targetObject == null)
            {
                return "Error: targetObject is not assigned";
            }

            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }

            return targetObject.name + "." + targetComponentText + "." + targetMethodText;
        }

        #endregion
    }

}
