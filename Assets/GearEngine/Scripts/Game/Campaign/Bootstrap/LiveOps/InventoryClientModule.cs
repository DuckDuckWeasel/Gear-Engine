using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class InventoryClientModule : GameClientModuleBase<InventoryGameData>, IInventoryService
    {
        public InventoryClientModule(IObjectResolver resolver, ILiveOpsService liveOps, GearCatalogSO catalog) : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public bool HasSavedInventory => data != null && data.GearIds.Count > 0;

        public IReadOnlyList<GearConfig> Owned => BuildOwnedList();

        private readonly ILiveOpsService liveOpsService;
        private readonly GearCatalogSO catalog;

        public event Action InventoryChanged;

        public bool TryAdd(GearConfig gear)
        {
            if (!TryValidateGearForAdd(gear))
            {
                return false;
            }

            data.GearIds.Add(gear.Id);
            PublishInventoryUpdated();
            return true;
        }

        public bool TryRemove(GearConfig gear)
        {
            if (!TryResolveRemovalIndex(gear, out int idx))
            {
                return false;
            }

            data.GearIds.RemoveAt(idx);
            PublishInventoryUpdated();
            return true;
        }

        public void Clear()
        {
            if (!EnsureInitialized("Clear"))
            {
                return;
            }

            data.GearIds.Clear();
            PublishInventoryUpdated();
        }

        private IReadOnlyList<GearConfig> BuildOwnedList()
        {
            if (data == null)
            {
                return Array.Empty<GearConfig>();
            }

            var list = new List<GearConfig>(data.GearIds.Count);
            foreach (string id in data.GearIds)
            {
                GearConfig g = catalog.Get(id);
                if (g != null)
                {
                    list.Add(g);
                }
            }

            return list;
        }

        private bool TryValidateGearForAdd(GearConfig gear)
        {
            if (gear == null)
            {
                return false;
            }

            if (!EnsureInitialized("TryAdd"))
            {
                return false;
            }

            if (string.IsNullOrEmpty(gear.Id))
            {
                Debug.LogError("[InventoryClientModule] TryAdd: gear has no Id.");
                return false;
            }

            return true;
        }

        private bool TryResolveRemovalIndex(GearConfig gear, out int idx)
        {
            idx = -1;
            if (gear == null || data?.GearIds == null)
            {
                return false;
            }

            idx = data.GearIds.FindIndex(id => id == gear.Id);
            return idx >= 0;
        }

        private bool EnsureInitialized(string operationLabel)
        {
            if (data != null)
            {
                return true;
            }

            Debug.LogError($"[InventoryClientModule] {operationLabel}: module data is not initialized.");
            return false;
        }

        private void PublishInventoryUpdated()
        {
            InventoryChanged?.Invoke();
            _ = SendInventoryAsync(new List<string>(data.GearIds));
        }

        private async Task SendInventoryAsync(List<string> ids)
        {
#if UNITY_EDITOR
            int n = ids != null ? ids.Count : 0;
            Debug.Log($"[InventoryClientModule] SetInventoryRequest starting ({n} id(s))...");
#endif
            try
            {
                await liveOpsService.CallAsync(new SetInventoryRequest(ids));
#if UNITY_EDITOR
                Debug.Log($"[InventoryClientModule] SetInventoryRequest finished OK ({n} id(s)).");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InventoryClientModule] SendInventoryAsync failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
