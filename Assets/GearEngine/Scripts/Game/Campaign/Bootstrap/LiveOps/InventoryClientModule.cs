using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class InventoryClientModule : GameClientModuleBase<InventoryGameData>, IInventoryService
    {
        public InventoryClientModule(ILiveOpsService liveOps, GearCatalogSO catalog) : base(liveOps)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public string MotorCogGearId => data?.MotorCogGearId ?? string.Empty;

        public bool HasSavedInventory => ownedRefs.Count > 0;

        public IReadOnlyList<OwnedGear> Owned => ownedRefs;

        private readonly GearCatalogSO catalog;
        private readonly List<OwnedGear> ownedRefs = new List<OwnedGear>();

        public event Action InventoryChanged;

        public OwnedGear Add(GearConfig gear)
        {
            if (!TryValidateGearForAdd(gear))
            {
                return null;
            }

            var owned = new OwnedGear { InstanceId = Guid.NewGuid().ToString("N"), Config = gear };
            ownedRefs.Add(owned);
            SyncModuleDataFromOwnedRefs();
            InventoryChanged?.Invoke();
            SchedulePersist();
            return owned;
        }

        public bool Remove(OwnedGear gear)
        {
            if (gear == null || !ownedRefs.Remove(gear))
            {
                return false;
            }

            SyncModuleDataFromOwnedRefs();
            InventoryChanged?.Invoke();
            SchedulePersist();
            return true;
        }

        public void Clear()
        {
            if (ownedRefs.Count == 0)
            {
                return;
            }

            ownedRefs.Clear();
            SyncModuleDataFromOwnedRefs();
            InventoryChanged?.Invoke();
            SchedulePersist();
        }

        protected override Task OnInitializedAsync(InventoryGameData moduleData)
        {
            ownedRefs.Clear();
            if (moduleData?.Gears != null)
            {
                foreach (OwnedGearEntry entry in moduleData.Gears)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.GearId) || string.IsNullOrEmpty(entry.InstanceId))
                    {
                        continue;
                    }

                    GearConfig cfg = catalog.Get(entry.GearId);
                    if (cfg == null)
                    {
                        Debug.LogError($"[InventoryClientModule] Unknown gear id in saved inventory: '{entry.GearId}'.");
                        continue;
                    }

                    ownedRefs.Add(new OwnedGear { InstanceId = entry.InstanceId, Config = cfg });
                }
            }

            return base.OnInitializedAsync(moduleData);
        }

        private void SyncModuleDataFromOwnedRefs()
        {
            if (data == null)
            {
                return;
            }

            data.Gears = ownedRefs
                .Select(o => new OwnedGearEntry { InstanceId = o.InstanceId, GearId = o.Config.Id })
                .ToList();
        }

        private bool TryValidateGearForAdd(GearConfig gear)
        {
            if (gear == null)
            {
                return false;
            }

            if (!EnsureInitialized("Add"))
            {
                return false;
            }

            if (string.IsNullOrEmpty(gear.Id))
            {
                Debug.LogError("[InventoryClientModule] Add: gear has no Id.");
                return false;
            }

            return true;
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

        private void SchedulePersist()
        {
            List<OwnedGearEntry> snapshot = ownedRefs
                .Select(o => new OwnedGearEntry { InstanceId = o.InstanceId, GearId = o.Config.Id })
                .ToList();
            _ = SendInventoryAsync(snapshot);
        }

        private async Task SendInventoryAsync(List<OwnedGearEntry> snapshot)
        {
#if UNITY_EDITOR
            int n = snapshot != null ? snapshot.Count : 0;
            Debug.Log($"[InventoryClientModule] SetInventoryRequest starting ({n} gear(s))...");
#endif
            try
            {
                await liveOps.CallAsync(new SetInventoryRequest(snapshot));
#if UNITY_EDITOR
                Debug.Log($"[InventoryClientModule] SetInventoryRequest finished OK ({n} gear(s)).");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InventoryClientModule] SendInventoryAsync failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
