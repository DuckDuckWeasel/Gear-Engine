using System;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultStatSlotViewModel : ViewModel
    {
        public ResultStatSlotViewModel(string label, string value)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Label { get; }

        public string Value { get; }
    }
}
