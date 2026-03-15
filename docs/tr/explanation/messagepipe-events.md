# Açıklama: MessagePipe Event Sistemi

## Neden event-driven iletişim

Birbiriyle etkileşen birçok sisteme sahip bir Unity projesinde, bileşenler arasındaki doğrudan metod çağrıları sıkı bağlantı oluşturur. `UIManager.UpdateScore()` çağıran bir `ScoreManager`, `UIManager` olmadan var olamaz, izole olarak test edilemez ve `UIManager` yeniden düzenlenirse bozulur.

MessagePipe, publisher'ları subscriber'lardan ayırarak bunu çözer: `ScoreManager` bir `ScoreChangedEvent` yayınlar; `UIManager` buna abone olur. İkisi de diğerinin var olduğundan haberdar değildir. Publisher'a dokunmadan subscriber'ları ekleyebilir, kaldırabilir veya değiştirebilirsiniz.

---

## VGameKit MessagePipe'ı nasıl entegre eder

`AbsMainLifetimeScope`, scope yapılandırması sırasında `builder.RegisterMessagePipe()`'ı bir kez çağırır. VGameKit'in hedeflediği Unity 2022.1+ ve VContainer 1.14.0+ sürümlerinde, `IPublisher<T>` ve `ISubscriber<T>` çiftleri otomatik olarak çözümlenir — manuel broker kaydı gerekmez.

```csharp
// Uygulama LifetimeScope alt sınıfınızda — AddBroker çağrısı gerekmez:
protected override void Configure(IContainerBuilder builder)
{
    base.Configure(builder); // MessagePipe'ı kaydeder

    // IPublisher<PlayerDiedEvent> ve ISubscriber<PlayerDiedEvent>
    // herhangi bir ek kayıt olmadan injection için hazırdır.
}
```

VContainer tarafından yönetilen herhangi bir sınıfta `IPublisher<T>` ve `ISubscriber<T>`'yi doğrudan `[Inject]` ile inject edin.

---

## Yayınlama ve abone olma

```csharp
// Publisher tarafı
public class EnemyController : SubscribableConcrete
{
    [Inject] private readonly IPublisher<PlayerDiedEvent> _diedPublisher;

    public void KillPlayer()
    {
        _diedPublisher.Publish(new PlayerDiedEvent { Cause = "enemy" });
    }
}

// Subscriber tarafı
public class GameOverPresenter : SubscribableConcrete
{
    [Inject] private readonly ISubscriber<PlayerDiedEvent> _diedSubscriber;

    protected override void Subscriptions()
    {
        _diedSubscriber
            .Subscribe(OnPlayerDied)
            .AddTo(_bagBuilder);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        GKLog.Log(LogState.Game, $"Oyuncu öldü: {evt.Cause}");
        // oyun bitti ekranını göster
    }
}
```

`AddTo(_bagBuilder)` zorunludur. `_bagBuilder` tüm abonelikleri toplar ve scope sona erdiğinde otomatik olarak dispose eder.

---

## Mesaj tipleri

MessagePipe birçok kalıbı destekler:

| Kalıp | Arayüzler | Kullanım durumu |
|---|---|---|
| Pub/Sub | `IPublisher<T>` / `ISubscriber<T>` | Ateş-ve-unut event'leri |
| Request/Response | `IRequestHandler<TReq, TRes>` | Bir servisten değer iste |
| Buffered | `IBufferedPublisher<T>` | Geç subscriber'lar son değeri alır |

VGameKit'in yerleşik sistemleri temel pub/sub kalıbını kullanır. Bir request/response akışı gerçekten uygun olmadıkça buna bağlı kalın.

---

## Yaşam döngüsü ve sızıntı önleme

Her abonelik bir delegate kaydıdır. Kaydı silinmemiş abonelikler nesne referanslarını süresiz tutar — Unity'de yaygın bir bellek sızıntısı kaynağı.

VGameKit'in subscribable temel sınıfları bunu otomatik olarak yönetir:

1. `AddTo(_bagBuilder)` ile eklenen abonelikler takip edilir.
2. VContainer scope'u dispose edildiğinde (sahne kaldırma, uygulama kapanma), `_bagBuilder` tüm abonelikleri tek bir çağrıda dispose eder.

Bir VGameKit sınıfında `AddTo(_bagBuilder)` olmadan asla abone olmayın. Subscribable sınıf dışındaki tek seferlik abonelikler için `subscription.Dispose()`'u manuel olarak çağırın.

---

## Kaçınılması gerekenler

- **`RegisterMessagePipe()`'ı birden fazla kez çağırmak**: VContainer'dan çalışma zamanı istisnasına neden olur. `AbsMainLifetimeScope` zaten çağırır; alt sınıfta tekrarlamayın.
- **`AddBroker<T>()` veya `RegisterMessageBroker<T>()` çağrısını manuel eklemek**: Unity 2022.1+ ve VContainer 1.14.0+ ile gerekmez. Gereksiz broker çağrıları eklemek herhangi bir etki yaratmaz, yalnızca scope'u karmaşıklaştırır.
- **`Subscriptions()` yerine `Awake` veya `Start`'ta abone olmak**: VGameKit'in başlatma sırası, `Subscriptions()`'ın injection sonrasında çalışmasını garanti eder. `Awake`/`Start`, DI tamamlanmadan önce çalışabilir.
- **Constructor'lardan yayınlamak**: publisher'ların henüz subscriber'ı olmayabilir. `IInitializable.Initialize`'dan veya daha sonrasından yayınlayın.
- **MessagePipe ile birlikte statik event'ler kullanmak**: statik event'ler DI yaşam döngüsünü ve scope disposal'ı atlatır.

---

## Ayrıca Bkz.

- [Referans: Subscription Sistemi](../reference/api/subscribable.md)
- [Açıklama: VContainer ve DI](./vcontainer-di.md)
- [Referans: App Manager](../reference/api/app-manager.md)
