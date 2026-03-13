# Google Mobile Ads Reference

**Namespace:** `VGameKit.GoogleAds.Runtime`
**Assembly:** `VGameKit.GoogleAds.Runtime`
**Files:** `Assets/VGameKit.GoogleAds/Runtime/`

---

## Overview

The `VGameKit.GoogleAds` module wraps the [Google Mobile Ads Unity SDK](https://developers.google.com/admob/unity) with:
- A central controller (`GoogleMobileAdsController`) handling consent, initialization, and ad lifecycle
- Individual ad type managers (`BannerAds`, `InterstitialAds`, `RewardedAds`)
- A UMP-based consent controller (`GoogleMobileAdsConsentController`)
- Platform-aware ad unit ID resolution (`AdsIds`)
- A MessagePipe-based event bus (`AdsEvent` / `AdsEventStatus`)
- A privacy button helper (`VGameKitPrivacyButton`)

---

## Type Overview

| Type | Kind | Description |
|---|---|---|
| `GoogleMobileAdsController` | `MonoBehaviour` | Central coordinator; configure in Inspector |
| `GoogleMobileAdsConsentController` | class | UMP consent flow |
| `AdsIds` | struct | Ad unit IDs per platform |
| `AdsBaseView<T, TModel>` | abstract class | Base for all ad views |
| `AdsItemModel` | class | Base ad configuration data |
| `InterstitialAdsItemModel` | class | Extends `AdsItemModel` with `UseAutoShow` |
| `BannerAds` | class | Banner lifecycle |
| `InterstitialAds` | class | Interstitial lifecycle |
| `RewardedAds` | class | Rewarded lifecycle |
| `AdsEvent` | class | MessagePipe event payload |
| `AdsEventStatus` | enum | Event type identifier |
| `AdsType` | enum | `Banner`, `Interstitial`, `Rewarded` |
| `CustomInitializeStatus` | enum | SDK init result |
| `CustomConsentStatus` | enum | UMP consent result |
| `VGameKitPrivacyButton` | `MonoBehaviour` | Button that shows the privacy form |

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
    Request,             // Caller requests that an ad be shown
    ResponseOpened,      // Ad is now visible to the user
    ResponseClosed,      // User dismissed the ad
    ResponseEarnReward,  // User earned a reward (rewarded ads only)
    ResponseShowFail,    // Ad failed to display
    ResponseLoadFail,    // Ad failed to load
    NotReadyYet,         // Ad is loaded but cooldown not elapsed
    NotRequestedYet      // Ad has not been loaded at all
}
```

---

## Class: `AdsEvent`

```csharp
public class AdsEvent
```

MessagePipe event payload used for both requesting ads and receiving responses.

| Property | Type | Description |
|---|---|---|
| `AdsType` | `AdsType` | Which ad type this event relates to |
| `From` | `string` | Caller tag — used for analytics event naming |
| `AdId` | `int` | Optional caller-assigned identifier; defaults to `-1` |
| `IsEarnedReward` | `bool` | Set to `true` when the user earns a reward |

```csharp
// Publish to request an ad
_adsEventPublisher.Publish(AdsEventStatus.Request,
    new AdsEvent(AdsType.Rewarded, from: "ShopScreen", adId: 42));

// Subscribe to responses
_adsEventSubscriber.Subscribe(AdsEventStatus.ResponseEarnReward, e =>
{
    if (e.IsEarnedReward) GrantReward(e.AdId);
}).AddTo(_bagBuilder);
```

---

## Class: `AdsIds`

```csharp
[Serializable]
public struct AdsIds
```

Assign in the Inspector on `GoogleMobileAdsController`. Contains production and built-in Google test IDs for both platforms.

### Production Fields

| Field | Platform |
|---|---|
| `androidBannerId` | Android banner |
| `androidInterstitialId` | Android interstitial |
| `androidRewardedId` | Android rewarded |
| `iosBannerId` | iOS banner |
| `iosInterstitialId` | iOS interstitial |
| `iosRewardedId` | iOS rewarded |

### Built-in Test ID Constants

| Constant | Type |
|---|---|
| `drdTestBannerId` | Android test banner |
| `drdTestInterstitialId` | Android test interstitial |
| `drdTestRewardedId` | Android test rewarded |
| `iosTestBannerId` | iOS test banner |
| `iosTestInterstitialId` | iOS test interstitial |
| `iosTestRewardedId` | iOS test rewarded |

### Helper Methods

```csharp
public string GetBannerId(bool useTestAds)
public string GetInterstitialId(bool useTestAds)
public string GetRewardedId(bool useTestAds)
```

> **Known bug:** The Android production branches reference `drdBannerId`, `drdInterstitialId`, `drdRewardedId` — fields that do not exist. This is a compile error on Android with `useTestAds = false`. Use `androidBannerId` etc. directly until the fix is applied.

---

## Class: `AdsItemModel`

```csharp
public class AdsItemModel
```

| Property | Type | Description |
|---|---|---|
| `AdsType` | `AdsType` | Ad category |
| `AdsId` | `string` | AdMob ad unit ID |
| `Duration` | `int` | Banner: reload interval in **minutes**; Interstitial: cooldown in **seconds** |

`InterstitialAdsItemModel` extends `AdsItemModel` with:

| Property | Type | Description |
|---|---|---|
| `UseAutoShow` | `bool` | If `true`, bypasses the cooldown check and shows immediately when ready |

---

## Class: `AdsBaseView<T, TModel>`

```csharp
public abstract class AdsBaseView<T, TModel> : IDisposable
    where T : class
    where TModel : AdsItemModel
```

Base for all three ad types.

| Member | Description |
|---|---|
| `onResponseAdEvent` | `Action<AdsEvent, AdsEventStatus>` — internal callback wired by `GoogleMobileAdsController` |
| `AdsItem` | The underlying SDK ad object |
| `AdsItemModel` | The configuration model |
| `IsLoaded` | Whether the ad is ready to show |
| `LoadAd()` | Trigger load |
| `RequestAd()` | Request a fresh ad from the SDK |
| `ShowAd()` | Display the ad |
| `HideAd()` | Hide without destroying (banner only) |
| `DestroyAd()` | Destroy and null `AdsItem` |
| `AddListeners()` | Subscribe to SDK callbacks |
| `RemoveListeners()` | Unsubscribe from SDK callbacks |
| `Dispose()` | Cleanup |

---

## Class: `BannerAds`

```csharp
public class BannerAds : AdsBaseView<BannerView, AdsItemModel>
```

| Behaviour | Detail |
|---|---|
| `RequestAd()` | Destroys old banner, creates adaptive-width `BannerView` anchored at bottom |
| `ShowAd()` | Calls `LoadAd()` on first show; starts a `UniTask` reload timer |
| Reload timer | `AdsItemModel.Duration` **minutes** between auto-reloads |
| `HideAd()` | `BannerView.Hide()` |
| `DestroyAd()` | Cancels timer, calls `BannerView.Destroy()` |
| `Dispose()` | Cancels reload `CancellationTokenSource` |

---

## Class: `InterstitialAds`

```csharp
public class InterstitialAds : AdsBaseView<InterstitialAd, InterstitialAdsItemModel>
```

| Behaviour | Detail |
|---|---|
| `RequestAd()` | `InterstitialAd.Load(id, request, callback)` |
| `ShowAd(AdsEvent e)` | Associates `e` then calls `ShowAd()` |
| `ShowAd()` | Shows if `Time.time - RequestTime >= Duration` OR `UseAutoShow`; fires `NotReadyYet` otherwise |
| Load fail | Fires `ResponseLoadFail`, retries after 35 s |
| `RequestTime` | `float` — set on load success; reset by `RewardedAds.ResponseClosed` to re-enable interstitials after rewarded |
| Events fired | `ResponseOpened`, `ResponseClosed`, `ResponseShowFail`, `ResponseLoadFail`, `NotReadyYet`, `NotRequestedYet` |

---

## Class: `RewardedAds`

```csharp
public class RewardedAds : AdsBaseView<RewardedAd, AdsItemModel>
```

| Behaviour | Detail |
|---|---|
| `RequestAd()` | `RewardedAd.Load(id, request, callback)` |
| `ShowAd(AdsEvent e)` | Associates `e` then calls `ShowAd()` |
| Reward | Sets `e.IsEarnedReward = true`, fires `ResponseEarnReward` in the reward callback |
| Load fail | Fires `ResponseLoadFail`, `onRewardedReadyStatusChanged(false)`, retries after 35 s |
| Ready status | `static event Action<bool> onRewardedReadyStatusChanged` — fires `true` on load, `false` on fail |

> **Known issue:** `HandleOnAdFullScreenContentOpened` only publishes `AdsEventStatus.ResponseOpened` inside `#if GA_ENABLED`. Without this symbol, the opened event is **never fired**.

> **Known issue:** `RemoveListeners()` sets `onRewardedReadyStatusChanged = null`, clearing all external subscribers.

---

## Class: `GoogleMobileAdsConsentController`

```csharp
public class GoogleMobileAdsConsentController
```

Manages UMP (User Messaging Platform) consent flow.

### Constructor

```csharp
public GoogleMobileAdsConsentController(string testDeviceId)
```

### Properties

| Property | Type | Description |
|---|---|---|
| `CanRequestAds` | `bool` | `true` if `ConsentStatus` is `Obtained` or `NotRequired` |
| `PrivacyBtnInteractable` | `static bool` | `true` when `PrivacyOptionsRequirementStatus == Required` |

### Methods

| Method | Description |
|---|---|
| `GatherConsent(CancellationToken)` | Async; requests consent parameters, shows UMP form if needed; returns `CustomConsentStatus` |
| `ShowPrivacyForm(CancellationToken)` | `static async`; shows the privacy options form on demand |

### `CustomConsentStatus`

```csharp
public enum CustomConsentStatus
{
    None, Failed, Unknown, NotRequired, Required, Obtained, ShowForm
}
```

> **Known issue:** `GatherConsent` sets `_initializeResult = CustomConsentStatus.None` then immediately `WaitUntil(() => _initializeResult == None)`. This condition is true on entry, so the wait exits instantly before the async UMP callback fires. The correct predicate should be `!= None`.

### Test mode

Define `GOOGLEADS_TESTDEVICE` in Scripting Define Symbols to enable UMP debug geography (`EEA`) and test device IDs.

---

## Class: `GoogleMobileAdsController`

```csharp
public class GoogleMobileAdsController : MonoBehaviour, IDisposable
```

Place in the scene. Configure all fields in the Inspector.

### Inspector Fields

| Field | Type | Default | Description |
|---|---|---|---|
| `_useConsent` | `bool` | `true` | Run UMP consent flow before initializing |
| `_useTestAds` | `bool` | `false` | Use Google test ad unit IDs |
| `_useBanner` | `bool` | `true` | Enable banner ads |
| `_bannerReloadMinutes` | `int` | `5` | Banner auto-reload interval (minutes) |
| `_useInterstitial` | `bool` | `true` | Enable interstitial ads |
| `_interstitialDurationSeconds` | `int` | `45` | Cooldown between interstitials (seconds) |
| `_interstitialAutoShow` | `bool` | `false` | Show immediately without cooldown |
| `_useRewarded` | `bool` | `false` | Enable rewarded ads |
| `_testDeviceIdIOS` | `string` | `""` | iOS test device hashed ID |
| `_testDeviceIdDRD` | `string` | `""` | Android test device hashed ID |
| `_googleAdsIds` | `AdsIds` | — | Production/test ad unit IDs |

### Static Properties

| Property | Type | Description |
|---|---|---|
| `IsInterstitialAdReady` | `static bool` | `_interstitialAds?.IsLoaded ?? false` |
| `IsRewardedAdReady` | `static bool` | `_rewardedAds?.IsLoaded ?? false` |

### Key Methods

| Method | Description |
|---|---|
| `Initialize(CancellationToken)` | `async UniTask` — runs consent if needed, initializes MobileAds SDK, then prepares all enabled ad types |
| `Dispose()` | Disposes banner, interstitial, rewarded, and event subscriptions |

### Initialization Flow

```
Initialize(token)
  ├── if _useConsent && !CanRequestAds → GatherConsent(token)
  └── InitializeGoogleMobileAds(token)
        MobileAds.Initialize(callback)
        WaitWhile(_initializeResult == None)
  └── PrepareAds()
        ├── PrepareBannerAds()     → BannerAds.RequestAd()
        ├── PrepareInterstitialAds() → InterstitialAds.LoadAd()
        └── PrepareRewardedAds()   → RewardedAds.LoadAd()
```

### DI Integration

`GoogleMobileAdsController` receives its MessagePipe subscriber/publisher via `[Inject] Construct(IObjectResolver resolver)`. Register it in your lifetime scope:

```csharp
// In LifetimeScope.Configure:
builder.RegisterComponentInHierarchy<GoogleMobileAdsController>();
builder.RegisterMessagePipe();
builder.RegisterMessageBroker<AdsEventStatus, AdsEvent>(options);
```

### Requesting an Ad

```csharp
// Any subscriber can trigger ads via MessagePipe
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

## See Also

- `GA_Initialization` — GameAnalytics initialization
- MessagePipe — `IPublisher<TKey, TMessage>`, `ISubscriber<TKey, TMessage>`
- VContainer — `[Inject]`, `IObjectResolver`
- Google Mobile Ads Unity SDK docs
