using System;
using System.Collections.Generic;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.ModuleRequests;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Offline
{
    /// <summary>
    /// Routes <see cref="ModuleRequest"/>s to in-memory handlers that mutate the cached
    /// <see cref="LiveOps.DTO.GameModule.IGameModuleData"/> instances and synthesize a response.
    /// Unknown requests fall through to a default response so the game can keep running.
    /// </summary>
    internal sealed class OfflineRequestRouter
    {
        private readonly OfflineLiveOpsService service;
        private readonly Dictionary<Type, Func<ModuleRequest, ModuleResponse>> handlers;

        public OfflineRequestRouter(OfflineLiveOpsService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            handlers = new Dictionary<Type, Func<ModuleRequest, ModuleResponse>>
            {
                [typeof(AddCurrencyRequest)] = r => HandleAddCurrency((AddCurrencyRequest)r),
                [typeof(SpendCurrencyRequest)] = r => HandleSpendCurrency((SpendCurrencyRequest)r),
                [typeof(SetInventoryRequest)] = r => HandleSetInventory((SetInventoryRequest)r),
            };
        }

        public ModuleResponse Route(ModuleRequest request)
        {
            if (request != null && handlers.TryGetValue(request.GetType(), out Func<ModuleRequest, ModuleResponse> handler))
            {
                return handler(request);
            }

            // Unknown requests: the caller's default response is fine. Log so devs can spot which
            // requests would need a real handler if their feature needs the response shape.
            if (request != null)
            {
                Debug.Log($"[OfflineLiveOps] No handler for {request.GetType().Name}; returning default response.");
            }

            return null;
        }

        private AddCurrencyResponse HandleAddCurrency(AddCurrencyRequest request)
        {
            if (!service.TryGetModule(out CurrencyGameData currency))
            {
                return new AddCurrencyResponse(request.CurrencyId, 0, 0);
            }

            CurrencyWallet wallet = currency.GetWallet(request.CurrencyId);
            if (wallet == null || request.Amount <= 0)
            {
                return new AddCurrencyResponse(request.CurrencyId, wallet?.Current ?? 0, 0);
            }

            long previous = wallet.Current;
            long next = wallet.Max.HasValue ? Math.Min(previous + request.Amount, wallet.Max.Value) : previous + request.Amount;
            wallet.Current = next;
            return new AddCurrencyResponse(request.CurrencyId, next, next - previous);
        }

        private SpendCurrencyResponse HandleSpendCurrency(SpendCurrencyRequest request)
        {
            if (!service.TryGetModule(out CurrencyGameData currency))
            {
                return new SpendCurrencyResponse(request.CurrencyId, 0, 0, false);
            }

            CurrencyWallet wallet = currency.GetWallet(request.CurrencyId);
            if (wallet == null || !wallet.CanSpend(request.Amount))
            {
                return new SpendCurrencyResponse(request.CurrencyId, wallet?.Current ?? 0, 0, false);
            }

            wallet.Current -= request.Amount;
            return new SpendCurrencyResponse(request.CurrencyId, wallet.Current, request.Amount, true);
        }

        private SetInventoryResponse HandleSetInventory(SetInventoryRequest request)
        {
            List<OwnedGearEntry> gears = request.Gears != null
                ? new List<OwnedGearEntry>(request.Gears)
                : new List<OwnedGearEntry>();

            if (service.TryGetModule(out InventoryGameData inventory))
            {
                inventory.Gears = new List<OwnedGearEntry>(gears);
            }

            return new SetInventoryResponse { Gears = gears };
        }
    }
}
