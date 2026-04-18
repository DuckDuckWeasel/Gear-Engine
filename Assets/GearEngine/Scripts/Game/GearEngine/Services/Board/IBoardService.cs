using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.GearEngine.Services.Board
{
    public interface IBoardService
    {
        BoardModel GetBoard();

        BoardConfigSO BoardConfig { get; }

        bool IsSimulationRunning { get; }

        int CurrentBoardGearCount { get; }

        int MaxAllowedBoardGears { get; }

        IGridNode GetNode(Vector2Int coord);

        IEnumerable<IGridNode> GetAllNodes();

        void ToggleSimulation();

        void LoadLayout(BoardLayoutData layout);

        void ExtractNodeForDrag(Vector2Int fromPos);

        bool TryMoveBoardGear(IGridNode node, Vector2Int toPos, Vector2Int fromPos);

        bool TryPlace(Vector2Int targetDropPos, GearConfigData gearData);

        bool TryRemoveBoardGear(IGridNode node);

        bool TryDeleteBoardGear(IGridNode node);

        void SnapNodeBackToOriginal(IGridNode node, Vector2Int originalPos);

        event Action<IGridNode> GearPlaced;

        event Action<IGridNode> GearRemoved;
    }
}
