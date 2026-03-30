# Tutorial: Building Your First Mini-Game with VGameKit

## What you will build

A minimal two-screen game: a **Main Menu** that transitions into a **HUD** screen. You will wire a menu manager, a process flow, and a spawner, and see all three cooperate via VContainer DI.

**Time:** ~35 minutes  
**Prerequisites:** Completed [Getting Started](./getting-started.md) tutorial; `VGameKit.Runtime` installed.

---

## Step 1 — Define the menu enum and events

```csharp
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public enum GameScreen
{
    MainMenu,
    HUD,
}

public class OpenGameMenuEvent : BaseOpenMenuEvent<GameScreen, MenuData>
{
    public OpenGameMenuEvent(GameScreen menuName, MenuData menuData) : base(menuName, menuData) { }
}

public class CloseGameMenuEvent : BaseCloseMenuEvent<GameScreen>
{
    public CloseGameMenuEvent(GameScreen menuName) : base(menuName) { }
}
```

Enums are the only valid identifiers for menu panels — never use raw strings. Navigation is driven by publishing these events through MessagePipe, keeping callers decoupled from the manager.

---

## Step 2 — Create the views

Create a MonoBehaviour prefab for each screen that inherits `BaseMenuView`:

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView
{
    // UI references (buttons, labels, etc.)
}

public class HUDView : BaseMenuView
{
    // UI references
}
```

---

## Step 3 — Define menu data

```csharp
using VGameKit.Runtime.UI.Menu;

public class HUDData : MenuData
{
    public int StartingScore;
}
```

Use `MenuData` directly when a screen needs no extra parameters. Subclass it when you need to pass typed data at open time.

---

## Step 4 — Create presenters

```csharp
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<GameScreen, MenuData, MainMenuView>
{
    public override GameScreen MenuName => GameScreen.MainMenu;
    public override MenuMode MenuMode => MenuMode.Single;

    protected override void OnShow(MenuData data)
    {
        GKLog.Log(LogState.Game, "Main menu opened.");
    }
}

public class HUDPresenter : BaseMenuPresenter<GameScreen, HUDData, HUDView>
{
    public override GameScreen MenuName => GameScreen.HUD;
    public override MenuMode MenuMode => MenuMode.Single;

    protected override void OnShow(HUDData data)
    {
        GKLog.Log(LogState.Game, "HUD opened.");
    }
}
```

`MenuMode.Single` causes the manager to close all other open panels before opening this one.

---

## Step 5 — Create the menu manager

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public class GameMenuManager : BaseMenuManager<GameScreen>
{
    [Inject] private readonly ISubscriber<OpenGameMenuEvent> _openSubscriber;
    [Inject] private readonly ISubscriber<CloseGameMenuEvent> _closeSubscriber;

    [Inject] private readonly MainMenuPresenter _mainMenuPresenter;
    [Inject] private readonly HUDPresenter _hudPresenter;

    public override void Subscriptions()
    {
        _openSubscriber.Subscribe(e => OpenMenu(e.MenuName, e.MenuData)).AddTo(_bagBuilder);
        _closeSubscriber.Subscribe(e => CloseMenu(e.MenuName)).AddTo(_bagBuilder);
    }

    protected override void OpenMenu(GameScreen menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case GameScreen.MainMenu:
                Open<MainMenuPresenter, MenuData>(_mainMenuPresenter, menuData);
                break;
            case GameScreen.HUD:
                Open<HUDPresenter, HUDData>(_hudPresenter, menuData);
                break;
        }
    }
}
```

`BaseMenuManager` is a MonoBehaviour — attach it to a GameObject in the scene and register it with `RegisterComponent`.

---

## Step 6 — Create the level-load flow

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.Log;

public class StartGameArgs : IProcessFlowArgs { }

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

## Step 7 — Register everything in the LifetimeScope

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.UI.Menu;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private MainMenuView _mainMenuViewPrefab;
    [SerializeField] private HUDView _hudViewPrefab;
    [SerializeField] private GameMenuManager _menuManager;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.RegisterComponent(_mainMenuViewPrefab);
        builder.RegisterComponent(_hudViewPrefab);
        builder.RegisterComponent(_menuManager);

        builder.RegisterMenuFactory<GameScreen, MainMenuPresenter, MainMenuView>(
            _mainMenuViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);
        builder.RegisterMenuFactory<GameScreen, HUDPresenter, HUDView>(
            _hudViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);

        builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<HUDPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.RegisterProcessFlow<StartGameArgs, StartGameFlow>(Lifetime.Singleton);
        builder.Register<MyAppManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Step 8 — Trigger the flow from the main menu

```csharp
using System;
using System.Threading;
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.UI.Menu;

public class MainMenuController : SubscribableConcrete
{
    [Inject] private readonly IPublisher<OpenGameMenuEvent> _openMenuPublisher;
    [Inject] private readonly Func<StartGameArgs, StartGameFlow> _startGameFlow;

    private CancellationTokenSource _cts;

    protected override void Init()
    {
        _cts = new CancellationTokenSource();
    }

    public override void Dispose()
    {
        _cts?.Cancel();
        base.Dispose();
    }

    public void OnPlayButtonPressed()
    {
        var flow = _startGameFlow(new StartGameArgs());
        flow.OnComplete(_ =>
            _openMenuPublisher.Publish(new OpenGameMenuEvent(GameScreen.HUD, new HUDData { StartingScore = 0 }))
        );
        flow.Execute(_cts.Token);
    }
}
```

Publishing `OpenGameMenuEvent` instead of calling `_menuManager.Open()` directly keeps `MainMenuController` decoupled from the manager implementation.

---

## Step 9 — Run the scene

1. Attach `GameLifetimeScope` to an empty GameObject.
2. Assign the prefab and manager references in the Inspector.
3. Enter Play Mode and click the Play button.
4. Observe the Console sequence:

```
[Game] Main menu opened.
[Game] StartGameFlow: loading game...
[Game] StartGameFlow: done.
[Game] HUD opened.
```

---

## What you learned

- Defining menu navigation via MessagePipe events rather than direct manager calls.
- Subclassing `BaseMenuPresenter<TEnum, TData, TView>` with three type parameters.
- Overriding `Subscriptions()` and `OpenMenu()` in `BaseMenuManager`.
- Registering MonoBehaviour managers with `RegisterComponent` and prefabs with `RegisterMenuFactory`.
- Encapsulating async game logic inside `BaseProcessFlow<TArgs>`.

---

## Next steps

- [Tutorial: UI System deep dive](./ui-system.md)
- [Tutorial: Spawner basics](./spawner-basics.md)
- [How-to: Manage process flows](../how-to/manage-processflows.md)
