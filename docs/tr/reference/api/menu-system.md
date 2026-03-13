# Menu System Referans

Bu doküman VGameKit menü sistemi için Türkçe referanstır. İçerik tipik olarak öteden beri kullanılan konseptlerle uyumludur.

## Genel Bakış / Overview
BaseMenuManager ve BaseMenuPresenter sınıfları, menüleri yönetmek için kullanılır. Menüler, göstermek/gizlemek için `Open`/`Close` metotlarına sahiptir ve veri aktarımı `MenuData` ile yapılır.

## Ana Sınıflar / Main Classes
- `BaseMenuManager<TMenuName>`: Genel menü yöneticisi, menü root ve aktif presenter'ları yönetir.
- `BaseMenuPresenter<TMenuName, TData, TMenu>`: Her menü için temel presenter, veri bağlama ve görünüm yönetimi sağlar.
- `IMenuPresenter<TMenuName>`: Menü presenter arayüzü.
- `IMenu<TMenu>` / `IMenuManager<TMenuName>`: Menü görünümü ve yöneticisi arayüzleri.
- `MenuMode` ve `MenuData`: Menü davranış modu ve menü verisi için temel tipler.

## Öne Çıkan Kavramlar / Key Concepts
- Single vs Additive modlar: `MenuMode` enum değerleriyle belirlenir.
- View ile Presenter ayrımı: View, kullanıcı arayüzünü, Presenter ise iş mantığını yönetir.
- Menü veri paylaşımı: `MenuData` türevleriyle yapılır.

## Örnek / Example
```csharp
public enum MenuNames { Main, Settings }

public class MainMenuPresenter : BaseMenuPresenter<MenuNames, MenuData, MainMenuView>
{
    public override MenuMode MenuMode => MenuMode.Single;
    public override MenuNames MenuName => MenuNames.Main;
}
```

## See Also
- `BaseMenuManager<TMenuName>`
- `BaseMenuPresenter<TMenuName, TData, TMenu>`
- `IMenuPresenter<TMenuName>`
