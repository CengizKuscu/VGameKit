# Tutorial: Learning the UI System

## What you will build

A two-panel UI (Main Menu + Settings), a confirmation popup, and the wiring that makes them work together via `BaseMenuManager`, `BaseMenuPresenter`, and the popup builder.

**Time:** ~25 minutes  
**Prerequisites:** Completed [Getting Started](en-Tutorial-Getting-Started); `VGameKit.Runtime` installed.

---

## Step 1 — Define menu identifiers

```csharp
public enum AppScreen
{
    MainMenu,
    Settings,
}
```

---

## Step 2 — Create the menu manager

```csharp
using VGameKit.Runtime.UI.Menu;

public class AppMenuManager : BaseMenuManager<AppScreen>
{
}
```

---

## Step 3 — Create and attach views

For each screen:

1. Create a Canvas GameObject (e.g., `MainMenuView`).
2. Add a C# component that inherits `BaseMenuView<AppScreen>`:

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView<AppScreen>
{
    public override AppScreen MenuId => AppScreen.MainMenu;
}
```

Repeat for `SettingsView` with `MenuId = AppScreen.Settings`.

---

## Step 4 — Create presenters

```csharp
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<AppScreen>
{
    [Inject] private readonly AppMenuManager _menuManager;

    public override AppScreen MenuId => AppScreen.MainMenu;

    protected override void OnOpen()
    {
        GKLog.Log(LogState.Game, "MainMenu opened.");
    }

    public void OnSettingsPressed()
    {
        _menuManager.Open(AppScreen.Settings);
        // MenuMode.Single closes MainMenu automatically
    }
}
```

---

## Step 5 — Create a popup model

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitModel : BasePopupModel
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

public class SettingsPresenter : BaseMenuPresenter<AppScreen>
{
    [Inject] private readonly PopupBuilder _popupBuilder;

    public override AppScreen MenuId => AppScreen.Settings;

    public void OnQuitPressed()
    {
        var model = new ConfirmQuitModel
        {
            Message = "Are you sure you want to quit?",
            OnConfirmed = () => UnityEngine.Application.Quit()
        };

        _popupBuilder
            .AddPopup<ConfirmQuitModel, ConfirmQuitPopup>(model)
            .OpenPopup();
    }
}
```

---

## Step 7 — Implement the popup view

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitPopup : BasePopupView<ConfirmQuitModel>
{
    protected override void OnOpen(ConfirmQuitModel model)
    {
        // Bind UI elements to model data
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
builder.Register<AppMenuManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
builder.Register<SettingsPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
builder.Register<PopupBuilder>(Lifetime.Singleton);
```

---

## Step 9 — Run the scene

Enter Play Mode. Click **Settings** — Main Menu should close and Settings should open. Click **Quit** — the confirmation popup should appear.

---

## What you learned

- How `BaseMenuManager<TEnum>` controls panel visibility with `MenuMode.Single`.
- How `BaseMenuPresenter` and `BaseMenuView` pair to represent each screen.
- How the popup builder chains `AddPopup` / `OpenPopup` with typed models.

---

## Next steps

- [Tutorial: Spawner basics](en-Tutorial-Spawner-Basics)
- [How-to: Create a menu](en-HowTo-Create-Menu)
- [Reference: Menu System](en-Reference-Menu-System)
- [Reference: Popup System](en-Reference-Popup-System)
