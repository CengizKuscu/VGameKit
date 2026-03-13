# Açıklama: VGameKit'te VContainer ve Bağımlılık Enjeksiyonu

## VGameKit neden bağımlılık enjeksiyonu kullanıyor

VGameKit, Unity için tasarlanmış hızlı ve düşük-tahsisatlı bir DI container'ı olan **VContainer** üzerine kurulmuştur. DI burada isteğe bağlı bir kolaylık değil, yapısal omurgadır. Her servis, presenter, havuz ve flow container üzerinden bağlanır. Bunu anlamak, framework'ü kendinizle savaşmadan genişletmenizi sağlar.

### DI'nin çözdüğü problem

DI olmayan bir Unity projesinde bileşenler, `GetComponent`, statik singleton'lar veya Inspector'daki doğrudan alan atamaları aracılığıyla işbirlikçilerini bulur. Her teknik gizli bağlantı oluşturur: bir presenter sahne hiyerarşisine müdahil olur, bir manager statik bir referans tutar ve testler ya da sahne değişiklikleri sessizce bozulur.

DI bunu tersine çevirir. Bileşenler neye ihtiyaç duyduklarını bildirir; container bunu nasıl sağlayacağına karar verir. Sonuç:

- **Açık bağımlılıklar** — her sınıfın gereksinimleri constructor veya `[Inject]` alanlarında görünür.
- **Yaşam döngüsü kontrolü** — container nesne ömürlerini yönetir (`Singleton`, `Scoped`, `Transient`), `GameObject.Destroy` değil.
- **Test edilebilirlik** — gerçek bir servisi taslakla değiştirmek, çağıranları yeniden yazmak yerine tek bir kaydı değiştirmeyi gerektirir.

---

## LifetimeScope hiyerarşisi

VContainer, kayıtları bir ağaç oluşturan **LifetimeScope**'lara düzenler:

```
AbsMainLifetimeScope  (uygulama düzeyi, tüm oturum boyunca yaşar)
    └── AbsBaseLifetimeScope  (sahne veya alt sistem düzeyi, sahneyle yok edilir)
```

`AbsMainLifetimeScope` köktür. Şunları yapar:

1. `builder.RegisterMessagePipe()`'ı dahili olarak çağırır ve seçenekleri `_messagePipeOpts`'ta saklar.
2. Alt sınıflara ek pub/sub broker'ları kaydetmeleri için `_messagePipeOpts`'u açar.

Alt sınıfınızda `RegisterMessagePipe()`'ı **tekrar çağırmayın** — container başına yalnızca bir kez çağrılmalıdır.

Alt scope'lar, üst scope'ta kayıtlı her şeyi miras alır. Bir sahne scope'u, onu yeniden kaydetmek zorunda kalmadan `ProcessFlowProvider`'ı (uygulama scope'unda kayıtlı) çözebilir.

---

## IAsyncStartable vs IInitializable

VContainer iki başlatma arayüzü sağlar:

| Arayüz | VContainer tarafından çağrılma | Tipik kullanım |
|---|---|---|
| `IInitializable` | Derleme sonrası senkron | Hafif init, I/O yok |
| `IAsyncStartable` | `IInitializable`'dan sonra asenkron | I/O, SDK init, sahne yüklemeleri |

`AbsAppManager`, `IAsyncStartable`'ı uygular. VContainer `StartAsync(CancellationToken)`'ı otomatik olarak çağırır. Manuel olarak çağırmayın; container dışından `await` etmeyin.

`SubscribableConcrete`, `IInitializable`'ı uygular. VContainer `Initialize()`'ı otomatik olarak çağırır. `Initialize` içinde sınıf, `Init()` ve `Subscriptions()` override'larınızı sırayla çağırır.

---

## Subscribable kalıpları

VGameKit, mesajlara tepki veren nesneler için iki temel sınıf sağlar:

### SubscribableConcrete

Düz C# sınıfları için (`MonoBehaviour` yok). VContainer bağımlılıkları inject eder, ardından `IInitializable` aracılığıyla `Initialize()`'ı çağırır.

```
VContainer derler → alanları inject eder → Initialize() çağırır
                                                └── Init()
                                                └── Subscriptions()
```

### SubscribableMonoBehaviour

`MonoBehaviour`'lar için. VContainer bağımlılıkları `[Inject] Construct()` metodu aracılığıyla inject eder; bu da `Init()` ve `Subscriptions()`'ı çağırır.

```
MonoBehaviour.Awake → VContainer.Inject → Construct() çağrılır
                                              └── Init()
                                              └── Subscriptions()
```

Her iki durumda da abonelikler `_bagBuilder`'da saklanır ve scope sona erdiğinde otomatik olarak dispose edilir.

---

## Yaygın kayıt kalıpları

```csharp
// Singleton: scope'un ömrü boyunca bir örnek
builder.Register<MyService>(Lifetime.Singleton);

// Scoped: üst scope başına bir tane; scope sona erdiğinde dispose edilir
builder.Register<EnemyPool>(Lifetime.Scoped);

// Bir arayüze karşı kaydet
builder.Register<MyAppManager>(Lifetime.Singleton)
    .AsImplementedInterfaces()
    .AsSelf();

// Sahnedeki bir MonoBehaviour'u kaydet
builder.RegisterComponentInHierarchy<MainMenuView>();

// Factory lambda kaydet
builder.RegisterInstance<Func<EnemyModel, Transform, EnemyItem>>(
    (model, parent) => Object.Instantiate(prefab, parent));
```

---

## Kaçınılması gerekenler

- **Scope dışında `new` kullanmak**: container'ın inject edemeyeceği, takip edemeyeceği veya dispose edemeyeceği nesneler oluşturur.
- **DI ile birlikte statik singleton'lar**: yaşam döngüsü yönetimini atlatır ve init sırasını öngörülemez yapar.
- **`Initialize()` veya `StartAsync()`'ı manuel çağırmak**: VContainer'ın başlatma sekansı bunları doğru zamanda çağırır. Tekrar çağırmak çift-başlatma hatalarına yol açar.
- **Scope'lar arası doğrudan alan erişimi**: alt scope'lar üst kayıtları çözebilir, ancak üst scope'lar asla alt scope'lara bağımlı olmamalıdır.

---

## Ayrıca Bkz.

- [Referans: Lifetime Scopes](../reference/api/lifetime-scopes.md)
- [Referans: Subscription Sistemi](../reference/api/subscribable.md)
- [Referans: App Manager](../reference/api/app-manager.md)
- [Açıklama: MessagePipe event sistemi](./messagepipe-events.md)
