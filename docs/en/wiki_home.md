# VGameKit

> **English** | [Türkçe](../tr/wiki_home.md)

A modular Unity framework for streamlined game development.
See [How to Install Modules](how-to/install-modules.md) to get started.

---

## Tutorials

| | |
|---|---|
| [Getting Started](tutorials/getting-started.md) | Bootstrap VGameKit in a new project |
| [First Game](tutorials/first-game.md) | Menu + process flow + DI wiring |
| [UI System](tutorials/ui-system.md) | Panels, presenters, views, popups |
| [Spawner Basics](tutorials/spawner-basics.md) | Object pooling with BaseSpawnPool |
| [Process Flows](tutorials/processflows.md) | Chained async sequences |

## How-to Guides

| | |
|---|---|
| [Install Modules](how-to/install-modules.md) | Git URL / manifest.json setup |
| [Create a Menu](how-to/create-menu.md) | BaseMenuManager + presenters |
| [Use the Spawner](how-to/use-spawner.md) | Pool registration and spawn/recycle |
| [Manage Process Flows](how-to/manage-processflows.md) | Create, chain, cancel flows |
| [Configure Logging](how-to/configure-logging.md) | GKLog + compile symbols |
| [Integrate Ads](how-to/integrate-ads.md) | Google Mobile Ads setup |
| [Integrate Analytics](how-to/integrate-analytics.md) | GameAnalytics setup |

## Reference

| | |
|---|---|
| [App Manager](reference/api/app-manager.md) | AbsAppManager / IAsyncStartable |
| [Lifetime Scopes](reference/api/lifetime-scopes.md) | AbsMainLifetimeScope / AbsBaseLifetimeScope |
| [Subscribable](reference/api/subscribable.md) | SubscribableConcrete / SubscribableMonoBehaviour |
| [Menu System](reference/api/menu-system.md) | BaseMenuManager / BaseMenuPresenter |
| [Popup System](reference/api/popup-system.md) | PopupBuilder / BasePopupModel |
| [Spawner](reference/api/spawner.md) | BaseSpawnPool |
| [Process Flows](reference/api/processflows.md) | BaseProcessFlow / ProcessFlowProvider |
| [Logging](reference/api/logging.md) | GKLog / LogState |
| [Configuration](reference/api/configuration.md) | GKConfig |
| [Utilities](reference/api/utilities.md) | MatrixId and helpers |
| [Google Ads](reference/api/google-ads.md) | GoogleMobileAdsController |
| [Game Analytics](reference/api/game-analytics.md) | GA_Initialization |
| [JSONKit](reference/api/jsonkit.md) | JSonKit |

## Explanation

| | |
|---|---|
| [VContainer & DI](explanation/vcontainer-di.md) | Why DI, LifetimeScope hierarchy |
| [MessagePipe Events](explanation/messagepipe-events.md) | Pub/sub architecture |
| [Spawner & Pooling](explanation/spawner-pooling.md) | Object pool design |
| [Async Patterns](explanation/async-patterns.md) | UniTask, CancellationToken |
| [Project Structure](explanation/project-structure.md) | Assembly layout, namespaces |
