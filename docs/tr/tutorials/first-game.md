# Tutorial: VGameKit ile İlk Mini Oyununuzu Oluşturun

## Ne İnşa Edeceksiniz

**Ana Menü**'den **HUD** ekranına geçiş yapan minimal iki ekranlı bir oyun. Bir menu manager, bir process flow ve bir spawner birleştirip VContainer DI aracılığıyla üçünün nasıl iş birliği yaptığını göreceksiniz.

**Süre:** ~35 dakika  
**Ön Koşullar:** [Başlarken](./getting-started.md) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu.

---

## Adım 1 — Menü enum'unu ve event'lerini tanımlayın

```csharp
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public enum GameScreen
{
    MainMenu,
    HUD,
}

public class OpenGameMenuEvent : BaseOpenMenuEvent<GameScreen, MenuData>
{
    public OpenGameMenuEvent(GameScreen menuName, MenuData menuData) : base(menuName, menuData) { }
}

public class CloseGameMenuEvent : BaseCloseMenuEvent<GameScreen>
{
    public CloseGameMenuEvent(GameScreen menuName) : base(menuName) { }
}
```

Enum'lar menü panelleri için geçerli tek tanımlayıcılardır — ham string kullanmayın. Navigasyon, bu event'lerin MessagePipe aracılığıyla publish edilmesiyle sağlanır; böylece çağıranlar manager implementasyonundan bağımsız kalır.

---

## Adım 2 — View'ları oluşturun

Her ekran için `BaseMenuView`'dan türeyen bir MonoBehaviour prefab oluşturun:

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView
{
    // UI referansları (butonlar, etiketler, vb.)
}

public class HUDView : BaseMenuView
{
    // UI referansları
}
```

---

## Adım 3 — Menü verisini tanımlayın

```csharp
using VGameKit.Runtime.UI.Menu;

public class HUDData : MenuData
{
    public int StartingScore;
}
```

Ekranın açılırken ek parametre gerektirmediği durumlarda doğrudan `MenuData` kullanın. Tipli veri geçmeniz gerektiğinde onu alt sınıflayın.

---

## Adım 4 — Presenter'ları oluşturun

```csharp
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<GameScreen, MenuData, MainMenuView>
{
    public override GameScreen MenuName => GameScreen.MainMenu;
    public override MenuMode MenuMode => MenuMode.Single;

    protected override void OnShow(MenuData data)
    {
        GKLog.Log(LogState.Game, "Ana menü açıldı.");
    }
}

public class HUDPresenter : BaseMenuPresenter<GameScreen, HUDData, HUDView>
{
    public override GameScreen MenuName => GameScreen.HUD;
    public override MenuMode MenuMode => MenuMode.Single;

    protected override void OnShow(HUDData data)
    {
        GKLog.Log(LogState.Game, "HUD açıldı.");
    }
}
```

`MenuMode.Single`, manager'ın bu ekranı açmadan önce diğer tüm açık panelleri kapatmasını sağlar.

---

## Adım 5 — Menü manager'ını oluşturun

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public class GameMenuManager : BaseMenuManager<GameScreen>
{
    [Inject] private readonly ISubscriber<OpenGameMenuEvent> _openSubscriber;
    [Inject] private readonly ISubscriber<CloseGameMenuEvent> _closeSubscriber;

    [Inject] private readonly MainMenuPresenter _mainMenuPresenter;
    [Inject] private readonly HUDPresenter _hudPresenter;

    public override void Subscriptions()
    {
        _openSubscriber.Subscribe(e => OpenMenu(e.MenuName, e.MenuData)).AddTo(_bagBuilder);
        _closeSubscriber.Subscribe(e => CloseMenu(e.MenuName)).AddTo(_bagBuilder);
    }

    protected override void OpenMenu(GameScreen menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case GameScreen.MainMenu:
                Open<MainMenuPresenter, MenuData>(_mainMenuPresenter, menuData);
                break;
            case GameScreen.HUD:
                Open<HUDPresenter, HUDData>(_hudPresenter, menuData);
                break;
        }
    }
}
```

`BaseMenuManager` bir MonoBehaviour'dır — sahnedeki bir GameObject'e ekleyin ve `RegisterComponent` ile kaydedin.

---

## Adım 6 — Seviye yükleme flow'unu oluşturun

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.Log;

public class StartGameArgs : IProcessFlowArgs { }

public class StartGameFlow : BaseProcessFlow<StartGameArgs>
{
    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.Game, "StartGameFlow: oyun yükleniyor...");
        await UniTask.Delay(500, cancellationToken: ctx); // asenkron işi simüle et
        GKLog.Log(LogState.Game, "StartGameFlow: tamamlandı.");
        return this;
    }
}
```

---

## Adım 7 — Her şeyi LifetimeScope'a kaydedin

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.UI.Menu;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private MainMenuView _mainMenuViewPrefab;
    [SerializeField] private HUDView _hudViewPrefab;
    [SerializeField] private GameMenuManager _menuManager;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.RegisterComponent(_mainMenuViewPrefab);
        builder.RegisterComponent(_hudViewPrefab);
        builder.RegisterComponent(_menuManager);

        builder.RegisterMenuFactory<GameScreen, MainMenuPresenter, MainMenuView>(
            _mainMenuViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);
        builder.RegisterMenuFactory<GameScreen, HUDPresenter, HUDView>(
            _hudViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);

        builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<HUDPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.RegisterProcessFlow<StartGameArgs, StartGameFlow>(Lifetime.Singleton);
        builder.Register<MyAppManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Adım 8 — Ana menü butonundan flow'u tetikleyin

```csharp
using System;
using System.Threading;
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.UI.Menu;

public class MainMenuController : SubscribableConcrete
{
    [Inject] private readonly IPublisher<OpenGameMenuEvent> _openMenuPublisher;
    [Inject] private readonly Func<StartGameArgs, StartGameFlow> _startGameFlow;

    private CancellationTokenSource _cts;

    protected override void Init()
    {
        _cts = new CancellationTokenSource();
    }

    public override void Dispose()
    {
        _cts?.Cancel();
        base.Dispose();
    }

    public void OnPlayButtonPressed()
    {
        var flow = _startGameFlow(new StartGameArgs());
        flow.OnComplete(_ =>
            _openMenuPublisher.Publish(new OpenGameMenuEvent(GameScreen.HUD, new HUDData { StartingScore = 0 }))
        );
        flow.Execute(_cts.Token);
    }
}
```

`_menuManager.Open()` yerine `OpenGameMenuEvent` publish etmek, `MainMenuController`'ı manager implementasyonundan bağımsız tutar.

---

## Adım 9 — Sahneyi çalıştırın

1. `GameLifetimeScope`'u bir boş GameObject'e ekleyin.
2. Inspector'da prefab ve manager referanslarını atayın.
3. Play Mode'a girin ve Play butonuna tıklayın.
4. Konsol sırasını gözlemleyin:

```
[Game] Ana menü açıldı.
[Game] StartGameFlow: oyun yükleniyor...
[Game] StartGameFlow: tamamlandı.
[Game] HUD açıldı.
```

---

## Ne Öğrendiniz

- Doğrudan manager çağrıları yerine MessagePipe event'leriyle menü navigasyonunu tanımlamayı.
- Üç tip parametreli `BaseMenuPresenter<TEnum, TData, TView>` alt sınıfı oluşturmayı.
- `BaseMenuManager`'da `Subscriptions()` ve `OpenMenu()` override etmeyi.
- MonoBehaviour manager'ları `RegisterComponent`, prefab'ları `RegisterMenuFactory` ile kaydetmeyi.
- Async oyun mantığını `BaseProcessFlow<TArgs>` içinde kapsüllemeyi.

---

## Sonraki Adımlar

- [Tutorial: UI Sistemi derinlemesine](./ui-system.md)
- [Tutorial: Spawner temelleri](./spawner-basics.md)
- [Nasıl yapılır: Process flow'ları yönet](../how-to/manage-processflows.md)
