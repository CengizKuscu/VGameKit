# Tutorial: Learning the UI System

## What you will build

A two-panel UI (Main Menu + Settings), a confirmation popup, and the wiring that makes them work together via `BaseMenuManager`, `BaseMenuPresenter`, and the popup builder.

**Time:** ~25 minutes  
**Prerequisites:** Completed [Getting Started](./getting-started.md); `VGameKit.Runtime` installed.

---

## Step 1 — Define menu identifiers and events

```csharp
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public enum AppScreen
{
    MainMenu,
    Settings,
}

public class OpenMenuEvent : BaseOpenMenuEvent<AppScreen, MenuData>
{
    public OpenMenuEvent(AppScreen menuName, MenuData menuData) : base(menuName, menuData) { }
}

public class CloseMenuEvent : BaseCloseMenuEvent<AppScreen>
{
    public CloseMenuEvent(AppScreen menuName) : base(menuName) { }
}
```

---

## Step 2 — Create views

Create a MonoBehaviour prefab for each screen that inherits `BaseMenuView`:

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView
{
    // UI references (buttons, labels, etc.)
}

public class SettingsView : BaseMenuView
{
    // UI references
}
```

---

## Step 3 — Create presenters

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<AppScreen, MenuData, MainMenuView>
{
    public override AppScreen MenuName => AppScreen.MainMenu;
    public override MenuMode MenuMode => MenuMode.Single;

    [Inject] private readonly IPublisher<OpenMenuEvent> _openMenuPublisher;

    protected override void OnShow(MenuData data)
    {
        GKLog.Log(LogState.Game, "MainMenu opened.");
    }

    public void OnSettingsPressed()
    {
        // MenuMode.Single closes MainMenu automatically when Settings opens
        _openMenuPublisher.Publish(new OpenMenuEvent(AppScreen.Settings, null));
    }
}

public class SettingsPresenter : BaseMenuPresenter<AppScreen, MenuData, SettingsView>
{
    public override AppScreen MenuName => AppScreen.Settings;
    public override MenuMode MenuMode => MenuMode.Single;
}
```

---

## Step 4 — Create the menu manager

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public class AppMenuManager : BaseMenuManager<AppScreen>
{
    [Inject] private readonly ISubscriber<OpenMenuEvent> _openSubscriber;
    [Inject] private readonly ISubscriber<CloseMenuEvent> _closeSubscriber;

    [Inject] private readonly MainMenuPresenter _mainMenuPresenter;
    [Inject] private readonly SettingsPresenter _settingsPresenter;

    public override void Subscriptions()
    {
        _openSubscriber.Subscribe(e => OpenMenuHandler(e)).AddTo(_bagBuilder);
        _closeSubscriber.Subscribe(e => CloseMenu(e.MenuName)).AddTo(_bagBuilder);
    }

    protected override void OpenMenu(AppScreen menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case AppScreen.MainMenu:
                Open<MainMenuPresenter, MenuData>(_mainMenuPresenter, menuData);
                break;
            case AppScreen.Settings:
                Open<SettingsPresenter, MenuData>(_settingsPresenter, menuData);
                break;
        }
    }

    private void OpenMenuHandler(OpenMenuEvent e) => OpenMenu(e.MenuName, e.MenuData);
}
```

---

## Step 5 — Create a popup model

`BasePopupModel<TPopupName>` requires the popup name enum as a type parameter.

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitModel : BasePopupModel<AppScreen>
{
    public string Message;
    public System.Action OnConfirmed;
}
```

---

## Step 6 — Build and show the popup

```csharp
using VContainer;
using VGameKit.Runtime.UI.Popup;

public class SettingsPresenter : BaseMenuPresenter<AppScreen, MenuData, SettingsView>
{
    [Inject] private readonly DemoPopupBuilder _popupBuilder;

    public override AppScreen MenuName => AppScreen.Settings;
    public override MenuMode MenuMode => MenuMode.Single;

    public void OnQuitPressed()
    {
        var model = new ConfirmQuitModel
        {
            Message = "Are you sure you want to quit?",
            OnConfirmed = () => UnityEngine.Application.Quit()
        };

        _popupBuilder
            .AddPopup(AppScreen.Settings, model)
            .OpenPopup();
    }
}
```

---

## Step 7 — Implement the popup view

Inherit from `BasePopup<TPopupName>`. Access the model via the `_model` field (type `BasePopupModel<TPopupName>`) and cast it to your concrete model type inside `OnShowBefore()`.

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitPopup : BasePopup<AppScreen>
{
    protected override void OnShowBefore()
    {
        var model = _model as ConfirmQuitModel;
        _messageLabel.text = model.Message;
        _confirmButton.onClick.AddListener(() =>
        {
            model.OnConfirmed?.Invoke();
            Close();
        });
    }
}
```

---

## Step 8 — Register in LifetimeScope

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.UI.Menu;

public class AppLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private MainMenuView _mainMenuViewPrefab;
    [SerializeField] private SettingsView _settingsViewPrefab;
    [SerializeField] private AppMenuManager _menuManager;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.RegisterComponent(_mainMenuViewPrefab);
        builder.RegisterComponent(_settingsViewPrefab);
        builder.RegisterComponent(_menuManager);

        builder.RegisterMenuFactory<AppScreen, MainMenuPresenter, MainMenuView>(
            _mainMenuViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);
        builder.RegisterMenuFactory<AppScreen, SettingsPresenter, SettingsView>(
            _settingsViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);

        builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<SettingsPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Step 9 — Run the scene

Enter Play Mode. Click **Settings** — Main Menu should close and Settings should open. Click **Quit** — the confirmation popup should appear.

---

## What you learned

- How `BaseMenuManager<TEnum>` controls panel visibility with `MenuMode.Single`.
- How navigation is driven by MessagePipe events, keeping callers decoupled from the manager.
- How `BaseMenuPresenter` and `BaseMenuView` pair to represent each screen.
- How the popup builder chains `AddPopup` / `OpenPopup` with typed models.

---

## Next steps

- [Tutorial: Spawner basics](./spawner-basics.md)
- [How-to: Create a menu](../how-to/create-menu.md)
- [Reference: Menu System](../reference/api/menu-system.md)
- [Reference: Popup System](../reference/api/popup-system.md)
