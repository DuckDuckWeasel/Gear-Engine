using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.Perks;
using GearEngine.Campaign.Presentation;
using LiveOps.Modules.DTO.Perks;
using LiveOps.Modules.DTO.ModuleRequests;
using PurchasePerkResponse = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkResponse;
using BurnPerkResponse = LiveOps.Modules.DTO.ModuleRequests.BurnPerkResponse;
using NUnit.Framework;
using Scaffold.LiveOps;
using GearEngine.Perks.Config;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class TalentPerksViewModelTests
    {
        // -----------------------------------------------------------------------
        // TalentPerksViewModel tests
        // -----------------------------------------------------------------------

        private global::GearEngine.Perks.Config.PerkItem CreateFakePerkConfig(string id)
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<global::GearEngine.Perks.Config.PerkItem>();
            var data = new global::GearEngine.Perks.Config.PerkItemData { Id = id };
            ViewModelTestInject.InjectPrivateField(config, "data", data);
            return config;
        }

        private global::GearEngine.Perks.Config.PerkCatalogSO CreateFakeCatalog()
        {
            var catalog = UnityEngine.ScriptableObject.CreateInstance<global::GearEngine.Perks.Config.PerkCatalogSO>();
            
            var grip = CreateFakePerkConfig("grip");
            var turbo = CreateFakePerkConfig("turbo");
            var nitro = CreateFakePerkConfig("nitro");

            catalog.SetRuntimeEntries(new[] { grip, turbo, nitro });
            return catalog;
        }

        [Test]
        public void Initialize_BuildsItemListFromOwnedPerks()
        {
            var fakePerks = new FakePerksClientModule(new[] { "grip", "grip", "turbo" });
            var catalog = CreateFakeCatalog();

            var vm = new TalentPerksViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "perksClient", fakePerks);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", null);
            ViewModelTestInject.InjectPrivateField(vm, "perkCatalog", catalog);
            ViewModelTestInject.InvokeInitialize(vm);

            Assert.That(vm.Items.Count, Is.EqualTo(3)); // No longer groups by count
        }

        [Test]
        public void BuyRandom_AddsNewItemWhenPerkIsNew()
        {
            var fakePerks = new FakePerksClientModule(new string[0])
            {
                PurchaseResponse = new PurchasePerkResponse { Success = true, UnlockedPerkId = "nitro" }
            };
            var catalog = CreateFakeCatalog();

            var vm = new TalentPerksViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "perksClient", fakePerks);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", null);
            ViewModelTestInject.InjectPrivateField(vm, "perkCatalog", catalog);
            ViewModelTestInject.InvokeInitialize(vm);

            vm.BuyRandom();

            Assert.That(vm.Items.Count, Is.EqualTo(1));
            Assert.That(vm.Items[0].Item.Id, Is.EqualTo("nitro"));
        }

        [Test]
        public void BuyRandom_DoesNotModifyListOnFailedPurchase()
        {
            var fakePerks = new FakePerksClientModule(new string[0])
            {
                PurchaseResponse = new PurchasePerkResponse { Success = false }
            };
            var catalog = CreateFakeCatalog();

            var vm = new TalentPerksViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "perksClient", fakePerks);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", null);
            ViewModelTestInject.InjectPrivateField(vm, "perkCatalog", catalog);
            ViewModelTestInject.InvokeInitialize(vm);

            vm.BuyRandom();

            Assert.That(vm.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task BurnPerk_CallsBurnOnClientAndUpdatesRevision()
        {
            var fakePerks = new FakePerksClientModule(new[] { "grip", "grip" })
            {
                BurnResponse = new BurnPerkResponse { Success = true, GoldEarned = 50 }
            };
            var catalog = CreateFakeCatalog();

            var vm = new TalentPerksViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "perksClient", fakePerks);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", null);
            ViewModelTestInject.InjectPrivateField(vm, "perkCatalog", catalog);
            ViewModelTestInject.InvokeInitialize(vm);

            int initialRevision = vm.ItemsRevision;
            await vm.BurnPerk("grip");

            Assert.That(vm.ItemsRevision, Is.GreaterThan(initialRevision));
        }

        [Test]
        public async Task BurnPerk_DoesNotUpdateRevisionOnFailure()
        {
            var fakePerks = new FakePerksClientModule(new[] { "grip" })
            {
                BurnResponse = new BurnPerkResponse { Success = false }
            };
            var catalog = CreateFakeCatalog();

            var vm = new TalentPerksViewModel();
            ViewModelTestInject.InjectPrivateField(vm, "perksClient", fakePerks);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", null);
            ViewModelTestInject.InjectPrivateField(vm, "perkCatalog", catalog);
            ViewModelTestInject.InvokeInitialize(vm);

            int initialRevision = vm.ItemsRevision;
            await vm.BurnPerk("grip");

            Assert.That(vm.ItemsRevision, Is.EqualTo(initialRevision));
        }

        // -----------------------------------------------------------------------
        // Fake client module
        // -----------------------------------------------------------------------

        private sealed class FakePerksClientModule : IPerksClientModule
        {
            private readonly string[] preloadedOwned;

            public PurchasePerkResponse PurchaseResponse { get; set; }
                = new PurchasePerkResponse { Success = false };

            public BurnPerkResponse BurnResponse { get; set; }
                = new BurnPerkResponse { Success = false };

            public FakePerksClientModule(string[] preloaded)
            {
                preloadedOwned = preloaded;
            }

            public IReadOnlyList<string> Unlocked => preloadedOwned;
            public long NextCost => 0;
            public long BurnReward => 0;
            public string CurrencyId => "gold";

            public Task InitializeAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<PurchasePerkResponse> PurchaseAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(PurchaseResponse);
            }

            public Task<BurnPerkResponse> BurnAsync(string perkId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(BurnResponse);
            }
        }
    }
}
