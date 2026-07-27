using System;
using Scaffold.VisualScripting.Authoring;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardAuthoringTarget
    {
        public BlackboardAuthoringTarget(Object owner, BlackboardDefinition definition, BlackboardAuthoringMetadata metadata, string displayName)
        {
            Owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? owner.name : displayName;
        }

        public Object Owner { get; }

        public BlackboardDefinition Definition { get; }

        public BlackboardAuthoringMetadata Metadata { get; }

        public string DisplayName { get; }
    }
}
