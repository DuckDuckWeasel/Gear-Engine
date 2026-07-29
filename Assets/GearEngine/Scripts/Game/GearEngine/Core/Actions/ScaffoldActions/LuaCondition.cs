using System;
using GearEngine.Core.Actions;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scaffold;
using MoonSharp.Interpreter;

namespace Scaffold
{
    [Serializable]
    public class LuaCondition : Condition
    {
        [Tooltip("Lua Environment to use to execute this Lua script (null for global)")]
        [SerializeField] protected LuaEnvironment luaEnvironment;

        [Tooltip("The lua comparison string to run; implicitly prepends 'return' onto this")]
        [TextArea]
        public string luaCompareString;
        [Tooltip("The Initialised")]
        protected bool initialised;
        [Tooltip("The Friendly name")]
        protected string friendlyName = "";
        [Tooltip("The Lua function")]
        protected Closure luaFunction;

        protected override bool EvaluateCondition()
        {
            bool condition = false;
            luaEnvironment.RunLuaFunction(luaFunction, false, (returnValue) =>
            {
                if (returnValue != null)
                {
                    condition = returnValue.Boolean;
                }
                else
                {
                    Debug.LogWarning("No return value from " + friendlyName);
                }
            });
            return condition;
        }

        protected override bool HasNeededProperties()
        {
            return !string.IsNullOrEmpty(luaCompareString);
        }

        protected virtual void Start()
        {
            InitExecuteLua();
        }

        protected virtual string GetLuaString()
        {
            return "return not not (" + luaCompareString + ")";
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
            friendlyName = GetLocationIdentifier();

            if (luaEnvironment == null)
            {
                throw new InvalidOperationException("Lua Condition requires an explicit Lua Environment reference.");
            }

            string s = GetLuaString();
            luaFunction = luaEnvironment.LoadLuaFunction(s, friendlyName);

            // Always initialise when playing in the editor.
            // Allows the user to edit the Lua script while the game is playing.
            if (!(Application.isPlaying && Application.isEditor))
            {
                initialised = true;
            }

        }

        #region Public members

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(luaCompareString))
            {
                return "Error: no lua compare string provided";
            }

            return luaCompareString;
        }

        #endregion
    }
}
