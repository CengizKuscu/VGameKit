# GameAnalytics Reference

**Namespace:** `VGameKit.GA.Runtime`
**Assembly:** `VGameKit.GA.Runtime`
**File:** `Assets/VGameKit.GA/Runtime/GA_Initialization.cs`

---

## Overview

The `VGameKit.GA` module provides a thin initialization wrapper around the [GameAnalytics Unity SDK](https://gameanalytics.com/docs/unity). It handles:
- Platform-aware SDK startup (iOS ATT request vs. direct initialize)
- App Tracking Transparency (ATT) callbacks on iOS
- Async await via UniTask until initialization completes

The module does **not** wrap individual event tracking calls — use `GameAnalytics.*` methods directly for design, progression, and error events.

---

## Class: `GA_Initialization`

```csharp
public class GA_Initialization : MonoBehaviour, IGameAnalyticsATTListener
```

Add this `MonoBehaviour` to your app's bootstrapping `GameObject` (e.g., the scene that contains your `AbsMainLifetimeScope`).

### Properties

| Property | Type | Description |
|---|---|---|
| `IsInitialized` | `bool` | `true` once the SDK fires its `onInitialize` callback with a success result |

---

### Method: `Initialize`

```csharp
public async UniTask Initialize()
```

Bootstraps the GameAnalytics SDK.

**Behaviour:**
1. Subscribes to `GameAnalytics.onInitialize`.
2. On **iOS**: calls `GameAnalytics.RequestTrackingAuthorization(this)` — the ATT prompt is shown; the appropriate `IGameAnalyticsATTListener` callback then calls `GameAnalytics.Initialize()`.
3. On **all other platforms**: calls `GameAnalytics.Initialize()` immediately.
4. `await UniTask.WaitUntil(() => IsInitialized)` — suspends until the SDK reports success or failure.

> Await this method before calling any `GameAnalytics.*` event methods.

---

### ATT Callbacks (`IGameAnalyticsATTListener`)

All four callbacks call `GameAnalytics.Initialize()` unconditionally. Tracking authorization level is handled internally by the SDK.

| Method | ATT Status |
|---|---|
| `GameAnalyticsATTListenerNotDetermined()` | User has not yet responded |
| `GameAnalyticsATTListenerRestricted()` | Tracking is restricted by policy |
| `GameAnalyticsATTListenerDenied()` | User denied tracking |
| `GameAnalyticsATTListenerAuthorized()` | User authorized tracking |

---

### Private Callback: `OnGameAnalyticsInitialized`

```csharp
private void OnGameAnalyticsInitialized(object sender, bool e)
```

Called by the `GameAnalytics.onInitialize` event:
- Sets `IsInitialized = GameAnalytics.Initialized` (independent of the `bool e` success flag — the SDK's own `Initialized` property is the authoritative source).
- Unsubscribes from `onInitialize` to prevent duplicate handling.

---

## Integration Pattern

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
            Debug.LogWarning("[VGameKit] GameAnalytics failed to initialize.");
        }
    }
}
```

---

## Compile Symbol: `GA_ENABLED`

Other VGameKit modules (`InterstitialAds`, `RewardedAds`) conditionally send GA design events under `#if GA_ENABLED`. Define this symbol in **Player Settings → Scripting Define Symbols** when GA is active in a build.

Without the symbol, the following events are silently skipped:
- `InterstitialAd:Showed`, `InterstitialAd:FailToLoad`, `InterstitialAd:FailToShow`
- `RewardedAd:Showed`, `RewardedAd:FailToLoad`, `RewardedAd:FailToShow`
- `RewardedAd:Showed:<from>`, `RewardedAd:FailToShow:<from>`

> **Known issue:** `RewardedAds.HandleOnAdFullScreenContentOpened` only publishes `AdsEventStatus.ResponseOpened` inside `#if GA_ENABLED`. Without the symbol, the opened event is **never fired**. This is a bug — the publish call should be outside the `#if` block.

---

## See Also

- `GoogleMobileAdsController` — Google Ads initialization (separate module)
- GameAnalytics Unity SDK docs
- `GKLog` — VGameKit logging
