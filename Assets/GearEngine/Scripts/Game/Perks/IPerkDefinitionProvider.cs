using GearEngine.Perks.Config;

namespace GearEngine.Perks
{
    public interface IPerkDefinitionProvider
    {
        bool TryGet(string perkId, out PerkItem definition);
    }
}
