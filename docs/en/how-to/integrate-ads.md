# How to Integrate Google Ads

## Overview

This guide walks through adding `VGameKit.GoogleAds` to a Unity project, configuring ad unit IDs, wiring consent, requesting ads via MessagePipe, and reacting to ad events.

**Prerequisites:** `VGameKit.Runtime` and `VGameKit.GoogleAds.Runtime` modules installed; Google Mobile Ads SDK (`com.google.ads.mobile 10.3.0`) resolved via External Dependency Manager.

> **Known bug (Android non-test):** `AdsIds.GetBannerId`, `GetInterstitialId`, and `GetRewardedId` reference nonexistent field names when `useTestAds = false` on Android. Use test IDs until this is fixed.

---

## Step 1 — Add the GoogleMobileAdsController to your scene

1. Create a new GameObject (e.g. `AdsController`) in your app-level scene.
2. Attach the `GoogleMobileAdsController` component.
3. Configure the Inspector fields:

| Field | Description |
|-------|-------------|
| **Use Consent** | Enable UMP consent gathering (required for EEA/UK) |
| **Use Test Ads** | Use Google's built-in test ad unit IDs during development |
| **Use Banner** | Enable banner ads |
| **Banner Reload Minutes** | How often the banner auto-reloads (in minutes) |
| **Use Interstitial** | Enable interstitial ads |
| **Interstitial Duration Seconds** | Minimum cooldown between interstitials (in seconds) |
| **Interstitial Auto Show** | Show interstitial automatically when ready and cooldown expires |
| **Use Rewarded** | Enable rewarded ads |
| **Test Device ID iOS** | Your iOS test device identifier from Xcode logs |
| **Test Device ID Android** | Your Android test device identifier from logcat |
| **Google Ads Ids** | The `AdsIds` struct containing your production ad unit IDs |

---

## Step 2 — Set up the AdsIds struct

Create a serializable struct holder or fill the `AdsIds` fields directly on the `GoogleMobileAdsController` component:

```
Google Ads Ids:
  Android:
    androidBannerId:       ca-app-pub-XXXX/YYYY (your production banner ID)
    androidInterstitialId: ca-app-pub-XXXX/YYYY
    androidRewardedId:     ca-app-pub-XXXX/YYYY
  iOS:
    iosBannerId:           ca-app-pub-XXXX/YYYY
    iosInterstitialId:     ca-app-pub-XXXX/YYYY
    iosRewardedId:         ca-app-pub-XXXX/YYYY
```

Leave **Use Test Ads** checked during development; Google's hardcoded test IDs are used automatically.

---

## Step 3 — Register MessagePipe keyed subscriptions in the LifetimeScope

`GoogleMobileAdsController` uses `ISubscriber<AdsEventStatus, AdsEvent>` and `IPublisher<AdsEventStatus, AdsEvent>`. These must be registered in the app-level `AbsMainLifetimeScope`:

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.GoogleAds.Runtime.Events;

public class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder); // registers MessagePipe globally

        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<AdsEventStatus, AdsEvent>(options);

        // Register the controller as a component already in the scene
        builder.RegisterComponentInHierarchy<GoogleMobileAdsController>();
    }
}
```

> `AbsMainLifetimeScope.Configure` calls `builder.RegisterMessagePipe()` internally. Do not call it twice; use the returned `options` to register additional brokers.

---

## Step 4 — Initialise ads during app startup

Call `GoogleMobileAdsController.Initialize` inside your `AbsAppManager.InitializeGame`:

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

`Initialize` handles consent gathering (if enabled), SDK init, and `PrepareAds()` in the correct order.

---

## Step 5 — Request an ad

Publish an `AdsEvent` via `IPublisher<AdsEventStatus, AdsEvent>` with status `AdsEventStatus.Request`:

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

The controller's internal subscriber dispatches the request to the correct ad type.

---

## Step 6 — React to ad events

Subscribe to `ISubscriber<AdsEventStatus, AdsEvent>` in any presenter that needs to know about ad lifecycle:

```csharp
public override void Subscriptions()
{
    _adsSubscriber
        .Subscribe(AdsEventStatus.ResponseOpened, e =>
            GKLog.Log(LogState.Ads, $"Ad opened: {e.AdsType}"))
        .AddTo(_bagBuilder);

    _adsSubscriber
        .Subscribe(AdsEventStatus.ResponseClosed, e =>
            GKLog.Log(LogState.Ads, $"Ad closed: {e.AdsType}"))
        .AddTo(_bagBuilder);

    _adsSubscriber
        .Subscribe(AdsEventStatus.ResponseRewarded, e =>
        {
            GKLog.Log(LogState.Ads, "Rewarded earned");
            e.OnComplete?.Invoke();
        })
        .AddTo(_bagBuilder);
}
```

---

## Ad event status quick reference

| Status | Meaning |
|--------|---------|
| `Request` | Published by your code to request an ad show |
| `ResponseOpened` | Ad started displaying |
| `ResponseClosed` | Ad dismissed by the user |
| `ResponseRewarded` | Rewarded ad grant confirmed |
| `ResponseFailed` | Ad failed to load or show |

---

## Checking ad readiness

```csharp
if (GoogleMobileAdsController.IsInterstitialAdReady)
{
    // safe to request an interstitial
}

if (GoogleMobileAdsController.IsRewardedAdReady)
{
    // safe to request a rewarded ad
}
```

These are static properties wrapping the underlying loaded state.

---

## See also

- [Google Ads reference](../reference/api/google-ads.md)
- [App Manager reference](../reference/api/app-manager.md)
- [Lifetime Scopes reference](../reference/api/lifetime-scopes.md)
