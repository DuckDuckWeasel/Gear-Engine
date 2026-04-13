using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.GearEngine
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

        [SerializeField] private List<BoardGearPlacementData> placements = new List<BoardGearPlacementData>();
    }
}
