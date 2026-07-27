using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting.Authoring
{
    [Serializable]
    public sealed class ActionGroupAuthoringMetadata
    {
        public ActionGroupAuthoringMetadata()
        {
        }

        public ActionGroupAuthoringMetadata(DefinitionId trackId, string name)
        {
            this.trackId = trackId;
            this.name = name ?? string.Empty;
        }

        public string GroupId => groupId;

        [SerializeField] private string groupId = Guid.NewGuid().ToString("N");

        public DefinitionId TrackId
        {
            get => trackId;
            set => trackId = value;
        }

        [SerializeField] private DefinitionId trackId;

        public string Name
        {
            get => name;
            set => name = value ?? string.Empty;
        }

        [SerializeField] private string name = "Group";

        public List<DefinitionId> ActionIds => actionIds;

        [SerializeField] private List<DefinitionId> actionIds = new List<DefinitionId>();
    }
}
