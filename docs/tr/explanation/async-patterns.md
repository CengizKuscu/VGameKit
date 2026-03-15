# Açıklama: UniTask ile Asenkron Kalıplar

## Neden Task veya coroutine yerine UniTask

Unity'nin `MonoBehaviour` coroutine'leri kullanışlıdır ancak önemli sınırlamaları vardır: değer döndüremezler, hata yönetimi zahmetlidir ve birleştirilemezler. Standart `System.Threading.Tasks.Task` çalışır, ancak her continuation için heap'te tahsis yapar ve Unity'nin tek thread'li oyun döngüsüyle kötü entegre olur.

**UniTask** (Cysharp) tahsisat-serbest, Unity'nin `PlayerLoop`'uyla entegredir, `CancellationToken` aracılığıyla iptali destekler ve `Task` ile aynı `async/await` sözdizimi ile birleşir. VGameKit UniTask'ı her yerde kullanır.

---

## UniTask vs UniTaskVoid

| Tip | Ne zaman kullanılır | Döndürür |
|---|---|---|
| `async UniTask` | Çağıran sonucu `await` edebilir | Awaitable, exception'ları yayabilir |
| `async UniTaskVoid` | Fire-and-forget; çağıran await etmez | Awaitable değil; exception'lar loglanır ama yayılmaz |

Process flow'lar ve çağıranın tamamlanmada sıralama yapması gereken herhangi bir asenkron metod için `UniTask` kullanın. Asenkron olması gereken ama hiç await edilmeyen event handler'lar ve Unity yaşam döngüsü callback'leri (`Start`, buton callback'leri) için `UniTaskVoid` kullanın.

```csharp
// Doğru: MonoBehaviour'da fire-and-forget
private async UniTaskVoid OnButtonClickedAsync()
{
    await LoadMenuAsync(destroyCancellationToken);
}

// Doğru: Awaitable flow adımı
public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
{
    await DoWorkAsync(ctx);
    return this;
}
```

---

## CancellationToken yayılımı

VGameKit'teki her asenkron metod bir `CancellationToken` kabul eder. Her `await` çağrısı boyunca iletin:

```csharp
public async UniTask LoadAsync(CancellationToken ctx)
{
    await DownloadAsync(ctx);         // iptali iletir
    await ParseAsync(ctx);            // iptali iletir
}
```

Bir timeout'a ihtiyaç duymadığınız sürece metod içinde `CancellationTokenSource` **oluşturmayın**. Yukarıdan iletilen token'ı tercih edin. Yerleşik `MonoBehaviour.destroyCancellationToken`, GameObject yok edildiğinde iptal eder.

`ProcessFlowProvider`, her flow için bir `CancellationTokenSource` oluşturur. `RemoveProcessFlow<T>()` çağrısı bu kaynağı iptal eder ve flow içindeki tüm `await` zinciri boyunca yayılır.

---

## Unity event'lerini await etme

UniTask, yaygın Unity kalıpları için awaitable wrapper'lar sağlar:

```csharp
// Bir frame bekle
await UniTask.Yield();

// N milisaniye bekle
await UniTask.Delay(500, cancellationToken: ctx);

// Bir koşul doğru olana kadar bekle
await UniTask.WaitUntil(() => _isReady, cancellationToken: ctx);

// Sonraki FixedUpdate için bekle
await UniTask.WaitForFixedUpdate();
```

Bu çağrılara her zaman `cancellationToken: ctx` iletin. Olmadan, işlem scope disposal sonrasında devam eder ve yok edilmiş nesnelere erişebilir.

---

## Asenkron flow'larda hata yönetimi

UniTask exception'ları standart `await` exception'ları gibi yayılır. Harici SDK çağrılarını sarın:

```csharp
public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
{
    try
    {
        await _sdk.InitAsync(ctx);
    }
    catch (OperationCanceledException)
    {
        // iptal edilmede beklenen; hata olarak loglama
        throw;
    }
    catch (System.Exception ex)
    {
        GKLog.Log(LogState.Error, $"SDK init başarısız: {ex.Message}");
        // zarif şekilde boz; kurtarılamaz değilse yeniden fırlatma
    }
    return this;
}
```

VGameKit'in iptal mekanizmasının doğru temizlik yapabilmesi için `OperationCanceledException`'ı yeniden fırlatın.

---

## Kaçınılması gerekenler

- **UniTask'ı bloke etmek**: ana thread'de `task.GetAwaiter().GetResult()` deadlock'a neden olur. Her zaman `await` kullanın.
- **CancellationToken'ları unutmak**: token'lar olmadan asenkron işlemler scope'larını aşar ve yok edilmiş nesnelere referans verir.
- **`async void`**: Unity exception'ları sessizce yutabilir. Bunun yerine `async UniTaskVoid` kullanın.
- **`using` olmadan iç içe `CancellationTokenSource`**: source'lar dispose edilmelidir. `using var cts = new CancellationTokenSource()` tercih edin.

---

## Ayrıca Bkz.

- [Referans: ProcessFlows](../reference/api/processflows.md)
- [Referans: App Manager](../reference/api/app-manager.md)
- [Tutorial: Process Flow'ları Öğrenme](../tutorials/processflows.md)
