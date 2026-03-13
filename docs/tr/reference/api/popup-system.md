# Popup Sistemi Referans

Bu belge VGameKit popup sisteminin Türkçe referansıdır.

## Genel Bakış / Overview
Popup sistemi, açılır pencereler (popups) için kuyruğa dayalı yönetim sağlar. Builder ile çoklu popup akışı oluşturulabilir; her popup bir model ile initialize edilir.

## Ana Tipler / Key Types
- `BasePopup<TPopupName>`: Popup temel sınıfı.
- `BasePopupModel<TPopupName>`: Popup veri modeli.
- `BasePopupBuilder<TPopupName>`: Popup oluşturucu ve akış yöneticisi.
- `IPopupName` (concrete enum): Popup isimleri için tip.

## Örnek / Example
```csharp
public enum PopupNames{ Info, Confirm }
public class ConfirmPopupModel : BasePopupModel<PopupNames> { public string Message; }
```

## See Also / See Also
- `Popup` temel sınıfları
- `DemoPopupBuilder` (örnek kullanım)
