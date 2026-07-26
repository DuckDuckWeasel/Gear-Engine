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
    /// <summary>
    /// Invokes a method of a component via reflection. Supports passing multiple parameters and storing returned values in a Scaffold variable.
    /// </summary>
    [CommandInfo("Scripting",
                 "Invoke Method",
                 "Invokes a method of a component via reflection. Supports passing multiple parameters and storing returned values in a Scaffold variable.")]
    [Serializable]
    public class InvokeMethod : ActionBase
    {
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

        protected virtual void Awake()
        {
            if (componentType == null)
            {
                componentType = ReflectionHelper.GetType(targetComponentAssemblyName);
            }

            if (objComponent == null)
            {
                objComponent = targetObject.GetComponent(componentType);
            }

            if (parameterTypes == null)
            {
                parameterTypes = GetParameterTypes();
            }

            if (objMethod == null)
            {
                objMethod = UnityEvent.GetValidMethodInfo(objComponent, targetMethod, parameterTypes);
            }
        }

        protected virtual IEnumerator ExecuteCoroutine()
        {
            yield return host.StartCoroutine((IEnumerator)objMethod.Invoke(objComponent, GetParameterValues()));

            if (callMode == CallMode.WaitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual System.Type[] GetParameterTypes()
        {
            System.Type[] types = new System.Type[methodParameters.Length];

            for (int i = 0; i < methodParameters.Length; i++)
            {
                InvokeMethodParameter item = methodParameters[i];
                Type objType = ReflectionHelper.GetType(item.objValue.typeAssemblyname);

                types[i] = objType;
            }

            return types;
        }

        protected virtual object[] GetParameterValues()
        {
            object[] values = new object[methodParameters.Length];
            Blackboard blackboard = GetBlackboard();

            for (int i = 0; i < methodParameters.Length; i++)
            {
                InvokeMethodParameter item = methodParameters[i];

                if (string.IsNullOrEmpty(item.variableKey))
                {
                    values[i] = item.objValue.GetValue();
                }
                else
                {
                    object objValue = null;
                    Variable variable = blackboard.GetVariable(item.variableKey);

                    if (variable != null)
                    {
                        switch (variable)
                        {
                            case IntegerVariable iVar: objValue = iVar.Value; break;
                            case BooleanVariable bVar: objValue = bVar.Value; break;
                            case FloatVariable fVar: objValue = fVar.Value; break;
                            case StringVariable sVar: objValue = sVar.Value; break;
                            case ColorVariable cVar: objValue = cVar.Value; break;
                            case GameObjectVariable goVar: objValue = goVar.Value; break;
                            case MaterialVariable mVar: objValue = mVar.Value; break;
                            case SpriteVariable spVar: objValue = spVar.Value; break;
                            case TextureVariable tVar: objValue = tVar.Value; break;
                            case Vector2Variable v2Var: objValue = v2Var.Value; break;
                            case Vector3Variable v3Var: objValue = v3Var.Value; break;
                            case ObjectVariable oVar: objValue = oVar.Value; break;
                        }
                    }

                    values[i] = objValue;
                }
            }

            return values;
        }

        protected virtual void SetVariable(string key, object value)
        {
            Blackboard blackboard = GetBlackboard();
            Variable variable = blackboard.GetVariable(key);

            if (variable == null)
            {
                return;
            }

            switch (variable)
            {
                case IntegerVariable iVar: iVar.Value = (int)value; break;
                case BooleanVariable bVar: bVar.Value = (bool)value; break;
                case FloatVariable fVar: fVar.Value = (float)value; break;
                case StringVariable sVar: sVar.Value = (string)value; break;
                case ColorVariable cVar: cVar.Value = (UnityEngine.Color)value; break;
                case GameObjectVariable goVar: goVar.Value = (UnityEngine.GameObject)value; break;
                case MaterialVariable mVar: mVar.Value = (UnityEngine.Material)value; break;
                case SpriteVariable spVar: spVar.Value = (UnityEngine.Sprite)value; break;
                case TextureVariable tVar: tVar.Value = (UnityEngine.Texture)value; break;
                case Vector2Variable v2Var: v2Var.Value = (UnityEngine.Vector2)value; break;
                case Vector3Variable v3Var: v3Var.Value = (UnityEngine.Vector3)value; break;
                case ObjectVariable oVar: oVar.Value = (UnityEngine.Object)value; break;
            }
        }

        #region Public members

        /// <summary>
        /// GameObject containing the component method to be invoked.
        /// </summary>
        public virtual GameObject TargetObject { get { return targetObject; } }

        public override void OnEnter()
        {
            try
            {
                if (targetObject == null || string.IsNullOrEmpty(targetComponentAssemblyName) || string.IsNullOrEmpty(targetMethod))
                {
                    Continue();
                    return;
                }

                if (returnValueType != "System.Collections.IEnumerator")
                {
                    object objReturnValue = objMethod.Invoke(objComponent, GetParameterValues());

                    if (saveReturnValue)
                    {
                        SetVariable(returnValueVariableKey, objReturnValue);
                    }

                    Continue();
                }
                else
                {
                    host.StartCoroutine(ExecuteCoroutine());

                    if (callMode == CallMode.Continue)
                    {
                        Continue();
                    }
                    else if (callMode == CallMode.Stop)
                    {
                        StopParentBlock();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InvokeMethod] Error invoking '{targetMethod}' on {targetComponentText}: {ex.Message}\n{ex.StackTrace}");
            }
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

    [System.Serializable]
    public class InvokeMethodParameter
    {
        [SerializeField]
        [Tooltip("The Obj value")]
        public ObjectValue objValue;

        [SerializeField]
        [Tooltip("The Variable key")]
        public string variableKey;
    }

    [System.Serializable]
    public class ObjectValue
    {
        [Tooltip("The Type assemblyname")]
        public string typeAssemblyname;
        [Tooltip("The Type fullname")]
        public string typeFullname;

        [Tooltip("The Int value")]
        public int intValue;
        [Tooltip("The Bool value")]
        public bool boolValue;
        [Tooltip("The Float value")]
        public float floatValue;
        [Tooltip("The String value")]
        public string stringValue;

        [Tooltip("The Color value")]
        public Color colorValue;
        [Tooltip("The Game object value")]
        public GameObject gameObjectValue;
        [Tooltip("The Material value")]
        public Material materialValue;
        public UnityEngine.Object objectValue;
        [Tooltip("The Sprite value")]
        public Sprite spriteValue;
        [Tooltip("The Texture value")]
        public Texture textureValue;
        [Tooltip("The Vector2 value")]
        public Vector2 vector2Value;
        [Tooltip("The Vector3 value")]
        public Vector3 vector3Value;

        public object GetValue()
        {
            switch (typeFullname)
            {
                case "System.Int32":
                    return intValue;
                case "System.Boolean":
                    return boolValue;
                case "System.Single":
                    return floatValue;
                case "System.String":
                    return stringValue;
                case "UnityEngine.Color":
                    return colorValue;
                case "UnityEngine.GameObject":
                    return gameObjectValue;
                case "UnityEngine.Material":
                    return materialValue;
                case "UnityEngine.Sprite":
                    return spriteValue;
                case "UnityEngine.Texture":
                    return textureValue;
                case "UnityEngine.Vector2":
                    return vector2Value;
                case "UnityEngine.Vector3":
                    return vector3Value;
                default:
                    Type objType = ReflectionHelper.GetType(typeAssemblyname);

                    if (objType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        return objectValue;
                    }
                    else if (objType.IsEnum())
                    {
                        return System.Enum.ToObject(objType, intValue);
                    }

                    break;
            }

            return null;
        }
    }

    public static class ReflectionHelper
    {
        static Dictionary<string, System.Type> types = new Dictionary<string, System.Type>();

        public static System.Type GetType(string AssemblyQualifiedNameTypeName)
        {
            if (types.ContainsKey(AssemblyQualifiedNameTypeName) && types[AssemblyQualifiedNameTypeName] != null)
            {
                return types[AssemblyQualifiedNameTypeName];
            }

            types[AssemblyQualifiedNameTypeName] = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.AssemblyQualifiedName == AssemblyQualifiedNameTypeName);

            return types[AssemblyQualifiedNameTypeName];
        }
    }
}
