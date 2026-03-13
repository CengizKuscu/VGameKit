# Subscribable System Reference

**Namespace:** `VGameKit.Runtime.Core`
**Assembly:** `VGameKit.Runtime`
**Files:**
- `Assets/VGameKit/Runtime/Core/ISubscribableObject.cs`
- `Assets/VGameKit/Runtime/Core/SubscribableConcrete.cs`
- `Assets/VGameKit/Runtime/Core/SubscribableMonoBehaviour.cs`

---

## Overview

The subscribable system provides a structured, leak-safe pattern for wiring MessagePipe subscriptions in VGameKit. All classes that subscribe to `IPublisher<T>` / `ISubscriber<T>` pairs should extend one of the base classes below. Subscriptions are collected into a `DisposableBag` and automatically disposed when the scope or `MonoBehaviour` is destroyed.

---

## Interface: `ISubscribableObject`

```csharp
public interface ISubscribableObject : IDisposable
```

Base contract for all subscribable types.

| Member | Description |
|---|---|
| `void Init()` | Initialization hook — called before `Subscriptions()` |
| `void Subscriptions()` | Wire MessagePipe subscriptions here; add each to `_bagBuilder` |
| `void Dispose()` | (from `IDisposable`) Disposes all subscriptions |

---

## Class: `SubscribableConcrete`

```csharp
public abstract class SubscribableConcrete : IInitializable, ISubscribableObject
```

Use this as the base class for **non-MonoBehaviour** services (DI-registered POCOs). VContainer calls `Initialize()` automatically via `IInitializable`.

### Fields

| Type | Name | Access | Description |
|---|---|---|---|
| `IDisposable` | `_subscription` | `private` | The built `DisposableBag` handle |
| `DisposableBagBuilder` | `_bagBuilder` | `protected` | Builder for adding subscriptions before `Build()` |

### Methods

#### `Initialize` *(called by VContainer)*

```csharp
public void Initialize()
{
    _bagBuilder = DisposableBag.CreateBuilder();
    Init();
    Subscriptions();
    _subscription = _bagBuilder.Build();
}
```

Lifecycle entry point. Creates the bag builder, calls `Init()`, calls `Subscriptions()`, then locks the bag. After this point `_bagBuilder` is sealed.

#### `Init` *(virtual)*

```csharp
public virtual void Init() { }
```

Override for setup logic that runs before subscriptions are wired (e.g., initialize pools, create child objects).

#### `Subscriptions` *(virtual)*

```csharp
public virtual void Subscriptions() { }
```

Override to add MessagePipe subscriptions:

```csharp
public override void Subscriptions()
{
    _mySubscriber.Subscribe(OnMyEvent).AddTo(_bagBuilder);
    _otherSubscriber.Subscribe(OnOtherEvent).AddTo(_bagBuilder);
}
```

#### `Dispose` *(virtual)*

```csharp
public virtual void Dispose()
{
    _subscription?.Dispose();
}
```

Disposes all subscriptions registered in the bag. Called by VContainer when the scope is destroyed.

---

## Class: `SubscribableMonoBehaviour`

```csharp
public class SubscribableMonoBehaviour : MonoBehaviour, ISubscribableObject
```

Use this as the base class for **MonoBehaviour** components that need MessagePipe subscriptions. Because VContainer cannot call `IInitializable` on `MonoBehaviour`, the setup is triggered via an `[Inject]`-tagged `Construct()` method instead.

### Fields

| Type | Name | Access | Description |
|---|---|---|---|
| `IDisposable` | `_subscription` | `private` | The built `DisposableBag` handle |
| `DisposableBagBuilder` | `_bagBuilder` | `protected` | Builder for adding subscriptions |

### Methods

#### `Construct` *(called by VContainer via `[Inject]`)*

```csharp
[Inject]
public virtual void Construct()
{
    _bagBuilder = DisposableBag.CreateBuilder();
    Init();
    Subscriptions();
    _subscription = _bagBuilder.Build();
}
```

Equivalent of `Initialize()` for MonoBehaviours. VContainer calls this after field injection.

#### `Init`, `Subscriptions` — same signatures as `SubscribableConcrete`

#### `OnDisable`

```csharp
protected virtual void OnDisable()
{
    _subscription?.Dispose();
}
```

Subscriptions are disposed when the `GameObject` is disabled, preventing delivery to inactive objects.

#### `Awake`, `OnEnable`, `OnDestroy`

All declared as `protected virtual` with empty bodies — override freely in subclasses.

---

## Comparison

| | `SubscribableConcrete` | `SubscribableMonoBehaviour` |
|---|---|---|
| Base type | `object` | `MonoBehaviour` |
| DI trigger | `IInitializable.Initialize()` | `[Inject] Construct()` |
| Dispose trigger | VContainer scope destroy | `OnDisable()` |
| Use for | Pure C# services, presenters | Unity components, views |

---

## Pattern: Implementing a Subscriber

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.App.Events;

public class GameScoreTracker : SubscribableConcrete
{
    [Inject] private readonly ISubscriber<AppReadyEvent> _appReadySubscriber;
    [Inject] private readonly IPublisher<ScoreChangedEvent> _scorePublisher;

    private int _score;

    public override void Init()
    {
        _score = 0;
    }

    public override void Subscriptions()
    {
        _appReadySubscriber.Subscribe(OnAppReady).AddTo(_bagBuilder);
    }

    private void OnAppReady(AppReadyEvent @event)
    {
        // Start tracking
    }

    public void AddScore(int points)
    {
        _score += points;
        _scorePublisher.Publish(new ScoreChangedEvent(_score));
    }
}
```

---

## See Also

- `AbsAppManager` — extends `SubscribableConcrete`
- `BaseMenuManager<TMenuName>` — extends `SubscribableMonoBehaviour`
- MessagePipe — `IPublisher<T>`, `ISubscriber<T>`, `DisposableBag`
- VContainer — `IInitializable`, `[Inject]`
