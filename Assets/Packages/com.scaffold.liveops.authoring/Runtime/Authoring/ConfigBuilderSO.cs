using System;
using LiveOps.DTO.Authoring;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring
{
    /// <summary>
    /// Base type for module-specific Remote Config builders (editor-authored ScriptableObjects).
    /// </summary>
    public abstract class ConfigBuilderSO<TConfig> : ConfigBuilderSOBase, IConfigBuilder<TConfig>
        where TConfig : class, new()
    {
        public abstract TConfig Build();

        public virtual void Apply(TConfig pulled)
        {
        }

        public override Type ConfigType => typeof(TConfig);

        public override object BuildBoxed() => Build();

        public override void ApplyBoxed(object pulled)
        {
            if (pulled == null)
            {
                return;
            }

            if (pulled is TConfig typed)
            {
                Apply(typed);
            }
        }
    }
}
