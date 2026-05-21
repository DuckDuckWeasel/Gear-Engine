using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using UnityEngine;

namespace GearEngine.GearEngine
{
    [Serializable]
    public sealed class BoardLayoutData
    {
        public BoardLayoutData()
        {
        }

        public BoardLayoutData(IEnumerable<BoardGearPlacementData> items)
        {
            if (items != null)
            {
                placements = new List<BoardGearPlacementData>(items);
            }
        }

        public IReadOnlyList<BoardGearPlacementData> Placements => placements;

        /// <summary>Builds a layout snapshot from live grid nodes (requires <see cref="GearItemData.SourceGearConfig"/>).</summary>
        public static BoardLayoutData FromNodes(IEnumerable<IGridNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            var items = new List<BoardGearPlacementData>();
            foreach (IGridNode node in nodes)
            {
                if (node?.ConfigData == null)
                {
                    continue;
                }

                GearItem source = node.ConfigData.SourceGearConfig;
                if (source == null)
                {
                    Debug.LogWarning($"[BoardLayoutData] Skipping node at {node.Position}: GearItemData has no SourceGearConfig.");
                    continue;
                }

                OwnedGear owner = node.ConfigData.Owner;
                items.Add(owner != null
                    ? new BoardGearPlacementData(node.Position, owner)
                    : new BoardGearPlacementData(node.Position, source));
            }

            return new BoardLayoutData(items);
        }

        [SerializeField] private List<BoardGearPlacementData> placements = new List<BoardGearPlacementData>();
    }
}
