# Açıklama: Proje Yapısı

## Genel Bakış

VGameKit, `Assets/` altında bağımsız Unity assembly definition paketleri kümesi olarak düzenlenmiştir. Her paket tek bir sorumluluğa sahiptir ve yalnızca ihtiyaç duyduğuna bağlıdır. Bu yapıyı anlamak, nereye kod ekleyeceğinizi, hangi assembly'lere referans vereceğinizi ve bir şey bozulduğunda nereye bakacağınızı bilmenizi sağlar.

---

## Üst düzey yapı

```
Assets/
├── VGameKit/           Core runtime — DI, menüler, popup'lar, spawner, flow'lar, loglama
├── VGameKit.GA/        GameAnalytics entegrasyonu
├── VGameKit.GoogleAds/ Google Mobile Ads entegrasyonu
├── VGameKit.IO/        JSON kalıcılık yardımcıları
└── Demo/               Örnek scope'lar ve sahneler; production build'lerde gönderilmez

docs/
├── en/                 İngilizce dokümantasyon (Diátaxis yapısı)
├── tr/                 Türkçe dokümantasyon (Diátaxis yapısı)
└── PLAN.md             Dahili dokümantasyon yol haritası (Türkçe)

ProjectSettings/        Unity tarafından yönetilir; elle düzenlemeyin
Packages/               Unity Package Manager manifest ve kilit dosyaları
```

---

## VGameKit — Core runtime

```
Assets/VGameKit/Runtime/
├── App/
│   └── AbsAppManager.cs          IAsyncStartable giriş noktası
├── Config/
│   └── GKConfig.cs               Ortam config'i için ScriptableObject
├── Core/
│   ├── AbsMainLifetimeScope.cs   Uygulama düzeyi DI kökü; MessagePipe'ı kaydeder
│   ├── AbsBaseLifetimeScope.cs   Sahne/alt sistem scope tabanı
│   ├── SubscribableConcrete.cs   IInitializable aracılığıyla düz-C# subscribable
│   └── SubscribableMonoBehaviour.cs  [Inject] aracılığıyla MonoBehaviour subscribable
├── Log/
│   ├── GKLog.cs                  Statik loglama cephesi
│   └── LogState.cs               Log seviyesi enum (Development, Game, Error, …)
├── ProcessFlows/
│   ├── BaseProcessFlow.cs        Asenkron, iptal edilebilir iş birimi
│   ├── ProcessFlowProvider.cs    Aktif flow yaşam döngülerini yönetir
│   ├── IFlowTask.cs              Senkron flow sözleşmesi (düşük seviye)
│   └── IFlowAsyncTask.cs         Asenkron flow sözleşmesi (düşük seviye)
├── UI/
│   ├── Menu/
│   │   ├── BaseMenuManager.cs    Açık panelleri takip eder; MenuMode'u uygular
│   │   ├── BaseMenuPresenter.cs  Bir panel için mantık katmanı
│   │   └── BaseMenuView.cs       Bir panel için görsel katman
│   └── Popup/
│       ├── PopupBuilder.cs       Akışkan popup oluşturma API'si
│       ├── BasePopupModel.cs     Bir popup için tiplendirilmiş veri
│       └── BasePopupView.cs      Bir popup için görsel katman
├── Spawner/
│   ├── BaseSpawnPool.cs          Genel nesne havuzu
│   └── SpawnerExtensions.cs      Havuz yardımcı metotları
└── Utilities/
    └── MatrixId.cs               Bileşik tanımlayıcı değer tipi
```

Core runtime, entegrasyon paketlerine (`VGameKit.GA`, `VGameKit.GoogleAds`) **hiçbir bağımlılığı yoktur**. Bu paketler core'a referans verir, tersi değil.

---

## VGameKit.GoogleAds

```
Assets/VGameKit.GoogleAds/Runtime/
├── GoogleMobileAdsController.cs  Başlatma ve izin orkestrasyon
├── GoogleMobileAdsConsentController.cs  UMP izin akışı
├── AdsIds.cs                     Platform/test reklam birimi ID'leri
├── BannerAds.cs
├── InterstitialAds.cs
└── RewardedAds.cs

Assets/VGameKit.GoogleAds/Editor/
└── AdsIdsEditor.cs               AdsIds için özel Inspector
```

`GOOGLEADS_TESTDEVICE` (test cihaz modunu zorlar) ile korunur ve normal Google Mobile Ads SDK varlığıyla etkinleştirilir.

---

## VGameKit.GA

```
Assets/VGameKit.GA/Runtime/
└── GA_Initialization.cs          GameAnalytics SDK bootstrap
```

`GA_ENABLED` ile korunur. Kod tabanındaki diğer tüm GA event çağrıları `#if GA_ENABLED` içinde sarılıdır.

---

## VGameKit.IO

```
Assets/VGameKit.IO/Runtime/
└── JSonKit.cs                    JSON okuma/yazma yardımcıları (Newtonsoft)
```

Kayıt oyunu kalıcılığı ve config serileştirme için kullanılır.

---

## Demo

```
Assets/Demo/
├── Runtime/                      Örnek LifetimeScope'lar ve controller'lar
└── Scenes/
    └── SampleScene.unity         Birincil smoke-test sahnesi
```

Demo kodu, herhangi bir production asmdef'in parçası değildir. Tüm sistemlerin doğru entegre olduğunu doğrulamak için vardır. `Assets/Demo/Scenes/SampleScene.unity`'deki sahne, `AGENTS.md`'de referans verilen kanonik Play-Mode smoke test'idir.

---

## Namespace kuralları

| Assembly | Namespace öneki |
|---|---|
| `VGameKit` (core) | `VGameKit.Runtime.*` |
| `VGameKit.IO` | `VGameKit.IO.Runtime` |
| `VGameKit.GA` | `VGameKit.GA.Runtime` |
| `VGameKit.GoogleAds` | `VGameKit.GoogleAds.Runtime` |

`Demo/`'daki oyuna özgü kod kendi namespace'ini kullanır (ör. `VGameKit.Demo`). Oyun kodunu `VGameKit.Runtime` namespace'ine koymayın.

---

## Kaçınılması gerekenler

- **Script'leri doğrudan `Assets/` köküne eklemek**: kodu her zaman bir asmdef sınırı içine yerleştirin.
- **Core'dan entegrasyon assembly'lerine referans vermek**: core bağımsız kalmalıdır.
- **`ProjectSettings/` veya `Packages/manifest.json`'ı elle düzenlemek**: Unity Editor veya Package Manager UI kullanın.
- **`VGameKit.Runtime`'a oyuna özgü mantık yerleştirmek**: core'u genel tutun; uygulama düzeyi alt sınıflarında özelleştirin.

---

## Ayrıca Bkz.

- [Referans: Lifetime Scopes](tr-Reference-Lifetime-Scopes)
- [Açıklama: VContainer ve DI](tr-Explanation-VContainer-DI)
- [AGENTS.md](../../AGENTS.md)
