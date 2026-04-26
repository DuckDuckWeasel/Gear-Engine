using System;
using LiveOps.DTO.Authoring;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring
{
    /// <summary>
    /// Non-generic marker so the deployment window can discover all builders with <c>AssetDatabase.FindAssets("t:ConfigBuilderSOBase")</c>.
    /// </summary>
    public abstract class ConfigBuilderSOBase : ScriptableObject
    {
        [Header("Profile")]
        [Tooltip("Omit for the default (Settings) variant. Assign a non-default profile to publish a Game Override.")]
        [SerializeField]
        private ConfigProfileSO profile;

        public abstract string ConfigKey { get; }

        public ConfigProfileSO Profile => profile;

        public string ProfileId
        {
            get
            {
                if (profile == null)
                {
                    return "default";
                }

                return string.IsNullOrEmpty(profile.ProfileId) ? "default" : profile.ProfileId;
            }
        }

        /// <summary>True when this builder writes the environment Settings (legacy path), not a Game Override file.</summary>
        public bool IsDefaultVariant => profile == null || profile.IsDefault;

        public abstract object BuildBoxed();

        public abstract void ApplyBoxed(object pulled);

        public abstract Type ConfigType { get; }
    }

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
