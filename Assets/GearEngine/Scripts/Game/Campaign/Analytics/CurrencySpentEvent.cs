using Scaffold.Analytics;

namespace GearEngine.Campaign.Analytics
{
    public class CurrencySpentEvent : AnalyticsEvent
    {
        public CurrencySpentEvent(string currencyId, long amount) : base("currency_spent")
        {
            SetParameter("currency_id", currencyId);
            SetParameter("amount", amount);
        }
    }
}
