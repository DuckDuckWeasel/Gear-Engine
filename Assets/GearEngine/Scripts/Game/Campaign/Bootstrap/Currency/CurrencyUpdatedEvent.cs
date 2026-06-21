using Scaffold.Events.Contracts;

namespace GearEngine.Currency.Events
{
    public record CurrencyUpdatedEvent : ContextEvent
    {
        public string CurrencyId { get; }
        public long OldAmount { get; }
        public long NewAmount { get; }

        public CurrencyUpdatedEvent(string currencyId, long oldAmount, long newAmount)
        {
            CurrencyId = currencyId;
            OldAmount = oldAmount;
            NewAmount = newAmount;
        }
    }
}
