# Menü Sistemi Referansı

**Ad Alanı:** `VGameKit.Runtime.UI.Menu`
**Derleme:** `VGameKit.Runtime`
**Dosyalar:** `Assets/VGameKit/Runtime/UI/Menu/`

---

## Genel Bakış

Menü sistemi, tiplendirilmiş enum anahtarlı bir yönetici üzerinde View-Presenter ayrımı kalıbını uygular. Her menü şunlarla ilişkilendirilir:
- **View** (`BaseMenuView` / `IMenu`) — `GameObject`'i kontrol eden Unity `MonoBehaviour`'u
- **Presenter** (`BaseMenuPresenter<TMenuName, TData, TMenu>`) — DI ile enjekte edilen mantık katmanı
- **Manager** (`BaseMenuManager<TMenuName>`) — aç/kapat işlemlerini yöneten `SubscribableMonoBehaviour`

Menü davranışı `MenuMode` ile kontrol edilir: `Single` açılırken diğer tüm menüleri kapatır; `Additive` diğerlerini açık bırakır.

---

## Tip Özeti

| Tip | Tür | Açıklama |
|---|---|---|
| `IMenu` | arayüz | Menü view sözleşmesi |
| `IMenuManager<TMenuName>` | arayüz | Yönetici sözleşmesi |
| `IMenuPresenter<TMenuName>` | arayüz | Presenter sözleşmesi |
| `MenuMode` | enum | `Single` veya `Additive` |
| `MenuData` | sınıf | Menü açılırken iletilen temel veri |
| `BaseMenuView` | sınıf | Varsayılan `MonoBehaviour` view uygulaması |
| `BaseMenuPresenter<TMenuName, TData, TMenu>` | soyut sınıf | Presenter tabanı |
| `BaseMenuManager<TMenuName>` | soyut sınıf | Yönetici tabanı |
| `MenuExtensions` | statik sınıf | DI yardımcısı |
| `BaseOpenMenuEvent<TMenuName, TMenuData>` | sınıf | Menü açmak için MessagePipe olayı |
| `BaseCloseMenuEvent<TMenuName>` | sınıf | Menü kapatmak için MessagePipe olayı |
| `BaseCloseOthersMenuEvent<TMenuName>` | sınıf | Diğerlerini kapatmak için MessagePipe olayı |

---

## Enum: `MenuMode`

```csharp
public enum MenuMode { Additive, Single }
```

| Değer | Davranış |
|---|---|
| `Additive` | Diğerlerini kapatmadan menüyü açar |
| `Single` | Açmadan önce `CloseOthers()` çağırır |

---

## Sınıf: `MenuData`

```csharp
public class MenuData { }
```

Boş temel sınıf. Menüye veri iletmek için tiplendirilmiş alt sınıflar oluşturun:

```csharp
public class MainMenuData : MenuData
{
    public int PlayerScore;
    public string PlayerName;
}
```

---

## Arayüz: `IMenu`

```csharp
public interface IMenu
```

| Üye | Açıklama |
|---|---|
| `bool ActiveSelf { get; }` | `GameObject`'in hiyerarşide aktif olup olmadığı |
| `bool DestroyWhenClosed { get; }` | `true` ise view kapatıldığında gizlenmek yerine yok edilir |
| `void SetActive(bool active)` | `GameObject`'i göster/gizle |
| `void InitializeView()` | Konum/ölçek/döndürmeyi başlangıç değerlerine sıfırlar |
| `void UpdateView()` | Yok edilmeden yeniden açılırken yenile |
| `void Open()` | View'ı etkinleştir |
| `void Close()` | View'ı devre dışı bırak |
| `void Dispose(bool disposing)` | Serbest bırak; `disposing && DestroyWhenClosed` ise `GameObject`'i yok et |

---

## Sınıf: `BaseMenuView`

```csharp
public class BaseMenuView : MonoBehaviour, IMenu
```

`IMenu`'nun varsayılan uygulaması. Menü prefab kökünüze ekleyin.

| Alan | Tip | Varsayılan | Açıklama |
|---|---|---|---|
| `_destroyWhenClosed` | `bool` | `false` | Inspector'da ayarlayın; `DestroyWhenClosed`'ı kontrol eder |

`InitializeView()`, `localPosition`, `localScale` ve `rotation`'ı başlangıç değerlerine / `Vector3.one`'a sıfırlar. Başlangıç durumunu özelleştirmek için geçersiz kılın.

---

## Sınıf: `BaseMenuPresenter<TMenuName, TData, TMenu>`

```csharp
public abstract class BaseMenuPresenter<TMenuName, TData, TMenu>
    : SubscribableConcrete, IMenuPresenter<TMenuName>
    where TMenuName : Enum
    where TData : MenuData
    where TMenu : BaseMenuView, IMenu
```

### Enjekte Edilen Alanlar

| Tip | Ad | Açıklama |
|---|---|---|
| `Func<TMenu>` | `_menuFactory` | DI fabrikası; view'ı örneklemek veya almak için çağırın |

### Soyut Özellikler

```csharp
public abstract MenuMode MenuMode { get; }
public abstract TMenuName MenuName { get; }
```

### Temel Metotlar

| Metot | Açıklama |
|---|---|
| `Open(TData data = null)` | View'ı alır/oluşturur, `OnShowBefore` → aç → `OnShow` çağırır |
| `Close()` | `OnHide` çağırır, view'ı kapatır; `DestroyWhenClosed` ise yok eder |
| `DestroyView()` | `OnHide` ardından view'da `Dispose(true)` çağırır |
| `GetView<TMenu1>()` | Mevcut view'ı `TMenu1`'e cast ederek döndürür |

### Sanal Kancalar

```csharp
protected virtual void OnShowBefore(TData data) { }   // SetActive(true) öncesi
protected virtual void OnShow(TData menuData) { }      // SetActive(true) sonrası
protected virtual void OnHide() { }                    // SetActive(false) öncesi
protected virtual void UpdatePresenter() { }           // Mevcut view yeniden açılırken
```

---

## Sınıf: `BaseMenuManager<TMenuName>`

```csharp
public abstract class BaseMenuManager<TMenuName>
    : SubscribableMonoBehaviour, IMenuManager<TMenuName>
    where TMenuName : Enum
```

### Seri Hale Getirilen Alanlar

| Tip | Ad | Açıklama |
|---|---|---|
| `Transform` | `_menuRoot` | Ebeveyn transform; `Init()` içinde alt nesneler temizlenir |

### Soyut Metot

```csharp
protected abstract void OpenMenu(TMenuName menuName, MenuData menuData);
```

Switch/dispatch mantığını burada uygulayın (presenter'ı çözümleyip `Open`'ı çağırın).

### Korunan Metotlar

| Metot | Açıklama |
|---|---|
| `Open<TPresenter, TMenuModel>(TPresenter, MenuData)` | `Single`/`Additive` modunu çözümler, listeye ekler, `Open` çağırır |
| `CloseMenu(TMenuName)` | Verilen isme ait presenter'ı bulup kapatır |
| `CloseOthers()` | Tüm açık menüleri kapatır |
| `CloseOthers(params TMenuName[])` | Listelenen menü adları dışındakileri kapatır |

---

## DI Uzantısı: `MenuExtensions.RegisterMenuFactory`

```csharp
public static void RegisterMenuFactory<TMenuName, TPresenter, TMenu>(
    this IContainerBuilder builder,
    TMenu prefab,
    Transform parent,
    Lifetime lifetime)
```

Presenter'ın zaten view'u varsa onu döndüren, yoksa `prefab`'dan yeni bir view örnekleyen `Func<TMenu>` fabrikası kaydeder.

```csharp
// LifetimeScope.Configure içinde:
builder.RegisterMenuFactory<MenuNames, MainMenuPresenter, MainMenuView>(
    _mainMenuPrefab, _menuRoot, Lifetime.Singleton);
```

---

## Olaylar

### `BaseOpenMenuEvent<TMenuName, TMenuData>`

```csharp
public class BaseOpenMenuEvent<TMenuName, TMenuData>
    where TMenuName : Enum
    where TMenuData : MenuData
```

| Özellik | Tip |
|---|---|
| `MenuName` | `TMenuName` |
| `MenuData` | `TMenuData` |

### `BaseCloseMenuEvent<TMenuName>`

| Özellik | Tip |
|---|---|
| `MenuName` | `TMenuName` |

### `BaseCloseOthersMenuEvent<TMenuName>`

> **Bilinen hata:** Yapıcıya iletilen `keepMenuNames` parametresi saklanmaz. Bağımsız olarak tüm menüler kapatılır.

---

## Somut Uygulama Örneği

```csharp
public enum MenuNames { Main, Settings, Pause }

public class MainMenuData : MenuData { public int Score; }

// View
public class MainMenuView : BaseMenuView { /* UI bileşenleri */ }

// Presenter
public class MainMenuPresenter
    : BaseMenuPresenter<MenuNames, MainMenuData, MainMenuView>
{
    public override MenuMode MenuMode => MenuMode.Single;
    public override MenuNames MenuName => MenuNames.Main;

    protected override void OnShow(MainMenuData data)
    {
        // data.Score'u UI'a bağla
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

## Ayrıca Bakınız

- `SubscribableMonoBehaviour` — `BaseMenuManager`'ın tabanı
- `SubscribableConcrete` — `BaseMenuPresenter`'ın tabanı
- `BasePopupBuilder` — modal UI için ayrı popup sistemi
