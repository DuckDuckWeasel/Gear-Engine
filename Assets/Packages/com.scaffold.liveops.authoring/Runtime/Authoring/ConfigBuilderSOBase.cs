using System;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring
{
    /// <summary>
    /// Non-generic marker so the deployment window can discover all builders with <c>AssetDatabase.FindAssets("t:ConfigBuilderSOBase")</c>.
    /// </summary>
    public abstract class ConfigBuilderSOBase : ScriptableObject
    {
        public abstract string ConfigKey { get; }

        public abstract object BuildBoxed();

        public abstract void ApplyBoxed(object pulled);

        public abstract Type ConfigType { get; }
    }
}
