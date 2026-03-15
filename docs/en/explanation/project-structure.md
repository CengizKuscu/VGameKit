# Explanation: Project Structure

## Overview

VGameKit is published as four independent Unity packages installed via Package Manager Git URLs. When installed into a game project, packages live in `Library/PackageCache/` — **no folders are created under `Assets/`**. The full source, including the Demo, exists only in this repository and is not visible to end users.

---

## How developers use VGameKit

Developers add the desired packages to their project's `Packages/manifest.json`:

```json
"com.cngz.vgamekit":           "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit#v0.0.4",
"com.cngz.vgamekit.io":        "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.IO#v0.0.4",
"com.cngz.vgamekit.ga":        "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GA#v0.0.4",
"com.cngz.vgamekit.googleads": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GoogleAds#v0.0.4"
```

Unity resolves them into `Library/PackageCache/`. Packages appear in the Package Manager window under *In Project* and their assemblies are referenced by name in the developer's own `.asmdef` files. Nothing from this repository appears under the developer's `Assets/` folder.

---

## Repository layout (this repo only)

This is the source repository. The layout below is only relevant when working on the framework itself.

```
Assets/
├── VGameKit/           Core runtime package source
├── VGameKit.GA/        GameAnalytics integration package source
├── VGameKit.GoogleAds/ Google Mobile Ads integration package source
├── VGameKit.IO/        JSON persistence package source
└── Demo/               Integration smoke-test; not published as a package

docs/
├── en/                 English documentation (Diátaxis layout)
├── tr/                 Turkish documentation (Diátaxis layout)
└── PLAN.md             Internal documentation roadmap (Turkish)

ProjectSettings/        Unity-managed; do not hand-edit
Packages/               Unity Package Manager manifest and lock files
scripts/                Developer tooling (e.g. publish_wiki.py)
```

---

## Package contents

### VGameKit — Core runtime

| Class | Responsibility |
|---|---|
| `AbsAppManager` | `IAsyncStartable` entry point |
| `AbsMainLifetimeScope` | App-level DI root; registers MessagePipe |
| `AbsBaseLifetimeScope` | Scene/subsystem scope base |
| `SubscribableConcrete` | Plain-C# subscribable via `IInitializable` |
| `SubscribableMonoBehaviour` | MonoBehaviour subscribable via `[Inject]` |
| `GKLog` / `LogState` | Static logging facade + log level enum |
| `BaseProcessFlow<TArgs>` | Async, cancellable unit of work |
| `ProcessFlowProvider` | Manages active flow lifecycles |
| `BaseMenuManager<TMenuName>` | Tracks open panels; enforces `MenuMode` |
| `BaseSpawnPool<TModel, TItem>` | Generic object pool |
| `GKConfig` | ScriptableObject for environment config |
| `MatrixId` | Composite identifier value type |

The core runtime has **no dependency** on the integration packages (`VGameKit.GA`, `VGameKit.GoogleAds`). Those packages reference core, not the other way around.

### VGameKit.GoogleAds

Wraps Google Mobile Ads SDK. Key classes: `GoogleMobileAdsController`, `GoogleMobileAdsConsentController`, `AdsIds`, `BannerAds`, `InterstitialAds`, `RewardedAds`.
Guarded by `GOOGLEADS_TESTDEVICE` (test device mode) and the presence of the Google Mobile Ads SDK.

### VGameKit.GA

Wraps GameAnalytics SDK. Key class: `GA_Initialization`.
Guarded by `GA_ENABLED`. All GA event calls elsewhere are wrapped in `#if GA_ENABLED`.

### VGameKit.IO

JSON helpers. Key class: `JSonKit` (Newtonsoft-backed read/write).
Used for save-game persistence and config serialisation.

---

## Demo (this repository only)

`Assets/Demo/` is part of this repository only — it is not published as a package and is never visible to end users. It exists solely to verify that all framework systems integrate correctly.

The scene at `Assets/Demo/Scenes/SampleScene.unity` is the canonical Play-Mode smoke test referenced in `AGENTS.md`.

---

## Namespace conventions

| Package | Namespace prefix |
|---|---|
| `VGameKit` (core) | `VGameKit.Runtime.*` |
| `VGameKit.IO` | `VGameKit.IO.Runtime` |
| `VGameKit.GA` | `VGameKit.GA.Runtime` |
| `VGameKit.GoogleAds` | `VGameKit.GoogleAds.Runtime` |

Game-specific code in `Demo/` uses its own namespace (e.g., `VGameKit.Demo`). Do not place game code in the `VGameKit.Runtime` namespace.

---

## What to avoid

- **Expecting any VGameKit folders under `Assets/` in consumer projects**: all packages live in `Library/PackageCache/` — do not copy package source into `Assets/`.
- **Referencing integration assemblies from core**: core must remain independent.
- **Hand-editing `ProjectSettings/` or `Packages/manifest.json`**: use Unity Editor or the Package Manager UI.
- **Placing game-specific logic in `VGameKit.Runtime`**: keep core generic; customise in app-level subclasses.

---

## See also

- [Reference: Lifetime Scopes](../reference/api/lifetime-scopes.md)
- [Explanation: VContainer and DI](./vcontainer-di.md)
- [AGENTS.md](../../AGENTS.md)
