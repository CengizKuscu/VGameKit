# Açıklama: Object Pooling ve Spawner Sistemi

## Neden object pooling

Unity'de `Instantiate` ve `Destroy` pahalıdır. Her çağrı bellek tahsis eder, garbage collection tetikler ve fizik ile render alt sistemlerini içerir. Sık oluşturulan nesneler için — düşmanlar, mermiler, partiküller — bu frame-rate sivrilmelerine yol açar.

Object pooling, önceden oluşturulmuş sabit sayıda örneği yeniden kullanarak bunu çözer. Artık gerekli olmayan bir nesne yok edilmez; devre dışı bırakılır ve havuza döndürülür. Yeni bir örnek gerektiğinde, havuz tahsis etmek yerine mevcut bir örneği yeniden etkinleştirir.

---

## BaseSpawnPool mimarisi

`BaseSpawnPool<TModel, TItem>`, VGameKit'in pooling temelidir:

```
BaseSpawnPool<TModel, TItem>
├── _spawnFunc: Func<TModel, Transform, TItem>   yeni örnekler oluşturur
├── _poolTarget: Transform                        pooled GameObject'ler için üst
├── _inactiveItems: Queue<TItem>                 yeniden kullanıma hazır etkin olmayan öğeler
└── _activeItems: List<TItem>                    şu anda aktif öğeler
```

### Factory kalıbı

Havuz, `Instantiate`'ı doğrudan çağırmaz. Inject edilmiş bir factory'ye devreder:

```csharp
Func<EnemyModel, Transform, EnemyItem> spawnFunc =
    (model, parent) => Object.Instantiate(prefab, parent);
```

Bu, havuzun kendisini test edilebilir ve prefab referanslarından bağımsız tutar. Factory, LifetimeScope'ta kayıtlıdır ve VContainer tarafından `_spawnFunc` olarak inject edilir — havuz asla `Resources.Load` veya sahne referansları kullanmaz.

---

## Spawn yaşam döngüsü

```
GetItem(model, parent)
    _inactiveItems dolu ise:
        öğeyi _inactiveItems'tan al (dequeue)
        item.ReInitialize(model)       ← yeni kullanım için yeniden yapılandır
        item.gameObject.SetActive(true)
        _activeItems'a ekle
    willGrow etkin ve _inactiveItems boş ise:
        _spawnFunc(model, parent) çağır  ← yalnızca havuz tükendiğinde tahsis et
        _activeItems'a ekle
    değilse:
        InvalidOperationException fırlat

ReleaseItem(item)
    item.gameObject.SetActive(false)
    _poolTarget'a yeniden ebeveynle
    _activeItems → _inactiveItems arasında taşı  ← sonraki GetItem için hazır

HideAllObjects()
    _activeItems içindeki her öğeyi serbest bırak  ← tümünü devre dışı bırakır, yok etmez

Dispose()
    RemoveAllObjects()               ← HideAllObjects()'i çağırır, ardından yok eder
    her iki koleksiyonu da temizle
```

---

## ISpawnItem sözleşmesi

Her poolable öğe iki metodu uygular:

| Metod | Ne zaman çağrılır | Amaç |
|---|---|---|
| `ReInitialize(model)` | `GetItem` içinde | Öğeye yeni yapılandırma uygula; tüm durumu sıfırla |
| `ReleaseItem(this)` | Öğenin kendi içinden | Öğeyi havuzuna geri döndür; devre dışı bırakmayı tetikler |

`ReInitialize`, hem yeni hem de yeniden kullanılan örneklerde çağrılır; bu nedenle öğeler kendilerini modelden tamamen sıfırlamalıdır — önceki kullanımdan taşınan durumu asla varsaymayın. `GetItem`, `ReInitialize`'ı otomatik olarak çağırır; çağıranlar `GetItem` sonrasında tekrar çağırmamalıdır.

---

## Yaşam süresi ve disposal

`Lifetime.Scoped` olarak kayıtlı havuzlarda VContainer, scope sona erdiğinde (sahne kaldırma, uygulama kapanma) `Dispose()`'u otomatik olarak çağırır. Bu, manuel temizlik gerektirmeden kalan tüm GameObject'leri yok eder.

`Lifetime.Singleton` olarak kayıtlı havuzlar sahne yüklemeleri boyunca devam eder. Singleton bir havuz sahneye özgü nesneler için kullanılıyorsa, sahne sona erdiğinde `Dispose()`'u manuel olarak çağırın ya da bunun yerine `Lifetime.Scoped` kullanın.

`Lifetime.Singleton` kullanmak ve açıkça dispose etmemek, sahne yüklemeleri arasında GameObject sızmasına yol açar.

---

## Kaçınılması gerekenler

- **Oyun mantığı içinde `Instantiate` çağırmak**: tüm oluşturmayı `GetItem` aracılığıyla havuz üzerinden yönlendirin.
- **Serbest bırakılmış öğelere referans tutmak**: `ReleaseItem` sonrasında öğe herhangi bir zamanda farklı bir çağırana verilebilir.
- **`ReInitialize`'da nesne yeniden oluşturmak**: `ReInitialize` yeniden yapılandırmalı, tahsis etmemeli. `ReInitialize` `Instantiate` çağırıyorsa, sızdıran bir havuzunuz var demektir.
- **`GetItem` sonrasında `ReInitialize`'ı manuel çağırmak**: `GetItem` zaten çağırır — iki kez çağırmak çift başlatma hatalarına yol açar.
- **Statik havuzlar**: statik durum sahne yüklemelerinde devam eder ve eski referanslar üretir.

---

## Ayrıca Bkz.

- [Referans: Spawner Sistemi](../reference/api/spawner.md)
- [Nasıl yapılır: Spawner kullan](../how-to/use-spawner.md)
- [Tutorial: Spawner temelleri](../tutorials/spawner-basics.md)
- [Açıklama: VContainer ve DI](./vcontainer-di.md)
