# Lifetime Scopes Reference

**Namespace:** `VGameKit.Runtime.Core`
**Assembly:** `VGameKit.Runtime`
**Files:**
- `Assets/VGameKit/Runtime/Core/AbsBaseLifetimeScope.cs`
- `Assets/VGameKit/Runtime/Core/AbsMainLifetimeScope.cs`

---

## Overview

VGameKit provides two abstract `LifetimeScope` base classes that all scopes in the project inherit from. They handle scope readiness tracking, logger setup, frame rate configuration, and MessagePipe registration, so concrete scopes only need to declare DI bindings.

---

## Inheritance Chain

```
VContainer.Unity.LifetimeScope
    └── AbsBaseLifetimeScope          ← readiness tracking + build log
            └── AbsMainLifetimeScope  ← frame rate + logger + MessagePipe
                    └── AppLifetimeScope   (concrete app scope)
            └── GameLifetimeScope     (concrete scene scope — extends AbsBaseLifetimeScope directly)
```

---

## AbsBaseLifetimeScope

```csharp
public abstract class AbsBaseLifetimeScope : LifetimeScope
```

### Properties

| Type | Name | Access | Description |
|---|---|---|---|
| `bool` | `IsReady` | `public get / private set` | `true` once the DI container is fully built |

### Configure

```csharp
protected override void Configure(IContainerBuilder builder)
{
    IsReady = false;
    builder.RegisterBuildCallback(_ =>
    {
        IsReady = true;
        GKLog.Log(LogState.Core, $"{this.GetType().Name} is ready.");
    });
}
```

Sets `IsReady = false` at the start of `Configure`, then registers a build callback that sets it to `true` and logs the scope name at `LogState.Core` once the container is built.

> **Important:** All concrete scopes **must** call `base.Configure(builder)` so the `IsReady` tracking and build log are active.

---

## AbsMainLifetimeScope

```csharp
public abstract class AbsMainLifetimeScope : AbsBaseLifetimeScope
```

Extends `AbsBaseLifetimeScope` with application-level setup: frame rate, logging, and MessagePipe.

### Serialized Fields

| Type | Name | Default | Description |
|---|---|---|---|
| `int` | `_targetFrameRate` | `60` | Set via Inspector; applied as `Application.targetFrameRate` |

### Protected Fields

| Type | Name | Description |
|---|---|---|
| `MessagePipeOptions` | `_messagePipeOpts` | Holds the registered MessagePipe options; available to subclasses |

### Configure

```csharp
protected override void Configure(IContainerBuilder builder)
{
    base.Configure(builder); // AbsBaseLifetimeScope: IsReady + build log

    // Enables Unity's logger unconditionally;
    // also calls GKLog.ReportLogLevel() when GAMEKIT_LOG is defined
    Debug.unityLogger.logEnabled = true;

    Application.targetFrameRate = _targetFrameRate;
    _messagePipeOpts = builder.RegisterMessagePipe();
}
```

`builder.RegisterMessagePipe()` registers the MessagePipe DI infrastructure. All `IPublisher<T>` / `ISubscriber<T>` pairs used throughout the app must be resolved from the same container that called this.

---

## Concrete Scope Patterns

### App Scope (root, one per application)

```csharp
public sealed class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        // Register app-level singletons
        builder.Register<AppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsSelf();
    }
}
```

### Scene/Game Scope (child of app scope)

```csharp
public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private DemoSpawnItem _demoSpawnItemPrefab;
    [SerializeField] private Transform _demoSpawnItemParent;
    [SerializeField] private DemoPopupBuilder _demoPopupBuilder;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterComponent(_demoSpawnItemParent);
        builder.RegisterComponent(_demoPopupBuilder);
        builder.Register<GameAppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterObjectSpawner<DemoSpawnItemModel, Transform, DemoSpawnItem>(
            _demoSpawnItemPrefab, Lifetime.Singleton, true);
    }
}
```

Scene scopes extend `AbsBaseLifetimeScope` (not `AbsMainLifetimeScope`) because MessagePipe and frame rate are already registered by the root scope.

---

## Checking Readiness

```csharp
// Poll from outside the scope
if (myLifetimeScope.IsReady)
{
    // Container is built, services are available
}
```

`IsReady` is useful in editor tools or async loading sequences where you need to wait for DI completion before resolving services.

---

## See Also

- `AbsAppManager` — registered in `AbsMainLifetimeScope` subclasses
- `SubscribableConcrete` / `SubscribableMonoBehaviour` — types registered within scopes
- VContainer documentation — `LifetimeScope`, `IContainerBuilder`
- MessagePipe documentation — `RegisterMessagePipe()`
