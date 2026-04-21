# LiveOps config builders (defaults)

These `*ConfigBuilder` assets drive **Window → LiveOps → Config Deployment** (sync to `Assets/LiveOps/RemoteConfig/*.rc`).

| Asset | Catalog / notes |
|-------|------------------|
| `TrackConfigBuilder` | References `CampaignTrackCatalog` under `Data/Campaign/Catalogs/` (same as scenes). |
| `RoguelikeConfigBuilder` | References `CampaignRoguelikeGearPool` (gear roll pool; separate from track routing). |
| `CardConfigBuilder` | References `Example_CardCatalog` under `Data/Cards/Examples/`. |
| `CurrencyConfigBuilder` | Inline currency rows (default: `gold`). |
| `InventoryConfigBuilder` | `baseSlots` only. |
| `LoadoutConfigBuilder` | `baseSlots` only. |

Reassign catalogs in the Inspector if your bootstrap uses different SO instances.
