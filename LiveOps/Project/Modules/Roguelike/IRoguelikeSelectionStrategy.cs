using System.Collections.Generic;

namespace GameModule.Modules.Roguelike
{
    public interface IRoguelikeSelectionStrategy
    {
        IReadOnlyList<string> Select(IReadOnlyList<string> pool, int count);
    }
}
