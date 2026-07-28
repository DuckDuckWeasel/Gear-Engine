using System;
using UnityEngine;

namespace Scaffold.VisualScripting.Authoring
{
    [Serializable]
    public sealed class BlockAuthoringMetadata
    {
        public BlockAuthoringMetadata()
        {
        }

        public BlockAuthoringMetadata(DefinitionId blockId, Rect position)
        {
            this.blockId = blockId;
            this.position = position;
        }

        public DefinitionId BlockId
        {
            get => blockId;
            set => blockId = value;
        }

        [SerializeField] private DefinitionId blockId;

        public Rect Position
        {
            get => position;
            set => position = value;
        }

        [SerializeField] private Rect position;

        public bool UseCustomTint
        {
            get => useCustomTint;
            set => useCustomTint = value;
        }

        [SerializeField] private bool useCustomTint;

        public Color Tint
        {
            get => tint;
            set => tint = value;
        }

        [SerializeField] private Color tint = Color.white;

        public string Description
        {
            get => description;
            set => description = value ?? string.Empty;
        }

        [SerializeField] private string description = string.Empty;

        public bool Expanded
        {
            get => expanded;
            set => expanded = value;
        }

        [SerializeField] private bool expanded = true;
    }
}
