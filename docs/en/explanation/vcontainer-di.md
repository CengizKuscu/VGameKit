# Explanation: VContainer and Dependency Injection in VGameKit

## Why VGameKit uses dependency injection

VGameKit is built around **VContainer**, a fast, allocation-friendly DI container designed for Unity. DI is not an optional convenience here — it is the structural backbone. Every service, presenter, pool, and flow is wired through the container. Understanding why helps you extend the framework without fighting it.

### The problem DI solves

In a Unity project without DI, components find their collaborators through `GetComponent`, static singletons, or direct field assignments in the Inspector. Each technique creates hidden coupling: a presenter reaches into the scene hierarchy, a manager holds a static reference, and tests or scene changes break things silently.

DI inverts this. Components declare what they need; the container decides how to provide it. The result is:

- **Explicit dependencies** — every class's requirements are visible in its constructor or `[Inject]` fields.
- **Lifecycle control** — the container owns object lifetimes (`Singleton`, `Scoped`, `Transient`), not `GameObject.Destroy`.
- **Testability** — substituting a real service for a stub requires changing one registration, not rewriting callers.

---

## LifetimeScope hierarchy

VContainer organises registrations into **LifetimeScopes**, which form a tree:

```
AbsMainLifetimeScope  (app-level, lives for the whole session)
    └── AbsBaseLifetimeScope  (scene- or subsystem-level, destroyed with the scene)
```

`AbsMainLifetimeScope` is the root. It:

1. Calls `builder.RegisterMessagePipe()` internally.

**Do not** call `RegisterMessagePipe()` again in your subclass — it must only be called once per container. On Unity 2022.1+ with VContainer 1.14.0+, `IPublisher<T>` and `ISubscriber<T>` are resolved automatically after `RegisterMessagePipe()`; no additional broker registration is required.

Child scopes inherit everything registered in a parent scope. A scene scope can resolve `ProcessFlowProvider` (registered in the app scope) without re-registering it.

The reverse is not true: a parent scope has no visibility into its children. `AbsMainLifetimeScope` cannot resolve a presenter or pool that is only registered in a scene scope. This is intentional — the app scope outlives all scene scopes, so depending on something that may not exist yet (or may have been destroyed) would make the dependency graph unpredictable. Design accordingly: anything the app scope needs must be registered in the app scope itself.

---

## IAsyncStartable vs IInitializable

VContainer provides two startup interfaces:

| Interface | Called by VContainer | Typical use |
|---|---|---|
| `IInitializable` | Synchronously after build | Light init, no I/O |
| `IAsyncStartable` | Async, after `IInitializable` | I/O, SDK init, scene loads |

`AbsAppManager` implements `IAsyncStartable`. VContainer calls `StartAsync(CancellationToken)` automatically. Do not call it manually; do not `await` it from outside the container.

`SubscribableConcrete` implements `IInitializable`. VContainer calls `Initialize()` automatically. Inside `Initialize`, the class calls your `Init()` and `Subscriptions()` overrides in order.

---

## Subscribable patterns

VGameKit provides two base classes for objects that react to messages:

### SubscribableConcrete

For plain C# classes (no `MonoBehaviour`). VContainer injects dependencies, then calls `Initialize()` via `IInitializable`.

```
VContainer builds → injects fields → calls Initialize()
                                         └── Init()
                                         └── Subscriptions()
```

### SubscribableMonoBehaviour

For `MonoBehaviour`s. VContainer injects dependencies through the `[Inject] Construct()` method, which then calls `Init()` and `Subscriptions()`.

```
MonoBehaviour.Awake → VContainer.Inject → Construct() called
                                              └── Init()
                                              └── Subscriptions()
```

In both cases, subscriptions are stored in `_bagBuilder` and disposed automatically when the scope ends.

---

## Common registration patterns

```csharp
// Singleton: one instance for the scope's lifetime
builder.Register<MyService>(Lifetime.Singleton);

// Scoped: one per parent scope; disposed when scope ends
builder.Register<EnemyPool>(Lifetime.Scoped);

// Register against an interface
builder.Register<MyAppManager>(Lifetime.Singleton)
    .AsImplementedInterfaces()
    .AsSelf();

// Register a MonoBehaviour from the scene (via [SerializeField] reference)
builder.RegisterComponent(_mainMenuView);

// Register a factory lambda
builder.RegisterInstance<Func<EnemyModel, Transform, EnemyItem>>(
    (model, parent) => Object.Instantiate(prefab, parent));
```

---

## What to avoid

- **`new` outside scopes**: creates objects the container cannot inject, track, or dispose.
- **Static singletons alongside DI**: they bypass lifetime management and make order-of-init unpredictable.
- **Calling `Initialize()` or `StartAsync()` manually**: VContainer's startup sequence calls these at the right moment. Calling them again produces double-initialisation bugs.
- **Cross-scope direct field access**: child scopes may resolve parent registrations, but parent scopes must never depend on children.

---

## See also

- [Reference: Lifetime Scopes](../reference/api/lifetime-scopes.md)
- [Reference: Subscription System](../reference/api/subscribable.md)
- [Reference: App Manager](../reference/api/app-manager.md)
- [Explanation: MessagePipe event system](./messagepipe-events.md)
