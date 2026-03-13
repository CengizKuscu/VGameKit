# Tutorial: Spawner Temelleri

## Ne İnşa Edeceksiniz

İsteğe bağlı prefab'ları örnekleyen, artık gerekli olmadıklarında geri dönüştüren ve sahne scope'u yok edildiğinde otomatik olarak temizleyen bir düşman havuzu.

**Süre:** ~25 dakika  
**Ön Koşullar:** [Başlarken](./getting-started.md) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu; hazır bir prefab mevcut.

---

## Adım 1 — Modeli oluşturun

Model, spawn edilen bir örneği yapılandıran veriyi tutar.

```csharp
public class EnemyModel
{
    public int Health;
    public float Speed;
    public UnityEngine.Vector3 SpawnPosition;
}
```

---

## Adım 2 — Havuz öğesini oluşturun

Öğe, havuzdaki bir düşman örneğini temsil eder.

```csharp
using UnityEngine;
using VGameKit.Runtime.Spawner;

public class EnemyItem : MonoBehaviour, ISpawnItem<EnemyModel>
{
    public void Initialize(EnemyModel model)
    {
        transform.position = model.SpawnPosition;
        // model verisini GameObject'e uygulayın
    }

    public void Recycle()
    {
        gameObject.SetActive(false);
    }
}
```

---

## Adım 3 — Havuzu oluşturun

`BaseSpawnPool<TModel, TItem>`'dan kalıtım alın:

```csharp
using UnityEngine;
using VGameKit.Runtime.Spawner;

public class EnemyPool : BaseSpawnPool<EnemyModel, EnemyItem>
{
    public EnemyPool(
        System.Func<EnemyModel, Transform, EnemyItem> factory,
        Transform poolRoot)
        : base(factory, poolRoot)
    {
    }
}
```

`BaseSpawnPool`, nesne etkinleştirme, kapasite yönetimi ve `Dispose()` temizliğini yönetir.

---

## Adım 4 — Havuzu LifetimeScope'a kaydedin

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    [SerializeField] private EnemyItem _enemyPrefab;
    [SerializeField] private Transform _poolRoot;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // DI dostu factory'yi kaydedin
        builder.RegisterInstance<System.Func<EnemyModel, Transform, EnemyItem>>(
            (model, parent) =>
            {
                var instance = Object.Instantiate(_enemyPrefab, parent);
                instance.Initialize(model);
                return instance;
            });

        builder.Register<EnemyPool>(Lifetime.Scoped).WithParameter(_poolRoot);
    }
}
```

`Lifetime.Scoped` kullanmak, scope sona erdiğinde `Dispose()`'un otomatik olarak çağrılmasını sağlar.

---

## Adım 5 — Düşmanları spawn edin ve geri dönüştürün

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;

public class EnemySpawner : SubscribableConcrete
{
    [Inject] private readonly EnemyPool _pool;

    public void SpawnEnemy(UnityEngine.Vector3 position)
    {
        var model = new EnemyModel
        {
            Health = 100,
            Speed = 5f,
            SpawnPosition = position
        };

        var enemy = _pool.Spawn(model);
        GKLog.Log(LogState.Development, $"Düşman spawn edildi: {position}");
    }

    public void RecycleAll()
    {
        _pool.RecycleAll();
        GKLog.Log(LogState.Development, "Tüm düşmanlar geri dönüştürüldü.");
    }
}
```

---

## Adım 6 — Sahneyi çalıştırın

1. `GameLifetimeScope`'u bir GameObject'e ekleyin ve prefab ile pool root'u atayın.
2. Play Mode'a girin ve bir test butonu veya `Update` döngüsünden `SpawnEnemy(Vector3.zero)` çağrısı yapın.
3. Konsolu ve sahne hiyerarşisini gözlemleyin — düşmanlar görünmeli, ardından geri dönüşümde devre dışı kalmalı.

---

## Ne Öğrendiniz

- `BaseSpawnPool<TModel, TItem>`'ın yeniden kullanılabilir bir nesne havuzunu nasıl yönettiğini.
- DI dostu `Func<TModel, Transform, TItem>` factory'yi nasıl inject edeceğinizi.
- `Lifetime.Scoped`'un havuz temizliğini scope disposal'a nasıl bağladığını.
- Öğe üzerindeki `Initialize` / `Recycle` kancalarının GameObject'leri yeniden oluşturmayı nasıl önlediğini.

---

## Sonraki Adımlar

- [Tutorial: Process Flow'lar](./processflows.md)
- [Nasıl yapılır: Spawner kullan](../how-to/use-spawner.md)
- [Referans: Spawner Sistemi](../reference/api/spawner.md)
