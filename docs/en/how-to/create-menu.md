# How to Create a Menu

## Overview

This guide shows how to add a new full-screen or overlay menu using the VGameKit menu system. The system requires five pieces: an **enum identifier**, a **View** (MonoBehaviour + prefab), **MenuData**, a **Presenter**, and a **Manager**.

Menu navigation is driven by **MessagePipe events**, not by calling the Manager directly. This keeps the code that triggers navigation decoupled from the Manager.

**Prerequisites:** `VGameKit.Runtime` module installed; an existing `AbsBaseLifetimeScope` for the scene.

---

## Step 1 — Define a menu name enum and events

Create the enum that identifies menus in your scene, and three menu event classes. Use a single enum per Manager.

```csharp
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public enum GameMenuName
{
    MainMenu,
    PauseMenu,
    LevelComplete,
}

// Open a menu
public class OpenMenuEvent : BaseOpenMenuEvent<GameMenuName, MenuData>
{
    public OpenMenuEvent(GameMenuName menuName, MenuData menuData) : base(menuName, menuData) { }
}

// Close a specific menu
public class CloseMenuEvent : BaseCloseMenuEvent<GameMenuName>
{
    public CloseMenuEvent(GameMenuName menuName) : base(menuName) { }
}

// Close all menus except the ones listed.
// BaseCloseOthersMenuEvent's constructor accepts the array but does not store it —
// define KeepMenuNames in the concrete class and assign it explicitly.
public class CloseOtherMenuEvent : BaseCloseOthersMenuEvent<GameMenuName>
{
    public GameMenuName[] KeepMenuNames { get; }
    public CloseOtherMenuEvent(params GameMenuName[] keepMenuNames)
        : base(keepMenuNames) => KeepMenuNames = keepMenuNames;
}
```

---

## Step 2 — Create the View

Create a MonoBehaviour that inherits `BaseMenuView`. Attach it to a UI prefab.

```csharp
using UnityEngine;
using VGameKit.Runtime.UI.Menu;

public class LevelCompleteView : BaseMenuView
{
    [SerializeField] private UnityEngine.UI.Text _scoreText;

    public void SetScore(int score) => _scoreText.text = score.ToString();
}
```

- Tick **Destroy When Closed** in the Inspector only if the prefab should be destroyed on close (default: reuse via `SetActive`).
- The prefab is instantiated under the Manager's `_menuRoot` transform at runtime; do **not** place it in the scene hierarchy directly.

---

## Step 3 — Define MenuData

`MenuData` is the base class for passing data to a presenter when a menu is opened. Use it directly when you have nothing to pass; subclass it when you do.

```csharp
using VGameKit.Runtime.UI.Menu;

// With data — subclass MenuData:
public class LevelCompleteData : MenuData
{
    public int Score;
    public int StarsEarned;
}
```

---

## Step 4 — Create the Presenter

Create a class that inherits `BaseMenuPresenter<TMenuName, TData, TMenu>`. The `TData` type parameter determines what type `OnShow` receives.

```csharp
using VGameKit.Runtime.UI.Menu;

public class LevelCompletePresenter : BaseMenuPresenter<GameMenuName, LevelCompleteData, LevelCompleteView>
{
    public override GameMenuName MenuName => GameMenuName.LevelComplete;
    public override MenuMode MenuMode => MenuMode.Additive; // opens on top of other menus

    protected override void OnShow(LevelCompleteData data)
    {
        View.SetScore(data.Score);
    }

    protected override void OnHide()
    {
        // Remove listeners, etc.
    }
}
```

Key overrides:

| Override | When called |
|---|---|
| `OnShow(data)` | After the view becomes active |
| `OnShowBefore(data)` | Before `Open()` sets the view active (only if `didAwake` is true) |
| `OnHide()` | Before the view is hidden or destroyed |
| `UpdatePresenter()` | When the view already exists and `Open()` is called again |

---

## Step 5 — Create the Manager

Create a MonoBehaviour that inherits `BaseMenuManager<TMenuName>`. Because `BaseMenuManager` extends `SubscribableMonoBehaviour`, subscribe to menu events inside `Subscriptions()`. Do **not** add public `Show()`/`Hide()` methods — all navigation goes through event publishing.

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public class MenuManager : BaseMenuManager<GameMenuName>
{
    [Inject] private readonly ISubscriber<OpenMenuEvent> _openMenuSubscriber;
    [Inject] private readonly ISubscriber<CloseMenuEvent> _closeMenuSubscriber;
    [Inject] private readonly ISubscriber<CloseOtherMenuEvent> _closeOtherMenuSubscriber;

    [Inject] private readonly LevelCompletePresenter _levelCompletePresenter;

    public override void Subscriptions()
    {
        _openMenuSubscriber.Subscribe(e => OpenMenuHandler(e)).AddTo(_bagBuilder);
        _closeMenuSubscriber.Subscribe(e => CloseMenuHandler(e)).AddTo(_bagBuilder);
        _closeOtherMenuSubscriber.Subscribe(e => CloseOtherMenuHandler(e)).AddTo(_bagBuilder);
    }

    protected override void OpenMenu(GameMenuName menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case GameMenuName.LevelComplete:
                // The second type parameter tells Open() what type to cast menuData to.
                Open<LevelCompletePresenter, LevelCompleteData>(_levelCompletePresenter, menuData);
                break;
        }
    }

    private void OpenMenuHandler(OpenMenuEvent e) => OpenMenu(e.MenuName, e.MenuData);
    private void CloseMenuHandler(CloseMenuEvent e) => CloseMenu(e.MenuName);
    private void CloseOtherMenuHandler(CloseOtherMenuEvent e) => CloseOthers(e.KeepMenuNames);
}
```

---

## Step 6 — Register in the LifetimeScope

In your scene's `AbsBaseLifetimeScope`, register the view prefab, the menu factory, and the presenter.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.UI.Menu;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private LevelCompleteView _levelCompleteViewPrefab;
    [SerializeField] private MenuManager _menuManager;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // Register the view prefab as a component (required for factory resolution)
        builder.RegisterComponent(_levelCompleteViewPrefab);

        // Register the manager (scene MonoBehaviour)
        builder.RegisterComponent(_menuManager);

        // Register the menu factory:
        // - TMenuName  : GameMenuName
        // - TPresenter : LevelCompletePresenter
        // - TMenu      : LevelCompleteView
        // _menuManager.MenuRoot: the transform views are parented under
        builder.RegisterMenuFactory<GameMenuName, LevelCompletePresenter, LevelCompleteView>(
            _levelCompleteViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);

        // Register the presenter (RegisterMenuFactory injects it)
        builder.Register<LevelCompletePresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

In the Inspector, drag the scene's `MenuManager` component into `_menuManager` and the prefab into `_levelCompleteViewPrefab`.

---

## Step 7 — Open a menu at runtime

Publish an `OpenMenuEvent` rather than calling the Manager directly.

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;

public class GameFlowPresenter : SubscribableConcrete
{
    [Inject] private readonly IPublisher<OpenMenuEvent> _openMenuPublisher;
    [Inject] private readonly IPublisher<CloseMenuEvent> _closeMenuPublisher;

    public void ShowLevelComplete(int score)
    {
        _openMenuPublisher.Publish(
            new OpenMenuEvent(GameMenuName.LevelComplete, new LevelCompleteData { Score = score }));
    }

    public void HideLevelComplete()
    {
        _closeMenuPublisher.Publish(new CloseMenuEvent(GameMenuName.LevelComplete));
    }
}
```

---

## MenuMode quick reference

| Mode | Behaviour |
|---|---|
| `MenuMode.Single` | Calls `CloseOthers()` before opening — use for full-screen menus |
| `MenuMode.Additive` | Opens on top of existing menus — use for overlays and HUD panels |

---

## See also

- [Menu System reference](../reference/api/menu-system.md)
- [Lifetime Scopes reference](../reference/api/lifetime-scopes.md)
- [Subscribable reference](../reference/api/subscribable.md)
