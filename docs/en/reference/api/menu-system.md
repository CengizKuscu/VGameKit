## Menu System Reference

English reference for the menu system in VGameKit.

## Overview
BaseMenuManager and BaseMenuPresenter are used to manage menus. Menus open/close via Open/Close and transfer data via MenuData.

## Key Classes
- `BaseMenuManager<TMenuName>`: General menu manager, handles root and active presenters
- `BaseMenuPresenter<TMenuName, TData, TMenu>`: Base presenter for each menu; handles data binding and view management
- `IMenuPresenter<TMenuName>`: Menu presenter interface
- `IMenu<TMenu>` / `IMenuManager<TMenuName>`: Menu view and manager interfaces
- `MenuMode` and `MenuData`: Menu behavior and data types

## Key Concepts
- Single vs Additive modes controlled by MenuMode
- View-Presenter separation: View handles UI, Presenter handles logic
- Menu data sharing via MenuData derivatives

## Example
```csharp
public enum MenuNames { Main, Settings }

public class MainMenuPresenter : BaseMenuPresenter<MenuNames, MenuData, MainMenuView>
{
    public override MenuMode MenuMode => MenuMode.Single;
    public override MenuNames MenuName => MenuNames.Main;
}
```

## See Also
- `BaseMenuManager<TMenuName>`
- `BaseMenuPresenter<TMenuName, TData, TMenu>`
- `IMenuPresenter<TMenuName>`
