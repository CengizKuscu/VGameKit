# Açıklama: MessagePipe Event Sistemi

## Neden event-driven iletişim

Birbiriyle etkileşen birçok sisteme sahip bir Unity projesinde, bileşenler arasındaki doğrudan metod çağrıları sıkı bağlantı oluşturur. `UIManager.UpdateScore()` çağıran bir `ScoreManager`, `UIManager` olmadan var olamaz, izole olarak test edilemez ve `UIManager` yeniden düzenlenirse bozulur.

MessagePipe, publisher'ları subscriber'lardan ayırarak bunu çözer: `ScoreManager` bir `ScoreChangedEvent` yayınlar; `UIManager` buna abone olur. İkisi de diğerinin var olduğundan haberdar değildir. Publisher'a dokunmadan subscriber'ları ekleyebilir, kaldırabilir veya değiştirebilirsiniz.

---

## VGameKit MessagePipe'ı nasıl entegre eder

`AbsMainLifetimeScope`, scope yapılandırması sırasında `builder.RegisterMessagePipe()`'ı bir kez çağırır ve döndürülen `MessagePipeOptions`'ı `_messagePipeOpts`'ta saklar. Alt sınıflar ek broker'ları kaydetmek için `_messagePipeOpts` kullanır — `RegisterMessagePipe()`'ı **tekrar çağırmamalıdır**.

```csharp
// Uygulama LifetimeScope alt sınıfınızda:
protected override void Configure(IContainerBuilder builder)
{
    base.Configure(builder); // MessagePipe'ı kaydeder, _messagePipeOpts'u saklar

    // Özel bir event için pub/sub çifti kaydedin:
    _messagePipeOpts.AddBroker<PlayerDiedEvent>();
}
```

Kayıt sonrasında, normal DI injection aracılığıyla `IPublisher<T>` ve `ISubscriber<T>`'yi çözün.

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

- **`RegisterMessagePipe()`'ı birden fazla kez çağırmak**: VContainer'dan çalışma zamanı istisnasına neden olur. Tüm broker kayıtları `_messagePipeOpts` üzerinden yapılmalıdır.
- **`Subscriptions()` yerine `Awake` veya `Start`'ta abone olmak**: VGameKit'in başlatma sırası, `Subscriptions()`'ın injection sonrasında çalışmasını garanti eder. `Awake`/`Start`, DI tamamlanmadan önce çalışabilir.
- **Constructor'lardan yayınlamak**: publisher'ların henüz subscriber'ı olmayabilir. `IInitializable.Initialize`'dan veya daha sonrasından yayınlayın.
- **MessagePipe ile birlikte statik event'ler kullanmak**: statik event'ler DI yaşam döngüsünü ve scope disposal'ı atlatır.

---

## Ayrıca Bkz.

- [Referans: Subscription Sistemi](../reference/api/subscribable.md)
- [Açıklama: VContainer ve DI](./vcontainer-di.md)
- [Referans: App Manager](../reference/api/app-manager.md)
