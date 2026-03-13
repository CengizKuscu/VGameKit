# AbsAppManager — App Manager Referansı

**Namespace:** `VGameKit.Runtime.App`
**Assembly:** `VGameKit.Runtime`
**Dosya:** `Assets/VGameKit/Runtime/App/AbsAppManager.cs`

---

## Genel Bakış

`AbsAppManager`, uygulama düzeyindeki bootstrap yöneticisi için abstract base sınıftır. VContainer'ın `IAsyncStartable` yaşam döngüsü arayüzünü uygular; bu, DI container tamamen oluşturulduktan sonra VContainer'ın `StartAsync`'ı otomatik olarak çağırdığı anlamına gelir.

Başlatma sırası:
1. `StartAsync`, `InitializeGame`'i (kullanıcı tarafından implement edilir) bekler.
2. Tamamlandığında, MessagePipe aracılığıyla `AppReadyEvent` yayımlanır.
3. `Subscriptions()` içinde kurulan abonelik üzerinden `OnAppReady` tetiklenir.

---

## Kalıtım

```
SubscribableConcrete : IInitializable, ISubscribableObject
    └── AbsAppManager : IAsyncStartable
            └── AppManager  (somut, kullanıcı tanımlı)
```

---

## Sınıf Tanımı

```csharp
public abstract class AbsAppManager : SubscribableConcrete, IAsyncStartable
```

---

## Enjekte Edilen Alanlar

| Tür | Ad | Enjekte Eden |
|---|---|---|
| `IPublisher<AppReadyEvent>` | `_appReadyPublisher` | `[Inject]` (VContainer) |
| `ISubscriber<AppReadyEvent>` | `_appReadySubscriber` | `[Inject]` (VContainer) |

---

## Metodlar

### `Subscriptions` *(override)*

```csharp
public override void Subscriptions()
```

`OnAppReady`'yi MessagePipe `AppReadyEvent` akışına bağlar. VContainer'ın `IInitializable` yaşam döngüsü sırasında `SubscribableConcrete.Initialize()` tarafından çağrılır. Scope yok edildiğinde abonelik otomatik olarak dispose edilir.

```csharp
_appReadySubscriber.Subscribe(OnAppReady).AddTo(_bagBuilder);
```

---

### `StartAsync`

```csharp
public async UniTask StartAsync(CancellationToken token)
```

VContainer, DI container hazır olduktan sonra bunu çağırır. Yürütme:
1. `InitializeGame(token)`'ı `AttachExternalCancellation(token).SuppressCancellationThrow()` ile bekler — iptal exception fırlatmaz, sessizce durur.
2. `new AppReadyEvent()` yayımlar.

| Parametre | Tür | Açıklama |
|---|---|---|
| `token` | `CancellationToken` | VContainer tarafından sağlanır; scope ömrüne bağlıdır |

---

### `InitializeGame` *(abstract)*

```csharp
protected abstract UniTask InitializeGame(CancellationToken token);
```

Tüm asenkron oyun başlatma mantığını buraya implement edin (config yükleme, SDK başlatma, veri önbelleğe alma, vb.). Bu görev tamamlanana kadar uygulama "hazır değil" sayılır.

---

### `OnAppReady` *(abstract)*

```csharp
protected abstract void OnAppReady(AppReadyEvent @event);
```

MessagePipe'tan `AppReadyEvent` alındığında çağrılır. Hazır sonrası mantığı buraya implement edin (örn. ana menüyü gösterme, müzik başlatma, ilk sahne geçişini tetikleme).

---

## AppReadyEvent

**Dosya:** `Assets/VGameKit/Runtime/App/Events/AppReadyEvent.cs`
**Namespace:** `VGameKit.Runtime.App.Events`

```csharp
public struct AppReadyEvent { }
```

Boş bir işaretçi struct'ı. `AbsAppManager.StartAsync` tarafından uygulama yaşam döngüsü boyunca tam olarak bir kez yayımlanır. Herhangi bir sistem `ISubscriber<AppReadyEvent>` aracılığıyla abone olabilir.

---

## DI Kaydı

`AbsMainLifetimeScope` alt sınıfınızda VContainer aracılığıyla kaydedin:

```csharp
public sealed class AppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder); // MessagePipe, frame rate, logger kaydeder
        builder.Register<AppManager>(Lifetime.Singleton).AsImplementedInterfaces();
        // AsImplementedInterfaces şunları sunar: IAsyncStartable, IInitializable, IDisposable
    }
}
```

`AsImplementedInterfaces()` zorunludur; aksi takdirde VContainer hem `IInitializable.Initialize()` hem de `IAsyncStartable.StartAsync()`'ı tanımaz ve çağırmaz.

---

## Somut Implementasyon Örneği

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.App;
using VGameKit.Runtime.App.Events;
using VGameKit.Runtime.Log;

public class AppManager : AbsAppManager
{
    protected override async UniTask InitializeGame(CancellationToken token)
    {
        GKLog.Log(LogState.Development, "AppManager: başlatılıyor...");
        await LoadRemoteConfigAsync(token);
        await InitializeAnalyticsAsync(token);
    }

    protected override void OnAppReady(AppReadyEvent @event)
    {
        GKLog.Log(LogState.Game, "AppManager: uygulama hazır");
        // Ana menüyü göster, müziği başlat, vb.
    }
}
```

---

## Diğer Sistemlerden AppReadyEvent'e Abone Olma

Uygulama scope'undaki herhangi bir sınıf bağımsız olarak abone olabilir:

```csharp
public class GameAppManager : SubscribableConcrete
{
    [Inject] private readonly ISubscriber<AppReadyEvent> _appReadySubscriber;

    public override void Subscriptions()
    {
        _appReadySubscriber.Subscribe(OnAppReady).AddTo(_bagBuilder);
    }

    private void OnAppReady(AppReadyEvent @event)
    {
        // Farklı bir sistemde uygulama hazırlığına tepki ver
    }
}
```

---

## Ayrıca Bkz.

- `SubscribableConcrete` — `_bagBuilder` ve `Initialize()` sağlayan base sınıf
- `AbsMainLifetimeScope` — `AppManager`'ın kaydedildiği kök scope
- `AppReadyEvent` — yayımlanan işaretçi olay
- `ProcessFlowProvider` — uygulama hazır olduktan sonra yaygın sonraki adım
