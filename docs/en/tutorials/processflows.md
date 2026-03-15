# Tutorial: Learning Process Flows

## What you will build

A three-step game-start sequence — **validate save**, **load assets**, **show HUD** — where each step is a separate `BaseProcessFlow`, they run in order, each is cancellable, and the result is displayed in the Console.

**Time:** ~30 minutes  
**Prerequisites:** Completed [Getting Started](./getting-started.md); `VGameKit.Runtime` installed.

---

## Step 1 — Create args classes

Each flow carries its own argument type.

```csharp
using VGameKit.Runtime.ProcessFlows;

// Implement IProcessFlowArgs directly — BaseProcessFlowArgs is a struct and cannot
// be inherited. Use it only for zero-input flows where you need a typed empty value.
public class ValidateSaveArgs : IProcessFlowArgs
{
    public string SaveSlot;
}

public class LoadAssetsArgs : IProcessFlowArgs
{
    public string[] BundleNames;
}

public class ShowHUDArgs : IProcessFlowArgs { }
```

---

## Step 2 — Implement ValidateSaveFlow

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class ValidateSaveFlow : BaseProcessFlow<ValidateSaveArgs>
{
    protected override void Initialize()
    {
        GKLog.Log(LogState.Development, $"ValidateSaveFlow: checking slot '{Args.SaveSlot}'");
    }

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        await UniTask.Delay(100, cancellationToken: ctx); // simulate I/O
        GKLog.Log(LogState.Game, "ValidateSaveFlow: save valid.");
        return this;
    }
}
```

---

## Step 3 — Implement LoadAssetsFlow

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class LoadAssetsFlow : BaseProcessFlow<LoadAssetsArgs>
{
    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        foreach (var bundle in Args.BundleNames)
        {
            GKLog.Log(LogState.Development, $"LoadAssetsFlow: loading '{bundle}'");
            await UniTask.Delay(150, cancellationToken: ctx);
        }
        GKLog.Log(LogState.Game, "LoadAssetsFlow: all assets loaded.");
        return this;
    }
}
```

---

## Step 4 — Implement ShowHUDFlow

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class ShowHUDFlow : BaseProcessFlow<ShowHUDArgs>
{
    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.Game, "ShowHUDFlow: HUD displayed.");
        await UniTask.CompletedTask;
        return this;
    }
}
```

---

## Step 5 — Register flows in the LifetimeScope

Use `RegisterProcessFlow<TArgs, TFlow>` to register a `Func<TArgs, TFlow>` factory for each flow. `ProcessFlowProvider` must also be registered explicitly — `RegisterProcessFlow` calls `container.Resolve<ProcessFlowProvider>()` internally and will fail at runtime if the provider is not in the container.

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // ProcessFlowProvider must be registered first — RegisterProcessFlow depends on it.
        // AsImplementedInterfaces() lets VContainer call IDisposable.Dispose() automatically.
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        // Each call registers a Func<TArgs, TFlow> factory
        builder.RegisterProcessFlow<ValidateSaveArgs, ValidateSaveFlow>(Lifetime.Singleton);
        builder.RegisterProcessFlow<LoadAssetsArgs, LoadAssetsFlow>(Lifetime.Singleton);
        builder.RegisterProcessFlow<ShowHUDArgs, ShowHUDFlow>(Lifetime.Singleton);

        builder.Register<GameStarter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Step 6 — Chain and execute the flows

Inject `Func<TArgs, TFlow>` factories directly — do not inject `ProcessFlowProvider` for flow creation. The caller owns a `CancellationTokenSource` and passes its token to `Execute`. Since `GameStarter` extends `SubscribableConcrete`, create `_cts` in `Init()` and cancel it by overriding `Dispose()`:

```csharp
using System;
using System.Threading;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class GameStarter : SubscribableConcrete
{
    [Inject] private readonly Func<ValidateSaveArgs, ValidateSaveFlow> _validateFlow;
    [Inject] private readonly Func<LoadAssetsArgs, LoadAssetsFlow> _loadFlow;
    [Inject] private readonly Func<ShowHUDArgs, ShowHUDFlow> _hudFlow;

    private CancellationTokenSource _cts;

    protected override void Init()
    {
        _cts = new CancellationTokenSource();
        StartSequence();
    }

    public override void Dispose()
    {
        _cts?.Cancel();
        base.Dispose();
    }

    private void StartSequence()
    {
        var validateFlow = _validateFlow(new ValidateSaveArgs { SaveSlot = "slot_0" });
        var loadFlow     = _loadFlow(new LoadAssetsArgs { BundleNames = new[] { "ui", "audio" } });
        var hudFlow      = _hudFlow(new ShowHUDArgs());

        // Chain: validate → load → hud
        validateFlow.AppendProcess(loadFlow);
        loadFlow.AppendProcess(hudFlow);

        hudFlow.OnComplete(_ =>
            GKLog.Log(LogState.Game, "GameStarter: start sequence complete."));

        validateFlow.Execute(_cts.Token);
    }
}
```

---

## Step 7 — Run and observe

Enter Play Mode. Console output should appear in order:

```
[Development] ValidateSaveFlow: checking slot 'slot_0'
[Game]        ValidateSaveFlow: save valid.
[Development] LoadAssetsFlow: loading 'ui'
[Development] LoadAssetsFlow: loading 'audio'
[Game]        LoadAssetsFlow: all assets loaded.
[Game]        ShowHUDFlow: HUD displayed.
[Game]        GameStarter: start sequence complete.
```

---

## Step 8 — Cancel mid-sequence

To cancel via the `CancellationTokenSource` already available in `GameStarter`, call `_cts.Cancel()` — this cancels all chained flows immediately:

```csharp
// Option A — cancel everything via the CancellationTokenSource (already in GameStarter):
_cts.Cancel();
// All flows in the chain receive the cancellation signal simultaneously.
```

To cancel only a specific flow type, inject `ProcessFlowProvider` into a separate manager class:

```csharp
// Option B — cancel a specific flow type via ProcessFlowProvider:
public class GameController : IInitializable, IDisposable
{
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    public void Initialize() { }
    public void Dispose() { }

    public void AbortLoading()
    {
        _flowProvider.RemoveProcessFlow<LoadAssetsFlow>();
        // Pending appended flows are also cancelled.
    }
}
```

---

## What you learned

- How to split complex sequences into discrete, single-responsibility flows.
- How `AppendProcess` chains flows without tight coupling.
- How `OnComplete` lets callers react without modifying the flow itself.
- How `CancellationTokenSource` cancels an entire chain, and how `RemoveProcessFlow<T>` cancels a specific flow type via `ProcessFlowProvider`.

---

## Next steps

- [How-to: Manage process flows](../how-to/manage-processflows.md)
- [Reference: ProcessFlows](../reference/api/processflows.md)
- [Reference: Logging](../reference/api/logging.md)
