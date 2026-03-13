# Tutorial: Building Your First Mini-Game with VGameKit

## What you will build

A minimal two-screen game: a **Main Menu** that transitions into a **Game** scene. You will wire a menu manager, a process flow, and a spawner, and see all three cooperate via VContainer DI.

**Time:** ~35 minutes  
**Prerequisites:** Completed [Getting Started](./getting-started.md) tutorial; `VGameKit.Runtime` installed.

---

## Step 1 — Define the menu enum

```csharp
public enum GameScreen
{
    MainMenu,
    HUD,
}
```

Enums are the only valid identifiers for menu panels — never use raw strings.

---

## Step 2 — Create the menu manager

```csharp
using VGameKit.Runtime.UI.Menu;

public class GameMenuManager : BaseMenuManager<GameScreen>
{
}
```

`BaseMenuManager<TEnum>` tracks open panels, handles `MenuMode.Single` (auto-close others), and provides `Open(GameScreen)` / `Close(GameScreen)`.

---

## Step 3 — Create presenters for each screen

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<GameScreen>
{
    public override GameScreen MenuId => GameScreen.MainMenu;

    protected override void OnOpen()
    {
        GKLog.Log(LogState.Game, "Main menu opened.");
    }
}
```

Repeat for `HUDPresenter` with `MenuId = GameScreen.HUD`.

---

## Step 4 — Create the level-load flow

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.Log;

public class StartGameArgs : BaseProcessFlowArgs { }

public class StartGameFlow : BaseProcessFlow<StartGameArgs>
{
    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.Game, "StartGameFlow: loading game...");
        await UniTask.Delay(500, cancellationToken: ctx); // simulate async work
        GKLog.Log(LogState.Game, "StartGameFlow: done.");
        return this;
    }
}
```

---

## Step 5 — Register everything in the LifetimeScope

```csharp
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.Register<GameMenuManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<HUDPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton);
        builder.Register<MyAppManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Step 6 — Trigger the flow from the main menu button

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class MainMenuController : SubscribableConcrete
{
    [Inject] private readonly GameMenuManager _menuManager;
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    public void OnPlayButtonPressed()
    {
        var flow = _flowProvider.CreateProcessFlow<StartGameArgs, StartGameFlow>(new StartGameArgs());
        flow.OnComplete(_ => _menuManager.Open(GameScreen.HUD));
        flow.Execute(flow.cancellationToken);
    }
}
```

---

## Step 7 — Run the scene

1. Attach `GameLifetimeScope` to an empty GameObject.
2. Enter Play Mode and click the Play button.
3. Observe the Console sequence:

```
[Game] Main menu opened.
[Game] StartGameFlow: loading game...
[Game] StartGameFlow: done.
[Game] HUD opened.
```

---

## What you learned

- Driving UI transitions with `BaseMenuManager<TEnum>` and typed presenters.
- Encapsulating async game logic inside `BaseProcessFlow<TArgs>`.
- Chaining a completion callback (`OnComplete`) to coordinate UI and logic.
- Keeping all wiring in the LifetimeScope.

---

## Next steps

- [Tutorial: UI System deep dive](./ui-system.md)
- [Tutorial: Spawner basics](./spawner-basics.md)
- [How-to: Manage process flows](../how-to/manage-processflows.md)
