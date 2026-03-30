# Açıklama: Proje Yapısı

## Genel Bakış

VGameKit, Package Manager Git URL'leri aracılığıyla yüklenen dört bağımsız Unity paketi olarak yayınlanmaktadır. Bir oyun projesine yüklendiğinde paketler `Library/PackageCache/` altına gelir — **`Assets/` altında herhangi bir klasör oluşturulmaz**. Demo dahil tüm kaynak kod yalnızca bu repoda bulunur ve son kullanıcılara görünmez.

---

## Geliştiriciler VGameKit'i nasıl kullanır

Geliştiriciler istedikleri paketleri projelerinin `Packages/manifest.json` dosyasına ekler:

```json
"com.cngz.vgamekit":           "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit#v0.0.5",
"com.cngz.vgamekit.io":        "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.IO#v0.0.5",
"com.cngz.vgamekit.ga":        "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GA#v0.0.5",
"com.cngz.vgamekit.googleads": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GoogleAds#v0.0.5"
```

Unity bunları `Library/PackageCache/` altına çözümler. Paketler Package Manager penceresinde *In Project* bölümünde görünür; assembly'lerine kendi `.asmdef` dosyalarınızdan isimle referans verirsiniz. Bu repodan hiçbir şey geliştiricinin `Assets/` klasörü altında görünmez.

---

## Repository yapısı (yalnızca bu repo)

Bu, kaynak reposudur. Aşağıdaki yapı yalnızca framework üzerinde çalışırken geçerlidir.

```
Assets/
├── VGameKit/           Core runtime paket kaynağı
├── VGameKit.GA/        GameAnalytics entegrasyon paketi kaynağı
├── VGameKit.GoogleAds/ Google Mobile Ads entegrasyon paketi kaynağı
├── VGameKit.IO/        JSON kalıcılık paketi kaynağı
└── Demo/               Entegrasyon smoke-test; paket olarak yayınlanmaz

docs/
├── en/                 İngilizce dokümantasyon (Diátaxis yapısı)
├── tr/                 Türkçe dokümantasyon (Diátaxis yapısı)
└── PLAN.md             Dahili dokümantasyon yol haritası (Türkçe)

ProjectSettings/        Unity tarafından yönetilir; elle düzenlemeyin
Packages/               Unity Package Manager manifest ve kilit dosyaları
scripts/                Geliştirici araçları (ör. publish_wiki.py)
```

---

## Paket içerikleri

### VGameKit — Core runtime

| Sınıf | Sorumluluk |
|---|---|
| `AbsAppManager` | `IAsyncStartable` giriş noktası |
| `AbsMainLifetimeScope` | Uygulama düzeyi DI kökü; MessagePipe'ı kaydeder |
| `AbsBaseLifetimeScope` | Sahne/alt sistem scope tabanı |
| `SubscribableConcrete` | `IInitializable` aracılığıyla düz-C# subscribable |
| `SubscribableMonoBehaviour` | `[Inject]` aracılığıyla MonoBehaviour subscribable |
| `GKLog` / `LogState` | Statik loglama cephesi + log seviyesi enum |
| `BaseProcessFlow<TArgs>` | Asenkron, iptal edilebilir iş birimi |
| `ProcessFlowProvider` | Aktif flow yaşam döngülerini yönetir |
| `BaseMenuManager<TMenuName>` | Açık panelleri takip eder; `MenuMode`'u uygular |
| `BaseSpawnPool<TModel, TItem>` | Genel nesne havuzu |
| `GKConfig` | Ortam config'i için ScriptableObject |
| `MatrixId` | Bileşik tanımlayıcı değer tipi |

Core runtime, entegrasyon paketlerine (`VGameKit.GA`, `VGameKit.GoogleAds`) **hiçbir bağımlılığı yoktur**. Bu paketler core'a referans verir, tersi değil.

### VGameKit.GoogleAds

Google Mobile Ads SDK'yı sarar. Temel sınıflar: `GoogleMobileAdsController`, `GoogleMobileAdsConsentController`, `AdsIds`, `BannerAds`, `InterstitialAds`, `RewardedAds`.
`GOOGLEADS_TESTDEVICE` (test cihaz modu) ve Google Mobile Ads SDK varlığıyla korunur.

### VGameKit.GA

GameAnalytics SDK'yı sarar. Temel sınıf: `GA_Initialization`.
`GA_ENABLED` ile korunur. Kod tabanındaki tüm GA event çağrıları `#if GA_ENABLED` içinde sarılıdır.

### VGameKit.IO

JSON yardımcıları. Temel sınıf: `JSonKit` (Newtonsoft destekli okuma/yazma).
Kayıt oyunu kalıcılığı ve config serileştirme için kullanılır.

---

## Demo (yalnızca bu repository)

`Assets/Demo/`, yalnızca bu repoda bulunur — paket olarak yayınlanmaz ve son kullanıcılara hiçbir zaman görünmez. Yalnızca tüm framework sistemlerinin doğru entegre olduğunu doğrulamak için mevcuttur.

`Assets/Demo/Scenes/SampleScene.unity`'deki sahne, `AGENTS.md`'de referans verilen kanonik Play-Mode smoke test'idir.

---

## Namespace kuralları

| Paket | Namespace öneki |
|---|---|
| `VGameKit` (core) | `VGameKit.Runtime.*` |
| `VGameKit.IO` | `VGameKit.IO.Runtime` |
| `VGameKit.GA` | `VGameKit.GA.Runtime` |
| `VGameKit.GoogleAds` | `VGameKit.GoogleAds.Runtime` |

`Demo/`'daki oyuna özgü kod kendi namespace'ini kullanır (ör. `VGameKit.Demo`). Oyun kodunu `VGameKit.Runtime` namespace'ine koymayın.

---

## Kaçınılması gerekenler

- **Tüketen projelerde `Assets/` altında VGameKit klasörü beklemek**: paketler `Library/PackageCache/` altındadır — paket kaynağını `Assets/` altına kopyalamayın.
- **Core'dan entegrasyon assembly'lerine referans vermek**: core bağımsız kalmalıdır.
- **`ProjectSettings/` veya `Packages/manifest.json`'ı elle düzenlemek**: Unity Editor veya Package Manager UI kullanın.
- **`VGameKit.Runtime`'a oyuna özgü mantık yerleştirmek**: core'u genel tutun; uygulama düzeyi alt sınıflarında özelleştirin.

---

## Ayrıca Bkz.

- [Referans: Lifetime Scopes](../reference/api/lifetime-scopes.md)
- [Açıklama: VContainer ve DI](./vcontainer-di.md)
- [AGENTS.md](../../AGENTS.md)
