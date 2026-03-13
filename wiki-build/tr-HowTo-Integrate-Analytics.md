# GameAnalytics Nasıl Entegre Edilir

## Genel Bakış

Bu rehber; bir Unity projesine `VGameKit.GA` eklemeyi, GameAnalytics SDK'yı yapılandırmayı, başlangıçta başlatmayı ve event göndermeyi kapsar.

**Ön Koşullar:** `VGameKit.Runtime` ve `VGameKit.GA.Runtime` modülleri kurulu; GameAnalytics SDK (`com.gameanalytics.sdk 7.10.5`) OpenUPM aracılığıyla çözümlenmiş.

---

## Adım 1 — GameAnalytics asset'ini yapılandırın

1. Unity Editor'da **Window > GameAnalytics > Setup** yoluna gidin.
2. Giriş yapın veya hesap oluşturun, ardından yeni bir oyun oluşturun ya da mevcut birini seçin.
3. SDK, `Assets/Resources/GameAnalytics/Settings.asset` dosyasını otomatik olarak oluşturur.
4. Her hedef platform (Android, iOS) için **Game Key** ve **Secret Key** alanlarının dolu olduğunu doğrulayın.

---

## Adım 2 — GA_Initialization'ı sahnenize ekleyin

1. Uygulama düzeyindeki sahnenizde yeni bir GameObject (örn. `GAInitializer`) oluşturun.
2. `GA_Initialization` bileşenini ekleyin.
3. iOS'ta `GA_Initialization`, başlatmadan önce otomatik olarak `GameAnalytics.RequestTrackingAuthorization` çağırır. Diğer tüm platformlarda doğrudan `GameAnalytics.Initialize()` çağırır.

```
Sahne hiyerarşisi:
  AppRoot (GameObject)
    └── GAInitializer  [GA_Initialization]
```

---

## Adım 3 — GA_Initialization'ı LifetimeScope'a kaydedin

```csharp
using VContainer;
using VGameKit.Runtime.Core;

public class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.RegisterComponentInHierarchy<GA_Initialization>();
    }
}
```

---

## Adım 4 — Uygulama başlangıcında başlatın

`GA_Initialization`'ı `AbsAppManager` alt sınıfınıza inject edin ve bekleyin:

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.App;
using VGameKit.GA.Runtime;

public class AppManager : AbsAppManager
{
    [Inject] private readonly GA_Initialization _gaInit;

    protected override async UniTask InitializeGame(CancellationToken token)
    {
        await _gaInit.Initialize();
        // GA artık başlatıldı; IsInitialized == true
    }
}
```

`GA_Initialization.Initialize()`, dönmeden önce `GameAnalytics.Initialized`'ın `true` olmasını bekler.

---

## Adım 5 — Event gönderin

`IsInitialized` `true` olduktan sonra doğrudan GameAnalytics SDK API'sini kullanın:

```csharp
using GameAnalyticsSDK;

// Design event (özel KPI'lar):
GameAnalytics.NewDesignEvent("Level:Start:1");
GameAnalytics.NewDesignEvent("Enemy:Killed", 1f);

// Progression event:
GameAnalytics.NewProgressionEvent(
    GAProgressionStatus.Start, "World1", "Level1");

GameAnalytics.NewProgressionEvent(
    GAProgressionStatus.Complete, "World1", "Level1");

// Resource event (sanal para birimi):
GameAnalytics.NewResourceEvent(
    GAResourceFlowType.Source, "Gold", 100f, "LevelComplete", "Reward");

// Error event:
GameAnalytics.NewErrorEvent(GAErrorSeverity.Error, "ShopPresenter'da NullRef");
```

---

## Adım 6 — Event'leri GA_ENABLED sembolü arkasında koruyun (isteğe bağlı)

`com.gameanalytics.sdk` mevcut olduğunda `GA_ENABLED` tanımı `VGameKit.GoogleAds.Runtime.asmdef` tarafından otomatik olarak ayarlanır. SDK yokken analitik çağrılarının derlenmemesini istiyorsanız:

```csharp
#if GA_ENABLED
GameAnalytics.NewDesignEvent("Player:LevelUp");
#endif
```

---

## iOS App Tracking Transparency

iOS'ta `GA_Initialization`, SDK'yı başlatmadan önce `RequestTrackingAuthorization` çağırır. Dört ATT geri çağrım yöntemi (`Authorized`, `Denied`, `Restricted`, `NotDetermined`) hepsi `GameAnalytics.Initialize()` çağırır; dolayısıyla SDK, kullanıcının tercihinden bağımsız olarak her zaman başlar.

ATT uyumu için tarafınızda ek kod gerekmez.

---

## Editor'da başlatmayı doğrulama

Editor'da `GA_Initialization.Initialize()`, `GameAnalytics.Initialized` `true` olduğunda tamamlanır. Bunu bekledikten sonra `_gaInit.IsInitialized`'ı kontrol ederek doğrulayabilirsiniz.

Analytics event'lerini Editor'da test etmek için GameAnalytics **Validator** penceresini kullanın (**Window > GameAnalytics > Validator**).

---

## Ayrıca Bkz.

- [GameAnalytics referansı](tr-Reference-Game-Analytics)
- [App Manager referansı](tr-Reference-App-Manager)
- [Modül kurulumu rehberi](tr-HowTo-Install-Modules)
