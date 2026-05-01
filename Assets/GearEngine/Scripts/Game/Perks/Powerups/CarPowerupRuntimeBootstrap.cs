using System.Collections.Generic;
using GearEngine.Perks;

namespace GearEngine.Perks.Powerups
{
    public static class CarPowerupRuntimeBootstrap
    {
        public static CarPowerupBuildResult Resolve(IPerkDefinitionProvider catalog, IEnumerable<string> collectedPerkIds)
        {
            if (catalog == null)
            {
                return CarPowerupBuildResult.Neutral;
            }

            var resolver = new CarPowerupBuildResolver(catalog);
            CarPowerupBuildContext ctx = resolver.Resolve(collectedPerkIds ?? System.Array.Empty<string>());
            return CarPowerupBuildResult.FromStats(ctx.Evaluate());
        }
    }
}
