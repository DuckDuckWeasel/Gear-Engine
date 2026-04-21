using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class InventoryClientModuleTests
    {
        [Test]
        public void Add_AppendsOwned_AndSendsSetInventoryRequest()
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

                    Assert.That(module.Add(g1), Is.Not.Null);
                    Assert.That(module.Owned.Count, Is.EqualTo(1));
                    Assert.That(fake.SetInventoryCalls.Count, Is.EqualTo(1));
                    Assert.That(fake.SetInventoryCalls[0].Count, Is.EqualTo(1));
                    Assert.That(fake.SetInventoryCalls[0][0].GearId, Is.EqualTo("g1"));
                    Assert.That(string.IsNullOrEmpty(fake.SetInventoryCalls[0][0].InstanceId), Is.False);
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
        public void Remove_ByReference_RemovesOwned()
        {
            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearCatalogSO catalog = null;
            try
            {
                catalog = ScriptableObject.CreateInstance<GearCatalogSO>();
                catalog.SetRuntimeEntries(new[] { g1 });

                var persistence = new InventoryPersistence();
                persistence.Gears.Add(new OwnedGearEntry { InstanceId = "a", GearId = "g1" });
                persistence.Gears.Add(new OwnedGearEntry { InstanceId = "b", GearId = "g1" });
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
                    OwnedGear first = module.Owned[0];
                    Assert.That(module.Remove(first), Is.True);
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

                    Assert.That(module.Add(g1), Is.Not.Null);
                    Assert.That(module.Add(g2), Is.Not.Null);
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
        public void Clear_WhenAlreadyEmpty_DoesNotRaiseEvent()
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

                    Assert.That(events, Is.EqualTo(0));
                    Assert.That(fake.SetInventoryCalls.Count, Is.EqualTo(callsBefore));
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

            public List<List<OwnedGearEntry>> SetInventoryCalls { get; } = new List<List<OwnedGearEntry>>();

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
                    List<OwnedGearEntry> copy = set.Gears != null
                        ? set.Gears.Select(g => new OwnedGearEntry { InstanceId = g.InstanceId, GearId = g.GearId }).ToList()
                        : new List<OwnedGearEntry>();
                    SetInventoryCalls.Add(copy);
                    return Task.FromResult((TResponse)(object)new SetInventoryResponse { Gears = copy });
                }

                throw new InvalidOperationException($"Unhandled request {request?.GetType().Name}");
            }
        }
    }
}
