# How to Manage Process Flows

## Overview

Process flows are discrete, cancellable, async units of work. `BaseProcessFlow<TArgs>` handles execution, chaining, cancellation, and completion callbacks. `ProcessFlowProvider` owns the lifecycle of all running flows.

**Prerequisites:** `VGameKit.Runtime` module installed; UniTask; an `AbsBaseLifetimeScope`.

---

## Step 1 — Define flow arguments

Create a class that implements `IProcessFlowArgs`. Pass data via a constructor so args are immutable at creation time.

```csharp
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelFlowArgs : IProcessFlowArgs
{
    public int LevelIndex { get; private set; }

    public LoadLevelFlowArgs(int levelIndex)
    {
        LevelIndex = levelIndex;
    }
}
```

`BaseProcessFlowArgs` is an empty struct convenience — use it only when the flow needs no input at all.

---

## Step 2 — Implement the flow

Create a class that inherits `BaseProcessFlow<TArgs>` and implements `AsyncExecute`.

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelFlow : BaseProcessFlow<LoadLevelFlowArgs>
{
    // Inject dependencies the flow needs
    [Inject] private readonly LevelPrefabs _levelPrefabs;

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        var levelPrefab = _levelPrefabs.Levels[Args.LevelIndex];

        LevelObj level = null;
        await UniTask.WaitUntil(
            () => level = Object.Instantiate(levelPrefab).GetComponent<LevelObj>(),
            cancellationToken: ctx);

        level.Initialize();
        return this;
    }
}
```

`AsyncExecute` must return `this` after finishing work. Returning `this` signals completion and triggers the continuation chain.

Override the optional `protected virtual void Initialize()` for any pre-execution setup that runs synchronously before `AsyncExecute`.

---

## Step 3 — Register in the LifetimeScope

Use `RegisterProcessFlow<TArgs, TFlow>` to register a `Func<TArgs, TFlow>` factory. Also register `ProcessFlowProvider` as a singleton — `RegisterProcessFlow` calls `container.Resolve<ProcessFlowProvider>()` internally and will throw a resolution error at runtime if the provider is missing.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class AppLifetimeScope : AbsMainLifetimeScope
{
    [SerializeField] private LevelPrefabs _levelPrefabs;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // ProcessFlowProvider must be registered explicitly — RegisterProcessFlow depends on it.
        // AsImplementedInterfaces() enables VContainer's automatic IDisposable.Dispose() call.
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        builder.RegisterComponent(_levelPrefabs);

        // AsImplementedInterfaces() lets VContainer call IInitializable.Initialize()
        // and IDisposable.Dispose(). AsSelf() also allows resolving DemoManager by
        // its concrete type if needed.
        builder.Register<DemoManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        // Registers Func<LoadLevelFlowArgs, LoadLevelFlow>
        builder.RegisterProcessFlow<LoadLevelFlowArgs, LoadLevelFlow>(Lifetime.Singleton);
    }
}
```

`RegisterProcessFlow` calls `builder.RegisterFactory` internally. It resolves `ProcessFlowProvider` from the container and delegates to `provider.CreateProcessFlow<TArgs, TFlow>(args)` when the factory is invoked.

---

## Step 4 — Create and execute a flow

Inject `Func<TArgs, TFlow>` directly — do not inject `ProcessFlowProvider` for flow creation. The caller owns a `CancellationTokenSource` and passes its token to `Execute`. Use `IInitializable` to create the token and `IDisposable` to cancel it:

```csharp
using System;
using System.Threading;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.ProcessFlows;

public class DemoManager : IInitializable, IDisposable
{
    [Inject] private readonly Func<LoadLevelFlowArgs, LoadLevelFlow> _loadLevelFlow;

    private CancellationTokenSource _cts;

    public void Initialize()
    {
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        _cts?.Cancel();
    }

    public void LoadLevel(int levelIndex)
    {
        var args = new LoadLevelFlowArgs(levelIndex);
        var flow = _loadLevelFlow(args);

        flow.OnComplete(f =>
        {
            if (f is LoadLevelFlow lf)
            {
                Debug.Log($"Level {lf.Args.LevelIndex} loaded successfully.");
            }
        });

        flow.Execute(_cts.Token);
    }
}
```

Calling `_loadLevelFlow(args)` triggers `provider.CreateProcessFlow` which: creates a new instance via `new()`, injects dependencies, calls `Initialize(args)`, and registers the flow with `AddProcessFlow` so it is auto-removed on completion.

---

## Step 5 — Chain flows

There are two ways to chain flows. Choose based on where the sequencing responsibility belongs.

### Option A — `AppendProcess` (caller-side chaining)

The caller creates both flows and wires them together before executing. Appended flows run **sequentially** in the order they were added, after the preceding flow's `AsyncExecute` returns `this`. Each appended flow's `Execute` is called automatically by `ContinuationFunction`; you never call it manually.

```csharp
[Inject] private readonly Func<LoadLevelFlowArgs, LoadLevelFlow> _loadLevelFlow;
[Inject] private readonly Func<SpawnEnemiesArgs, SpawnEnemiesFlow> _spawnEnemiesFlow;

public void StartLevel(int index)
{
    var loadFlow  = _loadLevelFlow(new LoadLevelFlowArgs(index));
    var spawnFlow = _spawnEnemiesFlow(new SpawnEnemiesArgs());

    loadFlow.AppendProcess(spawnFlow);
    loadFlow.Execute(_cts.Token);
    // Execution order: loadFlow.AsyncExecute → spawnFlow.AsyncExecute
    // spawnFlow.Execute is called automatically — do not call it manually
}
```

Multiple flows can be appended to the same root flow; they execute one after another in append order:

```csharp
rootFlow.AppendProcess(secondFlow);
rootFlow.AppendProcess(thirdFlow);
rootFlow.Execute(_cts.Token);
// Order: rootFlow → secondFlow → thirdFlow
```

### Option B — inline chaining inside `AsyncExecute`

A flow creates and executes the next flow from within its own `AsyncExecute`. Use this when the sequencing is an internal implementation detail of the flow itself, not a concern of the caller.

```csharp
public class LoadLevelFlow : BaseProcessFlow<LoadLevelFlowArgs>
{
    [Inject] private readonly Func<SpawnEnemiesArgs, SpawnEnemiesFlow> _spawnEnemiesFlow;

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        // ... level loading work ...

        // Kick off the next flow inline and wait for it
        var spawnFlow = _spawnEnemiesFlow(new SpawnEnemiesArgs());
        spawnFlow.Execute(ctx);
        await UniTask.WaitUntil(() => spawnFlow.IsCompleted, cancellationToken: ctx);

        return this;
    }
}
```

The caller only creates and executes `LoadLevelFlow`; `SpawnEnemiesFlow` is hidden inside it. Use this pattern when the follow-up flow is always required by the first one and has no standalone use.

**When to use which:**
- `AppendProcess` — the caller decides the sequence; flows are reusable independently.
- Inline `AsyncExecute` — the sequence is always fixed inside the flow; the caller does not need to know about it.

---

## Step 6 — React to completion

Register a callback before calling `Execute`:

```csharp
flow.OnComplete(completedFlow =>
{
    Debug.Log("Level loaded successfully.");
});
flow.Execute(_cts.Token);
```

---

## Step 7 — Cancel a flow

```csharp
// Cancel a specific type (inject ProcessFlowProvider for management operations):
[Inject] private readonly ProcessFlowProvider _flowProvider;

_flowProvider.RemoveProcessFlow<LoadLevelFlow>();

// Cancel all running flows:
_flowProvider.RemoveAllProcessFlows();
```

Calling `RemoveProcessFlow<T>` cancels the token, disposes the flow, and removes it from the provider's internal list.

---

## Flow lifecycle summary

```
RegisterProcessFlow  →  Func<TArgs, TFlow> injected into caller
        ↓
  factory(args) called
        ↓
  provider.CreateProcessFlow:
    new TFlow()  →  Inject()  →  Initialize(args)  →  AddProcessFlow()
        ↓
  flow.Execute(_cts.Token)
        ↓
  AsyncExecute()
        ↓
  ContinuationFunction()
  (appended flows run sequentially in append order)
        ↓
  onComplete callbacks
        ↓
  provider auto-removes & disposes
```

---

## See also

- [ProcessFlows reference](../reference/api/processflows.md)
- [Lifetime Scopes reference](../reference/api/lifetime-scopes.md)
- [Logging reference](../reference/api/logging.md)
