# Tutorial: VGameKit ile İlk Mini Oyununuzu Oluşturun

## Ne İnşa Edeceksiniz

**Ana Menü**'den **Oyun** sahnesine geçiş yapan minimal iki ekranlı bir oyun. Bir menu manager, bir process flow ve bir spawner birleştirip VContainer DI aracılığıyla üçünün nasıl iş birliği yaptığını göreceksiniz.

**Süre:** ~35 dakika  
**Ön Koşullar:** [Başlarken](./getting-started.md) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu.

---

## Adım 1 — Menü enum'unu tanımlayın

```csharp
public enum GameScreen
{
    MainMenu,
    HUD,
}
```

Enum'lar menü panelleri için geçerli tek tanımlayıcılardır — ham string kullanmayın.

---

## Adım 2 — Menü manager'ını oluşturun

```csharp
using VGameKit.Runtime.UI.Menu;

public class GameMenuManager : BaseMenuManager<GameScreen>
{
}
```

`BaseMenuManager<TEnum>`, açık panelleri takip eder, `MenuMode.Single`'ı (diğerlerini otomatik kapatma) yönetir ve `Open(GameScreen)` / `Close(GameScreen)` sağlar.

---

## Adım 3 — Her ekran için presenter oluşturun

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<GameScreen>
{
    public override GameScreen MenuId => GameScreen.MainMenu;

    protected override void OnOpen()
    {
        GKLog.Log(LogState.Game, "Ana menü açıldı.");
    }
}
```

`MenuId = GameScreen.HUD` ile `HUDPresenter` için tekrarlayın.

---

## Adım 4 — Seviye yükleme flow'unu oluşturun

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.Log;

public class StartGameArgs : BaseProcessFlowArgs { }

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

## Adım 5 — Her şeyi LifetimeScope'a kaydedin

```csharp
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.Register<GameMenuManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<HUDPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton);
        builder.Register<MyAppManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Adım 6 — Ana menü butonundan flow'u tetikleyin

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class MainMenuController : SubscribableConcrete
{
    [Inject] private readonly GameMenuManager _menuManager;
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    public void OnPlayButtonPressed()
    {
        var flow = _flowProvider.CreateProcessFlow<StartGameArgs, StartGameFlow>(new StartGameArgs());
        flow.OnComplete(_ => _menuManager.Open(GameScreen.HUD));
        flow.Execute(flow.cancellationToken);
    }
}
```

---

## Adım 7 — Sahneyi çalıştırın

1. `GameLifetimeScope`'u bir GameObject'e ekleyin.
2. Play Mode'a girin ve Play butonuna tıklayın.
3. Konsol sırasını gözlemleyin:

```
[Game] Ana menü açıldı.
[Game] StartGameFlow: oyun yükleniyor...
[Game] StartGameFlow: tamamlandı.
[Game] HUD açıldı.
```

---

## Ne Öğrendiniz

- `BaseMenuManager<TEnum>` ve tipli presenter'larla UI geçişlerini yönetmeyi.
- Async oyun mantığını `BaseProcessFlow<TArgs>` içinde kapsüllemeyi.
- UI ve mantığı koordine etmek için `OnComplete` tamamlanma geri çağrımını zincirlemeyi.
- Tüm bağlamaları LifetimeScope'da tutmayı.

---

## Sonraki Adımlar

- [Tutorial: UI Sistemi derinlemesine](./ui-system.md)
- [Tutorial: Spawner temelleri](./spawner-basics.md)
- [Nasıl yapılır: Process flow'ları yönet](../how-to/manage-processflows.md)
