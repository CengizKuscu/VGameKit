## Popup System Reference

English reference for the popup system in VGameKit.

## Overview
Popup system manages popups in a queue-based flow. It uses a builder to assemble a sequence of popups and initialize each with a corresponding model.

## Core Types
- `BasePopup<TPopupName>`
- `BasePopupModel<TPopupName>`
- `BasePopupBuilder<TPopupName>`
- Popup enum type (e.g. `PopupNames`)

## Example
```csharp
public enum PopupNames { Info, Confirm }
public class ConfirmPopupModel : BasePopupModel<PopupNames> { public string Message; }
```

## See Also
- Popup base classes
- DemoPopupBuilder (example usage)
