# GameAnalytics Referansı

**Ad Alanı:** `VGameKit.GA.Runtime`
**Derleme:** `VGameKit.GA.Runtime`
**Dosya:** `Assets/VGameKit.GA/Runtime/GA_Initialization.cs`

---

## Genel Bakış

`VGameKit.GA` modülü, [GameAnalytics Unity SDK'sı](https://gameanalytics.com/docs/unity) etrafında ince bir başlatma sarmalayıcısı sunar. Şunları gerçekleştirir:
- Platform duyarlı SDK başlatma (iOS ATT isteği karşı doğrudan başlatma)
- iOS'ta Uygulama Takip Şeffaflığı (ATT) geri çağrıları
- Başlatma tamamlanana kadar UniTask ile asenkron bekleme

Modül, tekil olay izleme çağrılarını sarmaz — tasarım, ilerleme ve hata olayları için `GameAnalytics.*` metotlarını doğrudan kullanın.

---

## Sınıf: `GA_Initialization`

```csharp
public class GA_Initialization : MonoBehaviour, IGameAnalyticsATTListener
```

Bu `MonoBehaviour`'u uygulamanızın önyükleme `GameObject`'ine ekleyin (örn. `AbsMainLifetimeScope`'unuzu içeren sahne).

### Özellikler

| Özellik | Tip | Açıklama |
|---|---|---|
| `IsInitialized` | `bool` | SDK, `onInitialize` geri çağrısını başarı sonucu ile tetiklediğinde `true` olur |

---

### Metot: `Initialize`

```csharp
public async UniTask Initialize()
```

GameAnalytics SDK'sını başlatır.

**Davranış:**
1. `GameAnalytics.onInitialize`'a abone olur.
2. **iOS'ta**: `GameAnalytics.RequestTrackingAuthorization(this)` çağırır — ATT istemi gösterilir; ardından uygun `IGameAnalyticsATTListener` geri çağrısı `GameAnalytics.Initialize()` çağırır.
3. **Diğer tüm platformlarda**: `GameAnalytics.Initialize()` doğrudan çağrılır.
4. `await UniTask.WaitUntil(() => IsInitialized)` — SDK başarı veya başarısızlık bildirene kadar askıya alınır.

> Herhangi bir `GameAnalytics.*` olay metodunu çağırmadan önce bu metodu bekleyin.

---

### ATT Geri Çağrıları (`IGameAnalyticsATTListener`)

Dört geri çağrının tümü koşulsuz olarak `GameAnalytics.Initialize()` çağırır. Takip yetkilendirme düzeyi SDK tarafından dahili olarak ele alınır.

| Metot | ATT Durumu |
|---|---|
| `GameAnalyticsATTListenerNotDetermined()` | Kullanıcı henüz yanıt vermedi |
| `GameAnalyticsATTListenerRestricted()` | Takip politika tarafından kısıtlandı |
| `GameAnalyticsATTListenerDenied()` | Kullanıcı takibi reddetti |
| `GameAnalyticsATTListenerAuthorized()` | Kullanıcı takibe izin verdi |

---

### Özel Geri Çağrı: `OnGameAnalyticsInitialized`

```csharp
private void OnGameAnalyticsInitialized(object sender, bool e)
```

`GameAnalytics.onInitialize` olayı tarafından çağrılır:
- `IsInitialized = GameAnalytics.Initialized` atar (`bool e` başarı bayrağından bağımsız — SDK'nın kendi `Initialized` özelliği yetkili kaynaktır).
- Yinelenen işlemeyi önlemek için `onInitialize`'dan aboneliği kaldırır.

---

## Entegrasyon Kalıbı

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using VGameKit.GA.Runtime;
using GameAnalyticsSDK;

public class AppBootstrapper : MonoBehaviour
{
    [SerializeField] private GA_Initialization _gaInit;

    private async UniTaskVoid Start()
    {
        await _gaInit.Initialize();

        if (_gaInit.IsInitialized)
        {
            GameAnalytics.NewDesignEvent("App:Started");
        }
        else
        {
            Debug.LogWarning("[VGameKit] GameAnalytics başlatılamadı.");
        }
    }
}
```

---

## Derleme Sembolü: `GA_ENABLED`

Diğer VGameKit modülleri (`InterstitialAds`, `RewardedAds`) GA tasarım olaylarını `#if GA_ENABLED` altında koşullu olarak gönderir. GA bir derlemede aktifken **Player Settings → Scripting Define Symbols** kısmında bu sembolü tanımlayın.

Sembol olmadan aşağıdaki olaylar sessizce atlanır:
- `InterstitialAd:Showed`, `InterstitialAd:FailToLoad`, `InterstitialAd:FailToShow`
- `RewardedAd:Showed`, `RewardedAd:FailToLoad`, `RewardedAd:FailToShow`
- `RewardedAd:Showed:<from>`, `RewardedAd:FailToShow:<from>`

> **Bilinen sorun:** `RewardedAds.HandleOnAdFullScreenContentOpened`, `AdsEventStatus.ResponseOpened`'ı yalnızca `#if GA_ENABLED` içinde yayımlar. Sembol olmadan açılış olayı **hiçbir zaman tetiklenmez**. Bu bir hatadır — yayımlama çağrısı `#if` bloğunun dışında olmalıdır.

---

## Ayrıca Bakınız

- `GoogleMobileAdsController` — Google Ads başlatma (ayrı modül)
- GameAnalytics Unity SDK belgeleri
- `GKLog` — VGameKit kayıt sistemi
