using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting.Authoring
{
    [Serializable]
    public sealed class BlackboardAuthoringMetadata
    {
        public Vector2 ScrollPosition
        {
            get => scrollPosition;
            set => scrollPosition = value;
        }

        [SerializeField] private Vector2 scrollPosition;

        public float Zoom
        {
            get => zoom;
            set => zoom = Mathf.Clamp(value, 0.25f, 2f);
        }

        [SerializeField] private float zoom = 1f;

        public DefinitionId SelectedBlockId
        {
            get => selectedBlockId;
            set => selectedBlockId = value;
        }

        [SerializeField] private DefinitionId selectedBlockId;

        public DefinitionId SelectedTrackId
        {
            get => selectedTrackId;
            set => selectedTrackId = value;
        }

        [SerializeField] private DefinitionId selectedTrackId;

        public List<DefinitionId> SelectedActionIds => selectedActionIds;

        [SerializeField] private List<DefinitionId> selectedActionIds = new List<DefinitionId>();

        public List<BlockAuthoringMetadata> BlockLayouts => blockLayouts;

        [SerializeField] private List<BlockAuthoringMetadata> blockLayouts = new List<BlockAuthoringMetadata>();

        public List<ActionGroupAuthoringMetadata> ActionGroups => actionGroups;

        [SerializeField] private List<ActionGroupAuthoringMetadata> actionGroups = new List<ActionGroupAuthoringMetadata>();

        public void ClearSelection()
        {
            selectedBlockId = DefinitionId.Empty;
            selectedTrackId = DefinitionId.Empty;
            selectedActionIds.Clear();
        }
    }
}
