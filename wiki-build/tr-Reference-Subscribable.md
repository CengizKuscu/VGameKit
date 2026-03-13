# Abone Sistemi Referansı

**Ad Alanı:** `VGameKit.Runtime.Core`
**Derleme:** `VGameKit.Runtime`
**Dosyalar:**
- `Assets/VGameKit/Runtime/Core/ISubscribableObject.cs`
- `Assets/VGameKit/Runtime/Core/SubscribableConcrete.cs`
- `Assets/VGameKit/Runtime/Core/SubscribableMonoBehaviour.cs`

---

## Genel Bakış

Abone sistemi, VGameKit'te MessagePipe aboneliklerini yapılandırmak için bellek sızıntısına karşı güvenli, standart bir yol sunar. `IPublisher<T>` / `ISubscriber<T>` çiftlerine abone olan tüm sınıflar aşağıdaki temel sınıflardan birini miras almalıdır. Abonelikler bir `DisposableBag` içinde toplanır ve kapsam ya da `MonoBehaviour` yok edildiğinde otomatik olarak serbest bırakılır.

---

## Arayüz: `ISubscribableObject`

```csharp
public interface ISubscribableObject : IDisposable
```

Abone olabilir tüm tipler için temel sözleşme.

| Üye | Açıklama |
|---|---|
| `void Init()` | Başlatma kancası — `Subscriptions()` çağrılmadan önce çalışır |
| `void Subscriptions()` | MessagePipe aboneliklerini burada kur; her birini `_bagBuilder`'a ekle |
| `void Dispose()` | (`IDisposable`'dan) Tüm abonelikleri serbest bırakır |

---

## Sınıf: `SubscribableConcrete`

```csharp
public abstract class SubscribableConcrete : IInitializable, ISubscribableObject
```

**MonoBehaviour olmayan** servisler (DI ile kaydedilen saf C# nesneleri) için temel sınıf. VContainer, `IInitializable` arayüzü aracılığıyla `Initialize()` metodunu otomatik olarak çağırır.

### Alanlar

| Tip | Ad | Erişim | Açıklama |
|---|---|---|---|
| `IDisposable` | `_subscription` | `private` | İnşa edilmiş `DisposableBag` tutamacı |
| `DisposableBagBuilder` | `_bagBuilder` | `protected` | `Build()` öncesinde abonelik eklemek için kullanılan inşaatçı |

### Metotlar

#### `Initialize` *(VContainer tarafından çağrılır)*

```csharp
public void Initialize()
{
    _bagBuilder = DisposableBag.CreateBuilder();
    Init();
    Subscriptions();
    _subscription = _bagBuilder.Build();
}
```

Yaşam döngüsü giriş noktası. Bag inşaatçısını oluşturur, `Init()` ve `Subscriptions()` çağrılarını yapar, ardından bag'i mühürler. Bu noktadan sonra `_bagBuilder` değiştirilemez.

#### `Init` *(sanal)*

```csharp
public virtual void Init() { }
```

Abonelikler kurulmadan önce çalışacak kurulum mantığı için geçersiz kılın (örn. havuz başlatma, alt nesne oluşturma).

#### `Subscriptions` *(sanal)*

```csharp
public virtual void Subscriptions() { }
```

MessagePipe aboneliklerini eklemek için geçersiz kılın:

```csharp
public override void Subscriptions()
{
    _mySubscriber.Subscribe(OnMyEvent).AddTo(_bagBuilder);
    _otherSubscriber.Subscribe(OnOtherEvent).AddTo(_bagBuilder);
}
```

#### `Dispose` *(sanal)*

```csharp
public virtual void Dispose()
{
    _subscription?.Dispose();
}
```

Bag'e kaydedilen tüm abonelikleri serbest bırakır. VContainer, kapsam yok edildiğinde bu metodu çağırır.

---

## Sınıf: `SubscribableMonoBehaviour`

```csharp
public class SubscribableMonoBehaviour : MonoBehaviour, ISubscribableObject
```

MessagePipe aboneliklerine ihtiyaç duyan **MonoBehaviour** bileşenleri için temel sınıf. VContainer, `MonoBehaviour` üzerinde `IInitializable`'ı çağıramadığından kurulum, `[Inject]` etiketli `Construct()` metodu aracılığıyla tetiklenir.

### Alanlar

| Tip | Ad | Erişim | Açıklama |
|---|---|---|---|
| `IDisposable` | `_subscription` | `private` | İnşa edilmiş `DisposableBag` tutamacı |
| `DisposableBagBuilder` | `_bagBuilder` | `protected` | Abonelik eklemek için kullanılan inşaatçı |

### Metotlar

#### `Construct` *(VContainer tarafından `[Inject]` ile çağrılır)*

```csharp
[Inject]
public virtual void Construct()
{
    _bagBuilder = DisposableBag.CreateBuilder();
    Init();
    Subscriptions();
    _subscription = _bagBuilder.Build();
}
```

MonoBehaviour'lar için `Initialize()`'ın karşılığı. VContainer, alan enjeksiyonundan sonra bunu çağırır.

#### `Init`, `Subscriptions` — `SubscribableConcrete` ile aynı imza

#### `OnDisable`

```csharp
protected virtual void OnDisable()
{
    _subscription?.Dispose();
}
```

`GameObject` devre dışı bırakıldığında abonelikler serbest bırakılır; bu sayede devre dışı nesnelere olay iletimi engellenir.

#### `Awake`, `OnEnable`, `OnDestroy`

Hepsi `protected virtual` olarak boş gövde ile tanımlanmıştır — alt sınıflarda serbestçe geçersiz kılınabilir.

---

## Karşılaştırma

| | `SubscribableConcrete` | `SubscribableMonoBehaviour` |
|---|---|---|
| Temel tip | `object` | `MonoBehaviour` |
| DI tetikleyici | `IInitializable.Initialize()` | `[Inject] Construct()` |
| Dispose tetikleyici | VContainer kapsam yıkımı | `OnDisable()` |
| Kullanım alanı | Saf C# servisler, presenter'lar | Unity bileşenleri, view'lar |

---

## Kalıp: Abone Uygulama

```csharp
using MessagePipe;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.App.Events;

public class GameScoreTracker : SubscribableConcrete
{
    [Inject] private readonly ISubscriber<AppReadyEvent> _appReadySubscriber;
    [Inject] private readonly IPublisher<ScoreChangedEvent> _scorePublisher;

    private int _score;

    public override void Init()
    {
        _score = 0;
    }

    public override void Subscriptions()
    {
        _appReadySubscriber.Subscribe(OnAppReady).AddTo(_bagBuilder);
    }

    private void OnAppReady(AppReadyEvent @event)
    {
        // Takibi başlat
    }

    public void AddScore(int points)
    {
        _score += points;
        _scorePublisher.Publish(new ScoreChangedEvent(_score));
    }
}
```

---

## Ayrıca Bakınız

- `AbsAppManager` — `SubscribableConcrete`'i miras alır
- `BaseMenuManager<TMenuName>` — `SubscribableMonoBehaviour`'u miras alır
- MessagePipe — `IPublisher<T>`, `ISubscriber<T>`, `DisposableBag`
- VContainer — `IInitializable`, `[Inject]`
