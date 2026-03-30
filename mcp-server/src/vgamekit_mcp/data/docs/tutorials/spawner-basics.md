# Tutorial: Spawner Basics

## What you will build

An enemy pool that instantiates prefabs on demand, recycles them when they are no longer needed, and cleans up automatically when the scene scope is destroyed.

**Time:** ~25 minutes  
**Prerequisites:** Completed [Getting Started](./getting-started.md); `VGameKit.Runtime` installed; a prefab ready.

---

## Step 1 — Create the model

A model holds the data that configures one spawned instance. Implement `ISpawnItemModel`.

```csharp
using VGameKit.Runtime.Spawner;

public class EnemyModel : ISpawnItemModel
{
    public int Health;
    public float Speed;
    public UnityEngine.Vector3 SpawnPosition;
}
```

> **Note:** The model may have no fields at all. An empty class that implements `ISpawnItemModel` is valid when the spawn signal itself is sufficient.

---

## Step 2 — Create the pool item

The item is a `MonoBehaviour` that implements `ISpawnItem<TModel>`. Override `ReInitialize` to reset state on each reuse. Call `ReleaseItem(this)` when the item is done.

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
        // Apply model data, reset animations, health, etc.
    }

    public void ReleaseItem(ISpawnItem item)
    {
        SpawnPool?.ReleaseItem(item);
    }

    public void Dispose() { }
}
```

> `ReInitialize` is called automatically by the pool inside `GetItem`. Do not call it manually after `GetItem`.

---

## Step 3 — Create the pool class

Inherit `BaseSpawnPool<TModel, TItem>` and pass all parameters through to the base constructor.

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

## Step 4 — Register in LifetimeScope

Use `RegisterObjectSpawner` to register the DI factory. Also register the parent `Transform` component so it can be injected into the pool owner.

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

        // Registers Func<EnemyModel, Transform, EnemyItem> in the container.
        // isInjectable: true so VContainer injects into instantiated prefabs.
        builder.RegisterObjectSpawner<EnemyModel, Transform, EnemyItem>(
            _enemyPrefab, Lifetime.Singleton, isInjectable: true);
    }
}
```

---

## Step 5 — Spawn and recycle enemies

Inject the factory and parent transform into the class that owns the pool.

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
        // GetItem automatically calls ReInitialize(model) — do not call it again.
        GKLog.Log(LogState.Development, $"Spawned enemy at {position}");
    }

    public void HideAll()
    {
        _pool.HideAllObjects();
        GKLog.Log(LogState.Development, "All enemies hidden.");
    }

    public override void Dispose()
    {
        base.Dispose();
        _pool?.Dispose();
    }
}
```

To return an individual enemy to the pool from inside the item:

```csharp
// Inside EnemyItem, called when the enemy dies:
public void OnDeath()
{
    ReleaseItem(this);
}
```

---

## Step 6 — Run the scene

1. Attach `GameLifetimeScope` to a GameObject and assign the prefab and enemy parent transform.
2. Enter Play Mode and call `SpawnEnemy(Vector3.zero)` from a test button or `Update` loop.
3. Observe the Console and scene hierarchy — enemies should appear, then deactivate on release.

---

## What you learned

- How `BaseSpawnPool<TModel, TItem>` manages a reusable object pool.
- How `RegisterObjectSpawner` registers a DI-friendly `Func<TModel, Transform, TItem>` factory.
- How `GetItem` automatically calls `ReInitialize` — avoiding double-initialization bugs.
- How `ReleaseItem` returns an item to the pool without destroying the `GameObject`.
- How `Dispose()` ties pool cleanup to scope disposal.

---

## Next steps

- [Tutorial: Process Flows](./processflows.md)
- [How-to: Use the spawner](../how-to/use-spawner.md)
- [Reference: Spawner System](../reference/api/spawner.md)
