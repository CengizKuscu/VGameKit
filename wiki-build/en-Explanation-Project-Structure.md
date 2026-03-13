# Explanation: Project Structure

## Overview

VGameKit is organised as a set of independent Unity assembly definition packages under `Assets/`. Each package has a single concern and depends only on what it needs. Understanding this layout helps you know where to add code, which assemblies to reference, and where to look when something breaks.

---

## Top-level layout

```
Assets/
├── VGameKit/           Core runtime — DI, menus, popups, spawner, flows, logging
├── VGameKit.GA/        GameAnalytics integration
├── VGameKit.GoogleAds/ Google Mobile Ads integration
├── VGameKit.IO/        JSON persistence helpers
└── Demo/               Example scopes and scenes; not shipped in production builds

docs/
├── en/                 English documentation (Diátaxis layout)
├── tr/                 Turkish documentation (Diátaxis layout)
└── PLAN.md             Internal documentation roadmap (Turkish)

ProjectSettings/        Unity-managed; do not hand-edit
Packages/               Unity Package Manager manifest and lock files
```

---

## VGameKit — Core runtime

```
Assets/VGameKit/Runtime/
├── App/
│   └── AbsAppManager.cs          IAsyncStartable entry point
├── Config/
│   └── GKConfig.cs               ScriptableObject for environment config
├── Core/
│   ├── AbsMainLifetimeScope.cs   App-level DI root; registers MessagePipe
│   ├── AbsBaseLifetimeScope.cs   Scene/subsystem scope base
│   ├── SubscribableConcrete.cs   Plain-C# subscribable via IInitializable
│   └── SubscribableMonoBehaviour.cs  MonoBehaviour subscribable via [Inject]
├── Log/
│   ├── GKLog.cs                  Static logging facade
│   └── LogState.cs               Log level enum (Development, Game, Error, …)
├── ProcessFlows/
│   ├── BaseProcessFlow.cs        Async, cancellable unit of work
│   ├── ProcessFlowProvider.cs    Manages active flow lifecycles
│   ├── IFlowTask.cs              Sync flow contract (low-level)
│   └── IFlowAsyncTask.cs         Async flow contract (low-level)
├── UI/
│   ├── Menu/
│   │   ├── BaseMenuManager.cs    Tracks open panels; enforces MenuMode
│   │   ├── BaseMenuPresenter.cs  Logic layer for one panel
│   │   └── BaseMenuView.cs       Visual layer for one panel
│   └── Popup/
│       ├── PopupBuilder.cs       Fluent popup construction API
│       ├── BasePopupModel.cs     Typed data for one popup
│       └── BasePopupView.cs      Visual layer for one popup
├── Spawner/
│   ├── BaseSpawnPool.cs          Generic object pool
│   └── SpawnerExtensions.cs      Pool utility helpers
└── Utilities/
    └── MatrixId.cs               Composite identifier value type
```

The core runtime has **no dependency** on the integration packages (`VGameKit.GA`, `VGameKit.GoogleAds`). Those packages reference the core, not the other way around.

---

## VGameKit.GoogleAds

```
Assets/VGameKit.GoogleAds/Runtime/
├── GoogleMobileAdsController.cs  Initialisation and consent orchestration
├── GoogleMobileAdsConsentController.cs  UMP consent flow
├── AdsIds.cs                     Platform/test ad unit IDs
├── BannerAds.cs
├── InterstitialAds.cs
└── RewardedAds.cs

Assets/VGameKit.GoogleAds/Editor/
└── AdsIdsEditor.cs               Custom Inspector for AdsIds
```

Guarded by `GOOGLEADS_TESTDEVICE` (forces test device mode) and activated by regular Google Mobile Ads SDK presence.

---

## VGameKit.GA

```
Assets/VGameKit.GA/Runtime/
└── GA_Initialization.cs          GameAnalytics SDK bootstrap
```

Guarded by `GA_ENABLED`. All GA event calls elsewhere in the codebase are wrapped in `#if GA_ENABLED`.

---

## VGameKit.IO

```
Assets/VGameKit.IO/Runtime/
└── JSonKit.cs                    JSON read/write helpers (Newtonsoft)
```

Used for save-game persistence and config serialisation.

---

## Demo

```
Assets/Demo/
├── Runtime/                      Example LifetimeScopes and controllers
└── Scenes/
    └── SampleScene.unity         Primary smoke-test scene
```

Demo code is not part of any production asmdef. It exists to verify that all systems integrate correctly. The scene at `Assets/Demo/Scenes/SampleScene.unity` is the canonical Play-Mode smoke test referenced in `AGENTS.md`.

---

## Namespace conventions

| Assembly | Namespace prefix |
|---|---|
| `VGameKit` (core) | `VGameKit.Runtime.*` |
| `VGameKit.IO` | `VGameKit.IO.Runtime` |
| `VGameKit.GA` | `VGameKit.GA.Runtime` |
| `VGameKit.GoogleAds` | `VGameKit.GoogleAds.Runtime` |

Game-specific code in `Demo/` uses its own namespace (e.g., `VGameKit.Demo`). Do not place game code in the `VGameKit.Runtime` namespace.

---

## What to avoid

- **Adding scripts directly to `Assets/` root**: always place code inside an asmdef boundary.
- **Referencing integration assemblies from core**: core must remain independent.
- **Hand-editing `ProjectSettings/` or `Packages/manifest.json`**: use Unity Editor or the Package Manager UI.
- **Placing game-specific logic in `VGameKit.Runtime`**: keep core generic; customise in app-level subclasses.

---

## See also

- [Reference: Lifetime Scopes](en-Reference-Lifetime-Scopes)
- [Explanation: VContainer and DI](en-Explanation-VContainer-DI)
- [AGENTS.md](../../AGENTS.md)
