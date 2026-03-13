# AbsAppManager — App Manager Reference

**Namespace:** `VGameKit.Runtime.App`
**Assembly:** `VGameKit.Runtime`
**File:** `Assets/VGameKit/Runtime/App/AbsAppManager.cs`

---

## Overview

`AbsAppManager` is the abstract base class for the application-level bootstrap manager. It implements VContainer's `IAsyncStartable` lifecycle interface, which means VContainer calls `StartAsync` automatically after the DI container is fully built.

The startup sequence is:
1. `StartAsync` awaits `InitializeGame` (user-implemented).
2. On completion, `AppReadyEvent` is published via MessagePipe.
3. `OnAppReady` is triggered via the subscription set up in `Subscriptions()`.

---

## Inheritance

```
SubscribableConcrete : IInitializable, ISubscribableObject
    └── AbsAppManager : IAsyncStartable
            └── AppManager  (concrete, user-defined)
```

---

## Class Declaration

```csharp
public abstract class AbsAppManager : SubscribableConcrete, IAsyncStartable
```

---

## Injected Fields

| Type | Name | Injected By |
|---|---|---|
| `IPublisher<AppReadyEvent>` | `_appReadyPublisher` | `[Inject]` (VContainer) |
| `ISubscriber<AppReadyEvent>` | `_appReadySubscriber` | `[Inject]` (VContainer) |

---

## Methods

### `Subscriptions` *(override)*

```csharp
public override void Subscriptions()
```

Wires `OnAppReady` to the `AppReadyEvent` MessagePipe stream. Called by `SubscribableConcrete.Initialize()` during VContainer's `IInitializable` lifecycle. The subscription is automatically disposed when the scope is destroyed.

```csharp
_appReadySubscriber.Subscribe(OnAppReady).AddTo(_bagBuilder);
```

---

### `StartAsync`

```csharp
public async UniTask StartAsync(CancellationToken token)
```

VContainer calls this after the DI container is ready. Execution:
1. Awaits `InitializeGame(token)` with `AttachExternalCancellation(token).SuppressCancellationThrow()` — cancellation does not throw, it silently stops.
2. Publishes `new AppReadyEvent()`.

| Parameter | Type | Description |
|---|---|---|
| `token` | `CancellationToken` | Provided by VContainer; tied to the scope lifetime |

---

### `InitializeGame` *(abstract)*

```csharp
protected abstract UniTask InitializeGame(CancellationToken token);
```

Implement all asynchronous game startup logic here (load configs, initialize SDKs, prefetch data, etc.). The app is considered "not ready" until this task completes.

---

### `OnAppReady` *(abstract)*

```csharp
protected abstract void OnAppReady(AppReadyEvent @event);
```

Called when `AppReadyEvent` is received from the MessagePipe bus. Implement post-ready logic here (e.g., show the main menu, start music, trigger first scene transition).

---

## AppReadyEvent

**File:** `Assets/VGameKit/Runtime/App/Events/AppReadyEvent.cs`
**Namespace:** `VGameKit.Runtime.App.Events`

```csharp
public struct AppReadyEvent { }
```

An empty marker struct. Published exactly once per application lifecycle by `AbsAppManager.StartAsync`. Any system can subscribe to it via `ISubscriber<AppReadyEvent>`.

---

## DI Registration

Register via VContainer in your `AbsMainLifetimeScope` subclass:

```csharp
public sealed class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder); // registers MessagePipe, frame rate, logger
        builder.Register<AppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        // AsImplementedInterfaces exposes: IAsyncStartable, IInitializable, IDisposable
    }
}
```

`AsImplementedInterfaces()` is required so VContainer recognises and calls both `IInitializable.Initialize()` and `IAsyncStartable.StartAsync()`.

---

## Concrete Implementation Example

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.App;
using VGameKit.Runtime.App.Events;
using VGameKit.Runtime.Log;

public class AppManager : AbsAppManager
{
    protected override async UniTask InitializeGame(CancellationToken token)
    {
        GKLog.Log(LogState.Development, "AppManager: initializing...");
        await LoadRemoteConfigAsync(token);
        await InitializeAnalyticsAsync(token);
    }

    protected override void OnAppReady(AppReadyEvent @event)
    {
        GKLog.Log(LogState.Game, "AppManager: app is ready");
        // Show main menu, start music, etc.
    }
}
```

---

## Subscribing to AppReadyEvent from Other Systems

Any class in the app scope can subscribe independently:

```csharp
public class GameAppManager : SubscribableConcrete
{
    [Inject] private readonly ISubscriber<AppReadyEvent> _appReadySubscriber;

    public override void Subscriptions()
    {
        _appReadySubscriber.Subscribe(OnAppReady).AddTo(_bagBuilder);
    }

    private void OnAppReady(AppReadyEvent @event)
    {
        // React to app readiness in a different system
    }
}
```

---

## See Also

- `SubscribableConcrete` — base class providing `_bagBuilder` and `Initialize()`
- `AbsMainLifetimeScope` — root scope where `AppManager` is registered
- `AppReadyEvent` — the published marker event
- `ProcessFlowProvider` — common next step after app is ready
