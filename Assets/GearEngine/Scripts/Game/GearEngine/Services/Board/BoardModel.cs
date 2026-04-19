using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using Scaffold.MVVM;

namespace GearEngine.GearEngine.Services.Board
{
    public partial class BoardModel : Model
    {
        public BoardRulesSO BoardRules { get; init; }

        [ObservableProperty]
        private bool isSimulationRunning;

        public ObservableCollection<IGridNode> Nodes { get; } = new ObservableCollection<IGridNode>();
    }
}
