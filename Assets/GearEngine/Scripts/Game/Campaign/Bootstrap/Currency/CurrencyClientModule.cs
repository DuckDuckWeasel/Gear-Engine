using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;
using Scaffold.Analytics;
using GearEngine.Campaign.Analytics;

namespace GearEngine.Currency
{
    public sealed class CurrencyClientModule : GameClientModuleBase<CurrencyGameData>
    {
        [Inject] private IAnalyticsService analyticsService;
        
        public CurrencyClientModule(ILiveOpsService liveOps) : base(liveOps)
        {
        }

        public IReadOnlyList<CurrencyWallet> Wallets => data?.Wallets;

        public CurrencyWallet GetWallet(string currencyId)
        {
            if (data == null || string.IsNullOrEmpty(currencyId))
            {
                return null;
            }

            return data.GetWallet(currencyId);
        }

        public async Task<AddCurrencyResponse> AddAsync(string currencyId, long amount, CancellationToken ct = default)
        {
            try
            {
                ValidateCurrencyOperation(currencyId, amount);
                return await AddCurrencyCoreAsync(currencyId, amount, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Debug.LogError($"[CurrencyClientModule] AddAsync({currencyId},{amount}) failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> TrySpendAsync(string currencyId, long amount, CancellationToken ct = default)
        {
            SpendCurrencyResponse response = await SpendAsync(currencyId, amount, ct);
            return response != null && response.Succeeded;
        }

        public async Task<SpendCurrencyResponse> SpendAsync(string currencyId, long amount, CancellationToken ct = default)
        {
            try
            {
                ValidateCurrencyOperation(currencyId, amount);
                return await SpendCurrencyCoreAsync(currencyId, amount, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Debug.LogError($"[CurrencyClientModule] SpendAsync({currencyId},{amount}) failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private async Task<AddCurrencyResponse> AddCurrencyCoreAsync(string currencyId, long amount, CancellationToken ct)
        {
            AddCurrencyResponse response = await liveOps.CallAsync(new AddCurrencyRequest(currencyId, amount), ct);
            RecordAnalytics(currencyId, response?.NewAmount);
            ApplyServerSnapshot(currencyId, response?.NewAmount);
            return response;
        }

        private async Task<SpendCurrencyResponse> SpendCurrencyCoreAsync(string currencyId, long amount, CancellationToken ct)
        {
            SpendCurrencyResponse response = await liveOps.CallAsync(new SpendCurrencyRequest(currencyId, amount), ct);
            RecordAnalytics(currencyId, response?.NewAmount);
            ApplyServerSnapshot(currencyId, response?.NewAmount);
            return response;
        }

        private void RecordAnalytics(string currencyId, long? newAmount)
        {
            if (!newAmount.HasValue) return;

            long previousAmount = GetWallet(currencyId)?.Current ?? 0;
            if (newAmount > previousAmount)
            {
                analyticsService?.Record(new CurrencyEarnedEvent(currencyId, newAmount.Value - previousAmount));
            }
            else if (newAmount < previousAmount)
            {
                analyticsService?.Record(new CurrencySpentEvent(currencyId, previousAmount - newAmount.Value));
            }
        }

        public void ApplyNestedAddCurrency(AddCurrencyResponse response)
        {
            if (response == null)
            {
                return;
            }

            ApplyServerSnapshot(response.CurrencyId, response.NewAmount);
        }

        public void SyncWalletBalance(string currencyId, long newAmount)
        {
            ApplyServerSnapshot(currencyId, newAmount);
        }

        private void ApplyServerSnapshot(string currencyId, long? newAmount)
        {
            if (data == null || !newAmount.HasValue)
            {
                return;
            }

            CurrencyWallet wallet = data.GetWallet(currencyId);
            if (wallet == null)
            {
                return;
            }

            wallet.Current = newAmount.Value;
        }

        private void ValidateCurrencyOperation(string currencyId, long amount)
        {
            if (string.IsNullOrEmpty(currencyId))
            {
                throw new ArgumentException("currencyId required", nameof(currencyId));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
        }
    }
}
