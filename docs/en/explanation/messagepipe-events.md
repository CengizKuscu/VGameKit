# Explanation: The MessagePipe Event System

## Why event-driven communication

In a Unity project with many interacting systems, direct method calls between components create tight coupling. A `ScoreManager` that calls `UIManager.UpdateScore()` directly cannot exist without `UIManager`, cannot be tested in isolation, and breaks if `UIManager` is refactored.

MessagePipe solves this by decoupling publishers from subscribers: the `ScoreManager` publishes a `ScoreChangedEvent`; the `UIManager` subscribes to it. Neither knows the other exists. You can add, remove, or replace subscribers without touching the publisher.

---

## How VGameKit integrates MessagePipe

`AbsMainLifetimeScope` calls `builder.RegisterMessagePipe()` once during scope construction. On Unity 2022.1+ with VContainer 1.14.0+ (which VGameKit targets), `IPublisher<T>` and `ISubscriber<T>` pairs are resolved automatically — no manual broker registration is needed.

```csharp
// Inside your app LifetimeScope subclass — no AddBroker calls required:
protected override void Configure(IContainerBuilder builder)
{
    base.Configure(builder); // registers MessagePipe

    // IPublisher<PlayerDiedEvent> and ISubscriber<PlayerDiedEvent>
    // are available for injection without any additional registration.
}
```

Inject `IPublisher<T>` and `ISubscriber<T>` directly via `[Inject]` in any class managed by VContainer.

---

## Publishing and subscribing

```csharp
// Publisher side
public class EnemyController : SubscribableConcrete
{
    [Inject] private readonly IPublisher<PlayerDiedEvent> _diedPublisher;

    public void KillPlayer()
    {
        _diedPublisher.Publish(new PlayerDiedEvent { Cause = "enemy" });
    }
}

// Subscriber side
public class GameOverPresenter : SubscribableConcrete
{
    [Inject] private readonly ISubscriber<PlayerDiedEvent> _diedSubscriber;

    protected override void Subscriptions()
    {
        _diedSubscriber
            .Subscribe(OnPlayerDied)
            .AddTo(_bagBuilder);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        GKLog.Log(LogState.Game, $"Player died: {evt.Cause}");
        // show game over screen
    }
}
```

`AddTo(_bagBuilder)` is mandatory. The `_bagBuilder` collects all subscriptions and disposes them automatically when the scope ends.

---

## Message types

MessagePipe supports several patterns:

| Pattern | Interfaces | Use case |
|---|---|---|
| Pub/Sub | `IPublisher<T>` / `ISubscriber<T>` | Fire-and-forget events |
| Request/Response | `IRequestHandler<TReq, TRes>` | Ask a service for a value |
| Buffered | `IBufferedPublisher<T>` | Late subscribers receive the last value |

VGameKit's built-in systems use the basic pub/sub pattern. Stick to it unless a request/response flow genuinely fits the problem.

---

## Lifecycle and leak prevention

Every subscription is a delegate registration. Unregistered subscriptions hold object references indefinitely — a common source of memory leaks in Unity.

VGameKit's subscribable base classes manage this automatically:

1. Subscriptions added with `AddTo(_bagBuilder)` are tracked.
2. When the VContainer scope is disposed (scene unload, app shutdown), the `_bagBuilder` disposes all subscriptions in one call.

Never subscribe without `AddTo(_bagBuilder)` in a VGameKit class. For one-shot subscriptions outside a subscribable class, call `subscription.Dispose()` manually.

---

## What to avoid

- **Calling `RegisterMessagePipe()` more than once**: causes a runtime exception from VContainer. `AbsMainLifetimeScope` already calls it; never repeat it in a subclass.
- **Manually calling `AddBroker<T>()` or `RegisterMessageBroker<T>()`**: not needed on Unity 2022.1+ with VContainer 1.14.0+. Adding unnecessary broker calls has no effect but clutters the scope.
- **Subscribing in `Awake` or `Start` instead of `Subscriptions()`**: VGameKit's startup order guarantees that `Subscriptions()` runs after injection. `Awake`/`Start` may run before DI is complete.
- **Publishing from constructors**: publishers may not yet have subscribers. Publish from `IInitializable.Initialize` or later.
- **Using static events alongside MessagePipe**: static events bypass the DI lifecycle and scope disposal.

---

## See also

- [Reference: Subscription System](../reference/api/subscribable.md)
- [Explanation: VContainer and DI](./vcontainer-di.md)
- [Reference: App Manager](../reference/api/app-manager.md)
