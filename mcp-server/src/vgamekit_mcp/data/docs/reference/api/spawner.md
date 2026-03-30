# Spawner System Reference

**Namespace:** `VGameKit.Runtime.Spawner`
**Assembly:** `VGameKit.Runtime`
**Files:** `Assets/VGameKit/Runtime/Spawner/`

---

## Overview

The spawner system provides a generic, DI-compatible object pool for Unity `MonoBehaviour` items. Pools pre-instantiate a fixed number of items, reuse them via `GetItem`, and return them via `ReleaseItem`. Growth is configurable.

---

## Type Overview

| Type | Kind | Description |
|---|---|---|
| `ISpawnItemModel` | interface | Marker for item data models |
| `ISpawnItem` | interface | Base spawn item contract |
| `ISpawnItem<TModel>` | interface | Typed spawn item with reinitialise hook |
| `BaseSpawnPool` | class | Non-generic pool base |
| `BaseSpawnPool<TModel, TItem>` | class | Generic pool implementation |
| `SpawnerExtensions` | static class | DI registration helper |

---

## Interface: `ISpawnItemModel`

```csharp
public interface ISpawnItemModel { }
```

Marker only. Implement on your data class/struct:

```csharp
public class EnemyModel : ISpawnItemModel
{
    public int Health;
    public Vector3 SpawnPosition;
}
```

---

## Interface: `ISpawnItem`

```csharp
public interface ISpawnItem : IDisposable
```

| Member | Description |
|---|---|
| `BaseSpawnPool SpawnPool { get; set; }` | Back-reference to the pool that owns this item |
| `void ReleaseItem(ISpawnItem item)` | Call to return this item to its pool |

---

## Interface: `ISpawnItem<TModel>`

```csharp
public interface ISpawnItem<TModel> : ISpawnItem
    where TModel : ISpawnItemModel
```

| Member | Description |
|---|---|
| `TModel ItemModel { get; set; }` | The data model assigned to this item |
| `void ReInitialize(TModel itemModel)` | Called by pool on `GetItem`; use to reset state |

---

## Class: `BaseSpawnPool` *(non-generic)*

```csharp
public class BaseSpawnPool : IDisposable
```

Non-generic base. Holds pool configuration fields. Extend via the generic version.

### Constructor

```csharp
public BaseSpawnPool(int poolSize, Transform poolTarget, bool willGrow)
```

| Parameter | Description |
|---|---|
| `poolSize` | Pre-allocated item count |
| `poolTarget` | Parent `Transform` for pooled items |
| `willGrow` | If `true`, creates new items when the pool is exhausted |

---

## Class: `BaseSpawnPool<TModel, TItem>`

```csharp
public class BaseSpawnPool<TModel, TItem> : BaseSpawnPool
    where TModel : class, ISpawnItemModel
    where TItem : MonoBehaviour, ISpawnItem<TModel>
```

### Constructor

```csharp
public BaseSpawnPool(
    Func<TModel, Transform, TItem> spawnFunc,
    int poolSize,
    Transform poolTarget,
    bool willGrow)
```

| Parameter | Description |
|---|---|
| `spawnFunc` | Factory delegate — creates a new `TItem`; provide via DI injection |
| `poolSize` | Pre-allocated count; items are created immediately in the constructor |
| `poolTarget` | Parent `Transform`; inactive items are reparented here |
| `willGrow` | Allow pool to exceed `poolSize` |

Pre-allocation: during construction, `spawnFunc(default, poolTarget)` is called `poolSize` times. Each item is deactivated and added to the inactive queue.

### Key Methods

#### `GetItem`

```csharp
public TItem GetItem(TModel data, Transform parent = null)
```

1. Dequeues from inactive items if available.
2. If `willGrow` and no inactive items: calls `spawnFunc` to create a new one.
3. If not `willGrow` and no inactive items: throws `InvalidOperationException`.
4. Calls `ReInitialize(data)` on the item.
5. Sets item active; adds to active list.

#### `ReleaseItem`

```csharp
public override void ReleaseItem(ISpawnItem spawnItem)
```

Sets item inactive, reparents to `_poolTarget`, adds back to inactive queue, removes from active list.

Call from the item itself:

```csharp
public void ReleaseItem(ISpawnItem item)
{
    SpawnPool?.ReleaseItem(item);
}
```

#### `HideAllObjects`

Deactivates all active items and moves them to the inactive queue. Does not destroy.

#### `Dispose` / `Dispose(bool)`

Destroys all active and inactive `GameObject`s. Call when the scene/scope ends.

```csharp
~BaseSpawnPool()  // finalizer also calls Dispose(false), skipping explicit cleanup
```

---

## DI Extension: `SpawnerExtensions.RegisterObjectSpawner`

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

Registers a `Func<TModel, TTransform, TItem>` factory into VContainer. When `isInjectable = true`, uses `container.Instantiate` (DI-injected prefab); otherwise uses `Object.Instantiate`.

```csharp
// In LifetimeScope.Configure:
builder.RegisterObjectSpawner<EnemyModel, Transform, EnemyItem>(
    _enemyPrefab, Lifetime.Singleton, isInjectable: true);

// Inject the factory wherever you create the pool:
[Inject] private readonly Func<EnemyModel, Transform, EnemyItem> _enemyFactory;

private EnemyPool _pool;

public override void Init()
{
    _pool = new BaseSpawnPool<EnemyModel, EnemyItem>(_enemyFactory, 10, _spawnParent, false);
}
```

---

## Concrete Item Example

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
        // Reset health, animations, etc.
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

## See Also

- `ISpawnItemModel` — data model marker
- `SpawnerExtensions` — DI registration
- VContainer `RegisterFactory` — underlying mechanism
