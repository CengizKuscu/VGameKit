# Tutorial: Spawner Temelleri

## Ne İnşa Edeceksiniz

İsteğe bağlı prefab'ları örnekleyen, artık gerekli olmadıklarında geri dönüştüren ve sahne scope'u yok edildiğinde otomatik olarak temizleyen bir düşman havuzu.

**Süre:** ~25 dakika  
**Ön Koşullar:** [Başlarken](./getting-started.md) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu; hazır bir prefab mevcut.

---

## Adım 1 — Modeli oluşturun

Model, spawn edilen bir örneği yapılandıran veriyi tutar. `ISpawnItemModel`'i uygulayın.

```csharp
using VGameKit.Runtime.Spawner;

public class EnemyModel : ISpawnItemModel
{
    public int Health;
    public float Speed;
    public UnityEngine.Vector3 SpawnPosition;
}
```

> **Not:** Modelin hiç field içermesi zorunlu değildir. Spawn sinyalinin kendisi yeterli olduğunda `ISpawnItemModel`'i uygulayan boş bir sınıf da geçerlidir.

---

## Adım 2 — Havuz öğesini oluşturun

Öğe, `ISpawnItem<TModel>` uygulayan bir `MonoBehaviour`'dur. Her yeniden kullanımda durumu sıfırlamak için `ReInitialize`'ı override edin. İşi bittiğinde `ReleaseItem(this)` çağırın.

```csharp
using UnityEngine;
using VGameKit.Runtime.Spawner;

public class EnemyItem : MonoBehaviour, ISpawnItem<EnemyModel>
{
    public BaseSpawnPool SpawnPool { get; set; }
    public EnemyModel ItemModel { get; set; }

    public void ReInitialize(EnemyModel model)
    {
        ItemModel = model;
        transform.position = model.SpawnPosition;
        // Model verisini uygulayın, animasyonları, canı vb. sıfırlayın.
    }

    public void ReleaseItem(ISpawnItem item)
    {
        SpawnPool?.ReleaseItem(item);
    }

    public void Dispose() { }
}
```

> `ReInitialize`, havuz tarafından `GetItem` içinde otomatik olarak çağrılır. `GetItem` sonrasında manuel olarak çağırmayın.

---

## Adım 3 — Havuz sınıfını oluşturun

`BaseSpawnPool<TModel, TItem>`'dan kalıtım alın ve tüm parametreleri temel yapıcıya geçirin.

```csharp
using System;
using UnityEngine;
using VGameKit.Runtime.Spawner;

public class EnemyPool : BaseSpawnPool<EnemyModel, EnemyItem>
{
    public EnemyPool(
        Func<EnemyModel, Transform, EnemyItem> spawnFunc,
        int poolSize,
        Transform poolTarget,
        bool willGrow)
        : base(spawnFunc, poolSize, poolTarget, willGrow) { }
}
```

---

## Adım 4 — LifetimeScope'a kaydedin

DI factory'yi kaydetmek için `RegisterObjectSpawner` kullanın. Aynı zamanda pool sahibine inject edilebilmesi için parent `Transform` bileşenini de kaydedin.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Spawner;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private EnemyItem _enemyPrefab;
    [SerializeField] private Transform _enemyParent;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.RegisterComponent(_enemyParent);

        // Container'a Func<EnemyModel, Transform, EnemyItem> kaydeder.
        // isInjectable: true iletin, böylece VContainer örneklendirilen prefab'lara inject eder.
        builder.RegisterObjectSpawner<EnemyModel, Transform, EnemyItem>(
            _enemyPrefab, Lifetime.Singleton, isInjectable: true);
    }
}
```

---

## Adım 5 — Düşmanları spawn edin ve serbest bırakın

Factory'yi ve parent transform'u pool'un sahibi olan sınıfa inject edin.

```csharp
using System;
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;

public class EnemySpawner : SubscribableConcrete
{
    [Inject] private readonly Func<EnemyModel, Transform, EnemyItem> _spawnFunc;
    [Inject] private readonly Transform _enemyParent;

    private EnemyPool _pool;

    public override void Init()
    {
        _pool = new EnemyPool(_spawnFunc, poolSize: 10, poolTarget: _enemyParent, willGrow: true);
    }

    public void SpawnEnemy(UnityEngine.Vector3 position)
    {
        var model = new EnemyModel { Health = 100, Speed = 5f, SpawnPosition = position };
        var enemy = _pool.GetItem(model, _enemyParent);
        // GetItem otomatik olarak ReInitialize(model) çağırır — tekrar çağırmayın.
        GKLog.Log(LogState.Development, $"Düşman spawn edildi: {position}");
    }

    public void HideAll()
    {
        _pool.HideAllObjects();
        GKLog.Log(LogState.Development, "Tüm düşmanlar gizlendi.");
    }

    public override void Dispose()
    {
        base.Dispose();
        _pool?.Dispose();
    }
}
```

Bir düşmanı öğenin kendi içinden havuza geri döndürmek için:

```csharp
// EnemyItem içinde, düşman öldüğünde:
public void OnDeath()
{
    ReleaseItem(this);
}
```

---

## Adım 6 — Sahneyi çalıştırın

1. `GameLifetimeScope`'u bir GameObject'e ekleyin ve prefab ile enemy parent transform'u atayın.
2. Play Mode'a girin ve bir test butonu veya `Update` döngüsünden `SpawnEnemy(Vector3.zero)` çağrısı yapın.
3. Konsolu ve sahne hiyerarşisini gözlemleyin — düşmanlar görünmeli, ardından serbest bırakıldığında devre dışı kalmalı.

---

## Ne Öğrendiniz

- `BaseSpawnPool<TModel, TItem>`'ın yeniden kullanılabilir bir nesne havuzunu nasıl yönettiğini.
- `RegisterObjectSpawner`'ın DI dostu `Func<TModel, Transform, TItem>` factory'yi nasıl kaydettiğini.
- `GetItem`'ın `ReInitialize`'ı otomatik olarak nasıl çağırdığını — çift başlatma hatalarını önler.
- `ReleaseItem`'ın `GameObject`'i yok etmeden öğeyi havuza nasıl geri döndürdüğünü.
- `Dispose()`'un havuz temizliğini scope disposal'a nasıl bağladığını.

---

## Sonraki Adımlar

- [Tutorial: Process Flow'lar](./processflows.md)
- [Nasıl yapılır: Spawner kullan](../how-to/use-spawner.md)
- [Referans: Spawner Sistemi](../reference/api/spawner.md)
