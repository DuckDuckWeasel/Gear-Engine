using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.GearEngine.Config;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class InventoryClientModuleTests
    {
        [Test]
        public void TryAdd_AppendsOwned_AndSendsSetInventoryRequest()
        {
            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearCatalogSO catalog = null;
            try
            {
                catalog = ScriptableObject.CreateInstance<GearCatalogSO>();
                catalog.SetRuntimeEntries(new[] { g1 });

                var moduleData = new InventoryGameData(new InventoryPersistence(), new InventoryConfig());
                var fake = new FakeLiveOpsService { ModuleData = moduleData };

                var builder = new ContainerBuilder();
                builder.RegisterInstance<ILiveOpsService>(fake);
                builder.RegisterInstance(catalog);
                builder.Register<InventoryClientModule>(Lifetime.Singleton);

                IObjectResolver container = builder.Build();
                try
                {
                    InventoryClientModule module = container.Resolve<InventoryClientModule>();
                    module.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                    Assert.That(module.TryAdd(g1), Is.True);
                    Assert.That(module.Owned.Count, Is.EqualTo(1));
                    Assert.That(fake.SetInventoryCalls.Count, Is.EqualTo(1));
                    Assert.That(fake.SetInventoryCalls[0], Does.Contain("g1"));
                }
                finally
                {
                    (container as IDisposable)?.Dispose();
                }
            }
            finally
            {
                if (catalog != null)
                {
                    UnityEngine.Object.DestroyImmediate(catalog);
                }

                CampaignTestUtilities.DestroyGearConfig(g1);
            }
        }

        [Test]
        public void TryRemove_FirstMatchingId_RemovesOwned()
        {
            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearCatalogSO catalog = null;
            try
            {
                catalog = ScriptableObject.CreateInstance<GearCatalogSO>();
                catalog.SetRuntimeEntries(new[] { g1 });

                var persistence = new InventoryPersistence();
                persistence.GearIds.Add("g1");
                persistence.GearIds.Add("g1");
                var moduleData = new InventoryGameData(persistence, new InventoryConfig());
                var fake = new FakeLiveOpsService { ModuleData = moduleData };

                var builder = new ContainerBuilder();
                builder.RegisterInstance<ILiveOpsService>(fake);
                builder.RegisterInstance(catalog);
                builder.Register<InventoryClientModule>(Lifetime.Singleton);

                IObjectResolver container = builder.Build();
                try
                {
                    InventoryClientModule module = container.Resolve<InventoryClientModule>();
                    module.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                    Assert.That(module.Owned.Count, Is.EqualTo(2));
                    Assert.That(module.TryRemove(g1), Is.True);
                    Assert.That(module.Owned.Count, Is.EqualTo(1));
                }
                finally
                {
                    (container as IDisposable)?.Dispose();
                }
            }
            finally
            {
                if (catalog != null)
                {
                    UnityEngine.Object.DestroyImmediate(catalog);
                }

                CampaignTestUtilities.DestroyGearConfig(g1);
            }
        }

        [Test]
        public void Clear_RemovesAllOwned_AndSendsEmptySetInventoryRequest()
        {
            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearConfig g2 = CampaignTestUtilities.CreateGearConfigWithData("g2");
            GearCatalogSO catalog = null;
            try
            {
                catalog = ScriptableObject.CreateInstance<GearCatalogSO>();
                catalog.SetRuntimeEntries(new[] { g1, g2 });

                var moduleData = new InventoryGameData(new InventoryPersistence(), new InventoryConfig());
                var fake = new FakeLiveOpsService { ModuleData = moduleData };

                var builder = new ContainerBuilder();
                builder.RegisterInstance<ILiveOpsService>(fake);
                builder.RegisterInstance(catalog);
                builder.Register<InventoryClientModule>(Lifetime.Singleton);

                IObjectResolver container = builder.Build();
                try
                {
                    InventoryClientModule module = container.Resolve<InventoryClientModule>();
                    module.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                    Assert.That(module.TryAdd(g1), Is.True);
                    Assert.That(module.TryAdd(g2), Is.True);
                    Assert.That(module.Owned.Count, Is.EqualTo(2));

                    int callsBeforeClear = fake.SetInventoryCalls.Count;
                    int events = 0;
                    module.InventoryChanged += () => events++;

                    module.Clear();

                    Assert.That(module.Owned.Count, Is.EqualTo(0));
                    Assert.That(events, Is.EqualTo(1));
                    Assert.That(fake.SetInventoryCalls.Count, Is.EqualTo(callsBeforeClear + 1));
                    Assert.That(fake.SetInventoryCalls[^1].Count, Is.EqualTo(0));
                }
                finally
                {
                    (container as IDisposable)?.Dispose();
                }
            }
            finally
            {
                if (catalog != null)
                {
                    UnityEngine.Object.DestroyImmediate(catalog);
                }

                CampaignTestUtilities.DestroyGearConfig(g1);
                CampaignTestUtilities.DestroyGearConfig(g2);
            }
        }

        [Test]
        public void Clear_WhenAlreadyEmpty_StillSendsRequest_AndRaisesEvent()
        {
            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearCatalogSO catalog = null;
            try
            {
                catalog = ScriptableObject.CreateInstance<GearCatalogSO>();
                catalog.SetRuntimeEntries(new[] { g1 });

                var moduleData = new InventoryGameData(new InventoryPersistence(), new InventoryConfig());
                var fake = new FakeLiveOpsService { ModuleData = moduleData };

                var builder = new ContainerBuilder();
                builder.RegisterInstance<ILiveOpsService>(fake);
                builder.RegisterInstance(catalog);
                builder.Register<InventoryClientModule>(Lifetime.Singleton);

                IObjectResolver container = builder.Build();
                try
                {
                    InventoryClientModule module = container.Resolve<InventoryClientModule>();
                    module.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                    Assert.That(module.Owned.Count, Is.EqualTo(0));

                    int events = 0;
                    module.InventoryChanged += () => events++;
                    int callsBefore = fake.SetInventoryCalls.Count;

                    module.Clear();

                    Assert.That(events, Is.EqualTo(1));
                    Assert.That(fake.SetInventoryCalls.Count, Is.EqualTo(callsBefore + 1));
                    Assert.That(fake.SetInventoryCalls[^1].Count, Is.EqualTo(0));
                }
                finally
                {
                    (container as IDisposable)?.Dispose();
                }
            }
            finally
            {
                if (catalog != null)
                {
                    UnityEngine.Object.DestroyImmediate(catalog);
                }

                CampaignTestUtilities.DestroyGearConfig(g1);
            }
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public InventoryGameData ModuleData { get; set; }

            public List<List<string>> SetInventoryCalls { get; } = new List<List<string>>();

            public T GetModuleData<T>()
                where T : class, IGameModuleData
            {
                if (typeof(T) == typeof(InventoryGameData))
                {
                    return ModuleData as T;
                }

                return null;
            }

            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                if (request is SetInventoryRequest set)
                {
                    SetInventoryCalls.Add(new List<string>(set.GearIds));
                    return Task.FromResult((TResponse)(object)new SetInventoryResponse { GearIds = set.GearIds });
                }

                throw new InvalidOperationException($"Unhandled request {request?.GetType().Name}");
            }
        }
    }
}
