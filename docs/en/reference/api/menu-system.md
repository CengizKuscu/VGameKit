# Menu System Reference

**Namespace:** `VGameKit.Runtime.UI.Menu`
**Assembly:** `VGameKit.Runtime`
**Files:** `Assets/VGameKit/Runtime/UI/Menu/`

---

## Overview

The menu system implements a View-Presenter separation pattern over a typed enum-keyed manager. Each menu is associated with:
- A **View** (`BaseMenuView` / `IMenu`) — the Unity `MonoBehaviour` that controls the `GameObject`
- A **Presenter** (`BaseMenuPresenter<TMenuName, TData, TMenu>`) — the DI-injected logic layer
- A **Manager** (`BaseMenuManager<TMenuName>`) — the `SubscribableMonoBehaviour` that orchestrates open/close

Menu behavior is controlled by `MenuMode`: `Single` closes all other menus on open; `Additive` keeps them open.

---

## Type Overview

| Type | Kind | Description |
|---|---|---|
| `IMenu` | interface | Menu view contract |
| `IMenuManager<TMenuName>` | interface | Manager contract |
| `IMenuPresenter<TMenuName>` | interface | Presenter contract |
| `MenuMode` | enum | `Single` or `Additive` |
| `MenuData` | class | Base data passed to a menu on open |
| `BaseMenuView` | class | Default `MonoBehaviour` view implementation |
| `BaseMenuPresenter<TMenuName, TData, TMenu>` | abstract class | Presenter base |
| `BaseMenuManager<TMenuName>` | abstract class | Manager base |
| `MenuExtensions` | static class | DI helper |
| `BaseOpenMenuEvent<TMenuName, TMenuData>` | class | MessagePipe event to open a menu |
| `BaseCloseMenuEvent<TMenuName>` | class | MessagePipe event to close a menu |
| `BaseCloseOthersMenuEvent<TMenuName>` | class | MessagePipe event to close all others |

---

## Enum: `MenuMode`

```csharp
public enum MenuMode { Additive, Single }
```

| Value | Behaviour |
|---|---|
| `Additive` | Opens the menu without closing others |
| `Single` | Calls `CloseOthers()` before opening |

---

## Class: `MenuData`

```csharp
public class MenuData { }
```

Empty base class. Create typed subclasses to pass data to a menu:

```csharp
public class MainMenuData : MenuData
{
    public int PlayerScore;
    public string PlayerName;
}
```

---

## Interface: `IMenu`

```csharp
public interface IMenu
```

| Member | Description |
|---|---|
| `bool ActiveSelf { get; }` | Whether the `GameObject` is active in hierarchy |
| `bool DestroyWhenClosed { get; }` | If true, the view is destroyed (not just hidden) on close |
| `void SetActive(bool active)` | Show/hide the `GameObject` |
| `void InitializeView()` | Reset position/scale/rotation to identity |
| `void UpdateView()` | Refresh when re-opened without destroying |
| `void Open()` | Activate the view |
| `void Close()` | Deactivate the view |
| `void Dispose(bool disposing)` | Dispose; if `disposing && DestroyWhenClosed`, destroy `GameObject` |

---

## Class: `BaseMenuView`

```csharp
public class BaseMenuView : MonoBehaviour, IMenu
```

Default implementation of `IMenu`. Attach to menu prefab root.

| Field | Type | Default | Description |
|---|---|---|---|
| `_destroyWhenClosed` | `bool` | `false` | Set in Inspector; controls `DestroyWhenClosed` |

`InitializeView()` resets `localPosition`, `localScale`, and `rotation` to identity/`Vector3.one`. Override to customize initial state.

---

## Class: `BaseMenuPresenter<TMenuName, TData, TMenu>`

```csharp
public abstract class BaseMenuPresenter<TMenuName, TData, TMenu>
    : SubscribableConcrete, IMenuPresenter<TMenuName>
    where TMenuName : Enum
    where TData : MenuData
    where TMenu : BaseMenuView, IMenu
```

### Injected Fields

| Type | Name | Description |
|---|---|---|
| `Func<TMenu>` | `_menuFactory` | DI factory; call to instantiate or retrieve the view |

### Abstract Properties

```csharp
public abstract MenuMode MenuMode { get; }
public abstract TMenuName MenuName { get; }
```

### Key Methods

| Method | Description |
|---|---|
| `Open(TData data = null)` | Gets/creates the view, calls `OnShowBefore`, opens, calls `OnShow` |
| `Close()` | Calls `OnHide`, closes view; if `DestroyWhenClosed`, destroys it |
| `DestroyView()` | Calls `OnHide` then `Dispose(true)` on the view |
| `GetView<TMenu1>()` | Returns the current view cast to `TMenu1` |

### Virtual Hooks

```csharp
protected virtual void OnShowBefore(TData data) { }   // before SetActive(true)
protected virtual void OnShow(TData menuData) { }      // after SetActive(true)
protected virtual void OnHide() { }                    // before SetActive(false)
protected virtual void UpdatePresenter() { }           // called when re-opening an existing view
```

---

## Class: `BaseMenuManager<TMenuName>`

```csharp
public abstract class BaseMenuManager<TMenuName>
    : SubscribableMonoBehaviour, IMenuManager<TMenuName>
    where TMenuName : Enum
```

### Serialized Fields

| Type | Name | Description |
|---|---|---|
| `Transform` | `_menuRoot` | Parent transform; children are cleared in `Init()` |

### Abstract Method

```csharp
protected abstract void OpenMenu(TMenuName menuName, MenuData menuData);
```

Implement the switch/dispatch logic (resolve the presenter, call `Open`).

### Protected Methods

| Method | Description |
|---|---|
| `Open<TPresenter, TMenuModel>(TPresenter, MenuData)` | Resolves `Single`/`Additive`, adds to list, calls `Open` |
| `CloseMenu(TMenuName)` | Finds and closes the presenter for the given name |
| `CloseOthers()` | Closes all open menus |
| `CloseOthers(params TMenuName[])` | Closes all except the listed menu names |

---

## DI Extension: `MenuExtensions.RegisterMenuFactory`

```csharp
public static void RegisterMenuFactory<TMenuName, TPresenter, TMenu>(
    this IContainerBuilder builder,
    TMenu prefab,
    Transform parent,
    Lifetime lifetime)
```

Registers a `Func<TMenu>` factory that either instantiates a new view from `prefab` or returns the existing view if the presenter already has one.

```csharp
// In LifetimeScope.Configure:
builder.RegisterMenuFactory<MenuNames, MainMenuPresenter, MainMenuView>(
    _mainMenuPrefab, _menuRoot, Lifetime.Singleton);
```

---

## Events

### `BaseOpenMenuEvent<TMenuName, TMenuData>`

```csharp
public class BaseOpenMenuEvent<TMenuName, TMenuData>
    where TMenuName : Enum
    where TMenuData : MenuData
```

| Property | Type |
|---|---|
| `MenuName` | `TMenuName` |
| `MenuData` | `TMenuData` |

### `BaseCloseMenuEvent<TMenuName>`

| Property | Type |
|---|---|
| `MenuName` | `TMenuName` |

### `BaseCloseOthersMenuEvent<TMenuName>`

> **Known issue:** The `keepMenuNames` parameter passed to the constructor is not stored. All menus are closed regardless of arguments.

---

## Concrete Implementation Example

```csharp
public enum MenuNames { Main, Settings, Pause }

public class MainMenuData : MenuData { public int Score; }

// View
public class MainMenuView : BaseMenuView { /* UI components */ }

// Presenter
public class MainMenuPresenter
    : BaseMenuPresenter<MenuNames, MainMenuData, MainMenuView>
{
    public override MenuMode MenuMode => MenuMode.Single;
    public override MenuNames MenuName => MenuNames.Main;

    protected override void OnShow(MainMenuData data)
    {
        // Bind data.Score to UI
    }
}

// Manager
public class GameMenuManager : BaseMenuManager<MenuNames>
{
    [Inject] private readonly MainMenuPresenter _mainMenuPresenter;
    [Inject] private readonly ISubscriber<BaseOpenMenuEvent<MenuNames, MainMenuData>> _openSubscriber;

    public override void Subscriptions()
    {
        _openSubscriber.Subscribe(e => OpenMenu(e.MenuName, e.MenuData)).AddTo(_bagBuilder);
    }

    protected override void OpenMenu(MenuNames menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case MenuNames.Main:
                Open<MainMenuPresenter, MainMenuData>(_mainMenuPresenter, menuData);
                break;
        }
    }
}
```

---

## See Also

- `SubscribableMonoBehaviour` — base of `BaseMenuManager`
- `SubscribableConcrete` — base of `BaseMenuPresenter`
- `BasePopupBuilder` — separate popup system for modal UI
