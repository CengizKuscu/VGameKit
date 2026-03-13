# Abone Sistemi Referans

Bu belge VGameKit’in abonelik sistemi için Türkçe referans içerir. Projenin mesaj tabanlı olay sistemiyle entegrasyonunu anlatır.

## Genel Bakış / Overview
`ISubscribableObject`, `SubscribableMonoBehaviour` ve `SubscribableConcrete` ile abonelikler yönetilir ve Disposable kalıplar ile bellek sızıntıları önlenir.

## Ana Sınıflar / Main Classes
- `ISubscribableObject` – Abone olabilir nesnelerin temel arayüzü.
- `SubscribableMonoBehaviour` – MonoBehaviour tabanlı abonelik yönetimi.
- `SubscribableConcrete` – MonoBehaviour olmayan abonelik yöneticileri için temel sınıf.

## Örnek / Example
```csharp
public class MyComponent : SubscribableMonoBehaviour
{
    protected override void Init()
    {
        // initialization
    }
}
```

## See Also / See Also
- `AppReadyEvent` (örnek olaylar)
- `MessagePipe` dokümantasyonu
