using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Loadout;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class LoadoutClientModule : GameClientModuleBase<LoadoutGameData>, IGearLoadoutService
    {
        private readonly ILiveOpsService liveOpsService;
        private readonly GearCatalogSO catalog;
        private readonly IInventoryService inventoryService;

        public LoadoutClientModule(IObjectResolver resolver, ILiveOpsService liveOps, GearCatalogSO catalog, IInventoryService inventoryService)
            : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        public bool HasSavedLoadout => data != null && data.Board.Count > 0;

        public BoardLayoutData GetBoardLayout()
        {
            if (!HasSavedLoadout)
            {
                return null;
            }

            Dictionary<string, OwnedGear> byInstanceId = inventoryService.Owned
                .Where(o => o != null && !string.IsNullOrEmpty(o.InstanceId))
                .ToDictionary(o => o.InstanceId, o => o);

            var items = new List<BoardGearPlacementData>(data.Board.Count);
            foreach (LoadoutPlacement p in data.Board)
            {
                if (p == null || string.IsNullOrEmpty(p.InstanceId))
                {
                    continue;
                }

                if (!byInstanceId.TryGetValue(p.InstanceId, out OwnedGear owner))
                {
                    Debug.LogError($"[LoadoutClientModule] No inventory entry for loadout instanceId '{p.InstanceId}'.");
                    continue;
                }

                items.Add(new BoardGearPlacementData(new Vector2Int(p.X, p.Y), owner));
            }

            return new BoardLayoutData(items);
        }

        /// <summary>Called when the local board layout changes; persists optimistically and fires LiveOps in the background.</summary>
        public void PersistBoardLayout(BoardLayoutData layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            List<LoadoutPlacement> placements = layout.Placements
                .Where(p => p?.Owner != null)
                .Select(p => new LoadoutPlacement
                {
                    InstanceId = p.Owner.InstanceId,
                    GearId = p.Owner.Config.Id,
                    X = p.Position.x,
                    Y = p.Position.y
                })
                .ToList();

            if (data != null)
            {
                data.Board = placements;
            }

            _ = SendBoardLayoutAsync(placements);
        }

        private async Task SendBoardLayoutAsync(List<LoadoutPlacement> placements)
        {
            try
            {
                await liveOpsService.CallAsync(new SaveBoardLayoutRequest(placements));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadoutClientModule] PersistBoardLayout failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
