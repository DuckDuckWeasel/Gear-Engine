# Implementing a News feature (snippet guide)

This document is **documentation only**: illustrative snippets, not shipped code.

**Normative reference (state and services):** [`Docs/Standards/State-and-Services-Standard.md`](Standards/State-and-Services-Standard.md) — two roles (Model, Service), three tiers, `ILiveOpsService.CallAsync` / `BeginBatch`, delta-first wire requests, no repository layer, **bind for state** (Rule 6), EventBus only when a listener cannot infer the signal from any model.

**Bind API:** List rows should be **`Scaffold.MVVM.Model`** instances (same base as `BoardModel` and other game models) so UI binding targets the same **`Model` / `ViewModel`** types the Bind API expects — see [`Docs/Infra/MVVM.md`](../Infra/MVVM.md).

**Normative reference (LiveOps API):** [`Docs/LiveOps/NewApiAndServices.md`](../LiveOps/NewApiAndServices.md) — shared DTOs, `[UsesGameApi]`, `IGameApiHandler`, `GameModule<T>` snapshots, and **`GameClientModuleBase<TGameData>`** for client bootstrap + commands (see [`CurrencyClientModule.cs`](../../Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/Currency/CurrencyClientModule.cs), [`InventoryClientModule.cs`](../../Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/LiveOps/InventoryClientModule.cs)).

**Normative reference (config authoring):** [`Docs/LiveOps/AuthoringPipeline.md`](../LiveOps/AuthoringPipeline.md), [`Docs/LiveOps/RemoteConfig.md`](../LiveOps/RemoteConfig.md), [`Assets/Packages/com.scaffold.liveops.authoring/README.md`](../../Assets/Packages/com.scaffold.liveops.authoring/README.md).

---

## Problem shape

- **News item:** a **message** plus a visibility window **`[StartUtc, EndUtc]`**.
- **Unread:** at least one in-window item with **`IsRead == false`**.
- **Recent:** in-window items the UI can surface (for example sorted by `StartUtc` descending).

**Minimal player data:** persist only what you cannot derive from catalog + server clock + snapshot. A small set of **read news ids** in persistence is enough at low cardinality; at scale, prefer a single **acknowledged revision** (see end).

**Tier:** **Tier 2** (service-gated): “mark read” is an intent with rules; server is authoritative on whether the id is valid and in-window.

**Typed reference (client):** per Rule 3 in the standard, **`MarkReadAsync`** takes the live row type the UI already has (**`NewsItemModel`**), not a bare string. The **wire DTO** carries **`newsId`** only inside `CallAsync`.

**Read state on the row:** expose **`NewsItemModel.IsRead`** so lists and badges bind per row without calling back into the module for “is this id read?”. The **client module** (Tier 2 command surface) performs **`CallAsync`**, then updates the model on success (Tier 2: callers outside the feature assembly do not assign **`IsRead`** — **`internal`** setter).

**Client shape:** use **`NewsClientModule : GameClientModuleBase<NewsGameData>, INewsService`** — one type owns LiveOps bootstrap (**`OnInitializedAsync`**) and remote intents, matching existing campaign modules. Avoid a separate “apply game data” API on a thin service that only forwards into the model; that duplicates the bootstrap seam the base class already defines.

---

## UI commands vs binding (why not `RelayCommand`)

[`RelayCommand`](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators) is optional **CommunityToolkit.Mvvm** sugar. It is **not** part of the State and Services standard. The standard cares that:

- **State** flows through observable models (bind `IsRead`, `Active`, aggregates).
- **Intents** go through the client module (`MarkReadAsync` on **`INewsService`** / **`NewsClientModule`**).

A Unity **View** can use a plain click handler that `await`s the service, or a small ViewModel method without `[RelayCommand]`. Below we use a **normal async method** on the ViewModel so the sample stays free of generator attributes while still separating Tier 0 focus state from Tier 2 commands.

---

## 1. Wire DTOs (shared assembly)

Shapes both client and server serialize. Keep fields primitive and stable.

```csharp
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.News
{
    public sealed class NewsItemDto
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("message")] public string Message { get; set; } = string.Empty;
        [JsonProperty("startUtc")] public DateTime StartUtc { get; set; }
        [JsonProperty("endUtc")] public DateTime EndUtc { get; set; }
    }

    /// <summary>Cloud Save payload — keep small.</summary>
    public sealed class NewsPersistence
    {
        [JsonProperty("readNewsIds")] public List<string> ReadNewsIds { get; set; } = new();
    }
}
```

Bootstrap snapshot module (optional but typical for inbox-style UI):

```csharp
using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.News
{
    public sealed class NewsGameData : IGameModuleData
    {
        public string Key => nameof(NewsGameData);

        [JsonProperty("activeNews")] public List<NewsItemDto> ActiveNews { get; set; } = new();
        [JsonProperty("readNewsIds")] public List<string> ReadNewsIds { get; set; } = new();
    }
}
```

---

## 2. Catalog config DTO (Remote Config payload)

Authoring turns a **builder asset** into **`Assets/LiveOps/RemoteConfig/News.rc`** (see §8). The server loads this key the same way as other modules (`IRemoteConfig.Get(context, ConfigKey, …)`).

```csharp
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.News
{
    /// <summary>Remote Config key should match <c>nameof(NewsCatalogConfig)</c> in module + builder.</summary>
    public sealed class NewsCatalogConfig
    {
        [JsonProperty("entries")] public List<NewsItemDto> Entries { get; set; } = new();
    }
}
```

---

## 3. GameApi requests and responses

One **intent** per request (`MarkNewsReadRequest`). Do not add `SetNewsReadStateRequest(entireList)` as a default write; that is a blob and needs a waiver under Rule 5 in the standard.

```csharp
using GameModuleDTO.GameApi;
using GameModuleDTO.ModuleRequests;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.News
{
    public sealed class MarkNewsReadResponse : ModuleResponse
    {
        [JsonProperty("succeeded")] public bool Succeeded { get; set; }
    }
}

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class MarkNewsReadRequest : ModuleRequest<MarkNewsReadResponse>
    {
        public MarkNewsReadRequest() { }

        public MarkNewsReadRequest(string newsId) => NewsId = newsId;

        [JsonProperty("newsId")] public string NewsId { get; set; } = string.Empty;
    }
}
```

---

## 4. Client models (Tier 2 — observable, read-only to external callers)

- **`NewsItemModel`** (one row) and **`NewsModel`** (aggregate) both derive from **`Scaffold.MVVM.Model`** so list rows participate in the same **Bind API** surface as the rest of the game (for example `BoardModel` in `GearEngine`).
- **`IsRead`** lives on the row model with an **`internal`** setter that uses **`SetProperty`** from **`CommunityToolkit.Mvvm.ComponentModel.ObservableObject`** (the same notification path `[ObservableProperty]` uses). That keeps Tier 2 **externally read-only** from other assemblies (Rule 4) while still allowing **`NewsModel` + `NewsClientModule`** in the feature assembly to apply bootstrap hydration and successful commands.
- **No `IsRead` lookup on the service** — bind **`NewsItemModel.IsRead`** per row and **`NewsModel.HasUnread`** for badges.

```csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GameModuleDTO.Modules.News;
using Scaffold.MVVM;

namespace YourGame.News
{
    /// <summary>Live row in the client snapshot (typed ref for <see cref="INewsService.MarkReadAsync"/>).</summary>
    public sealed class NewsItemModel : Model
    {
        private bool isRead;

        public NewsItemModel(NewsItemDto dto)
        {
            Id = dto.Id;
            Message = dto.Message;
            StartUtc = dto.StartUtc;
            EndUtc = dto.EndUtc;
        }

        public string Id { get; }
        public string Message { get; }
        public DateTime StartUtc { get; }
        public DateTime EndUtc { get; }

        /// <summary>Bind one-way from UI; only same-assembly code assigns.</summary>
        public bool IsRead
        {
            get => isRead;
            internal set => SetProperty(ref isRead, value);
        }
    }

    public class NewsModel : Model
    {
        private readonly ObservableCollection<NewsItemModel> active = new();

        public ReadOnlyObservableCollection<NewsItemModel> Active { get; }

        internal ObservableCollection<NewsItemModel> WritableActive => active;

        public NewsModel() => Active = new ReadOnlyObservableCollection<NewsItemModel>(active);

        /// <summary>
        /// Single entry point for “server snapshot → observable rows”. Called only from
        /// <see cref="NewsClientModule.OnInitializedAsync"/> when LiveOps delivers <see cref="NewsGameData"/>.
        /// </summary>
        internal void HydrateFromBootstrapSnapshot(NewsGameData snapshot)
        {
            var readLookup = new HashSet<string>(StringComparer.Ordinal);
            if (snapshot?.ReadNewsIds != null)
                foreach (var id in snapshot.ReadNewsIds)
                    if (!string.IsNullOrEmpty(id)) readLookup.Add(id);

            active.Clear();
            if (snapshot?.ActiveNews == null) return;

            foreach (var dto in snapshot.ActiveNews)
            {
                if (dto == null || string.IsNullOrEmpty(dto.Id)) continue;
                var item = new NewsItemModel(dto);
                if (readLookup.Contains(item.Id)) item.IsRead = true;
                active.Add(item);
            }
        }

        /// <summary>Local projection after a successful <c>MarkNewsReadRequest</c>; only same-assembly module calls this.</summary>
        internal void MarkAsRead(NewsItemModel item)
        {
            if (item == null) return;
            for (int i = 0; i < active.Count; i++)
            {
                if (!ReferenceEquals(active[i], item)) continue;
                item.IsRead = true;
                return;
            }
        }

        public bool HasUnread(DateTime utcNow)
        {
            for (int i = 0; i < active.Count; i++)
            {
                NewsItemModel n = active[i];
                if (IsActive(n, utcNow) && !n.IsRead) return true;
            }
            return false;
        }

        public IReadOnlyList<NewsItemModel> RecentActive(DateTime utcNow, int max = 8)
        {
            var list = new List<NewsItemModel>();
            for (int i = 0; i < active.Count; i++)
            {
                NewsItemModel n = active[i];
                if (IsActive(n, utcNow)) list.Add(n);
            }
            list.Sort((a, b) => b.StartUtc.CompareTo(a.StartUtc));
            if (list.Count > max) list.RemoveRange(max, list.Count - max);
            return list;
        }

        private static bool IsActive(NewsItemModel n, DateTime utcNow)
            => n != null && utcNow >= n.StartUtc && utcNow <= n.EndUtc;
    }
}
```

---

## 5. Client module — `GameClientModuleBase<NewsGameData>` (bootstrap + commands)

Per [`Docs/LiveOps/NewApiAndServices.md`](../LiveOps/NewApiAndServices.md) §2.5, the **client** owns:

1. **`OnInitializedAsync(NewsGameData)`** — the standard’s **only** “blob in” path: map the server snapshot into **`NewsModel`** once after LiveOps init (same pattern as **`InventoryClientModule.OnInitializedAsync`** building **`ownedRefs`** from **`InventoryGameData`**).
2. **`CallAsync`** for intents — same module type implements **`INewsService`** so UI and other systems resolve one registration.

There is **no** separate `ApplyGameData` / `ReplaceFromGameData` pipeline: those names implied an extra public hop and duplicated responsibility. **`NewsModel.HydrateFromBootstrapSnapshot`** stays **`internal`** and is invoked **only** from **`NewsClientModule.OnInitializedAsync`**.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.News;
using Scaffold.LiveOps;
using VContainer;

namespace YourGame.News
{
    public interface INewsService
    {
        NewsModel News { get; }
        Task<bool> MarkReadAsync(NewsItemModel item, DateTime utcNow, CancellationToken ct = default);
    }

    public sealed class NewsClientModule : GameClientModuleBase<NewsGameData>, INewsService
    {
        private readonly ILiveOpsService liveOpsService;
        private readonly NewsModel newsModel = new NewsModel();

        public NewsClientModule(IObjectResolver resolver, ILiveOpsService liveOps) : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
        }

        public NewsModel News => newsModel;

        protected override Task OnInitializedAsync(NewsGameData moduleData)
        {
            newsModel.HydrateFromBootstrapSnapshot(moduleData);
            return base.OnInitializedAsync(moduleData);
        }

        public async Task<bool> MarkReadAsync(NewsItemModel item, DateTime utcNow, CancellationToken ct = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (utcNow < item.StartUtc || utcNow > item.EndUtc) return false;

            var resp = await liveOpsService.CallAsync(new MarkNewsReadRequest(item.Id), ct);
            if (!resp.Succeeded) return false;

            newsModel.MarkAsRead(item);
            return true;
        }
    }
}
```

Register **`NewsClientModule`** as **`INewsService`** (and **`IGameClientModule`** if your bootstrap discovers modules that way). Optional **`IEventBus`**: only if a listener cannot use **`HasUnread`** / **`IsRead`** binding (Rule 6).

---

## 6. Usage (ViewModel: Tier 0 focus + bind; `INewsService` for intents)

- **Tier 0:** `Focused` (which row is expanded) — public setter on the ViewModel.
- **Tier 2 state:** bind list cells to **`News.Active`** and **`NewsItemModel.IsRead`**; bind badge to **`News.HasUnread(DateTime.UtcNow)`** (refresh `PropertyChanged` for aggregates when a read completes, as below).

```csharp
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace YourGame.News.Ui
{
    public partial class NewsInboxViewModel : ViewModel, IDisposable
    {
        private readonly INewsService news;

        public NewsInboxViewModel(INewsService news)
        {
            this.news = news;
            news.News.Active.CollectionChanged += OnActiveCollectionChanged;
            WireItemPropertyChanged();
        }

        public NewsModel News => news.News;

        /// <summary>Tier 0 — local only.</summary>
        [ObservableProperty] private NewsItemModel? focused;

        public bool HasUnread => news.News.HasUnread(DateTime.UtcNow);

        /// <summary>Plain method — no RelayCommand required by the standard.</summary>
        public async Task AcknowledgeAsync(NewsItemModel? item, CancellationToken ct = default)
        {
            if (item == null) return;
            await news.MarkReadAsync(item, DateTime.UtcNow, ct);
            OnPropertyChanged(nameof(HasUnread));
        }

        private void OnActiveCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (NewsItemModel o in e.OldItems) o.PropertyChanged -= OnItemPropertyChanged;
            WireItemPropertyChanged();
        }

        private void WireItemPropertyChanged()
        {
            foreach (NewsItemModel it in news.News.Active)
                it.PropertyChanged -= OnItemPropertyChanged;
            foreach (NewsItemModel it in news.News.Active)
                it.PropertyChanged += OnItemPropertyChanged;
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NewsItemModel.IsRead)) OnPropertyChanged(nameof(HasUnread));
        }

        public void Dispose()
        {
            news.News.Active.CollectionChanged -= OnActiveCollectionChanged;
            foreach (NewsItemModel it in news.News.Active) it.PropertyChanged -= OnItemPropertyChanged;
        }
    }
}
```

**View (Unity):** `button.onClick.AddListener(() => _ = viewModel.AcknowledgeAsync(row.Item, destroyCancellationToken));` — or call **`INewsService.MarkReadAsync`** (implemented by **`NewsClientModule`**) directly from the View if you have no Tier 0 state to hold.

---

## 7. Backend — `GameModule` snapshot

Load **`NewsCatalogConfig`** from Remote Config, filter **`Entries`** by server UTC window, merge **`NewsPersistence.ReadNewsIds`**, return **`NewsGameData`**. Use a **`ConfigKey`** constant matching the builder / `.rc` entry (see §8).

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.News;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.News
{
    public sealed class NewsModule : GameModule<NewsGameData>
    {
        public const string ConfigKey = nameof(NewsCatalogConfig);
        private const string PersistenceKey = nameof(NewsPersistence);

        public override async Task<IGameModuleData> Initialize(
            IExecutionContext context,
            IPlayerData player,
            IGameState gameState,
            IRemoteConfig remoteConfig)
        {
            var persistence = await player.Get(context, PersistenceKey, new NewsPersistence());
            var catalog = await remoteConfig.Get(context, ConfigKey, new NewsCatalogConfig());
            var active = new List<NewsItemDto>();
            DateTime now = DateTime.UtcNow;

            if (catalog?.Entries != null)
            {
                foreach (var row in catalog.Entries)
                {
                    if (row == null || string.IsNullOrEmpty(row.Id)) continue;
                    if (now < row.StartUtc || now > row.EndUtc) continue;
                    active.Add(row);
                }
            }

            await Task.CompletedTask;
            return new NewsGameData
            {
                ActiveNews = active,
                ReadNewsIds = new List<string>(persistence.ReadNewsIds),
            };
        }
    }
}
```

Register **`NewsModule`** in **`ModuleConfig`** when you implement the feature ([`Docs/LiveOps/NewApiAndServices.md`](../LiveOps/NewApiAndServices.md)).

---

## 8. Editor authoring and Remote Config deployment

Follow the same pipeline as Tracks / Inventory ([`Docs/LiveOps/AuthoringPipeline.md`](../LiveOps/AuthoringPipeline.md)):

| Step | Action |
|------|--------|
| 1 | Add **`NewsCatalogConfig`** under `LiveOps/LiveOps.DTO/Modules/News/` (see §2). |
| 2 | Implement **`NewsCatalogConfigBuilderSO : ConfigBuilderSO<NewsCatalogConfig>`** in your game authoring assembly (e.g. `GearEngine.Campaign.Authoring`). Set **`public override string ConfigKey => nameof(NewsCatalogConfig);`**, **`Build()`** returns entries from a **`NewsCatalogSO`** (or inline list) designers edit in the Editor. |
| 3 | **Window → LiveOps → Config Deployment** → **Sync** for that builder → writes **`Assets/LiveOps/RemoteConfig/News.rc`** with `entries.NewsCatalogConfig` matching the key naming in [`Docs/LiveOps/RemoteConfig.md`](../LiveOps/RemoteConfig.md). |
| 4 | **Window → Deployment** → deploy **`.rc`** to the linked UGS environment. |
| 5 | Add an EditMode test (optional but recommended) that **`Build()`** output matches the committed **`.rc`**, mirroring **`LiveOpsConfigBuilderAndRcTests`**. |

Do **not** hand-edit **`.rc`** for real content; regenerate from the builder so tests and Cloud payloads stay aligned ([`com.scaffold.liveops.authoring` README](../../Assets/Packages/com.scaffold.liveops.authoring/README.md)).

**Example builder skeleton:**

```csharp
using System.Collections.Generic;
using GameModuleDTO.Modules.News;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/News Catalog Config Builder", fileName = "NewsCatalogConfigBuilder")]
    public sealed class NewsCatalogConfigBuilderSO : ConfigBuilderSO<NewsCatalogConfig>
    {
        [SerializeField] private List<NewsItemDto> entries = new();

        public override string ConfigKey => nameof(NewsCatalogConfig);

        public override NewsCatalogConfig Build() => new NewsCatalogConfig { Entries = new List<NewsItemDto>(entries) };

        public override void Apply(NewsCatalogConfig pulled)
        {
            if (pulled?.Entries == null) return;
            entries = new List<NewsItemDto>(pulled.Entries);
        }
    }
}
```

In practice, replace **`List<NewsItemDto>`** with references to a **`NewsCatalogSO`** (ScriptableObject rows: id, message, start/end) and map **`Build()`** to **`NewsItemDto`**.

---

## 9. Backend — GameApi handler

Validate **`request.NewsId`** against the same catalog + window rules, append to **`NewsPersistence.ReadNewsIds`**, **`await player.Set(...)`**. Handler class under **`LiveOps/Project`** so it compiles into **`LiveOps.dll`** ([`Docs/LiveOps/NewApiAndServices.md`](../LiveOps/NewApiAndServices.md)).

```csharp
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.News;

namespace GameModule.Modules.News
{
    public sealed class MarkNewsReadHandler : IGameApiHandler<MarkNewsReadRequest, MarkNewsReadResponse>
    {
        private readonly NewsModule module;

        public MarkNewsReadHandler(NewsModule module) => this.module = module;

        public async Task<MarkNewsReadResponse> HandleAsync(GameApiSession session, MarkNewsReadRequest request)
        {
            // await module.TryMarkReadAsync(session, request.NewsId);
            await Task.CompletedTask;
            return new MarkNewsReadResponse { Succeeded = true };
        }
    }
}
```

---

## End-to-end flow

```mermaid
sequenceDiagram
    participant RC as Remote Config (News.rc)
    participant SMod as NewsModule (server)
    participant Client as NewsClientModule
    participant L as ILiveOpsService
    participant H as MarkNewsReadHandler
    participant UI as View / ViewModel

    RC->>SMod: Get(NewsCatalogConfig)
    SMod-->>Client: NewsGameData (GameDataRequest)
    Client->>Client: OnInitializedAsync → HydrateFromBootstrapSnapshot
    UI->>UI: bind NewsItemModel.IsRead, HasUnread
    UI->>Client: MarkReadAsync(item)
    Client->>L: CallAsync(MarkNewsReadRequest)
    L->>H: HandleAsync
    H-->>L: MarkNewsReadResponse
    Client->>Client: MarkAsRead(item)
```

---

## Scaling read state

If **`readNewsIds`** grows too large, replace it with **`LastAcknowledgedCatalogRevision`** (server bumps revision when the active set changes). **`HasUnread`** becomes “revision not yet acknowledged,” which is coarser than per-item unread but minimal on the wire and in storage.

---

## Related

- [`Docs/Standards/State-and-Services-Standard.md`](Standards/State-and-Services-Standard.md)
- [`Docs/LiveOps/NewApiAndServices.md`](../LiveOps/NewApiAndServices.md)
- [`Docs/LiveOps/AuthoringPipeline.md`](../LiveOps/AuthoringPipeline.md)
- [`Docs/LiveOps/RemoteConfig.md`](../LiveOps/RemoteConfig.md)
