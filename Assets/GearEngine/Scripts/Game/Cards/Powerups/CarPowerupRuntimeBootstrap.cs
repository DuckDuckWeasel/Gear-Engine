using System.Collections.Generic;
using GearEngine.Cards;

namespace GearEngine.Cards.Powerups
{
    public static class CarPowerupRuntimeBootstrap
    {
        public static CarPowerupBuildResult Resolve(ICardDefinitionProvider catalog, IEnumerable<string> collectedCardIds)
        {
            if (catalog == null)
            {
                return CarPowerupBuildResult.Neutral;
            }

            var resolver = new CarPowerupBuildResolver(catalog);
            CarPowerupBuildContext ctx = resolver.Resolve(collectedCardIds ?? System.Array.Empty<string>());
            return CarPowerupBuildResult.FromStats(ctx.Evaluate());
        }
    }
}
