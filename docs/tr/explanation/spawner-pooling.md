# Açıklama: Object Pooling ve Spawner Sistemi

## Neden object pooling

Unity'de `Instantiate` ve `Destroy` pahalıdır. Her çağrı bellek tahsis eder, garbage collection tetikler ve fizik ile render alt sistemlerini içerir. Sık oluşturulan nesneler için — düşmanlar, mermiler, partiküller — bu frame-rate sivrilmelerine yol açar.

Object pooling, önceden oluşturulmuş sabit sayıda örneği yeniden kullanarak bunu çözer. Artık gerekli olmayan bir nesne yok edilmez; devre dışı bırakılır ve havuza döndürülür. Yeni bir örnek gerektiğinde, havuz tahsis etmek yerine mevcut bir örneği yeniden etkinleştirir.

---

## BaseSpawnPool mimarisi

`BaseSpawnPool<TModel, TItem>`, VGameKit'in pooling temelidir:

```
BaseSpawnPool<TModel, TItem>
├── _factory: Func<TModel, Transform, TItem>   yeni örnekler oluşturur
├── _poolRoot: Transform                        pooled GameObject'ler için üst
├── _available: Stack<TItem>                   yeniden kullanıma hazır etkin olmayan öğeler
└── _active: List<TItem>                       şu anda aktif öğeler
```

### Factory kalıbı

Havuz, `Instantiate`'ı doğrudan çağırmaz. Inject edilmiş bir factory'ye devreder:

```csharp
Func<EnemyModel, Transform, EnemyItem> factory =
    (model, parent) => Object.Instantiate(prefab, parent);
```

Bu, havuzun kendisini test edilebilir ve prefab referanslarından bağımsız tutar. Factory, LifetimeScope'ta kayıtlıdır ve VContainer tarafından inject edilir — havuz asla `Resources.Load` veya sahne referansları kullanmaz.

---

## Spawn yaşam döngüsü

```
Spawn(model)
    _available dolu ise:
        öğeyi _available'dan al
        item.Initialize(model)         ← yeni kullanım için yeniden yapılandır
        item.gameObject.SetActive(true)
        _active'e ekle
    değilse:
        _factory(model, _poolRoot) çağır ← yalnızca havuz tükenendeye tahsis et
        _active'e ekle

Recycle(item)
    item.Recycle()                     ← çağıran "işim bitti" sinyali verir
    item.gameObject.SetActive(false)
    _active → _available arasında taşı

RecycleAll()
    _active içindeki her öğeyi geri dönüştür

Dispose()
    RecycleAll()
    tüm GameObject'leri yok et
    her iki koleksiyonu da temizle
```

---

## ISpawnItem sözleşmesi

Her poolable öğe iki metodu uygular:

| Metod | Ne zaman çağrılır | Amaç |
|---|---|---|
| `Initialize(model)` | Spawn'da | Öğeye yeni yapılandırma uygula |
| `Recycle()` | Geri dönüşümde | İç durumu sıfırla; devre dışı bırak |

`Initialize`, hem yeni hem de yeniden kullanılan örneklerde çağrılır, bu nedenle öğeler kendilerini modelden tamamen sıfırlamalıdır — önceki kullanımdan taşınan durumu asla varsaymayın.

---

## Scoped yaşam süresi ve disposal

Havuzlar `Lifetime.Scoped` olarak kaydedilir. VContainer, scope sona erdiğinde (sahne kaldırma, uygulama kapanma) `Dispose()`'u otomatik olarak çağırır. Bu, manuel temizlik gerektirmeden kalan tüm GameObject'leri yok eder.

`Lifetime.Singleton` olarak kaydetmek, sahne yüklemeleri arasında sızan GameObject'lere yol açar. Bir sahneye bağlı havuzlar için her zaman `Lifetime.Scoped` kullanın.

---

## Kaçınılması gerekenler

- **Oyun mantığı içinde `Instantiate` çağırmak**: tüm oluşturmayı havuz üzerinden yönlendirin.
- **Geri dönüştürülmüş öğelere referans tutmak**: `Recycle()` sonrasında öğe herhangi bir zamanda farklı bir çağırana verilebilir.
- **`Initialize`'da nesne yeniden oluşturmak**: `Initialize` yeniden yapılandırmalı, tahsis etmemeli. `Initialize` `Instantiate` çağırıyorsa, sızdıran bir havuzunuz var demektir.
- **Statik havuzlar**: statik durum sahne yüklemelerinde devam eder ve eski referanslar üretir.

---

## Ayrıca Bkz.

- [Referans: Spawner Sistemi](../reference/api/spawner.md)
- [Nasıl yapılır: Spawner kullan](../how-to/use-spawner.md)
- [Tutorial: Spawner temelleri](../tutorials/spawner-basics.md)
- [Açıklama: VContainer ve DI](./vcontainer-di.md)
