using Scaffold.Analytics;

namespace GearEngine.Campaign.Analytics
{
    public class CurrencyEarnedEvent : AnalyticsEvent
    {
        public CurrencyEarnedEvent(string currencyId, long amount) : base("currency_earned")
        {
            SetParameter("currency_id", currencyId);
            SetParameter("amount", amount);
        }
    }
}
