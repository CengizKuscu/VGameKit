# ProcessFlows Reference

**Namespace:** `VGameKit.Runtime.ProcessFlows`
**Assembly:** `VGameKit.Runtime`
**Files:** `Assets/VGameKit/Runtime/ProcessFlows/`

---

## Overview

The ProcessFlows system provides a structured pipeline for sequencing async game logic. A "process flow" is a discrete, cancellable unit of work that:
- Accepts typed arguments (`IProcessFlowArgs`)
- Executes async logic (`AsyncExecute`)
- Can chain additional flows via `AppendProcess`
- Fires a completion callback (`onComplete`)
- Is managed centrally by `ProcessFlowProvider`

---

## Type Overview

| Type | Kind | Description |
|---|---|---|
| `IProcessFlowArgs` | interface | Marker for flow argument bags |
| `BaseProcessFlowArgs` | struct | Empty default args (no arguments needed) |
| `IFlowTask<T>` | interface | Synchronous self-returning task |
| `IFlowAsyncTask<T>` | interface | Async self-returning task |
| `IProcessFlow` | interface | Full flow contract |
| `IProcessFlow<TArgs>` | interface | Typed flow with args accessor |
| `BaseProcessFlow<TArgs>` | abstract class | Base implementation |
| `ProcessFlowProvider` | class | Central registry and factory |
| `ProcessFlowsExtensions` | static class | DI registration helper |

---

## Interfaces

### `IProcessFlowArgs`

```csharp
public interface IProcessFlowArgs { }
```

Marker interface. Create a struct or class implementing this for each flow that needs typed input.

---

### `BaseProcessFlowArgs`

```csharp
public struct BaseProcessFlowArgs : IProcessFlowArgs { }
```

Empty args struct — use when a flow needs no input parameters.

---

### `IFlowTask<T>`

```csharp
public interface IFlowTask<T> where T : IFlowTask<T>
{
    T Execute();
}
```

Synchronous, self-returning task. Implement for non-async pipeline steps.

---

### `IFlowAsyncTask<T>`

```csharp
public interface IFlowAsyncTask<T> where T : IFlowAsyncTask<T>
{
    UniTask<T> Execute(CancellationToken ctx = default);
}
```

Async, self-returning task. Implement for async pipeline steps outside of the full `IProcessFlow` system.

---

### `IProcessFlow`

```csharp
public interface IProcessFlow : IDisposable
```

| Member | Type | Description |
|---|---|---|
| `onComplete` | `Action<IProcessFlow>` | Fired when all flows (including appended) complete |
| `Id` | `string` | Unique identifier (defaults to type name) |
| `IsCompleted` | `bool` | True after all chained flows finish |
| `cancellationTokenSource` | `CancellationTokenSource` | Lazy-created; use to cancel |
| `cancellationToken` | `CancellationToken` | Shorthand `=> cancellationTokenSource.Token` |
| `Initialize(IProcessFlowArgs)` | method | Sets args and calls protected `Initialize()` |
| `AppendProcess(IProcessFlow)` | method | Chains a flow to run after this one completes |
| `OnComplete(Action<IProcessFlow>)` | method | Adds callback; returns `this` for chaining |
| `Execute(CancellationToken)` | method | Fire-and-forget entry point |
| `AsyncExecute(CancellationToken)` | method | Awaitable entry point |
| `Cancel()` | method | Cancels via `CancellationTokenSource` |

---

### `IProcessFlow<TArgs>`

```csharp
public interface IProcessFlow<TArgs> : IProcessFlow
    where TArgs : IProcessFlowArgs
```

Adds `TArgs Args { get; }` to the base interface for typed argument access.

---

## Class: `BaseProcessFlow<TArgs>`

```csharp
public abstract class BaseProcessFlow<TArgs> : IProcessFlow<TArgs>
    where TArgs : IProcessFlowArgs
```

### Injected Fields

| Type | Name | Description |
|---|---|---|
| `IObjectResolver` | `_resolver` | VContainer resolver; available to subclasses for `Resolve<T>()` |

### Key Properties

| Property | Type | Notes |
|---|---|---|
| `Id` | `string` | `=> GetType().Name` |
| `Args` | `TArgs` | Set by `Initialize(IProcessFlowArgs)` |
| `IsCompleted` | `bool` | Set to `true` when continuation finishes |
| `cancellationTokenSource` | `CancellationTokenSource` | Lazy — created on first access |

### Abstract Method

```csharp
public abstract UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx);
```

Implement all async work here. Call appended sub-flows or await other tasks. When this returns, the continuation function chains appended flows and fires `onComplete`.

### Execute vs AsyncExecute

```csharp
// Fire and forget — safe to call from non-async context
flow.Execute(cancellationToken);

// Awaitable — use in an async context
var result = await flow.AsyncExecute(cancellationToken);
```

### Appending Flows

```csharp
flowA
    .AppendProcess(flowB)
    .AppendProcess(flowC)
    .OnComplete(f => Debug.Log("All done"));

flowA.Execute(token);
// Execution order: flowA → flowB → flowC → onComplete
```

---

## Class: `ProcessFlowProvider`

```csharp
public class ProcessFlowProvider : IDisposable
```

Central registry for active flows. Register as `Singleton` in your app scope.

### Key Methods

| Method | Description |
|---|---|
| `CreateProcessFlow<TArgs, TFlow>(TArgs args)` | Creates `TFlow` via `new()`, injects via VContainer, initialises with args, adds to registry |
| `GetProcessFlow<TFlow>()` | Returns the first active flow of type `TFlow` |
| `GetProcessFlows<TFlow>()` | Returns all active flows of type `TFlow` |
| `RemoveProcessFlow<TFlow>()` | Cancels + disposes + removes first match |
| `RemoveProcessFlows<TFlow>()` | Cancels + disposes + removes all matches |
| `RemoveAllProcessFlows()` | Cancels + disposes + clears all |
| `Dispose()` | Calls `RemoveAllProcessFlows()` |

> `AddProcessFlow` registers an `onComplete` callback that auto-removes the flow from the registry when it finishes.

---

## DI Registration

### Register the provider

```csharp
builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsSelf();
```

### Register a flow factory

```csharp
// In your LifetimeScope.Configure:
builder.RegisterProcessFlow<MyFlowArgs, MyFlow>(Lifetime.Singleton);
// Registers a Func<MyFlowArgs, MyFlow> factory
```

```csharp
// extension method implementation:
public static void RegisterProcessFlow<TArgs, TProcessFlow>(
    this IContainerBuilder builder, Lifetime lifetime)
    where TArgs : IProcessFlowArgs
    where TProcessFlow : IProcessFlow<TArgs>, new()
```

---

## Concrete Flow Example

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.Log;

public class LoadLevelArgs : IProcessFlowArgs
{
    public int LevelIndex;
}

public class LoadLevelFlow : BaseProcessFlow<LoadLevelArgs>
{
    [Inject] private readonly ISceneLoader _sceneLoader;

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.ProcessFlow, $"LoadLevelFlow: loading level {Args.LevelIndex}");
        await _sceneLoader.LoadAsync(Args.LevelIndex, ctx);
        GKLog.Log(LogState.ProcessFlow, "LoadLevelFlow: complete");
        return this;
    }
}
```

### Usage

```csharp
// Inject ProcessFlowProvider
[Inject] private readonly ProcessFlowProvider _flowProvider;

// Create and start the flow
var flow = _flowProvider.CreateProcessFlow<LoadLevelArgs, LoadLevelFlow>(
    new LoadLevelArgs { LevelIndex = 2 });

flow.OnComplete(_ => ShowLevelUI());
flow.Execute(destroyCancellationToken);
```

---

## See Also

- `AbsAppManager.StartAsync` — common entry point before flows are used
- `ProcessFlowProvider` — manages all active flows
- UniTask — async task library
- VContainer `IObjectResolver` — used for field injection in flows
