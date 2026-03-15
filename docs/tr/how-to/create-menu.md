# Menü Nasıl Oluşturulur

## Genel Bakış

Bu rehber, VGameKit menü sistemi kullanılarak yeni bir tam ekran veya katman menüsünün nasıl ekleneceğini gösterir. Sistem beş parça gerektirir: **enum tanımlayıcısı**, **View** (MonoBehaviour + prefab), **MenuData**, **Presenter** ve **Manager**.

Menü navigasyonu doğrudan Manager'a erişimle değil, **MessagePipe event'leri** aracılığıyla yapılır. Bu sayede menüyü açan kod Manager'ı tanımak zorunda kalmaz.

**Ön Koşullar:** `VGameKit.Runtime` modülü kurulu; sahnede mevcut bir `AbsBaseLifetimeScope`.

---

## Adım 1 — Menü adı enum'u ve event'leri tanımla

Sahnedeki menüleri tanımlayan enum'u ve üç menu event sınıfını tanımlayın. Her Manager için tek bir enum kullanın.

```csharp
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public enum GameMenuName
{
    MainMenu,
    PauseMenu,
    LevelComplete,
}

// Menü açma event'i
public class OpenMenuEvent : BaseOpenMenuEvent<GameMenuName, MenuData>
{
    public OpenMenuEvent(GameMenuName menuName, MenuData menuData) : base(menuName, menuData) { }
}

// Belirli bir menüyü kapatma event'i
public class CloseMenuEvent : BaseCloseMenuEvent<GameMenuName>
{
    public CloseMenuEvent(GameMenuName menuName) : base(menuName) { }
}

// Belirtilen menüler dışındakileri kapatma event'i.
// BaseCloseOthersMenuEvent'in constructor'ı diziyi alır ama saklamaz —
// KeepMenuNames'i somut sınıfta tanımlayıp açıkça atayın.
public class CloseOtherMenuEvent : BaseCloseOthersMenuEvent<GameMenuName>
{
    public GameMenuName[] KeepMenuNames { get; }
    public CloseOtherMenuEvent(params GameMenuName[] keepMenuNames)
        : base(keepMenuNames) => KeepMenuNames = keepMenuNames;
}
```

---

## Adım 2 — View oluştur

`BaseMenuView`'dan kalıtım alan bir MonoBehaviour oluşturun. Bir UI prefab'ına ekleyin.

```csharp
using UnityEngine;
using VGameKit.Runtime.UI.Menu;

public class LevelCompleteView : BaseMenuView
{
    // UI referansları buraya
    [SerializeField] private UnityEngine.UI.Text _scoreText;

    public void SetScore(int score) => _scoreText.text = score.ToString();
}
```

- **Destroy When Closed** Inspector'da yalnızca kapatıldığında prefab'ın yok edilmesi gerekiyorsa işaretleyin (varsayılan: `SetActive` ile yeniden kullanım).
- Prefab, Manager'ın `_menuRoot` transform'u altında çalışma zamanında oluşturulur; sahne hiyerarşisine **doğrudan yerleştirmeyin**.

---

## Adım 3 — MenuData tanımla

`MenuData`, menü açılırken presenter'a veri iletmek için kullanılan base class'tır. Taşıyacak veriniz yoksa doğrudan `MenuData` kullanabilirsiniz; veriniz varsa türetin.

```csharp
using VGameKit.Runtime.UI.Menu;

// Veri var — MenuData'dan türetin:
public class LevelCompleteData : MenuData
{
    public int Score;
    public int StarsEarned;
}
```

---

## Adım 4 — Presenter oluştur

`BaseMenuPresenter<TMenuName, TData, TMenu>`'dan kalıtım alan bir sınıf oluşturun. `TData` type parametresi bu presenter'ın `OnShow`'da alacağı veri türünü belirler.

```csharp
using VGameKit.Runtime.UI.Menu;

public class LevelCompletePresenter : BaseMenuPresenter<GameMenuName, LevelCompleteData, LevelCompleteView>
{
    public override GameMenuName MenuName => GameMenuName.LevelComplete;
    public override MenuMode MenuMode => MenuMode.Additive; // diğer menülerin üstüne açılır

    protected override void OnShow(LevelCompleteData data)
    {
        View.SetScore(data.Score);
    }

    protected override void OnHide()
    {
        // Listener temizleme vb.
    }
}
```

Önemli override'lar:

| Override | Ne zaman çağrılır |
|---|---|
| `OnShow(data)` | View aktif hale geldikten sonra |
| `OnShowBefore(data)` | `Open()` view'ı aktif etmeden önce (yalnızca `didAwake` true ise) |
| `OnHide()` | View gizlenmeden veya yok edilmeden önce |
| `UpdatePresenter()` | View zaten mevcutken `Open()` tekrar çağrıldığında |

---

## Adım 5 — Manager oluştur

`BaseMenuManager<TMenuName>`'dan kalıtım alan bir MonoBehaviour oluşturun. Manager, `SubscribableMonoBehaviour`'dan geldiği için `Subscriptions()` override'ı içinde menu event'lerine abone olur. Dışarıdan doğrudan çağrılacak `Show()`/`Hide()` metodları **eklemeyin** — navigasyon event publish ederek yapılır.

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public class MenuManager : BaseMenuManager<GameMenuName>
{
    [Inject] private readonly ISubscriber<OpenMenuEvent> _openMenuSubscriber;
    [Inject] private readonly ISubscriber<CloseMenuEvent> _closeMenuSubscriber;
    [Inject] private readonly ISubscriber<CloseOtherMenuEvent> _closeOtherMenuSubscriber;

    [Inject] private readonly LevelCompletePresenter _levelCompletePresenter;

    public override void Subscriptions()
    {
        _openMenuSubscriber.Subscribe(e => OpenMenuHandler(e)).AddTo(_bagBuilder);
        _closeMenuSubscriber.Subscribe(e => CloseMenuHandler(e)).AddTo(_bagBuilder);
        _closeOtherMenuSubscriber.Subscribe(e => CloseOtherMenuHandler(e)).AddTo(_bagBuilder);
    }

    protected override void OpenMenu(GameMenuName menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case GameMenuName.LevelComplete:
                // İkinci tip parametresi, menuData'nın hangi türe cast edileceğini belirtir.
                Open<LevelCompletePresenter, LevelCompleteData>(_levelCompletePresenter, menuData);
                break;
        }
    }

    private void OpenMenuHandler(OpenMenuEvent e) => OpenMenu(e.MenuName, e.MenuData);
    private void CloseMenuHandler(CloseMenuEvent e) => CloseMenu(e.MenuName);
    private void CloseOtherMenuHandler(CloseOtherMenuEvent e) => CloseOthers(e.KeepMenuNames);
}
```

---

## Adım 6 — LifetimeScope'a kaydet

`AbsBaseLifetimeScope`'unuzda view prefab'ını, menu factory'yi ve presenter'ı kaydedin.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.UI.Menu;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private LevelCompleteView _levelCompleteViewPrefab;
    [SerializeField] private MenuManager _menuManager;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // View prefab'ını component olarak kaydet (factory içinde Resolve için gerekli)
        builder.RegisterComponent(_levelCompleteViewPrefab);

        // Manager'ı kaydet (sahnedeki MonoBehaviour)
        builder.RegisterComponent(_menuManager);

        // Menu factory'yi kaydet:
        // - TMenuName   : GameMenuName
        // - TPresenter  : LevelCompletePresenter
        // - TMenu       : LevelCompleteView
        // _menuManager.MenuRoot: view'ların altına yerleştirileceği transform
        builder.RegisterMenuFactory<GameMenuName, LevelCompletePresenter, LevelCompleteView>(
            _levelCompleteViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);

        // Presenter'ı kaydet (RegisterMenuFactory presenter'ı inject eder)
        builder.Register<LevelCompletePresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

Inspector'da `_menuManager` alanına sahnedeki `MenuManager` bileşenini, `_levelCompleteViewPrefab` alanına prefab'ı sürükleyin.

---

## Adım 7 — Çalışma zamanında menüyü aç

Menüleri doğrudan Manager'a erişerek değil, `IPublisher` aracılığıyla event yayınlayarak açın ve kapatın.

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;

public class GameFlowPresenter : SubscribableConcrete
{
    [Inject] private readonly IPublisher<OpenMenuEvent> _openMenuPublisher;
    [Inject] private readonly IPublisher<CloseMenuEvent> _closeMenuPublisher;

    public void ShowLevelComplete(int score)
    {
        _openMenuPublisher.Publish(
            new OpenMenuEvent(GameMenuName.LevelComplete, new LevelCompleteData { Score = score }));
    }

    public void HideLevelComplete()
    {
        _closeMenuPublisher.Publish(new CloseMenuEvent(GameMenuName.LevelComplete));
    }
}
```

---

## MenuMode hızlı referansı

| Mod | Davranış |
|---|---|
| `MenuMode.Single` | Açılmadan önce `CloseOthers()` çağrılır — tam ekran menüler için |
| `MenuMode.Additive` | Mevcut menülerin üstüne açılır — overlay ve HUD paneller için |

---

## Ayrıca Bkz.

- [Menü Sistemi referansı](../reference/api/menu-system.md)
- [Lifetime Scopes referansı](../reference/api/lifetime-scopes.md)
- [Subscribable referansı](../reference/api/subscribable.md)
