using System.ComponentModel;

namespace GearEngine.Campaign.Presentation
{
    internal static class PostRaceViewBindings
    {
        public static void Detach(INotifyPropertyChanged model, PropertyChangedEventHandler handler)
        {
            if (model != null)
            {
                model.PropertyChanged -= handler;
            }
        }
    }
}
