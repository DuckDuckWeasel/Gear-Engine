using System;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.Analytics;
using Scaffold.AppFlow;
using UnityEngine;
using ScaffoldUgs = Scaffold.Ugs.Ugs;

namespace GearEngine.App.Bootstrap.Layers
{
    internal sealed class UgsAnalyticsInitializer : IAsyncInitializable
    {
        private readonly AnalyticsService analyticsService;
        private readonly OfflineSessionState offlineSessionState;
        private readonly ScaffoldUgs ugs;

        public UgsAnalyticsInitializer(
            ScaffoldUgs ugs,
            AnalyticsService analyticsService,
            OfflineSessionState offlineSessionState)
        {
            this.ugs = ugs ?? throw new ArgumentNullException(nameof(ugs));
            this.analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            this.offlineSessionState = offlineSessionState ?? throw new ArgumentNullException(nameof(offlineSessionState));
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    offlineSessionState.MarkOffline();
                    return;
                }

                await ugs.InitializeAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                analyticsService.Initialize();
                offlineSessionState.MarkOnline();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[UgsAnalyticsInitializer] Failed to initialize analytics after Unity Services. {exception}");
                offlineSessionState.MarkOffline();
            }
        }
    }
}
