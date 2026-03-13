# Menü Nasıl Oluşturulur

## Genel Bakış

Bu rehber, VGameKit menü sistemi kullanılarak yeni bir tam ekran veya katman menüsünün nasıl ekleneceğini gösterir. Sistem dört parça gerektirir: **enum tanımlayıcısı**, **View** (MonoBehaviour + prefab), **Presenter** ve **Manager**.

**Ön Koşullar:** `VGameKit.Runtime` modülü kurulu; sahnede mevcut bir `AbsBaseLifetimeScope`.

---

## Adım 1 — Menü adı enum'u tanımla

Sahnenizdeki menüleri tanımlayan enum'u oluşturun veya genişletin. Her manager için tek bir enum kullanın.

```csharp
public enum GameMenuName
{
    MainMenu,
    PauseMenu,
    SettingsMenu,
}
```

---

## Adım 2 — View oluştur

`BaseMenuView`'dan kalıtım alan ve `IMenu`'yu uygulayan bir MonoBehaviour oluşturun. Bir UI prefab'ına ekleyin.

```csharp
using UnityEngine;
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView, IMenu
{
    // Burada UI referansları ekleyin (butonlar, etiketler vb.)
    [SerializeField] private UnityEngine.UI.Button _playButton;

    public UnityEngine.UI.Button PlayButton => _playButton;
}
```

- **Destroy When Closed** seçeneğini yalnızca prefab'ın kapatıldığında yok edilmesi gerekiyorsa işaretleyin (varsayılan: `SetActive` ile yeniden kullanım).
- Prefab'ı çalışma zamanında Manager'ın `_menuRoot` transform'u altında parente edin; doğrudan sahne hiyerarşisine **yerleştirmeyin**.

---

## Adım 3 — Presenter oluştur

`BaseMenuPresenter<TMenuName, TData, TMenu>`'dan kalıtım alan bir sınıf oluşturun.

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<GameMenuName, MenuData, MainMenuView>
{
    public override GameMenuName MenuName => GameMenuName.MainMenu;
    public override MenuMode MenuMode => MenuMode.Single; // açılırken diğerlerini kapatır

    // Gerektiğinde bağımlılıkları inject edin
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

Önemli override'lar:

| Override | Ne zaman çağrılır |
|----------|-------------------|
| `OnShow(data)` | View aktif hale geldikten sonra |
| `OnShowBefore(data)` | `Open()` view'ı aktif etmeden önce (yalnızca `didAwake` true ise) |
| `OnHide()` | View gizlenmeden veya yok edilmeden önce |
| `UpdatePresenter()` | View zaten mevcutken `Open()` tekrar çağrıldığında |

---

## Adım 4 — Manager oluştur

`BaseMenuManager<TMenuName>`'dan kalıtım alan bir MonoBehaviour oluşturun ve sahne köküne ekleyin.

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

## Adım 5 — LifetimeScope'a kaydet

Sahnenizin `AbsBaseLifetimeScope`'unda her view için prefab factory'i ve her presenter'ı singleton olarak kaydedin.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private MainMenuView _mainMenuViewPrefab;
    [SerializeField] private Transform _menuRoot;  // GameMenuManager._menuRoot ile aynı Transform

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // View factory'i kaydet (DI ile menuRoot altında örneklendirme)
        builder.RegisterFactory<MainMenuView>(
            container => () => container.Instantiate(_mainMenuViewPrefab, _menuRoot),
            Lifetime.Singleton);

        // Presenter'ları kaydet
        builder.Register<MainMenuPresenter>(Lifetime.Singleton);
        builder.Register<PauseMenuPresenter>(Lifetime.Singleton);

        // Manager'ı kaydet (MonoBehaviour, RegisterComponent kullanın)
        // Bir GameObject'e GameMenuManager ekleyin ve bu alana sürükleyin:
        builder.RegisterComponentInHierarchy<GameMenuManager>();
    }
}
```

---

## Adım 6 — Çalışma zamanında menüyü aç

Navigasyonu tetiklemeniz gereken yerde `GameMenuManager`'ı inject edin:

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

## MenuMode hızlı referansı

| Mod | Davranış |
|-----|----------|
| `MenuMode.Single` | Açılmadan önce `CloseOthers()` çağrılır — tam ekran menüler için kullanın |
| `MenuMode.Additive` | Mevcut menülerin üzerine açılır — overlay ve HUD paneller için kullanın |

---

## Ayrıca Bkz.

- [Menü Sistemi referansı](../reference/api/menu-system.md)
- [Lifetime Scopes referansı](../reference/api/lifetime-scopes.md)
- [Subscribable referansı](../reference/api/subscribable.md)
