using System.Collections.Generic;
using System.Reflection;
using GearEngine.Cards.Powerups;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.Cards.Tests.Editor
{
    public sealed class CarPowerupBuildResolverTests
    {
        [Test]
        public void Resolve_SortsByPhaseThenAppliesMultipliers()
        {
            var catalog = ScriptableObject.CreateInstance<CardCatalogSO>();
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            var early = ScriptableObject.CreateInstance<MaxSpeedMultiplierModifierSO>();
            var late = ScriptableObject.CreateInstance<MaxSpeedMultiplierModifierSO>();

            SetField(early, "phase", CarPowerupApplyPhase.Post);
            SetField(early, "multiplier", 2f);
            SetField(late, "phase", CarPowerupApplyPhase.Base);
            SetField(late, "multiplier", 3f);

            SetField(card, "id", "c1");
            SetField(card, "modifiers", new List<CarPowerupModifierSO> { early, late });
            SetField(catalog, "cards", new List<CardDefinition> { card });

            try
            {
                var resolver = new CarPowerupBuildResolver(catalog);
                CarPowerupBuildContext ctx = resolver.Resolve(new[] { "c1" });
                CarPowerupStats stats = ctx.Evaluate();

                Assert.That(stats.MaxSpeedMultiplier, Is.EqualTo(6f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(early);
                Object.DestroyImmediate(late);
                Object.DestroyImmediate(card);
                Object.DestroyImmediate(catalog);
            }
        }

        private static void SetField<T>(object target, string name, T value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
