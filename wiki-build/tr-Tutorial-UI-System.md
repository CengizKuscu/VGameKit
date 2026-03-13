# Tutorial: UI Sistemini Öğrenme

## Ne İnşa Edeceksiniz

İki panelli bir UI (Ana Menü + Ayarlar), bir onay popup'ı ve bunları `BaseMenuManager`, `BaseMenuPresenter` ve popup builder aracılığıyla birbirine bağlayan bağlantılar.

**Süre:** ~25 dakika  
**Ön Koşullar:** [Başlarken](tr-Tutorial-Getting-Started) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu.

---

## Adım 1 — Menü tanımlayıcılarını tanımlayın

```csharp
public enum AppScreen
{
    MainMenu,
    Settings,
}
```

---

## Adım 2 — Menü manager'ını oluşturun

```csharp
using VGameKit.Runtime.UI.Menu;

public class AppMenuManager : BaseMenuManager<AppScreen>
{
}
```

---

## Adım 3 — View'ları oluşturun ve ekleyin

Her ekran için:

1. Bir Canvas GameObject oluşturun (ör. `MainMenuView`).
2. `BaseMenuView<AppScreen>`'den kalıtım alan bir C# bileşeni ekleyin:

```csharp
using VGameKit.Runtime.UI.Menu;

public class MainMenuView : BaseMenuView<AppScreen>
{
    public override AppScreen MenuId => AppScreen.MainMenu;
}
```

`MenuId = AppScreen.Settings` ile `SettingsView` için tekrarlayın.

---

## Adım 4 — Presenter'ları oluşturun

```csharp
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.UI.Menu;

public class MainMenuPresenter : BaseMenuPresenter<AppScreen>
{
    [Inject] private readonly AppMenuManager _menuManager;

    public override AppScreen MenuId => AppScreen.MainMenu;

    protected override void OnOpen()
    {
        GKLog.Log(LogState.Game, "Ana Menü açıldı.");
    }

    public void OnSettingsPressed()
    {
        _menuManager.Open(AppScreen.Settings);
        // MenuMode.Single, MainMenu'yü otomatik olarak kapatır
    }
}
```

---

## Adım 5 — Popup modeli oluşturun

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitModel : BasePopupModel
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

public class SettingsPresenter : BaseMenuPresenter<AppScreen>
{
    [Inject] private readonly PopupBuilder _popupBuilder;

    public override AppScreen MenuId => AppScreen.Settings;

    public void OnQuitPressed()
    {
        var model = new ConfirmQuitModel
        {
            Message = "Çıkmak istediğinizden emin misiniz?",
            OnConfirmed = () => UnityEngine.Application.Quit()
        };

        _popupBuilder
            .AddPopup<ConfirmQuitModel, ConfirmQuitPopup>(model)
            .OpenPopup();
    }
}
```

---

## Adım 7 — Popup view'ını uygulayın

```csharp
using VGameKit.Runtime.UI.Popup;

public class ConfirmQuitPopup : BasePopupView<ConfirmQuitModel>
{
    protected override void OnOpen(ConfirmQuitModel model)
    {
        // UI öğelerini model verisine bağlayın
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
builder.Register<AppMenuManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
builder.Register<SettingsPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
builder.Register<PopupBuilder>(Lifetime.Singleton);
```

---

## Adım 9 — Sahneyi çalıştırın

Play Mode'a girin. **Ayarlar**'a tıklayın — Ana Menü kapanmalı ve Ayarlar açılmalı. **Çıkış**'a tıklayın — onay popup'ı görünmelidir.

---

## Ne Öğrendiniz

- `BaseMenuManager<TEnum>`'ın `MenuMode.Single` ile panel görünürlüğünü nasıl kontrol ettiğini.
- `BaseMenuPresenter` ve `BaseMenuView`'ın her ekranı temsil etmek için nasıl eşleştiğini.
- Popup builder'ın tipli modellerle `AddPopup` / `OpenPopup`'ı nasıl zincirlediğini.

---

## Sonraki Adımlar

- [Tutorial: Spawner temelleri](tr-Tutorial-Spawner-Basics)
- [Nasıl yapılır: Menü oluştur](tr-HowTo-Create-Menu)
- [Referans: Menü Sistemi](tr-Reference-Menu-System)
- [Referans: Popup Sistemi](tr-Reference-Popup-System)
