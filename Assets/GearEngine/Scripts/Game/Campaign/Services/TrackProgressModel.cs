using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Services
{
    public partial class TrackProgressModel : Model
    {
        [ObservableProperty]
        private int currentTrackIndex;
    }
}
