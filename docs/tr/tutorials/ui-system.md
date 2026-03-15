# Tutorial: UI Sistemini Öğrenme

## Ne İnşa Edeceksiniz

İki panelli bir UI (Ana Menü + Ayarlar), bir onay popup'ı ve bunları `BaseMenuManager`, `BaseMenuPresenter` ve popup builder aracılığıyla birbirine bağlayan bağlantılar.

**Süre:** ~25 dakika  
**Ön Koşullar:** [Başlarken](./getting-started.md) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu.

---

## Adım 1 — Menü tanımlayıcılarını ve event'leri tanımlayın

```csharp
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public enum AppScreen
{
    MainMenu,
    Settings,
}

public class OpenMenuEvent : BaseOpenMenuEvent<AppScreen, MenuData>
{
    public OpenMenuEvent(AppScreen menuName, MenuData menuData) : base(menuName, menuData) { }
}

public class CloseMenuEvent : BaseCloseMenuEvent<AppScreen>
{
    public CloseMenuEvent(AppScreen menuName) : base(menuName) { }
}
```

---

## Adım 2 — View'ları oluşturun

Her ekran için `BaseMenuView`'dan kalıtım alan bir MonoBehaviour prefab oluşturun:

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView
{
    // UI referansları (butonlar, etiketler vb.)
}

public class SettingsView : BaseMenuView
{
    // UI referansları
}
```

---

## Adım 3 — Presenter'ları oluşturun

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<AppScreen, MenuData, MainMenuView>
{
    public override AppScreen MenuName => AppScreen.MainMenu;
    public override MenuMode MenuMode => MenuMode.Single;

    [Inject] private readonly IPublisher<OpenMenuEvent> _openMenuPublisher;

    protected override void OnShow(MenuData data)
    {
        GKLog.Log(LogState.Game, "Ana Menü açıldı.");
    }

    public void OnSettingsPressed()
    {
        // MenuMode.Single, Settings açılırken MainMenu'yü otomatik kapatır
        _openMenuPublisher.Publish(new OpenMenuEvent(AppScreen.Settings, null));
    }
}

public class SettingsPresenter : BaseMenuPresenter<AppScreen, MenuData, SettingsView>
{
    public override AppScreen MenuName => AppScreen.Settings;
    public override MenuMode MenuMode => MenuMode.Single;
}
```

---

## Adım 4 — Menu manager'ı oluşturun

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

public class AppMenuManager : BaseMenuManager<AppScreen>
{
    [Inject] private readonly ISubscriber<OpenMenuEvent> _openSubscriber;
    [Inject] private readonly ISubscriber<CloseMenuEvent> _closeSubscriber;

    [Inject] private readonly MainMenuPresenter _mainMenuPresenter;
    [Inject] private readonly SettingsPresenter _settingsPresenter;

    public override void Subscriptions()
    {
        _openSubscriber.Subscribe(e => OpenMenuHandler(e)).AddTo(_bagBuilder);
        _closeSubscriber.Subscribe(e => CloseMenu(e.MenuName)).AddTo(_bagBuilder);
    }

    protected override void OpenMenu(AppScreen menuName, MenuData menuData)
    {
        switch (menuName)
        {
            case AppScreen.MainMenu:
                Open<MainMenuPresenter, MenuData>(_mainMenuPresenter, menuData);
                break;
            case AppScreen.Settings:
                Open<SettingsPresenter, MenuData>(_settingsPresenter, menuData);
                break;
        }
    }

    private void OpenMenuHandler(OpenMenuEvent e) => OpenMenu(e.MenuName, e.MenuData);
}
```

---

## Adım 5 — Popup modeli oluşturun

`BasePopupModel<TPopupName>`, popup adı enum'unu tip parametresi olarak alır.

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitModel : BasePopupModel<AppScreen>
{
    public string Message;
    public System.Action OnConfirmed;
}
```

---

## Adım 6 — Popup'ı oluşturun ve gösterin

```csharp
using VContainer;
using VGameKit.Runtime.UI.Popup;

public class SettingsPresenter : BaseMenuPresenter<AppScreen, MenuData, SettingsView>
{
    [Inject] private readonly DemoPopupBuilder _popupBuilder;

    public override AppScreen MenuName => AppScreen.Settings;
    public override MenuMode MenuMode => MenuMode.Single;

    public void OnQuitPressed()
    {
        var model = new ConfirmQuitModel
        {
            Message = "Çıkmak istediğinizden emin misiniz?",
            OnConfirmed = () => UnityEngine.Application.Quit()
        };

        _popupBuilder
            .AddPopup(AppScreen.Settings, model)
            .OpenPopup();
    }
}
```

---

## Adım 7 — Popup view'ını uygulayın

`BasePopup<TPopupName>`'den kalıtım alın. Modele `_model` alanı (`BasePopupModel<TPopupName>` tipinde) üzerinden erişin ve `OnShowBefore()` içinde somut model tipinize cast edin.

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitPopup : BasePopup<AppScreen>
{
    protected override void OnShowBefore()
    {
        var model = _model as ConfirmQuitModel;
        _messageLabel.text = model.Message;
        _confirmButton.onClick.AddListener(() =>
        {
            model.OnConfirmed?.Invoke();
            Close();
        });
    }
}
```

---

## Adım 8 — LifetimeScope'a kaydedin

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.UI.Menu;

public class AppLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private MainMenuView _mainMenuViewPrefab;
    [SerializeField] private SettingsView _settingsViewPrefab;
    [SerializeField] private AppMenuManager _menuManager;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.RegisterComponent(_mainMenuViewPrefab);
        builder.RegisterComponent(_settingsViewPrefab);
        builder.RegisterComponent(_menuManager);

        builder.RegisterMenuFactory<AppScreen, MainMenuPresenter, MainMenuView>(
            _mainMenuViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);
        builder.RegisterMenuFactory<AppScreen, SettingsPresenter, SettingsView>(
            _settingsViewPrefab, _menuManager.MenuRoot, Lifetime.Singleton);

        builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<SettingsPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Adım 9 — Sahneyi çalıştırın

Play Mode'a girin. **Ayarlar**'a tıklayın — Ana Menü kapanmalı ve Ayarlar açılmalı. **Çıkış**'a tıklayın — onay popup'ı görünmelidir.

---

## Ne Öğrendiniz

- `BaseMenuManager<TEnum>`'ın `MenuMode.Single` ile panel görünürlüğünü nasıl kontrol ettiğini.
- Navigasyonun MessagePipe event'leriyle nasıl yürütüldüğünü; çağıranların Manager'dan nasıl bağımsız kaldığını.
- `BaseMenuPresenter` ve `BaseMenuView`'ın her ekranı temsil etmek için nasıl eşleştiğini.
- Popup builder'ın tipli modellerle `AddPopup` / `OpenPopup`'ı nasıl zincirlediğini.

---

## Sonraki Adımlar

- [Tutorial: Spawner temelleri](./spawner-basics.md)
- [Nasıl yapılır: Menü oluştur](../how-to/create-menu.md)
- [Referans: Menü Sistemi](../reference/api/menu-system.md)
- [Referans: Popup Sistemi](../reference/api/popup-system.md)
