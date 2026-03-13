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

public class ValidateSaveArgs : BaseProcessFlowArgs
{
    public string SaveSlot;
}

public class LoadAssetsArgs : BaseProcessFlowArgs
{
    public string[] BundleNames;
}

public class ShowHUDArgs : BaseProcessFlowArgs { }
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

## Step 5 — Register ProcessFlowProvider

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton);
        builder.Register<GameStarter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Step 6 — Chain and execute the flows

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class GameStarter : SubscribableConcrete
{
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    protected override void Init()
    {
        StartSequence();
    }

    private void StartSequence()
    {
        var validateFlow = _flowProvider.CreateProcessFlow<ValidateSaveArgs, ValidateSaveFlow>(
            new ValidateSaveArgs { SaveSlot = "slot_0" });

        var loadFlow = _flowProvider.CreateProcessFlow<LoadAssetsArgs, LoadAssetsFlow>(
            new LoadAssetsArgs { BundleNames = new[] { "ui", "audio" } });

        var hudFlow = _flowProvider.CreateProcessFlow<ShowHUDArgs, ShowHUDFlow>(
            new ShowHUDArgs());

        // Chain: validate → load → hud
        validateFlow.AppendProcess(loadFlow);
        loadFlow.AppendProcess(hudFlow);

        hudFlow.OnComplete(_ =>
            GKLog.Log(LogState.Game, "GameStarter: start sequence complete."));

        validateFlow.Execute(validateFlow.cancellationToken);
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

To simulate a cancellation (e.g., the player disconnects):

```csharp
_flowProvider.RemoveProcessFlow<LoadAssetsFlow>();
// All pending appended flows are also cancelled.
```

---

## What you learned

- How to split complex sequences into discrete, single-responsibility flows.
- How `AppendProcess` chains flows without tight coupling.
- How `OnComplete` lets callers react without modifying the flow itself.
- How `RemoveProcessFlow<T>` provides clean mid-sequence cancellation.

---

## Next steps

- [How-to: Manage process flows](../how-to/manage-processflows.md)
- [Reference: ProcessFlows](../reference/api/processflows.md)
- [Reference: Logging](../reference/api/logging.md)
