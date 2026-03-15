# VGameKit

> [English](Home) | **Türkçe**

Unity oyun geliştirmeyi kolaylaştıran modüler bir framework.
Kurulum için bkz. [Modül Kurulumu](how-to/install-modules.md).

---

## Tutorials

| | |
|---|---|
| [Başlarken](tutorials/getting-started.md) | VGameKit'i yeni bir projeye dahil edin |
| [İlk Oyun](tutorials/first-game.md) | Menü + process flow + DI bağlantısı |
| [UI Sistemi](tutorials/ui-system.md) | Panel, presenter, view ve popup'lar |
| [Spawner Temelleri](tutorials/spawner-basics.md) | BaseSpawnPool ile nesne havuzu |
| [Process Flow'lar](tutorials/processflows.md) | Zincirleme async diziler |

## Nasıl Yapılır

| | |
|---|---|
| [Modül Kurulumu](how-to/install-modules.md) | Git URL / manifest.json ile kurulum |
| [Menü Oluştur](how-to/create-menu.md) | BaseMenuManager + presenter |
| [Spawner Kullan](how-to/use-spawner.md) | Havuz kaydı, spawn ve geri dönüşüm |
| [Process Flow Yönet](how-to/manage-processflows.md) | Flow oluşturma, zincirleme, iptal |
| [Logging Yapılandır](how-to/configure-logging.md) | GKLog + derleme sembolleri |
| [Reklam Entegrasyonu](how-to/integrate-ads.md) | Google Mobile Ads kurulumu |
| [Analitik Entegrasyonu](how-to/integrate-analytics.md) | GameAnalytics kurulumu |

## Referans

| | |
|---|---|
| [App Manager](reference/api/app-manager.md) | AbsAppManager / IAsyncStartable |
| [Lifetime Scopes](reference/api/lifetime-scopes.md) | AbsMainLifetimeScope / AbsBaseLifetimeScope |
| [Subscribable](reference/api/subscribable.md) | SubscribableConcrete / SubscribableMonoBehaviour |
| [Menü Sistemi](reference/api/menu-system.md) | BaseMenuManager / BaseMenuPresenter |
| [Popup Sistemi](reference/api/popup-system.md) | PopupBuilder / BasePopupModel |
| [Spawner](reference/api/spawner.md) | BaseSpawnPool |
| [Process Flows](reference/api/processflows.md) | BaseProcessFlow / ProcessFlowProvider |
| [Loglama](reference/api/logging.md) | GKLog / LogState |
| [Konfigürasyon](reference/api/configuration.md) | GKConfig |
| [Araçlar](reference/api/utilities.md) | MatrixId ve yardımcılar |
| [Google Ads](reference/api/google-ads.md) | GoogleMobileAdsController |
| [Game Analytics](reference/api/game-analytics.md) | GA_Initialization |
| [JSONKit](reference/api/jsonkit.md) | JSonKit |

## Açıklama

| | |
|---|---|
| [VContainer & DI](explanation/vcontainer-di.md) | DI'nın nedeni, LifetimeScope hiyerarşisi |
| [MessagePipe Events](explanation/messagepipe-events.md) | Pub/sub mimarisi |
| [Spawner & Pooling](explanation/spawner-pooling.md) | Nesne havuzu tasarımı |
| [Async Patterns](explanation/async-patterns.md) | UniTask, CancellationToken |
| [Proje Yapısı](explanation/project-structure.md) | Assembly düzeni, namespace'ler |
