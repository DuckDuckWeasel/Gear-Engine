using System;
using System.Collections.Generic;
using UnityEngine;

using GearEngine.Core.Config;

namespace GearEngine.Perks.Config
{
    [CreateAssetMenu(fileName = "PerkCatalog", menuName = "GearEngine/Perks/Perk Catalog")]
    public sealed class PerkCatalogSO : BaseCatalogSO<PerkItem>, IPerkDefinitionProvider
    {
        protected override string GetId(PerkItem item)
        {
            return item?.Id;
        }
    }
}
