# VGameKit

> [English](Home) | **Türkçe**

Unity oyun geliştirmeyi kolaylaştıran modüler bir framework.
Kurulum için bkz. [Modül Kurulumu](tr-HowTo-Install-Modules).

---

## Tutorials

| | |
|---|---|
| [Başlarken](tr-Tutorial-Getting-Started) | VGameKit'i yeni bir projeye dahil edin |
| [İlk Oyun](tr-Tutorial-First-Game) | Menü + process flow + DI bağlantısı |
| [UI Sistemi](tr-Tutorial-UI-System) | Panel, presenter, view ve popup'lar |
| [Spawner Temelleri](tr-Tutorial-Spawner-Basics) | BaseSpawnPool ile nesne havuzu |
| [Process Flow'lar](tr-Tutorial-ProcessFlows) | Zincirleme async diziler |

## Nasıl Yapılır

| | |
|---|---|
| [Modül Kurulumu](tr-HowTo-Install-Modules) | Git URL / manifest.json ile kurulum |
| [Menü Oluştur](tr-HowTo-Create-Menu) | BaseMenuManager + presenter |
| [Spawner Kullan](tr-HowTo-Use-Spawner) | Havuz kaydı, spawn ve geri dönüşüm |
| [Process Flow Yönet](tr-HowTo-Manage-ProcessFlows) | Flow oluşturma, zincirleme, iptal |
| [Logging Yapılandır](tr-HowTo-Configure-Logging) | GKLog + derleme sembolleri |
| [Reklam Entegrasyonu](tr-HowTo-Integrate-Ads) | Google Mobile Ads kurulumu |
| [Analitik Entegrasyonu](tr-HowTo-Integrate-Analytics) | GameAnalytics kurulumu |

## Referans

| | |
|---|---|
| [App Manager](tr-Reference-App-Manager) | AbsAppManager / IAsyncStartable |
| [Lifetime Scopes](tr-Reference-Lifetime-Scopes) | AbsMainLifetimeScope / AbsBaseLifetimeScope |
| [Subscribable](tr-Reference-Subscribable) | SubscribableConcrete / SubscribableMonoBehaviour |
| [Menü Sistemi](tr-Reference-Menu-System) | BaseMenuManager / BaseMenuPresenter |
| [Popup Sistemi](tr-Reference-Popup-System) | PopupBuilder / BasePopupModel |
| [Spawner](tr-Reference-Spawner) | BaseSpawnPool |
| [Process Flows](tr-Reference-ProcessFlows) | BaseProcessFlow / ProcessFlowProvider |
| [Loglama](tr-Reference-Logging) | GKLog / LogState |
| [Konfigürasyon](tr-Reference-Configuration) | GKConfig |
| [Araçlar](tr-Reference-Utilities) | MatrixId ve yardımcılar |
| [Google Ads](tr-Reference-Google-Ads) | GoogleMobileAdsController |
| [Game Analytics](tr-Reference-Game-Analytics) | GA_Initialization |
| [JSONKit](tr-Reference-JSONKit) | JSonKit |

## Açıklama

| | |
|---|---|
| [VContainer & DI](tr-Explanation-VContainer-DI) | DI'nın nedeni, LifetimeScope hiyerarşisi |
| [MessagePipe Events](tr-Explanation-MessagePipe-Events) | Pub/sub mimarisi |
| [Spawner & Pooling](tr-Explanation-Spawner-Pooling) | Nesne havuzu tasarımı |
| [Async Patterns](tr-Explanation-Async-Patterns) | UniTask, CancellationToken |
| [Proje Yapısı](tr-Explanation-Project-Structure) | Assembly düzeni, namespace'ler |
