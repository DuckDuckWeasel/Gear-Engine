namespace GearEngine.Cards
{
    public interface ICardDefinitionProvider
    {
        bool TryGet(string cardId, out CardDefinition definition);
    }
}
