# Havuz Sistemi (Spawner) Referansı

**Ad Alanı:** `VGameKit.Runtime.Spawner`
**Derleme:** `VGameKit.Runtime`
**Dosyalar:** `Assets/VGameKit/Runtime/Spawner/`

---

## Genel Bakış

Spawner sistemi, Unity `MonoBehaviour` öğeleri için genel ve DI uyumlu bir nesne havuzu sunar. Havuzlar belirli sayıda öğeyi önceden örnekler, `GetItem` ile yeniden kullanır ve `ReleaseItem` ile geri alır. Büyüme yapılandırılabilir.

---

## Tip Özeti

| Tip | Tür | Açıklama |
|---|---|---|
| `ISpawnItemModel` | arayüz | Öğe veri modelleri için işaretçi |
| `ISpawnItem` | arayüz | Temel spawn öğesi sözleşmesi |
| `ISpawnItem<TModel>` | arayüz | Yeniden başlatma kancası olan tiplendirilmiş spawn öğesi |
| `BaseSpawnPool` | sınıf | Genel olmayan havuz tabanı |
| `BaseSpawnPool<TModel, TItem>` | sınıf | Genel havuz uygulaması |
| `SpawnerExtensions` | statik sınıf | DI kayıt yardımcısı |

---

## Arayüz: `ISpawnItemModel`

```csharp
public interface ISpawnItemModel { }
```

Yalnızca işaretçi. Veri sınıfınızda/struct'ınızda uygulayın:

```csharp
public class EnemyModel : ISpawnItemModel
{
    public int Health;
    public Vector3 SpawnPosition;
}
```

---

## Arayüz: `ISpawnItem`

```csharp
public interface ISpawnItem : IDisposable
```

| Üye | Açıklama |
|---|---|
| `BaseSpawnPool SpawnPool { get; set; }` | Bu öğeye sahip havuza geri referans |
| `void ReleaseItem(ISpawnItem item)` | Öğeyi havuzuna geri döndürmek için çağırın |

---

## Arayüz: `ISpawnItem<TModel>`

```csharp
public interface ISpawnItem<TModel> : ISpawnItem
    where TModel : ISpawnItemModel
```

| Üye | Açıklama |
|---|---|
| `TModel ItemModel { get; set; }` | Bu öğeye atanan veri modeli |
| `void ReInitialize(TModel itemModel)` | `GetItem` sırasında havuz tarafından çağrılır; durumu sıfırlamak için kullanın |

---

## Sınıf: `BaseSpawnPool` *(genel olmayan)*

```csharp
public class BaseSpawnPool : IDisposable
```

Genel olmayan temel. Havuz yapılandırma alanlarını barındırır. Genel versiyon aracılığıyla genişletin.

### Yapıcı

```csharp
public BaseSpawnPool(int poolSize, Transform poolTarget, bool willGrow)
```

| Parametre | Açıklama |
|---|---|
| `poolSize` | Önceden ayrılan öğe sayısı |
| `poolTarget` | Havuzlanan öğeler için ebeveyn `Transform` |
| `willGrow` | `true` ise havuz tükendiğinde yeni öğeler oluşturulur |

---

## Sınıf: `BaseSpawnPool<TModel, TItem>`

```csharp
public class BaseSpawnPool<TModel, TItem> : BaseSpawnPool
    where TModel : class, ISpawnItemModel
    where TItem : MonoBehaviour, ISpawnItem<TModel>
```

### Yapıcı

```csharp
public BaseSpawnPool(
    Func<TModel, Transform, TItem> spawnFunc,
    int poolSize,
    Transform poolTarget,
    bool willGrow)
```

| Parametre | Açıklama |
|---|---|
| `spawnFunc` | Fabrika delegesi — yeni `TItem` oluşturur; DI enjeksiyonu ile sağlayın |
| `poolSize` | Önceden ayrılan sayı; öğeler yapıcıda hemen oluşturulur |
| `poolTarget` | Ebeveyn `Transform`; etkin olmayan öğeler buraya taşınır |
| `willGrow` | Havuzun `poolSize`'ı aşmasına izin verir |

Ön tahsis: yapıcı sırasında `spawnFunc(default, poolTarget)`, `poolSize` kadar çağrılır. Her öğe devre dışı bırakılır ve etkin olmayan kuyruğa eklenir.

### Temel Metotlar

#### `GetItem`

```csharp
public TItem GetItem(TModel data, Transform parent = null)
```

1. Varsa etkin olmayan öğelerden kuyruktan çıkarır.
2. `willGrow` etkin ve etkin olmayan öğe yoksa: `spawnFunc` ile yeni öğe oluşturur.
3. `willGrow` etkin değil ve etkin olmayan öğe yoksa: `InvalidOperationException` fırlatır.
4. Öğe üzerinde `ReInitialize(data)` çağırır.
5. Öğeyi etkin yapar; aktif listeye ekler.

#### `ReleaseItem`

```csharp
public override void ReleaseItem(ISpawnItem spawnItem)
```

Öğeyi devre dışı yapar, `_poolTarget`'a yeniden ebeveynler, etkin olmayan kuyruğa geri ekler, aktif listeden çıkarır.

Öğenin kendisinden çağırın:

```csharp
public void ReleaseItem(ISpawnItem item)
{
    SpawnPool?.ReleaseItem(item);
}
```

#### `HideAllObjects`

Tüm aktif öğeleri devre dışı bırakır ve etkin olmayan kuyruğa taşır. Yok etmez.

#### `Dispose` / `Dispose(bool)`

Tüm aktif ve etkin olmayan `GameObject`'leri yok eder. Sahne/kapsam sona erdiğinde çağırın.

```csharp
~BaseSpawnPool()  // sonlandırıcı da Dispose(false) çağırır, açık temizliği atlar
```

---

## DI Uzantısı: `SpawnerExtensions.RegisterObjectSpawner`

```csharp
public static void RegisterObjectSpawner<TModel, TTransform, TItem>(
    this IContainerBuilder builder,
    TItem item,
    Lifetime lifetime,
    bool isInjectable = true)
    where TTransform : Transform
    where TModel : ISpawnItemModel
    where TItem : Component, ISpawnItem<TModel>
```

VContainer'a `Func<TModel, TTransform, TItem>` fabrikası kaydeder. `isInjectable = true` ise `container.Instantiate` (DI ile enjekte edilmiş prefab) kullanır; aksi hâlde `Object.Instantiate` kullanır.

```csharp
// LifetimeScope.Configure içinde:
builder.RegisterObjectSpawner<EnemyModel, Transform, EnemyItem>(
    _enemyPrefab, Lifetime.Singleton, isInjectable: true);

// Havuzu oluşturduğunuz yere fabrikayı enjekte edin:
[Inject] private readonly Func<EnemyModel, Transform, EnemyItem> _enemyFactory;

private EnemyPool _pool;

public override void Init()
{
    _pool = new BaseSpawnPool<EnemyModel, EnemyItem>(_enemyFactory, 10, _spawnParent, false);
}
```

---

## Somut Öğe Örneği

```csharp
public class EnemyItem : MonoBehaviour, ISpawnItem<EnemyModel>
{
    public BaseSpawnPool SpawnPool { get; set; }
    public EnemyModel ItemModel { get; set; }

    [SerializeField] private Renderer _renderer;

    public void ReInitialize(EnemyModel model)
    {
        ItemModel = model;
        transform.position = model.SpawnPosition;
        // Canı, animasyonları vb. sıfırla
    }

    public void ReleaseItem(ISpawnItem item)
    {
        SpawnPool?.ReleaseItem(item);
    }

    public void Dispose()
    {
        ReleaseItem(this);
    }
}
```

---

## Ayrıca Bakınız

- `ISpawnItemModel` — veri modeli işaretçisi
- `SpawnerExtensions` — DI kaydı
- VContainer `RegisterFactory` — altta yatan mekanizma
