# How to Use the Spawner

## Overview

The VGameKit spawner provides a DI-friendly object pool built on `BaseSpawnPool<TModel, TItem>`. Items are reused rather than destroyed. This guide shows how to define a pool, wire it through VContainer, and spawn/release items at runtime.

**Prerequisites:** `VGameKit.Runtime` module installed; an `AbsBaseLifetimeScope` for the scene.

---

## Step 1 — Define the model

Create a plain class that implements `ISpawnItemModel`. Keep it a simple data container.

```csharp
using VGameKit.Runtime.Spawner;

public class EnemyModel : ISpawnItemModel
{
    public int Health;
    public float Speed;
}
```

---

## Step 2 — Create the spawn item

Create a MonoBehaviour that implements `ISpawnItem<TModel>`. Override `ReInitialize` to reset state on each reuse.

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
        // Reset visual/logic state here
        // When the enemy dies, call ReleaseItem(this) to return it to the pool
    }

    public void ReleaseItem(ISpawnItem item)
    {
        SpawnPool?.ReleaseItem(item);
    }

    public void Dispose() { }
}
```

---

## Step 3 — Define the pool class

Create a concrete pool class that passes the type parameters to `BaseSpawnPool<TModel, TItem>`.

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

## Step 4 — Register in the LifetimeScope

Use `RegisterObjectSpawner` to register a DI factory that the pool will use to instantiate new items.

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
        // Pass isInjectable: true so VContainer injects into instantiated prefabs.
        builder.RegisterObjectSpawner<EnemyModel, Transform, EnemyItem>(
            _enemyPrefab, Lifetime.Singleton, isInjectable: true);
    }
}
```

---

## Step 5 — Create and use the pool at runtime

Inject the factory and the parent transform into the class that owns the pool:

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
        // Create a pool of 10 enemies that can grow if exhausted
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
        _pool?.Dispose(); // destroys all pooled GameObjects
    }
}
```

---

## Returning an item to the pool

From inside the item itself call `ReleaseItem(this)`, which delegates to `SpawnPool.ReleaseItem`:

```csharp
// Inside EnemyItem when the enemy dies:
public void OnDeath()
{
    ReleaseItem(this);
}
```

To hide all active items at once (e.g., on level end):

```csharp
_pool.HideAllObjects();
```

---

## Pool behaviour summary

| Parameter | Effect |
|-----------|--------|
| `poolSize` | Number of items pre-instantiated at construction |
| `willGrow: true` | New items are created when the pool is empty |
| `willGrow: false` | `InvalidOperationException` is thrown when the pool is empty |
| `Dispose()` | Destroys all active and inactive GameObjects and clears internal collections |

---

## See also

- [Spawner reference](../reference/api/spawner.md)
- [Lifetime Scopes reference](../reference/api/lifetime-scopes.md)
