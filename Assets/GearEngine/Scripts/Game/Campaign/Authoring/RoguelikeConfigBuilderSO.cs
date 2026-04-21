using System.Collections.Generic;
using GameModuleDTO.Modules.Roguelike;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine.Config;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Roguelike Config Builder", fileName = "RoguelikeConfigBuilder")]
    public sealed class RoguelikeConfigBuilderSO : ConfigBuilderSO<RoguelikeConfig>
    {
        [Header("Asset source")]
        [SerializeField]
        private RoguelikeGearPoolSO roguelikeGearPool;

        [Header("Asset-independent fields")]
        [SerializeField]
        private int optionsPerRoll = 3;

        public override string ConfigKey => nameof(RoguelikeConfig);

        public override RoguelikeConfig Build()
        {
            var cfg = new RoguelikeConfig
            {
                OptionsPerRoll = optionsPerRoll,
            };

            if (roguelikeGearPool == null)
            {
                return cfg;
            }

            IReadOnlyList<GearConfig> pool = roguelikeGearPool.GetRoguelikeGearOptions();
            for (int i = 0; i < pool.Count; i++)
            {
                GearConfig g = pool[i];
                if (g != null && !string.IsNullOrEmpty(g.Id))
                {
                    cfg.GearPool.Add(g.Id);
                }
            }

            return cfg;
        }

        public override void Apply(RoguelikeConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            optionsPerRoll = pulled.OptionsPerRoll;
        }
    }
}
