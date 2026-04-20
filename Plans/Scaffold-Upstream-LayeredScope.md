# Scaffold packages in Gear-Engine (UPM git)

Gear-Engine consumes **`com.scaffold.layeredscope`**, **`com.scaffold.ugs`**, **`com.scaffold.liveops`**, **`com.scaffold.cloudcode`**, **`com.scaffold.navigation`**, and the other `com.scaffold.*` entries via **git URLs** in [`Packages/manifest.json`](../Packages/manifest.json). Unity resolves them under `Library/PackageCache`; there are **no** vendored copies under `Assets/Packages/com.scaffold.*` in this repo.

Authoritative sources: [MgCohen/Scaffold](https://github.com/MgCohen/Scaffold) (`Assets/Packages/`).

To bump versions or pins, edit `manifest.json` (and let Unity refresh `packages-lock.json`), or point at a fork/tag/commit instead of `#main`.
