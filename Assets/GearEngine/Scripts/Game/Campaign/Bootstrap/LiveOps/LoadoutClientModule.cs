using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Loadout;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class LoadoutClientModule : GameClientModuleBase<LoadoutGameData>, IGearLoadoutService
    {
        private readonly ILiveOpsService liveOpsService;
        private readonly GearCatalogSO catalog;

        public LoadoutClientModule(IObjectResolver resolver, ILiveOpsService liveOps, GearCatalogSO catalog)
            : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public bool HasSavedLoadout => data != null && data.Board.Count > 0;

        public BoardLayoutData GetBoardLayout()
        {
            if (!HasSavedLoadout)
            {
                return null;
            }

            var items = new List<BoardGearPlacementData>(data.Board.Count);
            foreach (LoadoutPlacement p in data.Board)
            {
                GearConfig g = catalog.Get(p.GearId);
                if (g != null)
                {
                    items.Add(new BoardGearPlacementData(new Vector2Int(p.X, p.Y), g));
                }
            }

            return new BoardLayoutData(items);
        }

        public async void SaveBoardLayout(BoardLayoutData layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            try
            {
                List<LoadoutPlacement> placements = layout.Placements
                    .Where(p => p?.GearConfig != null && !string.IsNullOrEmpty(p.GearConfig.Id))
                    .Select(p => new LoadoutPlacement { GearId = p.GearConfig.Id, X = p.Position.x, Y = p.Position.y })
                    .ToList();
                SaveBoardLayoutResponse resp = await liveOpsService.CallAsync(new SaveBoardLayoutRequest(placements));
                if (resp != null && data != null)
                {
                    data.Board = placements;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadoutClientModule] SaveBoardLayout failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
