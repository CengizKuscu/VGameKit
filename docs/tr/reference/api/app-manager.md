# App Manager Referansı

Türkçe açıklama ve kullanım detayları için App Manager bileşeninin referansını sunar.

## Genel Bakış
AbsAppManager, Uygulama yaşam döngüsünü yöneten base sınıf olarak kullanılır. Uygulama hazır olduğunda AppReadyEvent yayımlanır ve bu olaya abonelikler tetiklenir.

## Temel Özellikler
- Asenkron başlatma (StartAsync)
- Uygulama odaklı olay yayımlama (AppReadyEvent)
- InitializeGame tarafından oyun başlatma akışı

## Ana Metodlar
- StartAsync(CancellationToken): Uygulamayı başlatır ve AppReadyEvent üretir.
- InitializeGame(CancellationToken): Oyun başlatma asenkron işlemleri için override edilmesi gereken abstract metod.
- OnAppReady(AppReadyEvent): AppReadyEvent tetiklendiğinde çalışan metot (abstrak implementasyon gerekir).

## Örnek Kullanım
```csharp
public class AppManager : AbsAppManager
{
    protected override UniTask InitializeGame(CancellationToken token)
    {
        // Oyun başlatma işlemleri
        return UniTask.CompletedTask;
    }

    protected override void OnAppReady(AppReadyEvent @event)
    {
        // App hazır olduğunda yapılacaklar
    }
}
```

## Bkz.
- AbsAppManager, IAsyncStartable
- AppReadyEvent
