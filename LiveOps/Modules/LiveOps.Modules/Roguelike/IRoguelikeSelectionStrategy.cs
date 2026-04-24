using System.Collections.Generic;

namespace LiveOps.Modules.Roguelike
{
    public interface IRoguelikeSelectionStrategy
    {
        IReadOnlyList<string> Select(IReadOnlyList<string> pool, int count);
    }
}
