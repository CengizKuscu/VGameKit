# Lifetime Scopes Referansı

**Namespace:** `VGameKit.Runtime.Core`
**Assembly:** `VGameKit.Runtime`
**Dosyalar:**
- `Assets/VGameKit/Runtime/Core/AbsBaseLifetimeScope.cs`
- `Assets/VGameKit/Runtime/Core/AbsMainLifetimeScope.cs`

---

## Genel Bakış

VGameKit, projedeki tüm scope'ların miras aldığı iki abstract `LifetimeScope` base sınıfı sunar. Scope hazırlık takibi, logger kurulumu, kare hızı yapılandırması ve MessagePipe kaydını üstlenirler; böylece somut scope'lar yalnızca DI bağlamalarını bildirmek zorunda kalır.

---

## Kalıtım Zinciri

```
VContainer.Unity.LifetimeScope
    └── AbsBaseLifetimeScope          ← hazırlık takibi + build logu
            └── AbsMainLifetimeScope  ← kare hızı + logger + MessagePipe
                    └── AppLifetimeScope   (somut uygulama scope'u)
            └── GameLifetimeScope     (somut sahne scope'u — doğrudan AbsBaseLifetimeScope'tan türer)
```

---

## AbsBaseLifetimeScope

```csharp
public abstract class AbsBaseLifetimeScope : LifetimeScope
```

### Özellikler

| Tür | Ad | Erişim | Açıklama |
|---|---|---|---|
| `bool` | `IsReady` | `public get / private set` | DI container tamamen oluşturulduğunda `true` |

### Configure

```csharp
protected override void Configure(IContainerBuilder builder)
{
    IsReady = false;
    builder.RegisterBuildCallback(_ =>
    {
        IsReady = true;
        GKLog.Log(LogState.Core, $"{this.GetType().Name} is ready.");
    });
}
```

`Configure` başında `IsReady = false` ayarlar, ardından container oluşturulduğunda `true` yapan ve scope adını `LogState.Core` ile loglayan bir build callback kaydeder.

> **Önemli:** Tüm somut scope'lar `IsReady` takibi ve build logu etkin olsun diye `base.Configure(builder)` çağırmalıdır.

---

## AbsMainLifetimeScope

```csharp
public abstract class AbsMainLifetimeScope : AbsBaseLifetimeScope
```

`AbsBaseLifetimeScope`'u uygulama düzeyindeki kurulumla genişletir: kare hızı, loglama ve MessagePipe.

### Serileştirilen Alanlar

| Tür | Ad | Varsayılan | Açıklama |
|---|---|---|---|
| `int` | `_targetFrameRate` | `60` | Inspector üzerinden ayarlanır; `Application.targetFrameRate` olarak uygulanır |

### Protected Alanlar

| Tür | Ad | Açıklama |
|---|---|---|
| `MessagePipeOptions` | `_messagePipeOpts` | Kayıtlı MessagePipe seçeneklerini tutar; alt sınıflara açık |

### Configure

```csharp
protected override void Configure(IContainerBuilder builder)
{
    base.Configure(builder); // AbsBaseLifetimeScope: IsReady + build log

    // Unity logger'ını koşulsuz etkinleştirir;
    // GAMEKIT_LOG tanımlıyken GKLog.ReportLogLevel() de çağırır
    Debug.unityLogger.logEnabled = true;

    Application.targetFrameRate = _targetFrameRate;
    _messagePipeOpts = builder.RegisterMessagePipe();
}
```

`builder.RegisterMessagePipe()`, MessagePipe DI altyapısını kaydeder. Uygulama genelinde kullanılan tüm `IPublisher<T>` / `ISubscriber<T>` çiftleri, bu çağrıyı yapan aynı container'dan çözümlenmelidir.

---

## Somut Scope Kalıpları

### Uygulama Scope'u (kök, uygulama başına bir tane)

```csharp
public sealed class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        // Uygulama düzeyindeki singleton'ları kaydet
        builder.Register<AppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsSelf();
    }
}
```

### Sahne/Oyun Scope'u (uygulama scope'unun çocuğu)

```csharp
public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private DemoSpawnItem _demoSpawnItemPrefab;
    [SerializeField] private Transform _demoSpawnItemParent;
    [SerializeField] private DemoPopupBuilder _demoPopupBuilder;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterComponent(_demoSpawnItemParent);
        builder.RegisterComponent(_demoPopupBuilder);
        builder.Register<GameAppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterObjectSpawner<DemoSpawnItemModel, Transform, DemoSpawnItem>(
            _demoSpawnItemPrefab, Lifetime.Singleton, true);
    }
}
```

Sahne scope'ları `AbsBaseLifetimeScope`'tan türer (Not: `AbsMainLifetimeScope`'tan değil); çünkü MessagePipe ve kare hızı zaten kök scope tarafından kaydedilmiştir.

---

## Hazırlık Kontrolü

```csharp
// Scope dışından sorgulama
if (myLifetimeScope.IsReady)
{
    // Container oluşturuldu, servisler kullanılabilir
}
```

`IsReady`, DI tamamlanmasını beklemeniz gereken editör araçlarında veya asenkron yükleme dizilerinde kullanışlıdır.

---

## Ayrıca Bkz.

- `AbsAppManager` — `AbsMainLifetimeScope` alt sınıflarında kaydedilir
- `SubscribableConcrete` / `SubscribableMonoBehaviour` — scope'lar içinde kaydedilen tipler
- VContainer dokümantasyonu — `LifetimeScope`, `IContainerBuilder`
- MessagePipe dokümantasyonu — `RegisterMessagePipe()`
