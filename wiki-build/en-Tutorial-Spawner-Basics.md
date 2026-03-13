# Tutorial: Spawner Basics

## What you will build

An enemy pool that instantiates prefabs on demand, recycles them when they are no longer needed, and cleans up automatically when the scene scope is destroyed.

**Time:** ~25 minutes  
**Prerequisites:** Completed [Getting Started](en-Tutorial-Getting-Started); `VGameKit.Runtime` installed; a prefab ready.

---

## Step 1 — Create the model

A model holds the data that configures one spawned instance.

```csharp
public class EnemyModel
{
    public int Health;
    public float Speed;
    public UnityEngine.Vector3 SpawnPosition;
}
```

---

## Step 2 — Create the pool item

The item represents one enemy instance in the pool.

```csharp
using UnityEngine;
using VGameKit.Runtime.Spawner;

public class EnemyItem : MonoBehaviour, ISpawnItem<EnemyModel>
{
    public void Initialize(EnemyModel model)
    {
        transform.position = model.SpawnPosition;
        // apply model data to the GameObject
    }

    public void Recycle()
    {
        gameObject.SetActive(false);
    }
}
```

---

## Step 3 — Create the pool

Inherit `BaseSpawnPool<TModel, TItem>`:

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

`BaseSpawnPool` handles object activation, capacity management, and the `Dispose()` teardown.

---

## Step 4 — Register the pool in LifetimeScope

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

        // Register the DI-friendly factory
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

Using `Lifetime.Scoped` ensures `Dispose()` is called automatically when the scope ends.

---

## Step 5 — Spawn and recycle enemies

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
        GKLog.Log(LogState.Development, $"Spawned enemy at {position}");
    }

    public void RecycleAll()
    {
        _pool.RecycleAll();
        GKLog.Log(LogState.Development, "All enemies recycled.");
    }
}
```

---

## Step 6 — Run the scene

1. Attach `GameLifetimeScope` to a GameObject and assign the prefab and pool root.
2. Enter Play Mode and call `SpawnEnemy(Vector3.zero)` from a test button or `Update` loop.
3. Observe the Console and scene hierarchy — enemies should appear, then deactivate on recycle.

---

## What you learned

- How `BaseSpawnPool<TModel, TItem>` manages a reusable object pool.
- How to inject a DI-friendly factory `Func<TModel, Transform, TItem>`.
- How `Lifetime.Scoped` ties pool cleanup to scope disposal.
- How `Initialize` / `Recycle` hooks on the item avoid re-creating GameObjects.

---

## Next steps

- [Tutorial: Process Flows](en-Tutorial-ProcessFlows)
- [How-to: Use the spawner](en-HowTo-Use-Spawner)
- [Reference: Spawner System](en-Reference-Spawner)
