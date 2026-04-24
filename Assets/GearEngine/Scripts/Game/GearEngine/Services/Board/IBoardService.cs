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

        BoardRulesSO BoardRules { get; }

        bool IsSimulationRunning { get; }

        int CurrentBoardGearCount { get; }

        int MaxAllowedBoardGears { get; }

        /// <summary>True when a motor (Core) gear is on the board and <see cref="IInventoryService.MotorCogGearId"/> is set, or when motor id is not configured (tests / empty inventory).</summary>
        bool ContainsMotorCog { get; }

        IGridNode GetNode(Vector2Int coord);

        IEnumerable<IGridNode> GetAllNodes();

        void ToggleSimulation();

        void LoadLayout(BoardLayoutData layout);

        bool TryMoveBoardGear(IGridNode node, Vector2Int toPos, Vector2Int fromPos);

        bool TryPlace(Vector2Int targetDropPos, GearConfigData gearData);

        bool TryRemoveBoardGear(IGridNode node);

        bool TryDeleteBoardGear(IGridNode node);

        void SnapNodeBackToOriginal(IGridNode node, Vector2Int originalPos);

        event Action<IGridNode> GearPlaced;

        event Action<IGridNode> GearRemoved;

        /// <summary>Fired when board node positions or count change (not when only simulation run state toggles).</summary>
        event Action BoardLayoutChanged;
    }

    /// <summary>
    /// When <see cref="BoardSlotCapacity"/> is greater than zero, caps how many gears may be placed on the board
    /// (e.g. LiveOps loadout baseSlots). Otherwise <see cref="BoardService"/> falls back to <see cref="BoardRulesSO.MaxBoardGears"/>.
    /// </summary>
    public interface IBoardSlotCapacityProvider
    {
        int BoardSlotCapacity { get; }
    }

    /// <summary>
    /// Sandbox / non-LiveOps scopes: no stricter cap than the grid fallback in <see cref="BoardService"/>.
    /// </summary>
    public sealed class UnlimitedBoardSlotCapacityProvider : IBoardSlotCapacityProvider
    {
        public int BoardSlotCapacity => int.MaxValue;
    }
}
