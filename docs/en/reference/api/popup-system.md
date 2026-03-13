# Popup System Reference

**Namespace:** `VGameKit.Runtime.UI.Popup`
**Assembly:** `VGameKit.Runtime`
**Files:** `Assets/VGameKit/Runtime/UI/Popup/`

---

## Overview

The popup system manages a queue-based sequential popup flow. Popups are assembled via a builder (`BasePopupBuilder`), each associated with a typed model (`BasePopupModel`). They open one at a time — when a popup closes, the next in the queue opens automatically. An optional `OnCompleteFlow` callback fires when the entire queue is exhausted.

---

## Type Overview

| Type | Kind | Description |
|---|---|---|
| `BasePopupBuilder<TPopupName>` | `MonoBehaviour` | Queue manager and popup factory |
| `BasePopup<TPopupName>` | `MonoBehaviour` | Individual popup view base |
| `BasePopupModel<TPopupName>` | class | Data passed to a popup on open |

---

## Class: `BasePopupModel<TPopupName>`

```csharp
public class BasePopupModel<TPopupName>
    where TPopupName : Enum
```

| Field | Type | Description |
|---|---|---|
| `OnClose` | `Action<TPopupName>` | Called when the popup closes; receives the popup's name |

Create typed subclasses to carry popup-specific data:

```csharp
public enum PopupNames { Info, Confirm }

public class ConfirmPopupModel : BasePopupModel<PopupNames>
{
    public string Title;
    public string Message;
    public Action OnConfirm;
    public Action OnCancel;
}
```

---

## Class: `BasePopup<TPopupName>`

```csharp
public class BasePopup<TPopupName> : MonoBehaviour, IDisposable
    where TPopupName : Enum
```

Attach to popup prefab root.

### Serialized Fields

| Type | Name | Description |
|---|---|---|
| `TPopupName` | `_popupName` | Set in Inspector; identifies this popup in the queue |

### Properties

| Type | Name | Description |
|---|---|---|
| `TPopupName` | `Name` | `=> _popupName` |

### Virtual Hooks

```csharp
public virtual void OnShowBefore() { }  // called before SetActive(true)
public virtual void OnShowAfter()  { }  // called after SetActive(true)
public virtual void OnHidePopup()  { }  // called before ClosePopup
```

### Key Methods

| Method | Description |
|---|---|
| `Open()` | `OnShowBefore` → `SetActive(true)` → `OnShowAfter` |
| `Close()` | Fires `_model.OnClose`, calls `OnHidePopup`, then `_builder.ClosePopup(this)` |
| `BreakPopupFlow()` | Fires `OnClose`, destroys self, calls `ClearPopups()` — aborts the entire queue |
| `Dispose()` | `Destroy(gameObject)` |

---

## Class: `BasePopupBuilder<TPopupName>`

```csharp
public class BasePopupBuilder<TPopupName> : MonoBehaviour, IDisposable
    where TPopupName : Enum
```

Place this `MonoBehaviour` in the scene. Assign the popup root `RectTransform` and all popup prefabs in the Inspector.

### Serialized Fields

| Type | Name | Description |
|---|---|---|
| `RectTransform` | `_popupRoot` | Parent for instantiated popups |
| `List<BasePopup<TPopupName>>` | `_prefabs` | All popup prefab variants |

### Injected Fields

| Type | Name | Description |
|---|---|---|
| `IObjectResolver` | `_resolver` | VContainer; used to `Instantiate` popups with DI |

### Queue API (chain-able)

```csharp
// Add a popup to the queue
public BasePopupBuilder<TPopupName> AddPopup<TModel>(TPopupName name, TModel model)
    where TModel : BasePopupModel<TPopupName>

// Set a callback for when the entire queue is exhausted
public BasePopupBuilder<TPopupName> OnCompleteFlow(Action onCompleteFlow)

// Start the queue — opens the first popup
public void OpenPopup()

// Close the current popup and open the next in queue (called internally by BasePopup.Close)
public void ClosePopup(BasePopup<TPopupName> popup)

// Abort the queue: closes current popup, clears queue, fires OnCompleteFlow
public void ClearPopups()

// IDisposable — calls ClearPopups
public void Dispose()
```

### `OpenPopup` behaviour

1. If queue is empty → calls `ClearPopups()` (fires `_onCompleteFlow` if set).
2. Peeks the next entry.
3. Finds the matching prefab from `_prefabs`.
4. `_resolver.Instantiate(prefab, _popupRoot.transform)` — DI-injected instantiation.
5. Calls `SetBuilder(this)`, `Init(model)`, `Open()` on the new popup.
6. Dequeues the entry, sets `_currentPopup`.

---

## Usage Example

```csharp
// In your scene MonoBehaviour or presenter:
[Inject] private readonly DemoPopupBuilder _popupBuilder;

void ShowConfirmFlow()
{
    _popupBuilder
        .AddPopup(PopupNames.Info, new InfoPopupModel { Message = "Welcome!" })
        .AddPopup(PopupNames.Confirm, new ConfirmPopupModel
        {
            Title = "Ready?",
            Message = "Start the game?",
            OnConfirm = StartGame,
        })
        .OnCompleteFlow(() => Debug.Log("All popups closed"))
        .OpenPopup();
}
```

---

## Concrete Popup Example

```csharp
public class ConfirmPopup : BasePopup<PopupNames>
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button _cancelBtn;

    private ConfirmPopupModel _confirmModel;

    public override void OnShowAfter()
    {
        _confirmModel = _model as ConfirmPopupModel;
        _titleText.text   = _confirmModel.Title;
        _messageText.text = _confirmModel.Message;

        _confirmBtn.onClick.AddListener(OnConfirm);
        _cancelBtn.onClick.AddListener(Close);
    }

    public override void OnHidePopup()
    {
        _confirmBtn.onClick.RemoveAllListeners();
        _cancelBtn.onClick.RemoveAllListeners();
    }

    private void OnConfirm()
    {
        _confirmModel.OnConfirm?.Invoke();
        Close();
    }
}
```

---

## See Also

- `BaseMenuManager` — non-modal menu system
- `IObjectResolver` — VContainer; used by `BasePopupBuilder` to instantiate prefabs
