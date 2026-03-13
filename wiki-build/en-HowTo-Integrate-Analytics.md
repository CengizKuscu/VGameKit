# How to Integrate GameAnalytics

## Overview

This guide covers adding `VGameKit.GA` to a Unity project, configuring the GameAnalytics SDK, initialising it at startup, and sending events.

**Prerequisites:** `VGameKit.Runtime` and `VGameKit.GA.Runtime` modules installed; GameAnalytics SDK (`com.gameanalytics.sdk 7.10.5`) resolved via OpenUPM.

---

## Step 1 — Configure the GameAnalytics asset

1. In the Unity Editor, go to **Window > GameAnalytics > Setup**.
2. Sign in or create an account, then create a new game or select an existing one.
3. The SDK creates `Assets/Resources/GameAnalytics/Settings.asset` automatically.
4. Confirm the **Game Key** and **Secret Key** for each target platform (Android, iOS) are filled in.

---

## Step 2 — Add GA_Initialization to your scene

1. Create a new GameObject (e.g. `GAInitializer`) in your app-level scene.
2. Attach the `GA_Initialization` component.
3. On iOS, `GA_Initialization` calls `GameAnalytics.RequestTrackingAuthorization` automatically before initialising. On all other platforms it calls `GameAnalytics.Initialize()` directly.

```
Scene hierarchy:
  AppRoot (GameObject)
    └── GAInitializer  [GA_Initialization]
```

---

## Step 3 — Register GA_Initialization in the LifetimeScope

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

## Step 4 — Initialise during app startup

Inject `GA_Initialization` into your `AbsAppManager` subclass and await it:

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
        // GA is now initialised; IsInitialized == true
    }
}
```

`GA_Initialization.Initialize()` waits until `GameAnalytics.Initialized` becomes `true` before returning.

---

## Step 5 — Send events

Use the GameAnalytics SDK API directly after `IsInitialized` is `true`:

```csharp
using GameAnalyticsSDK;

// Design event (custom KPIs):
GameAnalytics.NewDesignEvent("Level:Start:1");
GameAnalytics.NewDesignEvent("Enemy:Killed", 1f);

// Progression event:
GameAnalytics.NewProgressionEvent(
    GAProgressionStatus.Start, "World1", "Level1");

GameAnalytics.NewProgressionEvent(
    GAProgressionStatus.Complete, "World1", "Level1");

// Resource event (virtual currency):
GameAnalytics.NewResourceEvent(
    GAResourceFlowType.Source, "Gold", 100f, "LevelComplete", "Reward");

// Error event:
GameAnalytics.NewErrorEvent(GAErrorSeverity.Error, "NullRef in ShopPresenter");
```

---

## Step 6 — Guard events behind the GA_ENABLED symbol (optional)

When `com.gameanalytics.sdk` is present the `GA_ENABLED` define is set automatically by `VGameKit.GoogleAds.Runtime.asmdef`. If you want to guard analytics calls so they compile away when the SDK is absent:

```csharp
#if GA_ENABLED
GameAnalytics.NewDesignEvent("Player:LevelUp");
#endif
```

---

## iOS App Tracking Transparency

On iOS, `GA_Initialization` calls `RequestTrackingAuthorization` before initialising the SDK. The four ATT callback methods (`Authorized`, `Denied`, `Restricted`, `NotDetermined`) all call `GameAnalytics.Initialize()`, so the SDK always starts regardless of the user's choice.

No additional code is needed on your side for ATT compliance.

---

## Verifying initialization in the Editor

In the Editor, `GA_Initialization.Initialize()` completes when `GameAnalytics.Initialized` becomes `true`. You can confirm this by checking `_gaInit.IsInitialized` after awaiting.

To test analytics events in the Editor, use the GameAnalytics **Validator** window (**Window > GameAnalytics > Validator**).

---

## See also

- [GameAnalytics reference](en-Reference-Game-Analytics)
- [App Manager reference](en-Reference-App-Manager)
- [Install Modules how-to](en-HowTo-Install-Modules)
