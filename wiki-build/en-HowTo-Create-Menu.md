# How to Create a Menu

## Overview

This guide shows how to add a new full-screen or overlay menu using the VGameKit menu system. The system requires four pieces: an **enum identifier**, a **View** (MonoBehaviour + prefab), a **Presenter**, and a **Manager**.

**Prerequisites:** `VGameKit.Runtime` module installed; an existing `AbsBaseLifetimeScope` for the scene.

---

## Step 1 — Define a menu name enum

Create or extend the enum that identifies menus in your scene. Use a single enum per manager.

```csharp
public enum GameMenuName
{
    MainMenu,
    PauseMenu,
    SettingsMenu,
}
```

---

## Step 2 — Create the View

Create a MonoBehaviour that inherits `BaseMenuView` and implements `IMenu`. Attach it to a UI prefab.

```csharp
using UnityEngine;
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView, IMenu
{
    // Add UI references here (buttons, labels, etc.)
    [SerializeField] private UnityEngine.UI.Button _playButton;

    public UnityEngine.UI.Button PlayButton => _playButton;
}
```

- Tick **Destroy When Closed** in the Inspector only if the prefab should be destroyed on close (default: reuse via `SetActive`).
- Parent the prefab under the Manager's `_menuRoot` transform at runtime; do **not** place it in the scene hierarchy directly.

---

## Step 3 — Create the Presenter

Create a class that inherits `BaseMenuPresenter<TMenuName, TData, TMenu>`.

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<GameMenuName, MenuData, MainMenuView>
{
    public override GameMenuName MenuName => GameMenuName.MainMenu;
    public override MenuMode MenuMode => MenuMode.Single; // closes others on open

    // Inject dependencies as needed
    [Inject] private readonly IPublisher<StartGameEvent> _startGamePublisher;

    protected override void OnShow(MenuData menuData)
    {
        View.PlayButton.onClick.AddListener(OnPlayClicked);
    }

    protected override void OnHide()
    {
        View.PlayButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        _startGamePublisher.Publish(new StartGameEvent());
    }
}
```

Key overrides:

| Override | When called |
|----------|------------|
| `OnShow(data)` | After the view becomes active |
| `OnShowBefore(data)` | Before `Open()` sets the view active (only if `didAwake` is true) |
| `OnHide()` | Before the view is hidden or destroyed |
| `UpdatePresenter()` | When the view already exists and `Open()` is called again |

---

## Step 4 — Create the Manager

Create a MonoBehaviour that inherits `BaseMenuManager<TMenuName>` and add it to your scene root.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.UI.Menu;

public class GameMenuManager : BaseMenuManager<GameMenuName>
{
    [Inject] private readonly MainMenuPresenter _mainMenuPresenter;
    [Inject] private readonly PauseMenuPresenter _pauseMenuPresenter;

    protected override void OpenMenu(GameMenuName menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case GameMenuName.MainMenu:
                Open<MainMenuPresenter, MenuData>(_mainMenuPresenter, menuData);
                break;
            case GameMenuName.PauseMenu:
                Open<PauseMenuPresenter, MenuData>(_pauseMenuPresenter, menuData);
                break;
        }
    }

    public void Show(GameMenuName menuName, MenuData data = null)
    {
        OpenMenu(menuName, data);
    }

    public void Hide(GameMenuName menuName)
    {
        CloseMenu(menuName);
    }
}
```

---

## Step 5 — Register in the LifetimeScope

In your scene's `AbsBaseLifetimeScope`, register the prefab factory for each view and each presenter as a singleton.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private MainMenuView _mainMenuViewPrefab;
    [SerializeField] private Transform _menuRoot;  // same Transform assigned to GameMenuManager._menuRoot

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // Register the view factory (DI-instantiated under menuRoot)
        builder.RegisterFactory<MainMenuView>(
            container => () => container.Instantiate(_mainMenuViewPrefab, _menuRoot),
            Lifetime.Singleton);

        // Register presenters
        builder.Register<MainMenuPresenter>(Lifetime.Singleton);
        builder.Register<PauseMenuPresenter>(Lifetime.Singleton);

        // Register the manager (it's a MonoBehaviour, so use RegisterComponent)
        // Add GameMenuManager to a GameObject and drag it into this field:
        builder.RegisterComponentInHierarchy<GameMenuManager>();
    }
}
```

---

## Step 6 — Open a menu at runtime

Inject `GameMenuManager` wherever you need to trigger navigation:

```csharp
public class SomePresenter : SubscribableConcrete
{
    [Inject] private readonly GameMenuManager _menuManager;

    public override void Init()
    {
        _menuManager.Show(GameMenuName.MainMenu);
    }
}
```

---

## MenuMode quick reference

| Mode | Behaviour |
|------|-----------|
| `MenuMode.Single` | Calls `CloseOthers()` before opening — use for full-screen menus |
| `MenuMode.Additive` | Opens on top of existing menus — use for overlays and HUD panels |

---

## See also

- [Menu System reference](en-Reference-Menu-System)
- [Lifetime Scopes reference](en-Reference-Lifetime-Scopes)
- [Subscribable reference](en-Reference-Subscribable)
