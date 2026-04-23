using System;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Services.Board;
using Scaffold.AppFlow;

namespace GearEngine.Campaign.Bootstrap
{
    /// <summary>
    /// Subscribes to local board layout changes in the Campaign layer and persists to LiveOps loadout.
    /// </summary>
    public sealed class CampaignGearPersistenceHookup : IAsyncInitializable, IDisposable
    {
        private readonly IBoardService board;
        private readonly LoadoutClientModule loadout;

        public CampaignGearPersistenceHookup(IBoardService board, LoadoutClientModule loadout)
        {
            this.board = board;
            this.loadout = loadout;
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
            loadout.PersistBoardLayout(BoardLayoutData.FromNodes(board.GetAllNodes()));
        }
    }
}
