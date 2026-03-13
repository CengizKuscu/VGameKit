# Explanation: Object Pooling and the Spawner System

## Why object pooling

`Instantiate` and `Destroy` are expensive in Unity. Each call allocates memory, triggers garbage collection, and involves the physics and rendering subsystems. For frequently created objects — enemies, projectiles, particles — this produces frame-rate spikes.

Object pooling solves this by reusing a fixed set of pre-created instances. An object is not destroyed when it is no longer needed; it is deactivated and returned to the pool. When a new instance is needed, the pool reactivates an existing one instead of allocating.

---

## BaseSpawnPool architecture

`BaseSpawnPool<TModel, TItem>` is VGameKit's pooling foundation:

```
BaseSpawnPool<TModel, TItem>
├── _factory: Func<TModel, Transform, TItem>   creates new instances
├── _poolRoot: Transform                        parent for pooled GameObjects
├── _available: Stack<TItem>                   inactive items ready to reuse
└── _active: List<TItem>                       currently live items
```

### The factory pattern

The pool does not call `Instantiate` directly. It delegates to an injected factory:

```csharp
Func<EnemyModel, Transform, EnemyItem> factory =
    (model, parent) => Object.Instantiate(prefab, parent);
```

This keeps the pool itself testable and free of prefab references. The factory is registered in the LifetimeScope and injected by VContainer — the pool never uses `Resources.Load` or scene references.

---

## Spawn lifecycle

```
Spawn(model)
    if _available is not empty:
        pop item from _available
        item.Initialize(model)         ← re-configure for new use
        item.gameObject.SetActive(true)
        add to _active
    else:
        call _factory(model, _poolRoot) ← allocate only when pool is exhausted
        add to _active

Recycle(item)
    item.Recycle()                     ← caller signals "I'm done"
    item.gameObject.SetActive(false)
    move from _active → _available

RecycleAll()
    recycle every item in _active

Dispose()
    RecycleAll()
    destroy all GameObjects
    clear both collections
```

---

## ISpawnItem contract

Every poolable item implements two methods:

| Method | Called when | Purpose |
|---|---|---|
| `Initialize(model)` | On spawn | Apply fresh configuration to the item |
| `Recycle()` | On recycle | Reset internal state; deactivate |

`Initialize` is called on both new and reused instances, so items must fully reset themselves from the model — never assume state carried over from a previous use.

---

## Scoped lifetime and disposal

Pools are registered as `Lifetime.Scoped`. VContainer calls `Dispose()` automatically when the scope ends (scene unload, app shutdown). This destroys all remaining GameObjects without requiring manual cleanup.

Registering as `Lifetime.Singleton` risks leaked GameObjects across scene loads. Always use `Lifetime.Scoped` for pools tied to a scene.

---

## What to avoid

- **Calling `Instantiate` inside game logic**: route all creation through the pool.
- **Holding references to recycled items**: after `Recycle()`, the item may be given to a different caller at any time.
- **Re-creating objects on `Initialize`**: `Initialize` should reconfigure, not allocate. If `Initialize` calls `Instantiate`, you have a pool that leaks.
- **Static pools**: static state survives scene loads and produces stale references.

---

## See also

- [Reference: Spawner System](en-Reference-Spawner)
- [How-to: Use the spawner](en-HowTo-Use-Spawner)
- [Tutorial: Spawner basics](en-Tutorial-Spawner-Basics)
- [Explanation: VContainer and DI](en-Explanation-VContainer-DI)
