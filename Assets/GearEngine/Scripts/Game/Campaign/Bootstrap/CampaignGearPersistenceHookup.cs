using System;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Services.Board;
using Scaffold.AppFlow;
using Scaffold.Analytics;
using GearEngine.Campaign.Analytics;

namespace GearEngine.Campaign.Bootstrap
{
    /// <summary>
    /// Subscribes to local board layout changes in the Campaign layer and persists to LiveOps loadout.
    /// </summary>
    public sealed class CampaignGearPersistenceHookup : IAsyncInitializable, IDisposable
    {
        private readonly IBoardService board;
        private readonly LoadoutClientModule loadout;
        private readonly IAnalyticsService analytics;

        public CampaignGearPersistenceHookup(IBoardService board, LoadoutClientModule loadout, IAnalyticsService analytics)
        {
            this.board = board;
            this.loadout = loadout;
            this.analytics = analytics;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            board.BoardLayoutChanged += OnBoardLayoutChanged;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            board.BoardLayoutChanged -= OnBoardLayoutChanged;
        }

        private void OnBoardLayoutChanged()
        {
            var data = BoardLayoutData.FromNodes(board.GetAllNodes());
            loadout.PersistBoardLayout(data);
            analytics?.Record(new LoadoutUpdatedEvent(data.Placements.Count));
        }
    }
}
