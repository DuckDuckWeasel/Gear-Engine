using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Roguelike;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Bootstrap.LiveOps;
using NUnit.Framework;
using Scaffold.LiveOps;
using VContainer;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class RoguelikeClientModuleTests
    {
        [Test]
        public void EnsureCurrentRollAsync_IsIdempotent_AfterFirstDraw()
        {
            var fake = new FakeLiveOpsService { GameData = BuildEmpty() };
            IObjectResolver container = BuildContainer(fake);
            try
            {
                RoguelikeClientModule module = container.Resolve<RoguelikeClientModule>();
                module.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                IReadOnlyList<string> first = module.EnsureCurrentRollAsync().GetAwaiter().GetResult();
                IReadOnlyList<string> second = module.EnsureCurrentRollAsync().GetAwaiter().GetResult();

                Assert.That(first, Is.EquivalentTo(new[] { "g1", "g2", "g3" }));
                Assert.That(second, Is.EquivalentTo(first));
                Assert.That(fake.DrawCalls, Is.EqualTo(1));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        [Test]
        public void ClaimAsync_ClearsCurrentRoll_OnSuccess()
        {
            var fake = new FakeLiveOpsService { GameData = BuildWithRoll("g1", "g2") };
            IObjectResolver container = BuildContainer(fake);
            try
            {
                RoguelikeClientModule module = container.Resolve<RoguelikeClientModule>();
                module.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                bool ok = module.ClaimAsync("g1").GetAwaiter().GetResult();

                Assert.That(ok, Is.True);
                Assert.That(fake.LastClaimedId, Is.EqualTo("g1"));
                Assert.That(module.CurrentRollIds, Is.Not.Null);
                Assert.That(module.CurrentRollIds.Count, Is.EqualTo(0));
            }
            finally
            {
                (container as IDisposable)?.Dispose();
            }
        }

        private static RoguelikeGameData BuildEmpty()
        {
            return new RoguelikeGameData(new RoguelikePersistence(), new RoguelikeConfig { OptionsPerRoll = 3 });
        }

        private static RoguelikeGameData BuildWithRoll(params string[] ids)
        {
            var persistence = new RoguelikePersistence();
            persistence.CurrentRollIds.AddRange(ids);
            return new RoguelikeGameData(persistence, new RoguelikeConfig { OptionsPerRoll = 3 });
        }

        private static IObjectResolver BuildContainer(FakeLiveOpsService fake)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(fake);
            builder.Register<RoguelikeClientModule>(Lifetime.Singleton);
            return builder.Build();
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public RoguelikeGameData GameData { get; set; }

            public int DrawCalls { get; private set; }

            public string LastClaimedId { get; private set; }

            public T GetModuleData<T>()
                where T : class, IGameModuleData
            {
                if (typeof(T) == typeof(RoguelikeGameData))
                {
                    return GameData as T;
                }

                return null;
            }

            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                if (request is DrawRoguelikeRollRequest)
                {
                    DrawCalls++;
                    return Task.FromResult(
                        (TResponse)(object)new DrawRoguelikeRollResponse
                        {
                            CurrentRollIds = new List<string> { "g1", "g2", "g3" },
                        });
                }

                if (request is ClaimRoguelikePickRequest claim)
                {
                    LastClaimedId = claim.PickedGearId;
                    return Task.FromResult((TResponse)(object)new ClaimRoguelikePickResponse { Success = true });
                }

                throw new InvalidOperationException($"Unhandled request {request?.GetType().Name}");
            }
        }
    }
}
