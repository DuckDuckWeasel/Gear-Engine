using System;
using System.Collections.Generic;
using System.Reflection;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.LayeredScope;
using UnityEditor;
using UnityEngine;

namespace GearEngine.GearEngine.Editor.Cheats
{
    /// <summary>
    /// Play-mode editor menu items that mutate <see cref="IInventoryService"/> via the running app DI scope.
    /// </summary>
    internal static class InventoryCheats
    {
        private const string MenuRoot = "Gear Engine/Cheats/Inventory/";

        [MenuItem(MenuRoot + "Add Random Gear", false, 0)]
        public static void AddRandomGear()
        {
            if (!TryGetServices(out IInventoryService inventory, out GearCatalogSO catalog))
            {
                return;
            }

            IReadOnlyList<GearConfig> valid = GetValidCatalogGears(catalog);
            if (valid.Count == 0)
            {
                Debug.LogError("[InventoryCheats] Catalog has no valid gears (null entries or missing Id).");
                return;
            }

            GearConfig pick = valid[UnityEngine.Random.Range(0, valid.Count)];
            if (inventory.TryAdd(pick))
            {
                Debug.Log($"[InventoryCheats] Added gear '{pick.Id}'.");
            }
            else
            {
                Debug.LogError($"[InventoryCheats] TryAdd failed for '{pick?.Id}'.");
            }
        }

        [MenuItem(MenuRoot + "Add Random Gear", true)]
        public static bool ValidateAddRandomGear()
        {
            return EditorApplication.isPlaying;
        }

        [MenuItem(MenuRoot + "Add One Of Each Gear", false, 1)]
        public static void AddOneOfEachGear()
        {
            if (!TryGetServices(out IInventoryService inventory, out GearCatalogSO catalog))
            {
                return;
            }

            int added = 0;
            foreach (GearConfig g in catalog.All)
            {
                if (g == null || string.IsNullOrEmpty(g.Id))
                {
                    continue;
                }

                if (inventory.TryAdd(g))
                {
                    added++;
                }
            }

            Debug.Log($"[InventoryCheats] Added {added} gear instance(s) (one per catalog entry).");
        }

        [MenuItem(MenuRoot + "Add One Of Each Gear", true)]
        public static bool ValidateAddOneOfEachGear()
        {
            return EditorApplication.isPlaying;
        }

        [MenuItem(MenuRoot + "Clear Inventory", false, 2)]
        public static void ClearInventory()
        {
            if (!TryGetServices(out IInventoryService inventory, out _))
            {
                return;
            }

            int count = inventory.Owned.Count;
            inventory.Clear();
            Debug.Log($"[InventoryCheats] Cleared inventory (had {count} resolved gear(s) before clear).");
        }

        [MenuItem(MenuRoot + "Clear Inventory", true)]
        public static bool ValidateClearInventory()
        {
            return EditorApplication.isPlaying;
        }

        private static bool TryGetServices(out IInventoryService inventory, out GearCatalogSO catalog)
        {
            inventory = null;
            catalog = null;

            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[InventoryCheats] Available only in Play mode.");
                return false;
            }

            ApplicationBootstrap bootstrap = UnityEngine.Object.FindFirstObjectByType<ApplicationBootstrap>(FindObjectsInactive.Exclude);
            if (bootstrap == null)
            {
                Debug.LogError("[InventoryCheats] No ApplicationBootstrap found in the scene.");
                return false;
            }

            if (!TryGetApplicationHost(bootstrap, out ApplicationHost host))
            {
                Debug.LogError("[InventoryCheats] Could not resolve ApplicationHost (startup may not have finished).");
                return false;
            }

            try
            {
                inventory = host.Resolve<IInventoryService>();
                catalog = host.Resolve<GearCatalogSO>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InventoryCheats] Failed to resolve services: {ex.Message}\n{ex.StackTrace}");
                return false;
            }

            if (inventory == null || catalog == null)
            {
                Debug.LogError("[InventoryCheats] Resolved inventory or catalog was null.");
                return false;
            }

            LogResolvedInventoryService(inventory);
            return true;
        }

        private static void LogResolvedInventoryService(IInventoryService inventory)
        {
            Type impl = inventory.GetType();
            Debug.Log($"[InventoryCheats] IInventoryService implementation: {impl.FullName}");
            if (inventory is EmptyInventoryService)
            {
                Debug.LogWarning(
                    "[InventoryCheats] EmptyInventoryService is registered: adds do not update Owned or call SetInventoryRequest. " +
                    "Run the full Campaign app (InventoryClientModule) if you expect LiveOps persistence logs.");
            }
        }

        /// <summary>
        /// <see cref="ApplicationBootstrap.Host"/> is protected; use reflection so editor code can resolve from the live scope.
        /// </summary>
        private static bool TryGetApplicationHost(ApplicationBootstrap bootstrap, out ApplicationHost host)
        {
            host = null;
            PropertyInfo prop = typeof(ApplicationBootstrap).GetProperty(
                "Host",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null)
            {
                return false;
            }

            host = prop.GetValue(bootstrap) as ApplicationHost;
            return host != null;
        }

        private static IReadOnlyList<GearConfig> GetValidCatalogGears(GearCatalogSO catalog)
        {
            var list = new List<GearConfig>();
            foreach (GearConfig g in catalog.All)
            {
                if (g != null && !string.IsNullOrEmpty(g.Id))
                {
                    list.Add(g);
                }
            }

            return list;
        }
    }
}
