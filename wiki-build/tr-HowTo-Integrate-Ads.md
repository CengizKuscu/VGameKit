# Google Ads Nasıl Entegre Edilir

## Genel Bakış

Bu rehber; bir Unity projesine `VGameKit.GoogleAds` eklemeyi, reklam birimi kimliklerini yapılandırmayı, onay akışını bağlamayı, MessagePipe aracılığıyla reklam talep etmeyi ve reklam olaylarına tepki vermeyi adım adım açıklar.

**Ön Koşullar:** `VGameKit.Runtime` ve `VGameKit.GoogleAds.Runtime` modülleri kurulu; Google Mobile Ads SDK (`com.google.ads.mobile 10.3.0`) External Dependency Manager aracılığıyla çözümlenmiş.

> **Bilinen hata (Android test dışı):** `AdsIds.GetBannerId`, `GetInterstitialId` ve `GetRewardedId`, Android'de `useTestAds = false` iken var olmayan alan adlarına başvurur. Bu düzeltilinceye kadar test kimliklerini kullanın.

---

## Adım 1 — GoogleMobileAdsController'ı sahnenize ekleyin

1. Uygulama düzeyindeki sahnenizde yeni bir GameObject (örn. `AdsController`) oluşturun.
2. `GoogleMobileAdsController` bileşenini ekleyin.
3. Inspector alanlarını yapılandırın:

| Alan | Açıklama |
|------|----------|
| **Use Consent** | UMP onay toplama işlemini etkinleştirir (AEA/İngiltere için gerekli) |
| **Use Test Ads** | Geliştirme sırasında Google'ın dahili test reklam birimi kimliklerini kullanır |
| **Use Banner** | Banner reklamları etkinleştirir |
| **Banner Reload Minutes** | Banner'ın otomatik yeniden yükleme sıklığı (dakika cinsinden) |
| **Use Interstitial** | Geçiş reklamları etkinleştirir |
| **Interstitial Duration Seconds** | Geçiş reklamları arasındaki minimum bekleme süresi (saniye cinsinden) |
| **Interstitial Auto Show** | Hazır olduğunda ve bekleme süresi dolduğunda geçiş reklamını otomatik gösterir |
| **Use Rewarded** | Ödüllü reklamları etkinleştirir |
| **Test Device ID iOS** | Xcode loglarından alınan iOS test cihazı tanımlayıcınız |
| **Test Device ID Android** | Logcat'ten alınan Android test cihazı tanımlayıcınız |
| **Google Ads Ids** | Üretim reklam birimi kimliklerinizi içeren `AdsIds` struct'ı |

---

## Adım 2 — AdsIds struct'ını ayarlayın

Seri hale getirilebilir bir struct tutucu oluşturun veya `GoogleMobileAdsController` bileşeninde `AdsIds` alanlarını doğrudan doldurun:

```
Google Ads Ids:
  Android:
    androidBannerId:       ca-app-pub-XXXX/YYYY (üretim banner kimliğiniz)
    androidInterstitialId: ca-app-pub-XXXX/YYYY
    androidRewardedId:     ca-app-pub-XXXX/YYYY
  iOS:
    iosBannerId:           ca-app-pub-XXXX/YYYY
    iosInterstitialId:     ca-app-pub-XXXX/YYYY
    iosRewardedId:         ca-app-pub-XXXX/YYYY
```

Geliştirme sırasında **Use Test Ads** işaretini bırakın; Google'ın sabit kodlu test kimlikleri otomatik olarak kullanılır.

---

## Adım 3 — LifetimeScope'ta MessagePipe keyed subscription'larını kaydedin

`GoogleMobileAdsController`, `ISubscriber<AdsEventStatus, AdsEvent>` ve `IPublisher<AdsEventStatus, AdsEvent>` kullanır. Bunlar uygulama düzeyindeki `AbsMainLifetimeScope`'ta kayıtlı olmalıdır:

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.GoogleAds.Runtime.Events;

public class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder); // MessagePipe'ı global olarak kaydeder

        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<AdsEventStatus, AdsEvent>(options);

        // Controller'ı sahnede zaten bulunan bir bileşen olarak kaydet
        builder.RegisterComponentInHierarchy<GoogleMobileAdsController>();
    }
}
```

> `AbsMainLifetimeScope.Configure` dahili olarak `builder.RegisterMessagePipe()` çağırır. İki kez çağırmayın; ek broker'ları kaydetmek için döndürülen `options`'ı kullanın.

---

## Adım 4 — Uygulama başlangıcında reklamları başlatın

`AbsAppManager.InitializeGame` içinde `GoogleMobileAdsController.Initialize`'ı çağırın:

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.App;
using VGameKit.GoogleAds.Runtime;

public class AppManager : AbsAppManager
{
    [Inject] private readonly GoogleMobileAdsController _adsController;

    protected override async UniTask InitializeGame(CancellationToken token)
    {
        await _adsController.Initialize(token);
    }
}
```

`Initialize`, onay toplama (etkinleştirilmişse), SDK başlatma ve `PrepareAds()` işlemlerini doğru sırada yönetir.

---

## Adım 5 — Reklam talep edin

`IPublisher<AdsEventStatus, AdsEvent>` aracılığıyla `AdsEventStatus.Request` durumlu bir `AdsEvent` yayınlayın:

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.GoogleAds.Runtime;
using VGameKit.GoogleAds.Runtime.Events;

public class GameplayPresenter : SubscribableConcrete
{
    [Inject] private readonly IPublisher<AdsEventStatus, AdsEvent> _adsPublisher;

    public void ShowInterstitial()
    {
        _adsPublisher.Publish(AdsEventStatus.Request, new AdsEvent
        {
            AdsType = AdsType.Interstitial
        });
    }

    public void ShowRewarded(System.Action onRewarded)
    {
        _adsPublisher.Publish(AdsEventStatus.Request, new AdsEvent
        {
            AdsType = AdsType.Rewarded,
            OnComplete = onRewarded
        });
    }
}
```

Controller'ın dahili subscriber'ı isteği doğru reklam türüne iletir.

---

## Adım 6 — Reklam olaylarına tepki verin

Reklam yaşam döngüsünü bilmesi gereken herhangi bir presenter'da `ISubscriber<AdsEventStatus, AdsEvent>`'e abone olun:

```csharp
public override void Subscriptions()
{
    _adsSubscriber
        .Subscribe(AdsEventStatus.ResponseOpened, e =>
            GKLog.Log(LogState.Ads, $"Reklam açıldı: {e.AdsType}"))
        .AddTo(_bagBuilder);

    _adsSubscriber
        .Subscribe(AdsEventStatus.ResponseClosed, e =>
            GKLog.Log(LogState.Ads, $"Reklam kapandı: {e.AdsType}"))
        .AddTo(_bagBuilder);

    _adsSubscriber
        .Subscribe(AdsEventStatus.ResponseRewarded, e =>
        {
            GKLog.Log(LogState.Ads, "Ödül kazanıldı");
            e.OnComplete?.Invoke();
        })
        .AddTo(_bagBuilder);
}
```

---

## Reklam olay durumu hızlı referansı

| Durum | Anlamı |
|-------|--------|
| `Request` | Reklam gösterimi talep etmek için kodunuz tarafından yayınlanır |
| `ResponseOpened` | Reklam gösterilmeye başladı |
| `ResponseClosed` | Reklam kullanıcı tarafından kapatıldı |
| `ResponseRewarded` | Ödüllü reklam ödülü onaylandı |
| `ResponseFailed` | Reklam yüklenemedi veya gösterilemedi |

---

## Reklam hazırlığını kontrol etme

```csharp
if (GoogleMobileAdsController.IsInterstitialAdReady)
{
    // geçiş reklamı talep etmek güvenli
}

if (GoogleMobileAdsController.IsRewardedAdReady)
{
    // ödüllü reklam talep etmek güvenli
}
```

Bunlar, temel yüklenmiş durumu saran statik özelliklerdir.

---

## Ayrıca Bkz.

- [Google Ads referansı](tr-Reference-Google-Ads)
- [App Manager referansı](tr-Reference-App-Manager)
- [Lifetime Scopes referansı](tr-Reference-Lifetime-Scopes)
