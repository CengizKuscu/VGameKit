# How to Manage Process Flows

## Overview

Process flows are discrete, cancellable, async units of work. `BaseProcessFlow<TArgs>` handles execution, chaining, cancellation, and completion callbacks. `ProcessFlowProvider` owns the lifecycle of all running flows.

**Prerequisites:** `VGameKit.Runtime` module installed; UniTask; an `AbsBaseLifetimeScope`.

---

## Step 1 — Define flow arguments

Create a class that implements `IProcessFlowArgs`. It carries the data the flow needs at runtime.

```csharp
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelArgs : BaseProcessFlowArgs
{
    public int LevelIndex;
    public bool ShowLoadingScreen;
}
```

`BaseProcessFlowArgs` is a convenience base with no required members — you may also implement `IProcessFlowArgs` directly.

---

## Step 2 — Implement the flow

Create a class that inherits `BaseProcessFlow<TArgs>` and implements `AsyncExecute`.

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelFlow : BaseProcessFlow<LoadLevelArgs>
{
    // Inject any dependencies needed by this flow
    [Inject] private readonly SceneLoader _sceneLoader;

    protected override void Initialize()
    {
        GKLog.Log(LogState.ProcessFlow, $"LoadLevelFlow: Initialize level {Args.LevelIndex}");
    }

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.ProcessFlow, $"LoadLevelFlow: Loading level {Args.LevelIndex}");

        await _sceneLoader.LoadAsync(Args.LevelIndex, ctx);

        GKLog.Log(LogState.ProcessFlow, "LoadLevelFlow: Done");
        return this;
    }
}
```

`AsyncExecute` must return `this` (or the next `IProcessFlow`) after finishing work. Returning `this` signals completion and triggers the continuation chain.

---

## Step 3 — Register in the LifetimeScope

Register `ProcessFlowProvider` as a singleton. Individual flows are created via `new()` + `Inject()` inside the provider — **do not** register them as DI types.

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.Register<ProcessFlowProvider>(Lifetime.Singleton);
    }
}
```

---

## Step 4 — Create and execute a flow

Inject `ProcessFlowProvider` and call `CreateProcessFlow<TArgs, TFlow>`:

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameController : SubscribableConcrete
{
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    public void StartLoadLevel(int levelIndex)
    {
        var args = new LoadLevelArgs
        {
            LevelIndex = levelIndex,
            ShowLoadingScreen = true
        };

        var flow = _flowProvider.CreateProcessFlow<LoadLevelArgs, LoadLevelFlow>(args);
        flow.Execute(flow.cancellationToken);
    }
}
```

`CreateProcessFlow` handles `new()`, `Inject()`, `Initialize(args)`, and `AddProcessFlow()` in one call. The provider auto-removes and disposes the flow when it completes.

---

## Step 5 — Chain flows

Use `AppendProcess` to run a second flow after the first finishes:

```csharp
var loadFlow  = _flowProvider.CreateProcessFlow<LoadLevelArgs, LoadLevelFlow>(loadArgs);
var spawnFlow = _flowProvider.CreateProcessFlow<SpawnEnemiesArgs, SpawnEnemiesFlow>(spawnArgs);

loadFlow.AppendProcess(spawnFlow);
loadFlow.Execute(loadFlow.cancellationToken);
// spawnFlow.Execute is called automatically after loadFlow completes
```

---

## Step 6 — React to completion

Register a callback before calling `Execute`:

```csharp
flow.OnComplete(completedFlow =>
{
    GKLog.Log(LogState.Game, "Level loaded successfully.");
});
flow.Execute(flow.cancellationToken);
```

---

## Step 7 — Cancel a flow

```csharp
// Cancel a specific type:
_flowProvider.RemoveProcessFlow<LoadLevelFlow>();

// Cancel all running flows:
_flowProvider.RemoveAllProcessFlows();
```

Calling `RemoveProcessFlow<T>` cancels the token, disposes the flow, and removes it from the provider's internal list.

---

## Flow lifecycle summary

```
CreateProcessFlow  →  Initialize()  →  Execute()
      ↓                                    ↓
  AddProcessFlow                      AsyncExecute()
                                           ↓
                                    ContinuationFunction()
                                    (appended flows run here)
                                           ↓
                                    onComplete callbacks
                                           ↓
                                    provider auto-removes & disposes
```

---

## See also

- [ProcessFlows reference](en-Reference-ProcessFlows)
- [Lifetime Scopes reference](en-Reference-Lifetime-Scopes)
- [Logging reference](en-Reference-Logging)
