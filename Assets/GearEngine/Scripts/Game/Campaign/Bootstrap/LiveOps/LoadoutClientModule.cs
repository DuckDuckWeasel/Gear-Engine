using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class LoadoutClientModule : GameClientModuleBase<LoadoutGameData>, IGearLoadoutService, IBoardSlotCapacityProvider
    {
        public LoadoutClientModule(ILiveOpsService liveOps, IInventoryService inventoryService) : base(liveOps)
        {
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        public bool HasSavedLoadout => true;

        public int BoardSlotCapacity => data?.BaseSlots ?? 0;

        private readonly IInventoryService inventoryService;

        public BoardLayoutData GetBoardLayout()
        {
            Dictionary<string, OwnedGear> byId = BuildOwnedByInstanceId();
            List<BoardGearPlacementData> items;
            bool motorPresent = AppendPlacementsFromLoadout(byId, out items);
            EnsureMotorCogPlaced(items, motorPresent);
            return new BoardLayoutData(items);
        }

        public void PersistBoardLayout(BoardLayoutData layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            List<LoadoutPlacement> placements = ToLoadoutPlacements(layout);
            List<LoadoutPlacement> previous = CloneBoardSnapshot();
            ApplyOptimisticBoard(placements);
            _ = SendBoardLayoutAsync(placements, previous);
        }

        private Dictionary<string, OwnedGear> BuildOwnedByInstanceId()
        {
            return inventoryService.Owned
                .Where(o => o != null && !string.IsNullOrEmpty(o.InstanceId))
                .ToDictionary(o => o.InstanceId, o => o);
        }

        private bool AppendPlacementsFromLoadout(Dictionary<string, OwnedGear> byInstanceId, out List<BoardGearPlacementData> items)
        {
            List<LoadoutPlacement> board = data?.Board ?? new List<LoadoutPlacement>();
            items = new List<BoardGearPlacementData>(board.Count);
            bool motorPresent = false;
            string motorId = inventoryService.MotorCogGearId;

            foreach (LoadoutPlacement p in board)
            {
                AppendPlacementIfValid(p, byInstanceId, motorId, items, ref motorPresent);
            }

            return motorPresent;
        }

        private void AppendPlacementIfValid(LoadoutPlacement p, Dictionary<string, OwnedGear> byInstanceId, string motorId, List<BoardGearPlacementData> items, ref bool motorPresent)
        {
            if (p == null || string.IsNullOrEmpty(p.InstanceId))
            {
                return;
            }

            if (!byInstanceId.TryGetValue(p.InstanceId, out OwnedGear owner))
            {
                Debug.LogError($"[LoadoutClientModule] No inventory entry for loadout instanceId '{p.InstanceId}'.");
                return;
            }

            if (owner.Config != null && owner.Config.Id == motorId)
            {
                motorPresent = true;
            }

            AddPlacementFromLoadoutRow(p, owner, items);
        }

        private void AddPlacementFromLoadoutRow(LoadoutPlacement p, OwnedGear owner, List<BoardGearPlacementData> items)
        {
            Vector2Int pos = new Vector2Int(p.X, p.Y);
            BoardGearPlacementData row = new BoardGearPlacementData(pos, owner);
            items.Add(row);
        }

        private void EnsureMotorCogPlaced(List<BoardGearPlacementData> items, bool motorPresent)
        {
            if (motorPresent || string.IsNullOrEmpty(inventoryService.MotorCogGearId))
            {
                return;
            }

            OwnedGear motor = inventoryService.Owned.FirstOrDefault(o => o?.Config?.Id == inventoryService.MotorCogGearId);
            if (motor == null)
            {
                Debug.LogError("[LoadoutClientModule] Motor cog missing from inventory; cannot auto-place.");
                return;
            }

            Vector2Int cell = data != null
                ? new Vector2Int(data.MotorCogStartX, data.MotorCogStartY)
                : new Vector2Int(2, 2);
            items.RemoveAll(p => p.Position == cell);
            BoardGearPlacementData motorPlacement = new BoardGearPlacementData(cell, motor);
            items.Add(motorPlacement);
        }

        private List<LoadoutPlacement> CloneBoardSnapshot()
        {
            if (data?.Board == null)
            {
                return null;
            }

            return data.Board.Select(p => new LoadoutPlacement
            {
                InstanceId = p.InstanceId,
                GearId = p.GearId,
                X = p.X,
                Y = p.Y
            }).ToList();
        }

        private void ApplyOptimisticBoard(List<LoadoutPlacement> placements)
        {
            if (data != null)
            {
                data.Board = placements;
            }
        }

        private async Task SendBoardLayoutAsync(List<LoadoutPlacement> placements, List<LoadoutPlacement> previous)
        {
            try
            {
                SaveBoardLayoutRequest request = new SaveBoardLayoutRequest(placements);
                SaveBoardLayoutResponse response = await liveOps.CallAsync(request);
                RollbackIfRejected(response, previous);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadoutClientModule] PersistBoardLayout failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RollbackIfRejected(SaveBoardLayoutResponse response, List<LoadoutPlacement> previous)
        {
            if (response == null || !response.Rejected)
            {
                return;
            }

            if (data != null && previous != null)
            {
                data.Board = previous;
            }

            Debug.LogError($"[LoadoutClientModule] SaveBoardLayout rejected: {response.Reason}");
        }

        private static List<LoadoutPlacement> ToLoadoutPlacements(BoardLayoutData layout)
        {
            return layout.Placements
                .Where(p => p?.Owner != null)
                .Select(p => new LoadoutPlacement
                {
                    InstanceId = p.Owner.InstanceId,
                    GearId = p.Owner.Config.Id,
                    X = p.Position.x,
                    Y = p.Position.y
                })
                .ToList();
        }
    }
}
