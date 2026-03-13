# Spawner Nasıl Kullanılır

## Genel Bakış

VGameKit spawner, `BaseSpawnPool<TModel, TItem>` üzerine inşa edilmiş DI uyumlu bir nesne havuzu sağlar. Öğeler yok edilmek yerine yeniden kullanılır. Bu rehber; bir havuzun nasıl tanımlanacağını, VContainer aracılığıyla nasıl bağlanacağını ve çalışma zamanında öğelerin nasıl spawn edilip serbest bırakılacağını gösterir.

**Ön Koşullar:** `VGameKit.Runtime` modülü kurulu; sahnede bir `AbsBaseLifetimeScope`.

---

## Adım 1 — Modeli tanımla

`ISpawnItemModel`'i uygulayan sıradan bir sınıf oluşturun. Basit bir veri konteyneri olarak tutun.

```csharp
using VGameKit.Runtime.Spawner;

public class EnemyModel : ISpawnItemModel
{
    public int Health;
    public float Speed;
}
```

---

## Adım 2 — Spawn öğesini oluştur

`ISpawnItem<TModel>`'i uygulayan bir MonoBehaviour oluşturun. Her yeniden kullanımda durumu sıfırlamak için `ReInitialize`'ı override edin.

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
        // Görsel/mantık durumunu burada sıfırlayın
        // Düşman öldüğünde havuza geri döndürmek için ReleaseItem(this) çağrısı yapın
    }

    public void ReleaseItem(ISpawnItem item)
    {
        SpawnPool?.ReleaseItem(item);
    }

    public void Dispose() { }
}
```

---

## Adım 3 — Pool sınıfını tanımla

`BaseSpawnPool<TModel, TItem>`'a tip parametrelerini geçiren somut bir pool sınıfı oluşturun.

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

## Adım 4 — LifetimeScope'a kaydet

Pool'un yeni öğeleri örneklendirmek için kullanacağı DI factory'i kaydetmek için `RegisterObjectSpawner` kullanın.

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

## Adım 5 — Çalışma zamanında pool oluştur ve kullan

Factory'i ve parent transform'u pool'un sahibi olan sınıfa inject edin:

```csharp
using System;
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;

public class EnemySpawnController : SubscribableConcrete
{
    [Inject] private readonly Func<EnemyModel, Transform, EnemyItem> _spawnFunc;
    [Inject] private readonly Transform _enemyParent;

    private EnemyPool _pool;

    public override void Init()
    {
        // Tükeneninde büyüyebilen 10 düşmanlık bir havuz oluştur
        _pool = new EnemyPool(_spawnFunc, poolSize: 10, poolTarget: _enemyParent, willGrow: true);
    }

    public void SpawnEnemy()
    {
        var model = new EnemyModel { Health = 100, Speed = 3.5f };
        var enemy = _pool.GetItem(model, _enemyParent);
        GKLog.Log(LogState.Game, $"Spawned enemy: {enemy.name}");
    }

    public override void Dispose()
    {
        base.Dispose();
        _pool?.Dispose(); // tüm havuzlanmış GameObject'leri yok eder
    }
}
```

---

## Öğeyi havuza geri döndürme

Öğenin kendi içinden `ReleaseItem(this)` çağrısı yapın; bu, `SpawnPool.ReleaseItem`'a devredilir:

```csharp
// EnemyItem içinde düşman öldüğünde:
public void OnDeath()
{
    ReleaseItem(this);
}
```

Tüm aktif öğeleri aynı anda gizlemek için (örneğin seviye bitişinde):

```csharp
_pool.HideAllObjects();
```

---

## Pool davranışı özeti

| Parametre | Etkisi |
|-----------|--------|
| `poolSize` | Yapı sırasında önceden örneklendirilen öğe sayısı |
| `willGrow: true` | Pool boş olduğunda yeni öğeler oluşturulur |
| `willGrow: false` | Pool boş olduğunda `InvalidOperationException` fırlatılır |
| `Dispose()` | Tüm aktif ve etkin olmayan GameObject'leri yok eder, dahili koleksiyonları temizler |

---

## Ayrıca Bkz.

- [Spawner referansı](tr-Reference-Spawner)
- [Lifetime Scopes referansı](tr-Reference-Lifetime-Scopes)
