using System;
using GearEngine.Core.Actions;

using UnityEngine;
using MoonSharp.Interpreter;

namespace Scaffold
{
    /// <summary>
    /// Executes a Lua code chunk using a Lua Environment.
    /// </summary>
    [CommandInfo("Lua",
                 "Execute Lua",
                 "Executes a Lua code chunk using a Lua Environment.")]
    [Serializable]
    public class ExecuteLua : ActionBase
    {
        [Tooltip("Lua Environment to use to execute this Lua script")]
        [SerializeField] protected LuaEnvironment luaEnvironment;

        [Tooltip("A text file containing Lua script to execute.")]
        [SerializeField] protected TextAsset luaFile;

        [TextArea(10, 100)]
        [Tooltip("Lua script to execute. This text is appended to the contents of Lua file (if one is specified).")]
        [SerializeField] protected string luaScript;

        [Tooltip("Execute this Lua script as a Lua coroutine")]
        [SerializeField] protected bool runAsCoroutine = true;

        [Tooltip("Pause command execution until the Lua script has finished execution")]
        [SerializeField] protected bool waitUntilFinished = true;

        [Tooltip("A Blackboard Variable to store the returned value in.")]
        [VariableProperty()]
        [SerializeField] protected Variable returnVariable;

        protected string friendlyName = "";

        [Tooltip("The Initialised")]
        protected bool initialised;

        // Stores the compiled Lua code for fast execution later.
        [Tooltip("The Lua function")]
        protected Closure luaFunction;

        protected virtual void Start()
        {
            InitExecuteLua();
        }

        /// <summary>
        /// Initialises the Lua environment and compiles the Lua string for execution later on.
        /// </summary>
        protected virtual void InitExecuteLua()
        {
            if (initialised)
            {
                return;
            }

            // Cache a descriptive name to use in Lua error messages
            friendlyName = host.gameObject.name + "." + ParentBlock.BlockName + "." + "ExecuteLua #" + CommandIndex.ToString();

            Blackboard blackboard = GetBlackboard();

            // See if a Lua Environment has been assigned to this Blackboard
            if (luaEnvironment == null)
            {
                luaEnvironment = blackboard.LuaEnv;
            }

            if (luaEnvironment == null)
            {
                // No Lua Environment specified so just use any available or create one.
                luaEnvironment = LuaEnvironment.GetLua();
            }

            string s = GetLuaString();
            luaFunction = luaEnvironment.LoadLuaFunction(s, friendlyName);

            // Add a binding to the parent blackboard
            if (blackboard.LuaBindingName != "")
            {
                Table globals = luaEnvironment.Interpreter.Globals;
                if (globals != null)
                {
                    globals[blackboard.LuaBindingName] = blackboard;
                }
            }

            // Always initialise when playing in the editor.
            // Allows the user to edit the Lua script while the game is playing.
            if (!(Application.isPlaying && Application.isEditor))
            {
                initialised = true;
            }

        }

        protected virtual string GetLuaString()
        {
            if (luaFile == null)
            {
                return luaScript;
            }

            return luaFile.text + "\n" + luaScript;
        }

        protected virtual void StoreReturnVariable(DynValue returnValue)
        {
            if (returnVariable == null || returnValue == null)
            {
                return;
            }

            switch (returnVariable)
            {
                case BooleanVariable b when returnValue.Type == DataType.Boolean:
                    b.Value = returnValue.Boolean;
                    break;
                case IntegerVariable i when returnValue.Type == DataType.Number:
                    i.Value = (int)returnValue.Number;
                    break;
                case FloatVariable f when returnValue.Type == DataType.Number:
                    f.Value = (float)returnValue.Number;
                    break;
                case StringVariable s when returnValue.Type == DataType.String:
                    s.Value = returnValue.String;
                    break;
                case ColorVariable c when returnValue.Type == DataType.UserData:
                    c.Value = returnValue.CheckUserDataType<Color>("ExecuteLua.StoreReturnVariable");
                    break;
                case GameObjectVariable go when returnValue.Type == DataType.UserData:
                    go.Value = returnValue.CheckUserDataType<GameObject>("ExecuteLua.StoreReturnVariable");
                    break;
                case MaterialVariable m when returnValue.Type == DataType.UserData:
                    m.Value = returnValue.CheckUserDataType<Material>("ExecuteLua.StoreReturnVariable");
                    break;
                case ObjectVariable o when returnValue.Type == DataType.UserData:
                    o.Value = returnValue.CheckUserDataType<UnityEngine.Object>("ExecuteLua.StoreReturnVariable");
                    break;
                case SpriteVariable sp when returnValue.Type == DataType.UserData:
                    sp.Value = returnValue.CheckUserDataType<Sprite>("ExecuteLua.StoreReturnVariable");
                    break;
                case TextureVariable t when returnValue.Type == DataType.UserData:
                    t.Value = returnValue.CheckUserDataType<Texture>("ExecuteLua.StoreReturnVariable");
                    break;
                case Vector2Variable v2 when returnValue.Type == DataType.UserData:
                    v2.Value = returnValue.CheckUserDataType<Vector2>("ExecuteLua.StoreReturnVariable");
                    break;
                case Vector3Variable v3 when returnValue.Type == DataType.UserData:
                    v3.Value = returnValue.CheckUserDataType<Vector3>("ExecuteLua.StoreReturnVariable");
                    break;
                default:
                    Debug.LogError("Failed to convert " + returnValue.Type.ToLuaTypeString() + " return type to " + returnVariable.GetType().ToString());
                    break;
            }
        }

        #region Public members

        public override void OnEnter()
        {
            InitExecuteLua();

            if (luaFunction == null)
            {
                Continue();
            }

            luaEnvironment.RunLuaFunction(luaFunction, runAsCoroutine, (returnValue) =>
            {
                StoreReturnVariable(returnValue);
                if (waitUntilFinished)
                {
                    Continue();
                }
            });

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            return luaScript;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return returnVariable == variable || base.HasReference(variable);
        }

        #endregion
    }
}
