# Google Mobile Ads Referansı

**Ad Alanı:** `VGameKit.GoogleAds.Runtime`
**Derleme:** `VGameKit.GoogleAds.Runtime`
**Dosyalar:** `Assets/VGameKit.GoogleAds/Runtime/`

---

## Genel Bakış

`VGameKit.GoogleAds` modülü, [Google Mobile Ads Unity SDK'sını](https://developers.google.com/admob/unity) şunlarla sarar:
- Onay, başlatma ve reklam yaşam döngüsünü yöneten merkezi bir denetleyici (`GoogleMobileAdsController`)
- Tekil reklam tipi yöneticileri (`BannerAds`, `InterstitialAds`, `RewardedAds`)
- UMP tabanlı onay denetleyicisi (`GoogleMobileAdsConsentController`)
- Platform duyarlı reklam birimi kimliği çözümlemesi (`AdsIds`)
- MessagePipe tabanlı olay veri yolu (`AdsEvent` / `AdsEventStatus`)
- Gizlilik butonu yardımcısı (`VGameKitPrivacyButton`)

---

## Tip Özeti

| Tip | Tür | Açıklama |
|---|---|---|
| `GoogleMobileAdsController` | `MonoBehaviour` | Merkezi koordinatör; Inspector'da yapılandırın |
| `GoogleMobileAdsConsentController` | sınıf | UMP onay akışı |
| `AdsIds` | struct | Platform başına reklam birimi kimlikleri |
| `AdsBaseView<T, TModel>` | soyut sınıf | Tüm reklam view'larının tabanı |
| `AdsItemModel` | sınıf | Temel reklam yapılandırma verisi |
| `InterstitialAdsItemModel` | sınıf | `AdsItemModel`'i `UseAutoShow` ile genişletir |
| `BannerAds` | sınıf | Banner yaşam döngüsü |
| `InterstitialAds` | sınıf | Geçiş reklamı yaşam döngüsü |
| `RewardedAds` | sınıf | Ödüllü reklam yaşam döngüsü |
| `AdsEvent` | sınıf | MessagePipe olay yükü |
| `AdsEventStatus` | enum | Olay tipi tanımlayıcısı |
| `AdsType` | enum | `Banner`, `Interstitial`, `Rewarded` |
| `CustomInitializeStatus` | enum | SDK başlatma sonucu |
| `CustomConsentStatus` | enum | UMP onay sonucu |
| `VGameKitPrivacyButton` | `MonoBehaviour` | Gizlilik formunu gösteren buton |

---

## Enum: `AdsType`

```csharp
public enum AdsType { Banner, Interstitial, Rewarded }
```

---

## Enum: `AdsEventStatus`

```csharp
public enum AdsEventStatus
{
    Request,             // Çağıran reklam gösterilmesini ister
    ResponseOpened,      // Reklam kullanıcıya gösterildi
    ResponseClosed,      // Kullanıcı reklamı kapattı
    ResponseEarnReward,  // Kullanıcı ödül kazandı (yalnızca ödüllü reklamlar)
    ResponseShowFail,    // Reklam gösterilemedi
    ResponseLoadFail,    // Reklam yüklenemedi
    NotReadyYet,         // Reklam yüklendi ama bekleme süresi dolmadı
    NotRequestedYet      // Reklam henüz yüklenmedi
}
```

---

## Sınıf: `AdsEvent`

```csharp
public class AdsEvent
```

Hem reklam talep etmek hem de yanıt almak için kullanılan MessagePipe olay yükü.

| Özellik | Tip | Açıklama |
|---|---|---|
| `AdsType` | `AdsType` | Bu olayın ilgili olduğu reklam tipi |
| `From` | `string` | Çağıran etiketi — analitik olay adlandırması için kullanılır |
| `AdId` | `int` | İsteğe bağlı çağıran atanmış tanımlayıcı; varsayılan `-1` |
| `IsEarnedReward` | `bool` | Kullanıcı ödül kazandığında `true` yapılır |

```csharp
// Reklam talep etmek için yayımla
_adsEventPublisher.Publish(AdsEventStatus.Request,
    new AdsEvent(AdsType.Rewarded, from: "ShopScreen", adId: 42));

// Yanıtlara abone ol
_adsEventSubscriber.Subscribe(AdsEventStatus.ResponseEarnReward, e =>
{
    if (e.IsEarnedReward) GrantReward(e.AdId);
}).AddTo(_bagBuilder);
```

---

## Sınıf: `AdsIds`

```csharp
[Serializable]
public struct AdsIds
```

`GoogleMobileAdsController` üzerinde Inspector'da atayın. Her iki platform için üretim ve Google'ın yerleşik test kimliklerini içerir.

### Üretim Alanları

| Alan | Platform |
|---|---|
| `androidBannerId` | Android banner |
| `androidInterstitialId` | Android geçiş reklamı |
| `androidRewardedId` | Android ödüllü reklam |
| `iosBannerId` | iOS banner |
| `iosInterstitialId` | iOS geçiş reklamı |
| `iosRewardedId` | iOS ödüllü reklam |

### Yerleşik Test Kimliği Sabitleri

| Sabit | Tip |
|---|---|
| `drdTestBannerId` | Android test banner |
| `drdTestInterstitialId` | Android test geçiş reklamı |
| `drdTestRewardedId` | Android test ödüllü reklam |
| `iosTestBannerId` | iOS test banner |
| `iosTestInterstitialId` | iOS test geçiş reklamı |
| `iosTestRewardedId` | iOS test ödüllü reklam |

### Yardımcı Metotlar

```csharp
public string GetBannerId(bool useTestAds)
public string GetInterstitialId(bool useTestAds)
public string GetRewardedId(bool useTestAds)
```

> **Bilinen hata:** Android üretim dalları mevcut olmayan `drdBannerId`, `drdInterstitialId`, `drdRewardedId` alanlarına başvurur. Bu, `useTestAds = false` ile Android'de derleme hatasıdır. Düzeltme uygulanana kadar `androidBannerId` vb. alanları doğrudan kullanın.

---

## Sınıf: `AdsItemModel`

```csharp
public class AdsItemModel
```

| Özellik | Tip | Açıklama |
|---|---|---|
| `AdsType` | `AdsType` | Reklam kategorisi |
| `AdsId` | `string` | AdMob reklam birimi kimliği |
| `Duration` | `int` | Banner: yeniden yükleme aralığı **dakika**; Geçiş reklamı: bekleme süresi **saniye** |

`InterstitialAdsItemModel`, `AdsItemModel`'i şununla genişletir:

| Özellik | Tip | Açıklama |
|---|---|---|
| `UseAutoShow` | `bool` | `true` ise bekleme süresini atlar ve hazır olduğunda hemen gösterir |

---

## Sınıf: `AdsBaseView<T, TModel>`

```csharp
public abstract class AdsBaseView<T, TModel> : IDisposable
    where T : class
    where TModel : AdsItemModel
```

Üç reklam tipinin de tabanı.

| Üye | Açıklama |
|---|---|
| `onResponseAdEvent` | `Action<AdsEvent, AdsEventStatus>` — `GoogleMobileAdsController` tarafından bağlanan dahili geri çağrı |
| `AdsItem` | Altta yatan SDK reklam nesnesi |
| `AdsItemModel` | Yapılandırma modeli |
| `IsLoaded` | Reklamın gösterime hazır olup olmadığı |
| `LoadAd()` | Yüklemeyi tetikle |
| `RequestAd()` | SDK'dan taze reklam talep et |
| `ShowAd()` | Reklamı göster |
| `HideAd()` | Yok etmeden gizle (yalnızca banner) |
| `DestroyAd()` | Yok et ve `AdsItem`'ı null yap |
| `AddListeners()` | SDK geri çağrılarına abone ol |
| `RemoveListeners()` | SDK geri çağrılarından aboneliği kaldır |
| `Dispose()` | Temizlik |

---

## Sınıf: `BannerAds`

```csharp
public class BannerAds : AdsBaseView<BannerView, AdsItemModel>
```

| Davranış | Ayrıntı |
|---|---|
| `RequestAd()` | Eski banner'ı yok eder, dipte sabitlenmiş adaptif genişlikte `BannerView` oluşturur |
| `ShowAd()` | İlk gösterimde `LoadAd()` çağırır; `UniTask` yeniden yükleme zamanlayıcısı başlatır |
| Yeniden yükleme zamanlayıcısı | Otomatik yeniden yükleme arasında `AdsItemModel.Duration` **dakika** |
| `HideAd()` | `BannerView.Hide()` |
| `DestroyAd()` | Zamanlayıcıyı iptal eder, `BannerView.Destroy()` çağırır |
| `Dispose()` | Yeniden yükleme `CancellationTokenSource`'unu iptal eder |

---

## Sınıf: `InterstitialAds`

```csharp
public class InterstitialAds : AdsBaseView<InterstitialAd, InterstitialAdsItemModel>
```

| Davranış | Ayrıntı |
|---|---|
| `RequestAd()` | `InterstitialAd.Load(id, request, callback)` |
| `ShowAd(AdsEvent e)` | `e`'yi ilişkilendirir sonra `ShowAd()` çağırır |
| `ShowAd()` | `Time.time - RequestTime >= Duration` veya `UseAutoShow` ise gösterir; aksi hâlde `NotReadyYet` tetikler |
| Yükleme başarısız | `ResponseLoadFail` tetikler, 35 sn sonra yeniden dener |
| `RequestTime` | `float` — başarılı yüklemede ayarlanır; ödüllü sonrası geçiş reklamlarını yeniden etkinleştirmek için `RewardedAds.ResponseClosed` tarafından sıfırlanır |
| Tetiklenen olaylar | `ResponseOpened`, `ResponseClosed`, `ResponseShowFail`, `ResponseLoadFail`, `NotReadyYet`, `NotRequestedYet` |

---

## Sınıf: `RewardedAds`

```csharp
public class RewardedAds : AdsBaseView<RewardedAd, AdsItemModel>
```

| Davranış | Ayrıntı |
|---|---|
| `RequestAd()` | `RewardedAd.Load(id, request, callback)` |
| `ShowAd(AdsEvent e)` | `e`'yi ilişkilendirir sonra `ShowAd()` çağırır |
| Ödül | Ödül geri çağrısında `e.IsEarnedReward = true` yapar, `ResponseEarnReward` tetikler |
| Yükleme başarısız | `ResponseLoadFail`, `onRewardedReadyStatusChanged(false)` tetikler, 35 sn sonra yeniden dener |
| Hazır durumu | `static event Action<bool> onRewardedReadyStatusChanged` — yüklemede `true`, başarısızlıkta `false` tetikler |

> **Bilinen sorun:** `HandleOnAdFullScreenContentOpened`, `AdsEventStatus.ResponseOpened`'ı yalnızca `#if GA_ENABLED` içinde yayımlar. Sembol olmadan açılış olayı **hiçbir zaman tetiklenmez**.

> **Bilinen sorun:** `RemoveListeners()`, `onRewardedReadyStatusChanged = null` yapar; tüm dış aboneleri temizler.

---

## Sınıf: `GoogleMobileAdsConsentController`

```csharp
public class GoogleMobileAdsConsentController
```

UMP (Kullanıcı Mesajlaşma Platformu) onay akışını yönetir.

### Yapıcı

```csharp
public GoogleMobileAdsConsentController(string testDeviceId)
```

### Özellikler

| Özellik | Tip | Açıklama |
|---|---|---|
| `CanRequestAds` | `bool` | `ConsentStatus`, `Obtained` veya `NotRequired` ise `true` |
| `PrivacyBtnInteractable` | `static bool` | `PrivacyOptionsRequirementStatus == Required` ise `true` |

### Metotlar

| Metot | Açıklama |
|---|---|
| `GatherConsent(CancellationToken)` | Asenkron; onay parametrelerini talep eder, gerekirse UMP formunu gösterir; `CustomConsentStatus` döndürür |
| `ShowPrivacyForm(CancellationToken)` | `static async`; gizlilik seçenekleri formunu isteğe bağlı olarak gösterir |

### `CustomConsentStatus`

```csharp
public enum CustomConsentStatus
{
    None, Failed, Unknown, NotRequired, Required, Obtained, ShowForm
}
```

> **Bilinen sorun:** `GatherConsent`, `_initializeResult = CustomConsentStatus.None` atar sonra hemen `WaitUntil(() => _initializeResult == None)` bekler. Bu koşul girişte doğrudur, dolayısıyla asenkron UMP geri çağrısı tetiklenmeden bekleme anında çıkar. Doğru yüklem `!= None` olmalıdır.

### Test modu

UMP hata ayıklama coğrafyasını (`EEA`) ve test cihaz kimliklerini etkinleştirmek için Scripting Define Symbols kısmına `GOOGLEADS_TESTDEVICE` ekleyin.

---

## Sınıf: `GoogleMobileAdsController`

```csharp
public class GoogleMobileAdsController : MonoBehaviour, IDisposable
```

Sahnede bulundurun. Tüm alanları Inspector'da yapılandırın.

### Inspector Alanları

| Alan | Tip | Varsayılan | Açıklama |
|---|---|---|---|
| `_useConsent` | `bool` | `true` | Başlatmadan önce UMP onay akışını çalıştır |
| `_useTestAds` | `bool` | `false` | Google test reklam birimi kimliklerini kullan |
| `_useBanner` | `bool` | `true` | Banner reklamları etkinleştir |
| `_bannerReloadMinutes` | `int` | `5` | Banner otomatik yeniden yükleme aralığı (dakika) |
| `_useInterstitial` | `bool` | `true` | Geçiş reklamlarını etkinleştir |
| `_interstitialDurationSeconds` | `int` | `45` | Geçiş reklamları arasındaki bekleme süresi (saniye) |
| `_interstitialAutoShow` | `bool` | `false` | Bekleme süresi olmadan hemen göster |
| `_useRewarded` | `bool` | `false` | Ödüllü reklamları etkinleştir |
| `_testDeviceIdIOS` | `string` | `""` | iOS test cihazı karma kimliği |
| `_testDeviceIdDRD` | `string` | `""` | Android test cihazı karma kimliği |
| `_googleAdsIds` | `AdsIds` | — | Üretim/test reklam birimi kimlikleri |

### Statik Özellikler

| Özellik | Tip | Açıklama |
|---|---|---|
| `IsInterstitialAdReady` | `static bool` | `_interstitialAds?.IsLoaded ?? false` |
| `IsRewardedAdReady` | `static bool` | `_rewardedAds?.IsLoaded ?? false` |

### Temel Metotlar

| Metot | Açıklama |
|---|---|
| `Initialize(CancellationToken)` | `async UniTask` — gerekirse onay çalıştırır, MobileAds SDK'sını başlatır, ardından etkin reklam tiplerini hazırlar |
| `Dispose()` | Banner, geçiş reklamı, ödüllü reklam ve olay aboneliklerini serbest bırakır |

### Başlatma Akışı

```
Initialize(token)
  ├── _useConsent && !CanRequestAds ise → GatherConsent(token)
  └── InitializeGoogleMobileAds(token)
        MobileAds.Initialize(callback)
        WaitWhile(_initializeResult == None)
  └── PrepareAds()
        ├── PrepareBannerAds()       → BannerAds.RequestAd()
        ├── PrepareInterstitialAds() → InterstitialAds.LoadAd()
        └── PrepareRewardedAds()     → RewardedAds.LoadAd()
```

### DI Entegrasyonu

`GoogleMobileAdsController`, MessagePipe abone/yayımcısını `[Inject] Construct(IObjectResolver resolver)` aracılığıyla alır. Yaşam döngüsü kapsamınıza kaydedin:

```csharp
// LifetimeScope.Configure içinde:
builder.RegisterComponentInHierarchy<GoogleMobileAdsController>();
builder.RegisterMessagePipe();
builder.RegisterMessageBroker<AdsEventStatus, AdsEvent>(options);
```

### Reklam Talep Etme

```csharp
// Herhangi bir abone MessagePipe aracılığıyla reklam tetikleyebilir
[Inject] private readonly IPublisher<AdsEventStatus, AdsEvent> _adsPublisher;

void ShowInterstitial()
{
    _adsPublisher.Publish(AdsEventStatus.Request,
        new AdsEvent(AdsType.Interstitial, from: "LevelComplete"));
}

void ShowRewarded()
{
    _adsPublisher.Publish(AdsEventStatus.Request,
        new AdsEvent(AdsType.Rewarded, from: "ShopScreen", adId: 1));
}
```

---

## Ayrıca Bakınız

- `GA_Initialization` — GameAnalytics başlatma
- MessagePipe — `IPublisher<TKey, TMessage>`, `ISubscriber<TKey, TMessage>`
- VContainer — `[Inject]`, `IObjectResolver`
- Google Mobile Ads Unity SDK belgeleri
