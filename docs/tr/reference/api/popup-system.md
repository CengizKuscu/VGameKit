# Popup Sistemi Referansı

**Ad Alanı:** `VGameKit.Runtime.UI.Popup`
**Derleme:** `VGameKit.Runtime`
**Dosyalar:** `Assets/VGameKit/Runtime/UI/Popup/`

---

## Genel Bakış

Popup sistemi, kuyruk tabanlı sıralı bir popup akışı yönetir. Popup'lar bir inşaatçı (`BasePopupBuilder`) aracılığıyla oluşturulur; her biri tiplendirilmiş bir model (`BasePopupModel`) ile ilişkilendirilir. Popup'lar birer birer açılır — bir popup kapandığında kuyruktaki sıradaki otomatik olarak açılır. İsteğe bağlı bir `OnCompleteFlow` geri çağrısı, tüm kuyruk tükendiğinde tetiklenir.

---

## Tip Özeti

| Tip | Tür | Açıklama |
|---|---|---|
| `BasePopupBuilder<TPopupName>` | `MonoBehaviour` | Kuyruk yöneticisi ve popup fabrikası |
| `BasePopup<TPopupName>` | `MonoBehaviour` | Tekil popup view tabanı |
| `BasePopupModel<TPopupName>` | sınıf | Popup açılırken iletilen veri |

---

## Sınıf: `BasePopupModel<TPopupName>`

```csharp
public class BasePopupModel<TPopupName>
    where TPopupName : Enum
```

| Alan | Tip | Açıklama |
|---|---|---|
| `OnClose` | `Action<TPopupName>` | Popup kapandığında çağrılır; popup adını parametre olarak alır |

Popup'a özgü veri taşımak için tiplendirilmiş alt sınıflar oluşturun:

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

## Sınıf: `BasePopup<TPopupName>`

```csharp
public class BasePopup<TPopupName> : MonoBehaviour, IDisposable
    where TPopupName : Enum
```

Popup prefab kökünüze ekleyin.

### Seri Hale Getirilen Alanlar

| Tip | Ad | Açıklama |
|---|---|---|
| `TPopupName` | `_popupName` | Inspector'da ayarlayın; kuyruktaki popup'ı tanımlar |

### Özellikler

| Tip | Ad | Açıklama |
|---|---|---|
| `TPopupName` | `Name` | `=> _popupName` |

### Sanal Kancalar

```csharp
public virtual void OnShowBefore() { }  // SetActive(true) öncesi çağrılır
public virtual void OnShowAfter()  { }  // SetActive(true) sonrası çağrılır
public virtual void OnHidePopup()  { }  // ClosePopup öncesi çağrılır
```

### Temel Metotlar

| Metot | Açıklama |
|---|---|
| `Open()` | `OnShowBefore` → `SetActive(true)` → `OnShowAfter` |
| `Close()` | `_model.OnClose`'u tetikler, `OnHidePopup` çağırır, ardından `_builder.ClosePopup(this)` |
| `BreakPopupFlow()` | `OnClose`'u tetikler, kendini yok eder, `ClearPopups()` çağırır — tüm kuyruğu iptal eder |
| `Dispose()` | `Destroy(gameObject)` |

---

## Sınıf: `BasePopupBuilder<TPopupName>`

```csharp
public class BasePopupBuilder<TPopupName> : MonoBehaviour, IDisposable
    where TPopupName : Enum
```

Bu `MonoBehaviour`'u sahnede bulundurun. Inspector'da popup kökü `RectTransform`'unu ve tüm popup prefab'larını atayın.

### Seri Hale Getirilen Alanlar

| Tip | Ad | Açıklama |
|---|---|---|
| `RectTransform` | `_popupRoot` | Örneklenen popup'lar için ebeveyn |
| `List<BasePopup<TPopupName>>` | `_prefabs` | Tüm popup prefab varyantları |

### Enjekte Edilen Alanlar

| Tip | Ad | Açıklama |
|---|---|---|
| `IObjectResolver` | `_resolver` | VContainer; popup'ları DI ile örneklemek için kullanılır |

### Kuyruk API'si (zincirlenebilir)

```csharp
// Kuyruğa popup ekle
public BasePopupBuilder<TPopupName> AddPopup<TModel>(TPopupName name, TModel model)
    where TModel : BasePopupModel<TPopupName>

// Tüm kuyruk tükendiğinde tetiklenecek geri çağrıyı ayarla
public BasePopupBuilder<TPopupName> OnCompleteFlow(Action onCompleteFlow)

// Kuyruğu başlat — ilk popup'ı aç
public void OpenPopup()

// Mevcut popup'ı kapat ve kuyruktaki sıradakini aç (BasePopup.Close tarafından dahili olarak çağrılır)
public void ClosePopup(BasePopup<TPopupName> popup)

// Kuyruğu durdur: mevcut popup'ı kapat, kuyruğu temizle, OnCompleteFlow'u tetikle
public void ClearPopups()

// IDisposable — ClearPopups çağırır
public void Dispose()
```

### `OpenPopup` Davranışı

1. Kuyruk boşsa → `ClearPopups()` çağırır (ayarlanmışsa `_onCompleteFlow`'u tetikler).
2. Sıradaki girişe bakar.
3. `_prefabs` listesinden eşleşen prefab'ı bulur.
4. `_resolver.Instantiate(prefab, _popupRoot.transform)` — DI ile örnekleme.
5. Yeni popup üzerinde `SetBuilder(this)`, `Init(model)`, `Open()` çağırır.
6. Girişi kuyruktan çıkarır, `_currentPopup`'ı ayarlar.

---

## Kullanım Örneği

```csharp
// Sahne MonoBehaviour veya presenter'ınızda:
[Inject] private readonly DemoPopupBuilder _popupBuilder;

void ShowConfirmFlow()
{
    _popupBuilder
        .AddPopup(PopupNames.Info, new InfoPopupModel { Message = "Hoş geldiniz!" })
        .AddPopup(PopupNames.Confirm, new ConfirmPopupModel
        {
            Title = "Hazır mısınız?",
            Message = "Oyunu başlat?",
            OnConfirm = StartGame,
        })
        .OnCompleteFlow(() => Debug.Log("Tüm popup'lar kapandı"))
        .OpenPopup();
}
```

---

## Somut Popup Örneği

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

## Ayrıca Bakınız

- `BaseMenuManager` — modal olmayan menü sistemi
- `IObjectResolver` — VContainer; `BasePopupBuilder` tarafından prefab örneklemek için kullanılır
